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
	public Dictionary<string, LiteShop> Saves = [];
	private string _activeRunProfileKey;
	[YamlIgnore]
	internal string ActiveRunProfileKey => _activeRunProfileKey;

	// False until the shop file has been read (or found missing). Until then no
	// shop is created, so an early lookup can't put an empty shop into the save.
	private bool _fileLoaded;
	private bool _preLoadLookupLogged;

	private const string KeyPrefix = "profile:";

	// The only place a profile key is built. Keyed by creation timestamp alone, so
	// renaming a profile keeps its shop. 0.2.12 and earlier used
	// "profile:{CreationTimestamp}_{Name}"; those keys are migrated on load and on lookup.
	public static string ProfileKey(ProfileSaveData profile) =>
		profile != null ? KeyPrefix + profile.CreationTimestamp : null;

	// The stable key that a legacy "profile:{ts}_{name}" key belongs to, or null if key is not a legacy key.
	private static string StableKeyOfLegacyKey(string key)
	{
		if (key == null || !key.StartsWith(KeyPrefix, StringComparison.Ordinal))
			return null;
		int separator = key.IndexOf('_', KeyPrefix.Length);
		return separator > KeyPrefix.Length ? key.Substring(0, separator) : null;
	}

	private string GetProfileKey()
	{
		var gm = GameMaster.Instance;
		return gm != null ? ProfileKey(gm.CurrentProfile) : null;
	}

	internal void MarkFileLoaded()
	{
		_fileLoaded = true;
	}

	public LiteShop GetShopForCurrentProfile() => GetOrCreateShop(GetProfileKey(), "current profile");

	private LiteShop GetShopForProfileKey(string key) => GetOrCreateShop(key, "active run profile");

	/// <summary>
	/// Finds the profile's shop without creating one, migrating a legacy key if needed.
	/// </summary>
	public LiteShop FindShop(ProfileSaveData profile) => FindShop(ProfileKey(profile));

	private LiteShop FindShop(string key)
	{
		if (string.IsNullOrEmpty(key) || Saves == null)
			return null;

		if (Saves.TryGetValue(key, out var shop))
			return shop;

		return MigrateLegacyKeys(key);
	}

	private LiteShop GetOrCreateShop(string key, string lookup)
	{
		if (string.IsNullOrEmpty(key) || Saves == null)
			return null;

		var shop = FindShop(key);
		if (shop != null)
			return shop;

		if (!_fileLoaded)
		{
			if (!_preLoadLookupLogged)
			{
				_preLoadLookupLogged = true;
				ShopSaveLoader.Trace($"Shop lookup for {key} ({lookup}) before the shop file loaded; returning no shop. (Logged once per session.)");
			}
			return null;
		}

		shop = new LiteShop();
		Saves[key] = shop;
		LogCreatedShop(key, lookup);
		return shop;
	}

	/// <summary>
	/// If key is missing, moves the shop of a legacy "{key}_{name}" entry under key.
	/// With several legacy entries (the profile was renamed under 0.2.12 and its shop
	/// restarted), the richest is moved and the others stay in the save unchanged.
	/// Returns the shop now under key, or null if there was nothing to migrate.
	/// </summary>
	private LiteShop MigrateLegacyKeys(string key)
	{
		if (Saves == null || Saves.ContainsKey(key))
			return null;

		var candidates = Saves
			.Where(kv => kv.Value != null && StableKeyOfLegacyKey(kv.Key) == key)
			.OrderByDescending(kv => ShopWorth(kv.Value))
			.ThenByDescending(kv => TierCount(kv.Value))
			.ThenBy(kv => kv.Key, StringComparer.Ordinal)
			.ToList();
		if (candidates.Count == 0)
			return null;

		var chosen = candidates[0];
		// The legacy entry leaves the file in the same write that adds key, so the
		// data is never on disk under neither key.
		Saves.Remove(chosen.Key);
		Saves[key] = chosen.Value;

		if (candidates.Count == 1)
			BepinexPlugin.log?.LogInfo($"[ShopSave] Migrated shop {DescribeShop(chosen.Key, chosen.Value)} to key {key}.");
		else
			BepinexPlugin.log?.LogWarning($"[ShopSave] Found {candidates.Count} legacy shops for {key}; migrated the richest, {DescribeShop(chosen.Key, chosen.Value)}. Left in the save unchanged: {string.Join("; ", candidates.Skip(1).Select(kv => DescribeShop(kv.Key, kv.Value)))}");

		return chosen.Value;
	}

	private void MigrateAllLegacyKeys()
	{
		if (Saves == null)
			return;

		var stableKeys = Saves.Keys
			.Select(StableKeyOfLegacyKey)
			.Where(k => k != null && !Saves.ContainsKey(k))
			.Distinct()
			.ToList();
		foreach (var key in stableKeys)
			MigrateLegacyKeys(key);
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

	private static int TierCount(LiteShop shop) =>
		shop.Items != null ? shop.Items.Values.Where(i => i != null).Sum(i => i.CurrentTier) : 0;

	private static string DescribeShop(string key, LiteShop shop) =>
		$"{key} (money={shop.MoneyOwned}, tiers={TierCount(shop)}, challenger={shop.ChallengerModeEnabled})";

	private void LogCreatedShop(string key, string lookup)
	{
		var others = string.Join(", ", Saves.Keys.Where(k => k != key));
		BepinexPlugin.log?.LogWarning($"[ShopSave] Created new empty shop for key {key} ({lookup}); it was not in the save. Other keys: [{others}]");
	}

	private string GetActiveRunProfileKey()
	{
		if (_activeRunProfileKey != null)
			return _activeRunProfileKey;

		if (GameMaster.Instance?.CurrentGameRun == null)
			return GetProfileKey();

		_activeRunProfileKey = GetProfileKey();
		return _activeRunProfileKey;
	}

	private void ClearActiveRunProfileKey()
	{
		_activeRunProfileKey = null;
	}

	public void Save(int amount, bool ending = true)
	{
		var shop = GetShopForProfileKey(GetActiveRunProfileKey());
		if (shop == null || !shop.ChallengerModeEnabled)
			return;

		int adjusted = ApplyBluePointMults(shop, GameMaster.Instance?.CurrentGameRun, amount);
		if (adjusted > 0)
			shop.AddMoney(adjusted);
		if (ending)
			ClearActiveRunProfileKey();
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
	/// </summary>
	public void Restore(string context)
	{
		try
		{
			var active = MiniTracker.Instance.CustomGrSaveData;
			if (active == null)
			{
				ReconcileSaves(Saves);
				// No active instance; set this deserialized instance as active
				MiniTracker.Instance.SetActive(this);
				MigrateAllLegacyKeys();
				MarkFileLoaded();
				return;
			}

			// Merge saved shops into active instance
			foreach (var kv in Saves)
				active.Saves[kv.Key] = LiteShop.ReconcileWithDefaults(kv.Value);
			active.MigrateAllLegacyKeys();
			active.MarkFileLoaded();
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogError($"[ShopSave] Restore failed ({context}); shops read from disk may not have been applied: {e}");
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
