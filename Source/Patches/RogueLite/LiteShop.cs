using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace lvalonmima.Source.Patches;

public class ShopItem
{
	// Serializable-friendly for YamlDotNet
	public string Id { get; set; }
	public int MaxTier { get; set; }
	public int CurrentTier { get; set; }
	public List<int> TierCosts { get; set; }
	public int Initial { get; set; }
	public int Delta { get; set; }
	public bool IsAlpha { get; set; }
	public bool IsMaxTier => CurrentTier >= MaxTier;
	public string Tier => CurrentTier.ToString() + " / " + MaxTier;
	public ShopItem()
	{
		TierCosts = [];
		Initial = 0;
		Delta = 0;
		IsAlpha = false;
	}

	public ShopItem(string name, List<int> tierCosts)
		: this(name, tierCosts, 0)
	{
	}

	public ShopItem(string name, List<int> tierCosts, int initial, int delta = 1, bool isAlpha = false)
	{
		if (tierCosts == null || tierCosts.Count == 0)
			throw new ArgumentException("Item must have at least one tier.");

		Id = name;
		TierCosts = [.. tierCosts];
		MaxTier = tierCosts.Count;
		CurrentTier = 0;
		Initial = initial;
		Delta = delta;
		IsAlpha = isAlpha;
	}


	public int GetNextTierCost()
	{
		if (IsMaxTier)
			throw new InvalidOperationException("Item already at max tier.");

		return TierCosts[CurrentTier];
	}

	public bool PurchaseNextTier(LiteShop shop)
	{
		if (IsMaxTier)
			return false;
		if (Id == "refund")
		{
			foreach (var item in shop.Items.Values)
			{
				while (item.Id != "refund" && item.CurrentTier > 0)
				{
					item.Refund(shop);
				}
			}
			return true;
		}
		shop.AddMoney(-GetNextTierCost());
		CurrentTier++;
		return true;
	}

	public void Refund(LiteShop shop)
	{
		if (CurrentTier > 0)
		{
			CurrentTier--;
			shop.AddMoney(GetNextTierCost());
		}
	}
}

public class LiteShop //per-profile
{
	public Dictionary<int, string> QuestRolledSlots { get; set; }
	public HashSet<int> QuestSoldSlots { get; set; }

	public Dictionary<string, int> QuestProgress { get; set; }
	public Dictionary<string, string> QuestRequirements { get; set; }
	public HashSet<string> QuestCompletedCards { get; set; }
	public int MoneyOwned { get; private set; }

	public bool ChallengerModeEnabled { get; set; }
	public Dictionary<string, int> BPProgress { get; set; }
	public Dictionary<string, List<(string, int)>> RunModifiersByTimestamp { get; set; }
	public List<string> CurrentRunCompletedQuests { get; set; }
	public Dictionary<string, List<string>> RunCompletedQuestsByTimestamp { get; set; }
	public Dictionary<string, int> QuestModifiers { get; set; }

	public Dictionary<string, ShopItem> Items { get; private set; }
	[YamlIgnore]
	public string init = "init.";
	[YamlIgnore]
	public string discount = "discount.";
	[YamlIgnore]
	public string feature = "feature.";
	[YamlIgnore]
	public string battle = "battle.";
	[YamlIgnore]
	public string alter = "alter.";
	[YamlIgnore]
	public string difficulty = "difficulty.";
	[YamlIgnore]
	public List<string> InitItems => [.. Items.Where(kvp => kvp.Key.StartsWith(init)).Select(kvp => kvp.Value.Id)];
	[YamlIgnore]
	public List<string> DiscountItems => [.. Items.Where(kvp => kvp.Key.StartsWith(discount)).Select(kvp => kvp.Value.Id)];
	[YamlIgnore]
	public List<string> FeatureItems => [.. Items.Where(kvp => kvp.Key.StartsWith(feature)).Select(kvp => kvp.Value.Id)];
	[YamlIgnore]
	public List<string> BattleItems => [.. Items.Where(kvp => kvp.Key.StartsWith(battle)).Select(kvp => kvp.Value.Id)];
	[YamlIgnore]
	public List<string> AlterItems => [.. Items.Where(kvp => kvp.Key.StartsWith(alter)).Select(kvp => kvp.Value.Id)];
	[YamlIgnore]
	public List<string> DifficultyItems => [.. Items.Where(kvp => kvp.Key.StartsWith(difficulty)).Select(kvp => kvp.Value.Id)];
	[YamlIgnore]
	public List<string> AllItems => [.. Items.Select(kvp => kvp.Value.Id)];

	public LiteShop()
	{
		QuestRolledSlots = [];
		QuestSoldSlots = [];
		QuestProgress = [];
		QuestRequirements = [];
		QuestCompletedCards = new HashSet<string>(StringComparer.Ordinal);
		MoneyOwned = 0;
		ChallengerModeEnabled = false;
		BPProgress = [];
		RunModifiersByTimestamp = [];
		CurrentRunCompletedQuests = [];
		RunCompletedQuestsByTimestamp = [];
		QuestModifiers = [];
		Items = [];
		AddItem(new ShopItem(init + "fp", [240, 1200, 3600, 14400, 28800], 0));
		AddItem(new ShopItem(init + "sp", [160, 800, 2400, 9600, 19200], 0));
		AddItem(new ShopItem(init + "hp", [200, 480, 1100, 2400, 4800, 9600, 14400, 19200, 24000, 28800], 0, 10));
		AddItem(new ShopItem(init + "gold", [100, 200, 400, 800, 1600, 2400, 4800, 7200, 9600, 12000], 0, 10));
		AddItem(new ShopItem(init + "card", [36000], 0));
		AddItem(new ShopItem(init + "exhibit", [6000], 0));
		AddItem(new ShopItem(init + "solo", [4500], 0, 1));

		AddItem(new ShopItem(discount + "sc", [500, 5000, 10000, 25000, 50000], 0, 10));
		AddItem(new ShopItem(discount + "shop", [1200, 5000, 7500, 15000, 30000], 0, 5));
		AddItem(new ShopItem(discount + "upgrade", [2500, 10000], 0, 25));
		AddItem(new ShopItem(discount + "remove", [10000], 0, 25));

		AddItem(new ShopItem(feature + "teasync", [17000], 0));
		AddItem(new ShopItem(feature + "gapple", [1700, 6800, 17000], 5, -1));
		AddItem(new ShopItem(feature + "sponsor", [170, 680, 1700, 5100, 10200], 0, 5));

		AddItem(new ShopItem(battle + "block", [200, 500, 1200, 2750, 6000], 0, 2));
		AddItem(new ShopItem(battle + "graze", [10000], 0));
		AddItem(new ShopItem(battle + "heal", [100, 250, 800, 2400, 5700, 10000, 15000, 20000, 25000, 30000], 0));
		AddItem(new ShopItem(battle + "seedraw", [1000], 0));
		AddItem(new ShopItem(battle + "rolldiscard", [5000], 0, 1));
		AddItem(new ShopItem(battle + "hacks", [1300], 0, 13));

		AddItem(new ShopItem(alter + "freechoice", [12000], 0));
		AddItem(new ShopItem(alter + "wings", [12000], 0));
		AddItem(new ShopItem(alter + "blankcard", [24000], 0));

		AddItem(new ShopItem(difficulty + "reverse", [0], 0));
		AddItem(new ShopItem(difficulty + "ascension", [0], 0));

		AddItem(new ShopItem("refund", [0], 0));
	}

	public LiteShop(int startingMoney) : this()
	{
		MoneyOwned = startingMoney;
	}

	public static LiteShop ReconcileWithDefaults(LiteShop saved)
	{
		var latest = new LiteShop
		{
			MoneyOwned = saved?.MoneyOwned ?? 0,
			ChallengerModeEnabled = saved?.ChallengerModeEnabled ?? false,
			QuestProgress = saved?.QuestProgress != null
				? new Dictionary<string, int>(saved.QuestProgress)
				: [],
			QuestRequirements = saved?.QuestRequirements != null
				? new Dictionary<string, string>(saved.QuestRequirements)
				: [],
			QuestCompletedCards = saved?.QuestCompletedCards != null
				? new HashSet<string>(saved.QuestCompletedCards.Where(id => !string.IsNullOrEmpty(id)), StringComparer.Ordinal)
				: new HashSet<string>(StringComparer.Ordinal),
			BPProgress = saved?.BPProgress != null
				? new Dictionary<string, int>(saved.BPProgress)
				: [],
			RunModifiersByTimestamp = saved?.RunModifiersByTimestamp != null
				? new Dictionary<string, List<(string, int)>>(saved.RunModifiersByTimestamp
					.Where(kvp => !string.IsNullOrEmpty(kvp.Key))
					.ToDictionary(
						kvp => kvp.Key,
						kvp => kvp.Value == null
							? []
							: kvp.Value.Where(entry => !string.IsNullOrEmpty(entry.Item1)).ToList(),
						StringComparer.Ordinal),
					StringComparer.Ordinal)
				: new Dictionary<string, List<(string, int)>>(StringComparer.Ordinal),
			CurrentRunCompletedQuests = saved?.CurrentRunCompletedQuests != null
				? [.. saved.CurrentRunCompletedQuests.Where(id => !string.IsNullOrEmpty(id))]
				: [],
			RunCompletedQuestsByTimestamp = saved?.RunCompletedQuestsByTimestamp != null
				? new Dictionary<string, List<string>>(saved.RunCompletedQuestsByTimestamp
					.Where(kvp => !string.IsNullOrEmpty(kvp.Key))
					.ToDictionary(
						kvp => kvp.Key,
						kvp => kvp.Value == null
							? []
							: kvp.Value.Where(id => !string.IsNullOrEmpty(id)).ToList(),
						StringComparer.Ordinal),
					StringComparer.Ordinal)
				: new Dictionary<string, List<string>>(StringComparer.Ordinal),
			QuestModifiers = saved?.QuestModifiers != null
				? new Dictionary<string, int>(saved.QuestModifiers)
				: []
		};

		if (saved?.QuestRolledSlots != null)
			latest.QuestRolledSlots = new Dictionary<int, string>(saved.QuestRolledSlots);
		if (saved?.QuestSoldSlots != null)
			latest.QuestSoldSlots = [.. saved.QuestSoldSlots];

		if (saved?.Items == null)
			return latest;

		int refundTotal = 0;

		// First, apply saved tiers to matching latest items and refund any tiers that no longer exist in the new shop
		foreach (var (key, value) in latest.Items)
		{
			if (!saved.Items.TryGetValue(key, out var savedItem))
				continue;

			int savedTier = Math.Max(0, savedItem.CurrentTier);
			int latestMax = value.MaxTier;

			int clampedTier = Math.Max(0, Math.Min(savedTier, latestMax));
			// If saved had more tiers than the new max, refund the over-tier costs using the saved item's cost list
			if (savedTier > clampedTier && savedItem?.TierCosts != null)
			{
				int start = Math.Min(clampedTier, savedItem.TierCosts.Count);
				int end = Math.Min(savedTier, savedItem.TierCosts.Count);
				for (int i = start; i < end; i++)
					refundTotal += savedItem.TierCosts[i];
			}

			value.CurrentTier = clampedTier;
		}

		// Next, any items present in the saved shop but removed in the new shop should be fully refunded
		foreach (var (key, value) in saved.Items)
		{
			if (latest.Items.ContainsKey(key))
				continue;

			var savedItem = value;
			if (savedItem == null || savedItem.TierCosts == null)
				continue;

			int tiersToRefund = Math.Min(savedItem.CurrentTier, savedItem.TierCosts.Count);
			for (int i = 0; i < tiersToRefund; i++)
				refundTotal += savedItem.TierCosts[i];
		}

		latest.MoneyOwned += refundTotal;
		return latest;
	}

	public void AddItem(ShopItem item)
	{
		if (Items.ContainsKey(item.Id))
			throw new InvalidOperationException("Item already exists in shop.");

		Items[item.Id] = item;
	}

	public bool CanPurchase(string itemName)
	{
		if (!Items.TryGetValue(itemName, out var item))
			return false;
		if (item.IsAlpha)
			return false;

		if (item.IsMaxTier)
			return false;

		return MoneyOwned >= item.GetNextTierCost();
	}

	public bool Purchase(string itemName)
	{
		if (!Items.TryGetValue(itemName, out var item))
			return false;
		if (item.IsAlpha)
			return false;

		if (item.IsMaxTier)
			return false;

		if (MoneyOwned < item.GetNextTierCost())
			return false;

		return item.PurchaseNextTier(this);
	}

	public ShopItem GetItem(string itemName)
	{
		Items.TryGetValue(itemName, out var item);
		return item;
	}

	public void AddMoney(int amount)
	{
		MoneyOwned += amount;
	}
	public void Refund(ShopItem item)
	{
		item.Refund(this);
	}
}
