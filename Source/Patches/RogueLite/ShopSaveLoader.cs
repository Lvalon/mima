using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.SaveData;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Panels;
using YamlDotNet.Serialization;
using static LBoL.Presentation.GameMaster;
using lvalonmima.Exhibits;

namespace lvalonmima.Source.Patches;

public static class ShopSaveLoader
{
	// Steam Cloud syncs only *.sav files (and Profiles/*.json) at the save
	// root, so the shop file uses that extension. Its .bak, .tmp and
	// .corrupt-* siblings end in other extensions and stay local.
	private const string SaveFileName = "lvalonmimaShopSave.sav";
	// Written by 0.2.12 and the builds before the rename. It is only ever
	// read: when no .sav file exists it is loaded and copied to the .sav
	// name, and it is never modified, renamed or deleted, so a downgrade or a
	// failed first write loses nothing.
	private const string LegacySaveFileName = "lvalonmimaShopSave.txt";
	// Version of the file's shape, written as the top-level SchemaVersion on
	// every save. Files without the key (0.2.12 and earlier) read as 0. Bump it
	// together with a new step in Migrate whenever a serialized field is
	// renamed or reshaped.
	internal const int CurrentSchemaVersion = 1;
	// Set by a load that found an unreadable copy and could not move it aside,
	// so the next save would overwrite the only copy of that data. Saves are
	// skipped while it is set. Every load clears it before it looks at the
	// files, so it never outlasts the session; the next launch tries the move
	// again.
	private static string _saveBlockedBy;
	private static readonly HashSet<string> LoggedOnce = new(StringComparer.Ordinal);
	private static MessageContent _pendingLoadProblemDialog;
	private static bool _loadProblemDialogShown;
	// Single on/off switch for the load/save and profile-change trace lines.
	// Errors and warnings are logged either way.
	public static bool TraceEnabled = true;
	public static bool IsGameRunRestoreInProgress { get; private set; }

	private static readonly byte[] EncryptionKey =
	[
		0x4C, 0x76, 0x61, 0x6C, 0x6F, 0x6E, 0x6D, 0x69,
		0x6D, 0x61, 0x53, 0x68, 0x6F, 0x70, 0x4B, 0x65,
		0x79, 0x32, 0x30, 0x32, 0x34, 0x53, 0x65, 0x63,
		0x72, 0x65, 0x74, 0x44, 0x61, 0x74, 0x61, 0x21
	];

	private static readonly byte[] EncryptionIV =
	[
		0x21, 0x61, 0x74, 0x61, 0x44, 0x74, 0x65, 0x72,
		0x63, 0x65, 0x53, 0x34, 0x32, 0x30, 0x32, 0x79
	];

	private static byte[] Encrypt(byte[] data)
	{
		using Aes aes = Aes.Create();
		aes.Key = EncryptionKey;
		aes.IV = EncryptionIV;
		aes.Mode = CipherMode.CBC;
		aes.Padding = PaddingMode.PKCS7;

		using ICryptoTransform encryptor = aes.CreateEncryptor();
		using MemoryStream ms = new();
		using CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write);
		cs.Write(data, 0, data.Length);
		cs.FlushFinalBlock();
		return ms.ToArray();
	}

	private static byte[] Decrypt(byte[] encryptedData)
	{
		using Aes aes = Aes.Create();
		aes.Key = EncryptionKey;
		aes.IV = EncryptionIV;
		aes.Mode = CipherMode.CBC;
		aes.Padding = PaddingMode.PKCS7;

		using ICryptoTransform decryptor = aes.CreateDecryptor();
		using MemoryStream ms = new(encryptedData);
		using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
		using MemoryStream resultStream = new();
		cs.CopyTo(resultStream);
		return resultStream.ToArray();
	}

	// Every file the shop save touches is derived from GetSaveFilePath or
	// GetLegacySaveFilePath, so renaming the save is a change to SaveFileName
	// alone.
	private static string GetSaveFilePath()
	{
		return Path.Combine(GameMaster.PlatformHandler.GetSaveDataFolder(), SaveFileName);
	}

	private static string GetLegacySaveFilePath()
	{
		return Path.Combine(GameMaster.PlatformHandler.GetSaveDataFolder(), LegacySaveFileName);
	}

	private static string GetTempFilePath(string filePath) => filePath + ".tmp";

	private static string GetBackupFilePath(string filePath) => filePath + ".bak";

	private static string GetCorruptFilePath(string filePath) => $"{filePath}.corrupt-{DateTime.Now:yyyyMMdd-HHmmss}";

	// Writes to a temp file, flushes it to disk, then swaps it in, so a crash
	// or force-quit leaves either the old file or the new one, never a
	// truncated one. The file being replaced is kept as the .bak.
	private static void WriteFileAtomic(string filePath, byte[] data)
	{
		var folder = Path.GetDirectoryName(filePath);
		if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
			Directory.CreateDirectory(folder);

		var tempPath = GetTempFilePath(filePath);
		using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
		{
			stream.Write(data, 0, data.Length);
			stream.Flush(true);
		}

		if (!File.Exists(filePath))
		{
			File.Move(tempPath, filePath);
			return;
		}

		var backupPath = GetBackupFilePath(filePath);
		try
		{
			File.Replace(tempPath, filePath, backupPath);
		}
		catch (Exception e)
		{
			if (Once("File.Replace fallback"))
				BepinexPlugin.log?.LogWarning($"[ShopSave] File.Replace failed, using delete-and-move instead (logged once per session): {e.Message}");
			// Replace can fail after it has already moved the old file to .bak,
			// so only move what is still there.
			if (File.Exists(filePath))
			{
				if (File.Exists(backupPath))
					File.Delete(backupPath);
				File.Move(filePath, backupPath);
			}
			File.Move(tempPath, filePath);
		}
	}

	public static void SetGameRunRestoreInProgress(bool inProgress)
	{
		IsGameRunRestoreInProgress = inProgress;
	}

	public static bool GetGameRunRestoreInProgress()
	{
		return IsGameRunRestoreInProgress;
	}

	internal static void Trace(string message)
	{
		if (TraceEnabled)
			BepinexPlugin.log?.LogInfo("[ShopSave] " + message);
	}

	// True the first time it is called with id in this session, for lines that
	// are logged once per session.
	internal static bool Once(string id) => LoggedOnce.Add(id);

	private static long GetFileLength(string filePath)
	{
		try
		{
			return File.Exists(filePath) ? new FileInfo(filePath).Length : -1;
		}
		catch (Exception)
		{
			return -1;
		}
	}

	private static string DescribeState(LiteProfileSaveData csd)
	{
		if (csd == null)
			return "no save data registered";

		var key = LiteProfileSaveData.CurrentProfileKey();
		var shopText = "no shop for key";
		if (key != null && csd.Saves != null && csd.Saves.TryGetValue(key, out var shop) && shop != null)
			shopText = $"money={shop.MoneyOwned}, tiers={shop.TierCount()}, challenger={shop.ChallengerModeEnabled}";

		return $"key={key ?? "<no profile>"}, activeRunKey={csd.ActiveRunProfileKey ?? "<none>"}, {shopText}, keysInSave={csd.Saves?.Count ?? 0}";
	}

	public static void Save(string reason)
	{
		var filePath = GetSaveFilePath();
		if (_saveBlockedBy != null && File.Exists(_saveBlockedBy))
		{
			if (Once("save blocked"))
				BepinexPlugin.log?.LogWarning($"[ShopSave] Save skipped ({reason}): the unreadable {Path.GetFileName(_saveBlockedBy)} could not be moved aside, so it is left untouched and changes from this session are not saved. (Logged once per session.)");
			Trace($"Save skipped ({reason}): an unreadable copy could not be moved aside. {DescribeState(MiniTracker.Instance.CustomGrSaveData)}");
			return;
		}

		long lengthBefore = GetFileLength(filePath);
		var customData = MiniTracker.Instance.CustomGrSaveData;
		var csd = customData;
		var shop = customData?.GetShopForCurrentProfile();
		Dictionary<string, int> originalQuestProgress = null;
		Dictionary<string, string> originalQuestRequirements = null;
		HashSet<string> originalCompletedQuestCards = null;
		Dictionary<string, int> originalQuestModifiers = null;
		try
		{
			if (shop != null)
			{
				originalQuestProgress = shop.QuestProgress != null
					? new Dictionary<string, int>(shop.QuestProgress, StringComparer.Ordinal)
					: new Dictionary<string, int>(StringComparer.Ordinal);

				originalQuestRequirements = shop.QuestRequirements != null
					? new Dictionary<string, string>(shop.QuestRequirements, StringComparer.Ordinal)
					: new Dictionary<string, string>(StringComparer.Ordinal);

				originalCompletedQuestCards = shop.QuestCompletedCards != null
					? new HashSet<string>(shop.QuestCompletedCards.Where(id => !string.IsNullOrEmpty(id)), StringComparer.Ordinal)
					: new HashSet<string>(StringComparer.Ordinal);

				originalQuestModifiers = shop.QuestModifiers != null
					? new Dictionary<string, int>(shop.QuestModifiers, StringComparer.Ordinal)
					: new Dictionary<string, int>(StringComparer.Ordinal);

				Dictionary<string, int> persistedProgress = originalQuestProgress
					.Where(kvp => !string.IsNullOrEmpty(kvp.Key))
					.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);

				Dictionary<string, string> persistedRequirements = originalQuestRequirements
					.Where(kvp => !string.IsNullOrEmpty(kvp.Key)
						&& !string.IsNullOrEmpty(kvp.Value)
						&& persistedProgress.ContainsKey(kvp.Key))
					.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);

				HashSet<string> persistedCompleted = new(
					originalCompletedQuestCards.Where(id => !string.IsNullOrEmpty(id)),
					StringComparer.Ordinal);

				Dictionary<string, int> persistedModifiers = originalQuestModifiers
					.Where(kvp => !string.IsNullOrEmpty(kvp.Key))
					.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);

				shop.QuestProgress = persistedProgress;
				shop.QuestRequirements = persistedRequirements;
				shop.QuestCompletedCards = persistedCompleted;
				shop.QuestModifiers = persistedModifiers;

			}

			// csd.Save(); // Note: csd.Save() adds BluePoints from current run, usually not desired if just saving shop state from menu.
			// However, if we are just persisting the current state (which includes purchased items), we just serialize csd.

			csd.SchemaVersion = CurrentSchemaVersion;
			using StringWriter stringWriter = new() { NewLine = "\n" };
			var seBuilder = new SerializerBuilder().DisableAliases();
			csd.TypeConverters().Do((tc) => { seBuilder = seBuilder.WithTypeConverter(tc); });

			seBuilder.Build().Serialize(stringWriter, csd);
			var data = SaveDataHelper.EncodeYaml(stringWriter.ToString(), false);
			var encryptedData = Encrypt(data);

			WriteFileAtomic(filePath, encryptedData);
			Trace($"Save ({reason}): {DescribeState(csd)}, file {lengthBefore} -> {GetFileLength(filePath)} bytes (wrote {encryptedData.Length})");
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogError($"[ShopSave] Save failed ({reason}), file {lengthBefore} -> {GetFileLength(filePath)} bytes: {e}");
		}
		finally
		{
			if (shop != null)
			{
				shop.QuestProgress = originalQuestProgress ?? new Dictionary<string, int>(StringComparer.Ordinal);
				shop.QuestRequirements = originalQuestRequirements ?? new Dictionary<string, string>(StringComparer.Ordinal);
				shop.QuestCompletedCards = originalCompletedQuestCards ?? new HashSet<string>(StringComparer.Ordinal);
				shop.QuestModifiers = originalQuestModifiers ?? new Dictionary<string, int>(StringComparer.Ordinal);
			}
		}
	}

	// Load order:
	// - No file of the .sav family (.sav, .sav.tmp, .sav.bak) exists: the legacy
	//   .txt family is read (LoadWithoutSaveFile).
	// - The .sav and the legacy .txt both exist and the .txt is newer: an older
	//   build saved after this one (a downgrade), so the .txt is read and copied
	//   to the .sav name.
	// - Otherwise the .sav family is read, newest copy first. Unreadable copies
	//   are moved to .corrupt-<ts>. With no readable copy, the legacy .txt family
	//   is the last resort, and after that the shop starts empty.
	// Saving works after every outcome, except for the rest of the session when
	// an unreadable copy could not be moved aside.
	public static void Load(string reason)
	{
		var customData = MiniTracker.Instance.CustomGrSaveData ?? MiniTracker.LoadedFromDiskCustomGrSaveData;
		if (customData == null)
		{
			Trace($"Load ({reason}) skipped: no save data registered");
			return;
		}
		if (MiniTracker.Instance.CustomGrSaveData == null)
			MiniTracker.Instance.SetActive(customData);
		var csd = customData;
		_saveBlockedBy = null;

		var filePath = GetSaveFilePath();
		var tempPath = GetTempFilePath(filePath);
		var backupPath = GetBackupFilePath(filePath);
		var legacyPath = GetLegacySaveFilePath();
		bool mainExists = File.Exists(filePath);

		if (!mainExists && !File.Exists(tempPath) && !File.Exists(backupPath))
		{
			LoadWithoutSaveFile(csd, filePath, legacyPath, reason);
			TryShowPendingLoadProblemDialog();
			return;
		}

		if (mainExists && LoadNewerLegacy(csd, filePath, legacyPath, reason))
		{
			TryShowPendingLoadProblemDialog();
			return;
		}

		if (!mainExists)
			BepinexPlugin.log?.LogWarning($"[ShopSave] Load ({reason}): {filePath} is missing but a temp or backup copy exists; an earlier save was probably interrupted. Trying those.");

		// The temp file is only a candidate when the main file is missing: then
		// a swap was interrupted after the old file moved away, and the temp
		// file was flushed before the swap began, so it is the newest copy.
		var candidates = mainExists ? new[] { filePath, backupPath } : new[] { tempPath, backupPath };
		var unreadable = new List<string>();
		if (TryReadFirst(csd, candidates, reason, unreadable, out var readFrom, out var loadedData, out var bytes) && readFrom == filePath)
		{
			ApplyLoaded(loadedData, filePath, reason);
			TryShowPendingLoadProblemDialog();
			return;
		}

		// Saving resumes from here, so first move every unreadable copy to
		// where later saves will not overwrite it.
		var keptText = MoveAsideUnreadable(unreadable);

		if (readFrom != null)
		{
			AdoptCopy(filePath, readFrom, loadedData, bytes, reason, legacy: false);
			BepinexPlugin.log?.LogWarning($"[ShopSave] Load ({reason}) recovered from {readFrom}. Unreadable copies kept: {keptText}");
			// A missing main file with a good temp or backup is an interrupted
			// save, not a bad one, so the player is only told when a copy was
			// unreadable.
			if (unreadable.Count > 0)
				QueueLoadProblemDialog(_saveBlockedBy != null ? "Shop.SaveProblem.Unreadable" : "Shop.SaveProblem.Recovered", keptText);
			TryShowPendingLoadProblemDialog();
			return;
		}

		// No copy of the .sav family is readable. The legacy family is older,
		// but it is better than an empty shop.
		var legacyUnreadable = new List<string>();
		if (TryReadFirst(csd, LegacyCandidates(legacyPath), reason, legacyUnreadable, out readFrom, out loadedData, out bytes))
		{
			AdoptCopy(filePath, readFrom, loadedData, bytes, reason, legacy: true);
			BepinexPlugin.log?.LogError($"[ShopSave] Load ({reason}): no readable copy of {SaveFileName} was found, so the shop was restored from legacy {readFrom}, which may be missing recent progress. Unreadable copies kept: {keptText}");
			if (_saveBlockedBy != null)
				QueueLoadProblemDialog("Shop.SaveProblem.Unreadable", keptText);
			else
				QueueLoadProblemDialog("Shop.SaveProblem.Legacy", keptText, Path.GetFileName(readFrom));
			TryShowPendingLoadProblemDialog();
			return;
		}

		StartFresh(csd, reason, $"no readable copy of {SaveFileName} or legacy {LegacySaveFileName} was found", keptText);
		TryShowPendingLoadProblemDialog();
	}

	// Called only when no file of the .sav family exists. Reads the legacy .txt
	// family (see LegacyCandidates) and writes the readable copy to the .sav
	// name byte for byte, so later loads use it. No legacy file is ever
	// modified, renamed or deleted, unreadable ones included: nothing writes the
	// .txt names any more, so they cannot be overwritten either. With only
	// unreadable legacy copies the shop starts empty in the .sav, and the next
	// launch reads that .sav.
	private static void LoadWithoutSaveFile(LiteProfileSaveData csd, string filePath, string legacyPath, string reason)
	{
		var unreadable = new List<string>();
		bool read = TryReadFirst(csd, LegacyCandidates(legacyPath), reason, unreadable, out var readFrom, out var loadedData, out var bytes);
		var unreadableText = unreadable.Count > 0 ? string.Join(", ", unreadable.Select(Path.GetFileName)) : "-";

		if (read)
		{
			AdoptCopy(filePath, readFrom, loadedData, bytes, reason, legacy: true);
			if (unreadable.Count > 0)
			{
				BepinexPlugin.log?.LogWarning($"[ShopSave] Load ({reason}) recovered from legacy {readFrom}. Unreadable legacy copies left in place: {unreadableText}");
				QueueLoadProblemDialog("Shop.SaveProblem.Recovered", unreadableText);
			}
			return;
		}

		if (unreadable.Count == 0)
		{
			csd.MarkFileLoaded();
			Trace($"Load ({reason}): no file at {filePath}. {DescribeState(csd)}");
			return;
		}

		StartFresh(csd, reason, $"only unreadable copies of legacy {LegacySaveFileName} were found and {SaveFileName} does not exist yet; saves go to {SaveFileName} and the legacy files are left in place", unreadableText);
	}

	// When the legacy .txt was modified after the .sav, an older build of the
	// mod (which only knows the .txt) saved after this one last did: the player
	// downgraded and played. That .txt is then loaded and copied to the .sav
	// name, as when no .sav exists. Returns false, and the .sav family is read
	// as usual, when the .txt is not newer or cannot be read.
	private static bool LoadNewerLegacy(LiteProfileSaveData csd, string filePath, string legacyPath, string reason)
	{
		if (!File.Exists(legacyPath))
			return false;

		DateTime saveTime, legacyTime;
		try
		{
			saveTime = File.GetLastWriteTime(filePath);
			legacyTime = File.GetLastWriteTime(legacyPath);
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogWarning($"[ShopSave] Load ({reason}): could not compare the modified times of {SaveFileName} and legacy {LegacySaveFileName}; using {SaveFileName}: {e.Message}");
			return false;
		}

		if (legacyTime <= saveTime)
		{
			if (Once("legacy ignored"))
				Trace($"Both {SaveFileName} ({DescribeFile(filePath)}) and legacy {LegacySaveFileName} ({DescribeFile(legacyPath)}) exist; {SaveFileName} is newer and used, and the legacy file is ignored and left untouched. (Logged once per session.)");
			return false;
		}

		BepinexPlugin.log?.LogWarning($"[ShopSave] Load ({reason}): legacy {LegacySaveFileName} (modified {legacyTime:yyyy-MM-dd HH:mm:ss}) is newer than {SaveFileName} (modified {saveTime:yyyy-MM-dd HH:mm:ss}), so an older build of the mod saved after this one. Loading the legacy file and copying it to {SaveFileName}; the previous {SaveFileName} becomes {Path.GetFileName(GetBackupFilePath(filePath))} until the next save replaces it.");
		if (!TryReadSaveFile(csd, legacyPath, reason, out var loadedData, out var bytes))
		{
			BepinexPlugin.log?.LogWarning($"[ShopSave] Load ({reason}): the newer legacy {LegacySaveFileName} could not be read, so {SaveFileName} is used and progress saved by the older build is not loaded. The legacy file is left in place.");
			return false;
		}

		AdoptCopy(filePath, legacyPath, loadedData, bytes, reason, legacy: true);
		return true;
	}

	// Legacy .txt family, newest copy first, mirroring the .sav order: with the
	// .txt present it is .txt, then .txt.bak (the save before it), then .txt.tmp
	// (a write whose swap never ran; it may be torn, so it comes last); with the
	// .txt missing, a swap was interrupted after the old file moved away, so
	// .txt.tmp is the newest copy, then .txt.bak.
	private static string[] LegacyCandidates(string legacyPath)
	{
		var legacyTempPath = GetTempFilePath(legacyPath);
		var legacyBackupPath = GetBackupFilePath(legacyPath);
		return File.Exists(legacyPath)
			? [legacyPath, legacyBackupPath, legacyTempPath]
			: [legacyTempPath, legacyBackupPath];
	}

	// Reads the candidates in order and stops at the first readable one.
	// Missing candidates are skipped; unreadable ones are added to unreadable.
	private static bool TryReadFirst(LiteProfileSaveData csd, IEnumerable<string> candidates, string reason, List<string> unreadable, out string readFrom, out LiteProfileSaveData loadedData, out byte[] bytes)
	{
		foreach (var candidate in candidates)
		{
			if (!File.Exists(candidate))
				continue;
			if (TryReadSaveFile(csd, candidate, reason, out loadedData, out bytes))
			{
				readFrom = candidate;
				return true;
			}
			unreadable.Add(candidate);
		}
		readFrom = null;
		loadedData = null;
		bytes = null;
		return false;
	}

	// Moves each unreadable copy to .corrupt-<ts>, where later saves will not
	// overwrite it. A copy that cannot be moved would be overwritten by the
	// next save, so saving is blocked for the rest of the session instead.
	// Returns the names the copies are now kept under, for the log and dialog.
	private static string MoveAsideUnreadable(List<string> unreadable)
	{
		var kept = new List<string>();
		foreach (var path in unreadable)
		{
			var corruptPath = GetCorruptFilePath(path);
			try
			{
				File.Move(path, corruptPath);
				kept.Add(Path.GetFileName(corruptPath));
				BepinexPlugin.log?.LogWarning($"[ShopSave] Kept unreadable {path} as {corruptPath}");
			}
			catch (Exception e)
			{
				_saveBlockedBy ??= path;
				kept.Add(Path.GetFileName(path));
				BepinexPlugin.log?.LogError($"[ShopSave] Could not move unreadable {path} to {corruptPath}, so saving is off for this session to keep it from being overwritten: {e}");
			}
		}
		return kept.Count > 0 ? string.Join(", ", kept) : "-";
	}

	// Nothing could be read: the shop starts empty and, unless an unreadable
	// copy is still in the way, saving resumes. MarkFileLoaded lets lookups
	// create shops again, so Blue Points can bank.
	private static void StartFresh(LiteProfileSaveData csd, string reason, string what, string keptText)
	{
		csd.MarkFileLoaded(loadFailed: true);
		if (_saveBlockedBy != null)
		{
			BepinexPlugin.log?.LogError($"[ShopSave] Load failed ({reason}): {what}. The shop starts empty and is not saved this session. Unreadable copies: {keptText}");
			QueueLoadProblemDialog("Shop.SaveProblem.Unreadable", keptText);
			return;
		}
		BepinexPlugin.log?.LogError($"[ShopSave] Load failed ({reason}): {what}. The shop starts empty and saving resumes. Unreadable copies kept: {keptText}");
		QueueLoadProblemDialog("Shop.SaveProblem.Fresh", keptText);
	}

	// Puts a copy read from another file (the .bak, the .tmp or a legacy file)
	// in place as the .sav, byte for byte, so the next load reads it directly,
	// then applies it. The source file is left as it is. While saving is
	// blocked the copy is only applied: the write would rotate an unreadable
	// copy that could not be moved aside.
	private static void AdoptCopy(string filePath, string readFrom, LiteProfileSaveData loadedData, byte[] bytes, string reason, bool legacy)
	{
		var source = legacy ? "legacy " + readFrom : readFrom;
		if (_saveBlockedBy == null)
		{
			try
			{
				WriteFileAtomic(filePath, bytes);
				Trace($"Load ({reason}): copied {source} to {filePath}{(legacy ? "; the legacy file is left in place." : ".")}");
			}
			catch (Exception e)
			{
				BepinexPlugin.log?.LogError($"[ShopSave] Could not copy {source} to {filePath}; the next save will write it{(legacy ? ", and the legacy file is left in place" : "")}: {e}");
			}
		}
		ApplyLoaded(loadedData, readFrom, reason);
	}

	private static string DescribeFile(string path)
	{
		try
		{
			if (!File.Exists(path))
				return "missing";
			var info = new FileInfo(path);
			return $"{info.Length} bytes, modified {info.LastWriteTime:yyyy-MM-dd HH:mm:ss}";
		}
		catch (Exception)
		{
			return "unknown";
		}
	}

	private static void ApplyLoaded(LiteProfileSaveData loadedData, string path, string reason)
	{
		long fileLength = GetFileLength(path);
		loadedData.Restore($"{reason}, {Path.GetFileName(path)} {fileLength} bytes");
		Trace($"Load ({reason}): {Path.GetFileName(path)} {fileLength} bytes, {loadedData.Saves?.Count ?? 0} keys on disk. {DescribeState(MiniTracker.Instance.CustomGrSaveData)}");
	}

	// Reads, decrypts, deserializes, migrates and reconciles one file. It does
	// not touch the active save data; the caller restores the result.
	private static bool TryReadSaveFile(LiteProfileSaveData csd, string path, string reason, out LiteProfileSaveData loadedData, out byte[] rawBytes)
	{
		loadedData = null;
		rawBytes = null;
		long fileLength = GetFileLength(path);
		try
		{
			var deBuilder = new DeserializerBuilder().IgnoreUnmatchedProperties();
			csd.TypeConverters().Do((tc) => { deBuilder = deBuilder.WithTypeConverter(tc); });

			var bytes = File.ReadAllBytes(path);
			var decodedYaml = SaveDataHelper.DecodeYaml(Decrypt(bytes));
			var data = (LiteProfileSaveData)deBuilder.Build().Deserialize(decodedYaml, csd.GetType());
			if (data == null)
				throw new InvalidDataException("the file holds no save data");

			Migrate(data, path);

			if (data.Saves != null)
				foreach (var key in data.Saves.Keys.ToList())
					data.Saves[key] = LiteShop.ReconcileWithDefaults(data.Saves[key]);

			loadedData = data;
			rawBytes = bytes;
			return true;
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogError($"[ShopSave] Could not read {path} ({reason}), {fileLength} bytes: {e}");
			return false;
		}
	}

	// Brings a freshly deserialized file up to CurrentSchemaVersion, one step
	// per version, before it is merged into the active save data. A future step
	// that renames a field needs the file's raw YAML too, because
	// IgnoreUnmatchedProperties has already dropped any key the current classes
	// no longer have.
	private static void Migrate(LiteProfileSaveData data, string path)
	{
		int fromVersion = data.SchemaVersion;
		if (fromVersion > CurrentSchemaVersion)
		{
			BepinexPlugin.log?.LogWarning($"[ShopSave] {path} has schema version {fromVersion}, newer than this mod's {CurrentSchemaVersion}; fields this version does not know are dropped on the next save.");
			return;
		}
		if (fromVersion == CurrentSchemaVersion)
			return;

		while (data.SchemaVersion < CurrentSchemaVersion)
		{
			switch (data.SchemaVersion)
			{
				case 0:
					MigrateV0ToV1(data);
					break;
				default:
					throw new InvalidDataException($"no migration from schema version {data.SchemaVersion}");
			}
			data.SchemaVersion++;
		}
		Trace($"Migrated {Path.GetFileName(path)} from schema version {fromVersion} to {CurrentSchemaVersion}");
	}

	// Version 0 is the unversioned file written by 0.2.12 and earlier. Its
	// fields have the same names and shapes as version 1; version 1 adds the
	// SchemaVersion key and means the shops are keyed by "profile:{ts}" alone,
	// so the legacy "profile:{ts}_{name}" keys are migrated here.
	private static void MigrateV0ToV1(LiteProfileSaveData data)
	{
		data.MigrateLegacyProfileKeys();
	}

	// Tells the player once per session, the same way the game reports a
	// corrupted run save (GameMaster.ShowErrorDialogWithKey).
	private static void QueueLoadProblemDialog(string subTextKey, params string[] arguments)
	{
		if (_loadProblemDialogShown || _pendingLoadProblemDialog != null)
			return;

		_pendingLoadProblemDialog = new MessageContent
		{
			TextKey = "Shop.SaveProblem",
			SubTextKey = subTextKey,
			SubTextArguments = arguments,
			Icon = MessageIcon.Error,
		};
	}

	private static void TryShowPendingLoadProblemDialog()
	{
		if (_pendingLoadProblemDialog == null || !UiManager.IsInitialized)
			return;

		try
		{
			UiManager.GetDialog<MessageDialog>().Show(_pendingLoadProblemDialog);
			_pendingLoadProblemDialog = null;
			_loadProblemDialogShown = true;
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogWarning($"[ShopSave] Could not show the save problem dialog yet; it is retried on the next load: {e.Message}");
		}
	}
}

[HarmonyPatch(typeof(MainMenuPanel), nameof(MainMenuPanel.Awake))]
class MainMenuPanel_Awake_Patch
{
	static void Postfix()
	{
		ShopSaveLoader.Load("MainMenuPanel.Awake");
		LiteShopButton.RefreshMainMenuButtonLabel();
	}
}

[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.Save))]
[HarmonyPriority(Priority.VeryLow)]
class GameRunController_Save_Patch
{

	static void Postfix()
	{
		// 2do maybe. Optimize memory usage by not storing container values statically.
		var customData = MiniTracker.Instance?.CustomGrSaveData;
		if (customData == null)
			return;

		customData.Save(0, false);

		ShopSaveLoader.Save("GameRunController.Save");
	}
}

[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.Restore))]
[HarmonyPriority(Priority.VeryHigh)]
class GameRunController_Restore_Patch
{
	static void Prefix()
	{
		ShopSaveLoader.SetGameRunRestoreInProgress(true);
	}

	// The mod's only Restore postfix, so the order of its steps is fixed here.
	// Restore returns the new run before GameMaster.CurrentGameRun is set, so
	// __result is the only reliable "current run" here.
	// The game treats an exception from Restore as a corrupt run save and
	// deletes game{i}.sav, so a bug in a step must not escape: each step
	// catches and logs its own, and the run is kept.
	static void Postfix(GameRunController __result)
	{
		RunStep("load the shop file", () => ShopSaveLoader.Load("GameRunController.Restore"));
		// A restored run keeps its saved owner profile; see RunOwnerPatches.cs.
		RunStep("set the run's owner profile", () => MiniTracker.Instance.CustomGrSaveData?.OnRunRestored(__result));
		RunStep("hydrate the Quest Board", () => exquesting.ProcessDeferredRestoreHydration(__result));
		RunStep("re-apply per-stage shop effects", () => ShopModHandlers.ReapplyPerStageEffectsOnRestore(__result));
		RunStep("re-add the wings map mode", () =>
		{
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop != null && shop.ChallengerModeEnabled && shop.GetItem("alter.wings")?.CurrentTier > 0)
				GameMaster.Instance?.StartCoroutine(DeferredAddMapModeOverrider());
		});
	}

	private static void RunStep(string step, Action action)
	{
		try
		{
			action();
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogError($"[ShopSave] GameRunController.Restore postfix could not {step}; the run is kept and continues without it: {e}");
		}
	}

	// Harmony skips postfixes when the original throws (the game does that on a
	// corrupt save and catches it itself), so the bookkeeping is cleared here.
	// The postfix catches its own exceptions, so __exception is always the
	// game's; it is left to propagate to the game's own handler.
	static void Finalizer(Exception __exception)
	{
		ShopSaveLoader.SetGameRunRestoreInProgress(false);
		int dropped = exquesting.DiscardDeferredRestoreHydration();
		if (__exception != null)
			BepinexPlugin.log?.LogWarning($"[ShopSave] GameRunController.Restore threw; cleared restore-in-progress and dropped {dropped} deferred Quest Board hydration(s): {__exception}");
	}

	private static IEnumerator DeferredAddMapModeOverrider()
	{
		yield return null;

		var gameRun = Singleton<GameMaster>.Instance?.CurrentGameRun;
		if (gameRun?._mapModeOverriders != null && !gameRun._mapModeOverriders.Contains(RogueliteCrosser.Instance))
		{
			gameRun._mapModeOverriders.Add(RogueliteCrosser.Instance);
			gameRun.CheckMapMode();
		}
	}
}

[HarmonyPatch(typeof(GameMaster))]
[HarmonyPatch(nameof(CoAbandonGameRun))]
public static class CoAbandonGameRun_Postfix
{
	static void Postfix(GameStatisticData data)
	{
		var customData = MiniTracker.Instance.CustomGrSaveData;
		try
		{
			// Save logic from LiteProfileSaveData (add BluePoints)
			customData.Save(0, false);

			// Serialize to disk
			ShopSaveLoader.Save("GameMaster.CoAbandonGameRun");
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogWarning($"[ShopSave] Saving the shop after abandoning the run failed, so shop changes since the last save may be lost: {e}");
		}
	}
}
