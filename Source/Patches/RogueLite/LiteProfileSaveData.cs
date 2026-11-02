using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LBoL.Core;
using LBoL.Core.SaveData;
using LBoL.Presentation;
using LBoLEntitySideloader.PersistentValues;
using LBoLEntitySideloader.Utils;
using YamlDotNet.Serialization;

namespace lvalonmima.Source.Patches;

public class LiteProfileSaveData
{
	public const string filePrefix = "gr";
	public static void RegisterCustomSaveData(LiteProfileSaveData customSaveData, string GUID)
	{
		var ass = Assembly.GetCallingAssembly();
		InternalRegisterCustomData(ass, customSaveData);
	}
	public void RegisterSelf(string GUID)
	{
		var ass = Assembly.GetCallingAssembly();
		InternalRegisterCustomData(ass, this);
	}

	public SaveDataID GetID(string GUID) => new()
	{
		GUID = GUID,
		Name = Name,
		midfix = filePrefix
	};

	private static void InternalRegisterCustomData(Assembly assembly, LiteProfileSaveData customData)
	{
		// If the registering assembly was loaded from disk, remember it persistently so it survives reloads
		if (assembly.IsLoadedFromDisk())
			MiniTracker.AddLoadedFromDisk(customData);

		// Set the active singleton instance (SetActive will log on duplicates)
		MiniTracker.Instance.SetActive(customData);

	}
	[YamlIgnore]
	public virtual string Name => "";
	[YamlIgnore]
	public bool DeleteFileOnGamerunEnd => false;
	internal bool EncodeToBinary => true;
	public virtual IEnumerable<IYamlTypeConverter> TypeConverters()
	{
		yield break;
	}
	// Top-level file schema version. ShopSaveLoader.Save sets it before every
	// write; files from before it existed (0.2.12) have no such key, read as 0
	// and go through ShopSaveLoader.Migrate.
	public int SchemaVersion;
	public Dictionary<string, LiteShop> Saves = [];
	private string _activeRunProfileKey;
	[YamlIgnore]
	internal string ActiveRunProfileKey => _activeRunProfileKey;

	// False until the shop file has been read (or found missing). Until then no
	// shop is created, so an early lookup can't put an empty shop into the save.
	private bool _fileLoaded;
	// True when the last load found no readable copy and started the shop empty.
	private bool _fileLoadFailed;

	private const string KeyPrefix = "profile:";

	// The only place a profile key is built. Keyed by creation timestamp alone, so
	// renaming a profile keeps its shop. 0.2.12 and earlier used
	// "profile:{CreationTimestamp}_{Name}"; those keys are migrated when a
	// version 0 file is read (MigrateLegacyProfileKeys).
	public static string ProfileKey(ProfileSaveData profile) =>
		profile != null ? KeyPrefix + profile.CreationTimestamp : null;

	// The current profile's key, or null when there is no GameMaster or profile.
	internal static string CurrentProfileKey()
	{
		var gm = GameMaster.Instance;
		return gm != null ? ProfileKey(gm.CurrentProfile) : null;
	}

	// The stable key that a legacy "profile:{ts}_{name}" key belongs to, or null if key is not a legacy key.
	private static string StableKeyOfLegacyKey(string key)
	{
		if (key == null || !key.StartsWith(KeyPrefix, StringComparison.Ordinal))
			return null;
		int separator = key.IndexOf('_', KeyPrefix.Length);
		return separator > KeyPrefix.Length ? key.Substring(0, separator) : null;
	}

	internal void MarkFileLoaded(bool loadFailed = false)
	{
		_fileLoaded = true;
		_fileLoadFailed = loadFailed;
	}

	public LiteShop GetShopForCurrentProfile() => GetOrCreateShop(CurrentProfileKey(), "current profile");

	private LiteShop GetShopForProfileKey(string key) => GetOrCreateShop(key, "active run profile");

	/// <summary>
	/// Finds the current profile's shop without creating one.
	/// </summary>
	public LiteShop FindShopForCurrentProfile() => FindShop(CurrentProfileKey());

	private LiteShop FindShop(string key)
	{
		if (string.IsNullOrEmpty(key) || Saves == null)
			return null;
		return Saves.TryGetValue(key, out var shop) ? shop : null;
	}

	private LiteShop GetOrCreateShop(string key, string reason)
	{
		if (string.IsNullOrEmpty(key) || Saves == null)
			return null;

		var shop = FindShop(key);
		if (shop != null)
			return shop;

		if (!_fileLoaded)
		{
			if (ShopSaveLoader.Once("pre-load lookup"))
				ShopSaveLoader.Trace($"Shop lookup for {key} ({reason}) before the shop file loaded; returning no shop. (Logged once per session.)");
			return null;
		}

		shop = new LiteShop();
		Saves[key] = shop;
		LogCreatedShop(key, reason);
		return shop;
	}

	/// <summary>
	/// Schema step 0 -> 1: for every legacy "profile:{ts}_{name}" key whose stable key
	/// "profile:{ts}" is missing, moves a legacy shop under the stable key. With several
	/// legacy entries (the profile was renamed under 0.2.12 and its shop restarted), the
	/// richest is moved and the others stay in the save unchanged. It runs on the data
	/// read from a version 0 file, before that is merged into the active save data, so
	/// every load of the file migrates the same way and no legacy key is merged back in.
	/// </summary>
	internal void MigrateLegacyProfileKeys()
	{
		if (Saves == null)
			return;

		var groups = Saves
			.Where(kv => kv.Value != null)
			.Select(kv => (stableKey: StableKeyOfLegacyKey(kv.Key), entry: kv))
			.Where(x => x.stableKey != null && !Saves.ContainsKey(x.stableKey))
			.GroupBy(x => x.stableKey, x => x.entry)
			.ToList();
		foreach (var group in groups)
		{
			var candidates = group
				.OrderByDescending(kv => ShopWorth(kv.Value))
				.ThenByDescending(kv => kv.Value.TierCount())
				.ThenBy(kv => kv.Key, StringComparer.Ordinal)
				.ToList();
			var chosen = candidates[0];
			// The legacy entry leaves the file in the same write that adds the stable
			// key, so the data is never on disk under neither key.
			Saves.Remove(chosen.Key);
			Saves[group.Key] = chosen.Value;

			if (candidates.Count == 1)
				ShopSaveLoader.Trace($"Migrated shop {DescribeShop(chosen.Key, chosen.Value)} to key {group.Key}.");
			else
				BepinexPlugin.log?.LogWarning($"[ShopSave] Found {candidates.Count} legacy shops for {group.Key}; migrated the richest, {DescribeShop(chosen.Key, chosen.Value)}. Left in the save unchanged: {string.Join("; ", candidates.Skip(1).Select(kv => DescribeShop(kv.Key, kv.Value)))}");
		}
	}

	// Money plus the BluePoints spent on bought tiers, so a shop that spent its money still ranks high.
	private static long ShopWorth(LiteShop shop)
	{
		long worth = shop.MoneyOwned;
		if (shop.Items == null)
			return worth;

		foreach (var item in shop.Items.Values)
		{
			if (item?.TierCosts == null)
				continue;
			int bought = Math.Min(item.CurrentTier, item.TierCosts.Count);
			for (int i = 0; i < bought; i++)
				worth += item.TierCosts[i];
		}
		return worth;
	}

	private static string DescribeShop(string key, LiteShop shop) =>
		$"{key} (money={shop.MoneyOwned}, tiers={shop.TierCount()}, challenger={shop.ChallengerModeEnabled})";

	// A new profile getting its first shop is routine, so that is a trace. It is a
	// warning when the save holds other keys for the same timestamp, or when the
	// file could not be read, because then the profile's progress may be lost.
	private void LogCreatedShop(string key, string reason)
	{
		var sameProfile = Saves.Keys.Where(k => StableKeyOfLegacyKey(k) == key).ToList();
		if (sameProfile.Count == 0 && !_fileLoadFailed)
		{
			ShopSaveLoader.Trace($"Created new empty shop for key {key} ({reason}).");
			return;
		}

		var cause = _fileLoadFailed
			? "the shop file could not be read"
			: $"the save has other keys for this profile: [{string.Join(", ", sameProfile)}]";
		var others = string.Join(", ", Saves.Keys.Where(k => k != key));
		BepinexPlugin.log?.LogWarning($"[ShopSave] Created new empty shop for key {key} ({reason}); it was not in the save and {cause}. Other keys: [{others}]");
	}

	// Run flag naming the profile that owns the run: RunOwnerFlagPrefix + ProfileKey,
	// e.g. "lvalonmima.owner:profile:1700000000". It is saved with the run, so it
	// survives quit-and-Continue and can't be left over from another profile's run.
	private const string RunOwnerFlagPrefix = "lvalonmima.owner:";

	private static string RunOwnerKey(GameRunController run) =>
		ShopModHandlers.RunFlagPayloads(run, RunOwnerFlagPrefix).FirstOrDefault(key => key.Length > 0);

	private static void SetRunOwnerKey(GameRunController run, string key)
	{
		if (run?.ExtraFlags == null || string.IsNullOrEmpty(key))
			return;
		run.ExtraFlags.RemoveWhere(flag => flag != null && flag.StartsWith(RunOwnerFlagPrefix, StringComparison.Ordinal));
		run.ExtraFlags.Add(RunOwnerFlagPrefix + key);
	}

	/// <summary>
	/// The profile key that owns run: its owner flag, or for a run without one (saved by
	/// 0.2.12 or earlier) the current profile, which is then written to the run.
	/// With no run, the current profile.
	/// </summary>
	private string ResolveRunOwnerKey(GameRunController run)
	{
		if (run == null)
			return CurrentProfileKey();

		string key = RunOwnerKey(run);
		if (key == null)
		{
			key = CurrentProfileKey();
			if (key == null)
				return null;
			SetRunOwnerKey(run, key);
			ShopSaveLoader.Trace($"Run has no owner profile flag (saved by 0.2.12 or earlier); assigned it to the current profile {key}");
		}

		if (_activeRunProfileKey != key)
		{
			ShopSaveLoader.Trace($"Active run profile key {_activeRunProfileKey ?? "<none>"} -> {key} (current profile {CurrentProfileKey() ?? "<none>"})");
			_activeRunProfileKey = key;
		}
		return key;
	}

	/// <summary>
	/// A new run belongs to the profile it was started with.
	/// </summary>
	internal void OnRunStarted(GameRunController run, ProfileSaveData owner)
	{
		ClearActiveRunProfileKey("run start");
		SetRunOwnerKey(run, ProfileKey(owner) ?? CurrentProfileKey());
		ResolveRunOwnerKey(run);
	}

	/// <summary>
	/// A restored run keeps its saved owner; a legacy run without one gets the current profile.
	/// </summary>
	internal void OnRunRestored(GameRunController run)
	{
		ClearActiveRunProfileKey("run restore");
		ResolveRunOwnerKey(run);
	}

	internal void ClearActiveRunProfileKey(string reason)
	{
		if (_activeRunProfileKey == null)
			return;
		ShopSaveLoader.Trace($"Cleared active run profile key {_activeRunProfileKey} ({reason})");
		_activeRunProfileKey = null;
	}

	/// <summary>
	/// The shop of the profile that owns run (see ResolveRunOwnerKey).
	/// </summary>
	public LiteShop GetShopForRun(GameRunController run) => GetShopForProfileKey(ResolveRunOwnerKey(run));

	public void Save(int amount, bool ending = true)
	{
		var gm = GameMaster.Instance;
		Save(amount, gm != null ? gm.CurrentGameRun : null, ending);
	}

	/// <summary>
	/// Banks amount Blue Points, after the owner's multipliers, to the profile that owns gameRun.
	/// </summary>
	public void Save(int amount, GameRunController gameRun, bool ending = true)
	{
		string key = ResolveRunOwnerKey(gameRun);
		var shop = GetShopForProfileKey(key);
		if (shop != null && shop.ChallengerModeEnabled)
		{
			int adjusted = ApplyBluePointMults(shop, gameRun, amount);
			if (adjusted > 0)
			{
				shop.AddMoney(adjusted);
				ShopSaveLoader.Trace($"Banked {adjusted} Blue Points (base {amount}) to {key}; current profile {CurrentProfileKey() ?? "<none>"}, money now {shop.MoneyOwned}");
			}
		}
		if (ending)
			ClearActiveRunProfileKey("run end");
	}

	private static int ApplyBluePointMults(LiteShop shop, GameRunController gameRun, int bluePoint)
	{
		double mult = 1.0;
		if (shop?.GetItem("difficulty.reverse")?.CurrentTier > 0)
		{
			if (!IsWinningResult(gameRun?.GameRunRecord?.ResultType ?? GameResultType.Failure))
				return 0;
			mult *= 0.5;
		}

		if (shop?.GetItem("difficulty.ascension")?.CurrentTier > 0)
			mult *= 2;

		return toolbox.Round(mult * bluePoint);
	}

	private static bool IsWinningResult(GameResultType resultType)
	{
		return resultType != GameResultType.Failure;
	}

	/// <summary>
	/// Restore merges deserialized data into the active LiteProfileSaveData instance (if present), or registers itself as active.
	/// The data has already been migrated to the current schema (ShopSaveLoader.Migrate).
	/// </summary>
	public void Restore(string reason)
	{
		try
		{
			var active = MiniTracker.Instance.CustomGrSaveData;
			if (active == null)
			{
				ReconcileSaves(Saves);
				// No active instance; set this deserialized instance as active
				MiniTracker.Instance.SetActive(this);
				MarkFileLoaded();
				return;
			}

			// Merge saved shops into active instance
			foreach (var kv in Saves)
				active.Saves[kv.Key] = LiteShop.ReconcileWithDefaults(kv.Value);
			active.MarkFileLoaded();
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogError($"[ShopSave] Restore failed ({reason}); shops read from disk may not have been applied: {e}");
		}
	}

	private static void ReconcileSaves(Dictionary<string, LiteShop> saves)
	{
		if (saves == null || saves.Count == 0)
			return;

		var keys = new List<string>(saves.Keys);
		foreach (var key in keys)
			saves[key] = LiteShop.ReconcileWithDefaults(saves[key]);
	}

	/// <summary>
	/// Execute code after gamerun has ended. GameMaster.Instance.CurrentGameRun might still be available at that point.
	/// </summary>
	public virtual void OnGamerunEnded()
	{
	}
}
