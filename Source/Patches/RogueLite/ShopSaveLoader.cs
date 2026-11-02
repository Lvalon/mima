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
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using static LBoL.Presentation.GameMaster;
using lvalonmima.Exhibits;

namespace lvalonmima.Source.Patches;

public static class ShopSaveLoader
{
	private const string SaveFileName = "lvalonmimaShopSave.txt";
	// Version of the file's shape, written as the top-level SchemaVersion on
	// every save. Files without the key (0.2.12 and earlier) read as 0. Bump it
	// together with a new step in Migrate whenever a serialized field is
	// renamed or reshaped.
	internal const int CurrentSchemaVersion = 1;
	private static bool _loadFailed;
	private static bool _blockedSaveWarned;
	private static bool _replaceFallbackWarned;
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

	// Every file the shop save touches is derived from GetSaveFilePath, so
	// renaming the save is a change to SaveFileName alone.
	private static string GetSaveFilePath()
	{
		return Path.Combine(GameMaster.PlatformHandler.GetSaveDataFolder(), SaveFileName);
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
			if (!_replaceFallbackWarned)
			{
				_replaceFallbackWarned = true;
				BepinexPlugin.log?.LogWarning($"[ShopSave] File.Replace failed, using delete-and-move instead (logged once per session): {e.Message}");
			}
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

		var gm = GameMaster.Instance;
		var key = LiteProfileSaveData.ProfileKey(gm != null ? gm.CurrentProfile : null);
		var shopText = "no shop for key";
		if (key != null && csd.Saves != null && csd.Saves.TryGetValue(key, out var shop) && shop != null)
		{
			int tiers = shop.Items != null ? shop.Items.Values.Where(i => i != null).Sum(i => i.CurrentTier) : 0;
			shopText = $"money={shop.MoneyOwned}, tiers={tiers}, challenger={shop.ChallengerModeEnabled}";
		}

		return $"key={key ?? "<no profile>"}, activeRunKey={csd.ActiveRunProfileKey ?? "<none>"}, {shopText}, keysInSave={csd.Saves?.Count ?? 0}";
	}

	public static void Save(string reason)
	{
		var filePath = GetSaveFilePath();
		if (_loadFailed && File.Exists(filePath))
		{
			if (!_blockedSaveWarned)
			{
				_blockedSaveWarned = true;
				BepinexPlugin.log?.LogWarning($"[ShopSave] Save skipped ({reason}): no readable copy of {SaveFileName} could be loaded, so the file is left untouched and changes from this session are not saved. (Logged once per session.)");
			}
			Trace($"Save skipped ({reason}): earlier load failed. {DescribeState(MiniTracker.Instance.CustomGrSaveData)}");
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

	public static void Load(string location)
	{
		var customData = MiniTracker.Instance.CustomGrSaveData ?? MiniTracker.LoadedFromDiskCustomGrSaveData;
		if (customData == null)
		{
			Trace($"Load ({location}) skipped: no save data registered");
			return;
		}
		if (MiniTracker.Instance.CustomGrSaveData == null)
			MiniTracker.Instance.SetActive(customData);
		var csd = customData;

		var filePath = GetSaveFilePath();
		var tempPath = GetTempFilePath(filePath);
		var backupPath = GetBackupFilePath(filePath);
		bool mainExists = File.Exists(filePath);

		if (!mainExists && !File.Exists(tempPath) && !File.Exists(backupPath))
		{
			csd.MarkFileLoaded();
			Trace($"Load ({location}): no file at {filePath}. {DescribeState(csd)}");
			return;
		}

		if (mainExists && TryReadSaveFile(csd, filePath, location, out var loadedData, out _))
		{
			ApplyLoaded(loadedData, filePath, location);
			TryShowPendingLoadProblemDialog();
			return;
		}

		if (!mainExists)
			BepinexPlugin.log?.LogWarning($"[ShopSave] Load ({location}): {filePath} is missing but a temp or backup copy exists; an earlier save was probably interrupted. Trying those.");

		// The temp file is only a candidate when the main file is missing: then
		// a swap was interrupted after the old file moved away, and the temp
		// file was flushed before the swap began, so it is the newest copy.
		var unreadable = new List<string>();
		if (mainExists)
			unreadable.Add(filePath);
		var candidates = mainExists ? new[] { backupPath } : new[] { tempPath, backupPath };
		string recoveredFrom = null;
		byte[] recoveredBytes = null;
		LiteProfileSaveData recoveredData = null;
		foreach (var candidate in candidates)
		{
			if (!File.Exists(candidate))
				continue;
			if (TryReadSaveFile(csd, candidate, location, out recoveredData, out recoveredBytes))
			{
				recoveredFrom = candidate;
				break;
			}
			unreadable.Add(candidate);
		}

		if (recoveredFrom == null && mainExists)
		{
			// Nothing to recover from: block saving so the unreadable file stays
			// as it is and can still be recovered by hand.
			_loadFailed = true;
			BepinexPlugin.log?.LogError($"[ShopSave] Load failed ({location}): no readable copy of {SaveFileName} was found. {filePath} is left untouched and saving is blocked until a later load succeeds.");
			QueueLoadProblemDialog("Shop.SaveProblem.Unreadable", Path.GetFileName(filePath));
			TryShowPendingLoadProblemDialog();
			return;
		}

		// Saving resumes from here, so first move every unreadable copy to
		// where later saves will not overwrite it.
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
				BepinexPlugin.log?.LogError($"[ShopSave] Could not move unreadable {path} to {corruptPath}; the next save may overwrite it: {e}");
			}
		}
		var keptText = kept.Count > 0 ? string.Join(", ", kept) : "-";

		if (recoveredFrom == null)
		{
			// Nothing was restored, so this counts as a load of an empty file.
			_loadFailed = false;
			csd.MarkFileLoaded();
			BepinexPlugin.log?.LogError($"[ShopSave] Load failed ({location}): no readable copy of {SaveFileName} was found; the shop starts empty and saving resumes. Unreadable copies kept: {keptText}");
			QueueLoadProblemDialog("Shop.SaveProblem.Fresh", keptText);
			TryShowPendingLoadProblemDialog();
			return;
		}

		// Put the recovered copy back as the main file, byte for byte, so the
		// next load reads it directly. The .bak is left as it is.
		try
		{
			WriteFileAtomic(filePath, recoveredBytes);
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogError($"[ShopSave] Could not write the copy recovered from {recoveredFrom} back to {filePath}; the next save will write it: {e}");
		}

		ApplyLoaded(recoveredData, recoveredFrom, location);
		BepinexPlugin.log?.LogWarning($"[ShopSave] Load ({location}) recovered from {recoveredFrom}. Unreadable copies kept: {keptText}");
		// A missing main file with a good temp or backup is an interrupted
		// save, not a bad one, so the player is only told when a copy was
		// unreadable.
		if (unreadable.Count > 0)
			QueueLoadProblemDialog("Shop.SaveProblem.Recovered", keptText);
		TryShowPendingLoadProblemDialog();
	}

	private static void ApplyLoaded(LiteProfileSaveData loadedData, string path, string location)
	{
		long fileLength = GetFileLength(path);
		loadedData.Restore($"{location}, {Path.GetFileName(path)} {fileLength} bytes");
		_loadFailed = false;
		Trace($"Load ({location}): {Path.GetFileName(path)} {fileLength} bytes, {loadedData.Saves?.Count ?? 0} keys on disk. {DescribeState(MiniTracker.Instance.CustomGrSaveData)}");
	}

	// Reads, decrypts, deserializes, migrates and reconciles one file. It does
	// not touch the active save data; the caller restores the result.
	private static bool TryReadSaveFile(LiteProfileSaveData csd, string path, string location, out LiteProfileSaveData loadedData, out byte[] rawBytes)
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

			Migrate(data, decodedYaml, path);

			if (data.Saves != null)
			{
				foreach (var key in data.Saves.Keys.ToList())
					data.Saves[key] = LiteShop.ReconcileWithDefaults(data.Saves[key]);
			}

			loadedData = data;
			rawBytes = bytes;
			return true;
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogError($"[ShopSave] Could not read {path} ({location}), {fileLength} bytes: {e}");
			return false;
		}
	}

	// Brings a freshly deserialized file up to CurrentSchemaVersion, one step
	// per version. Each step gets the file's raw YAML as well as the typed
	// data, because IgnoreUnmatchedProperties has already dropped any key the
	// current classes no longer have: a rename reads the old key from `raw`
	// and writes it into `data`.
	private static void Migrate(LiteProfileSaveData data, string yaml, string path)
	{
		int fromVersion = data.SchemaVersion;
		if (fromVersion > CurrentSchemaVersion)
		{
			BepinexPlugin.log?.LogWarning($"[ShopSave] {path} has schema version {fromVersion}, newer than this mod's {CurrentSchemaVersion}; fields this version does not know are dropped on the next save.");
			return;
		}
		if (fromVersion == CurrentSchemaVersion)
			return;

		var raw = ParseRawYaml(yaml);
		while (data.SchemaVersion < CurrentSchemaVersion)
		{
			switch (data.SchemaVersion)
			{
				case 0:
					MigrateV0ToV1(raw, data);
					break;
				default:
					throw new InvalidDataException($"no migration from schema version {data.SchemaVersion}");
			}
			data.SchemaVersion++;
		}
		Trace($"Migrated {Path.GetFileName(path)} from schema version {fromVersion} to {CurrentSchemaVersion}");
	}

	// Version 0 is the unversioned file written by 0.2.12 and earlier. Its
	// fields have the same names and shapes as version 1, so nothing needs
	// converting; version 1 only adds the SchemaVersion key.
	private static void MigrateV0ToV1(YamlMappingNode raw, LiteProfileSaveData data)
	{
	}

	private static YamlMappingNode ParseRawYaml(string yaml)
	{
		var stream = new YamlStream();
		stream.Load(new StringReader(yaml));
		if (stream.Documents.Count == 0)
			return new YamlMappingNode();
		return stream.Documents[0].RootNode as YamlMappingNode ?? new YamlMappingNode();
	}

	// Tells the player once per session, the same way the game reports a
	// corrupted run save (GameMaster.ShowErrorDialogWithKey).
	private static void QueueLoadProblemDialog(string subTextKey, string fileNames)
	{
		if (_loadProblemDialogShown || _pendingLoadProblemDialog != null)
			return;

		_pendingLoadProblemDialog = new MessageContent
		{
			TextKey = "Shop.SaveProblem",
			SubTextKey = subTextKey,
			SubTextArguments = [fileNames],
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

	// Restore returns the new run before GameMaster.CurrentGameRun is set, so
	// __result is the only reliable "current run" here.
	static void Postfix(GameRunController __result)
	{
		ShopSaveLoader.Load("GameRunController.Restore");
		exquesting.ProcessDeferredRestoreHydration(__result);
		ShopModHandlers.ReapplyPerStageEffectsOnRestore(__result);

		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop != null && shop.ChallengerModeEnabled && shop.GetItem("alter.wings")?.CurrentTier > 0)
			GameMaster.Instance?.StartCoroutine(DeferredAddMapModeOverrider());
	}

	// Harmony skips postfixes when the original throws (the game does that on a
	// corrupt save and catches it itself), so the bookkeeping is cleared here.
	// The exception is left to propagate to the game's own handler.
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
			BepinexPlugin.log?.LogWarning(e);
		}
	}
}
