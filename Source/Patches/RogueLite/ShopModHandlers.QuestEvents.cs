using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.GapOptions;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Cirno;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.EnemyUnits.Normal;
using LBoL.EntityLib.EnemyUnits.Normal.Bats;
using LBoL.EntityLib.EnemyUnits.Normal.Drones;
using LBoL.EntityLib.EnemyUnits.Normal.Guihuos;
using LBoL.EntityLib.EnemyUnits.Normal.Maoyus;
using LBoL.EntityLib.EnemyUnits.Normal.Ravens;
using LBoL.EntityLib.EnemyUnits.Normal.Shenlings;
using LBoL.EntityLib.EnemyUnits.Normal.Yinyangyus;
using LBoL.EntityLib.EnemyUnits.Opponent;
using LBoL.EntityLib.Exhibits.Common;
using LBoL.EntityLib.StatusEffects.Enemy.Seija;
using LBoL.EntityLib.StatusEffects.Marisa;
using LBoL.Presentation;
using LBoL.Presentation.UI.Panels;
using LBoLEntitySideloader.CustomHandlers;
using lvalonmima.Cards;
using lvalonmima.Exhibits;
using lvalonmima.StatusEffects;

namespace lvalonmima.Source.Patches;

public class RogueliteCrosser : IMapModeOverrider
{
	public GameRunMapMode? MapMode => GameRunMapMode.Crossing;
	private static readonly RogueliteCrosser _instance = new();
	static RogueliteCrosser() { }
	private RogueliteCrosser() { }
	public static RogueliteCrosser Instance => _instance;
	public void OnEnteredWithMode()
	{
	}
}

public partial class ShopModHandlers
{
	private static GameRunController CachedGameRun;

	private static float LastAppliedShopDiscountFactor = 1f;

	private static int LastAppliedSeeOrder = 0;

	private static int LastAppliedBlankCard = 0;

	public static void DeckCardsAdded(CardsEventArgs args)
	{
		GameRunController gameRun = GameMaster.Instance.CurrentGameRun;
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop == null || !gameRun.Player.HasExhibit<exquesting>())
			return;
		var exhibit = gameRun.Player.GetExhibit<exquesting>();
		bool questProgressChanged = false;

		List<Card> reqQuests =
		[
			Library.CreateCard<cardquest2>(),
			Library.CreateCard<cardquest13>(),
			Library.CreateCard<cardquest20>(),
		];
		foreach (Card card in reqQuests)
		{
			string questCardId = card?.Id;
			if (string.IsNullOrEmpty(questCardId) || exhibit.IsQuestCardSoldOut(questCardId) || exhibit.IsQuestCardCompleted(questCardId))
				continue;

			if (exhibit.PendingQuestProgress.TryGetValue(questCardId, out int progress) && progress < card.Config.Value1)
			{
				switch (questCardId)
				{
					case nameof(cardquest2):
						if (!gameRun.BaseDeck.Any(c => c.IsBasic))
							continue;
						break;
					case nameof(cardquest13):
						if (!gameRun.BaseDeck.Any(c => c.CardType == CardType.Misfortune && !c.Unremovable))
							continue;
						break;
					default:
						break;
				}

				int adding = 0;

				switch (questCardId)
				{
					case nameof(cardquest2):
						string requiredType = exhibit.TryGetQuestRequirement(questCardId, out string encodedRequirement) ? encodedRequirement : null;
						if (string.IsNullOrEmpty(requiredType))
							continue;
						requiredType = requiredType.Split('!').LastOrDefault(); // { "TypeAttack", "TypeDefense", "TypeSkill", "TypeAbility" };
						switch (requiredType)
						{
							case "TypeAttack":
								if (args.Cards.Any(c => c.Config.Type == CardType.Attack && c.Config.Rarity == Rarity.Common))
									adding++;
								break;
							case "TypeDefense":
								if (args.Cards.Any(c => c.Config.Type == CardType.Defense && c.Config.Rarity == Rarity.Common))
									adding++;
								break;
							case "TypeSkill":
								if (args.Cards.Any(c => c.Config.Type == CardType.Skill && c.Config.Rarity == Rarity.Common))
									adding++;
								break;
							case "TypeAbility":
								if (args.Cards.Any(c => c.Config.Type == CardType.Ability && c.Config.Rarity == Rarity.Uncommon))
									adding++;
								break;
							default:
								break;
						}
						break;
					case nameof(cardquest13):
						adding += args.Cards.Count(c => c.CardType == CardType.Misfortune);
						break;
					case nameof(cardquest20) when exhibit.TryGetQuestRequirement(questCardId, out string req):
						if (args.Cards.Any(c => c.Id == req))
							adding++;
						break;
					default:
						break;
				}
				if (adding == 0)
					continue;
				exhibit.PendingQuestProgress[questCardId] = progress + adding;
				questProgressChanged = true;
				if (exhibit.PendingQuestProgress[questCardId] >= card.Config.Value1)
				{
					switch (questCardId)
					{
						case nameof(cardquest2):
							gameRun.RemoveDeckCard(gameRun.BaseDeck.Where(c => c.IsBasic).Sample(gameRun.CardRng));
							break;
						case nameof(cardquest13):
							gameRun.RemoveDeckCards(gameRun.BaseDeck.Where(c => c.CardType == CardType.Misfortune && !c.Unremovable));
							if (!gameRun.Player.HasExhibit<ChuRenou>())
								GameMaster.DebugGainExhibit(Library.CreateExhibit<ChuRenou>());
							break;
						case nameof(cardquest20):
							List<Card> stones =
							[
								Library.CreateCard<cardstone1>(),
								// Library.CreateCard<cardstone2>(),
								Library.CreateCard<cardstone4>(),
							];
							if (gameRun.BaseDeck.Any(c => (c.Config.RelativeEffects.Contains(nameof(Graze)) && !c.IsUpgraded) || (c.Config.UpgradedRelativeEffects.Contains(nameof(Graze)) && c.IsUpgraded)))
								stones.Add(Library.CreateCard<cardstone2>());
							if (gameRun.Puzzles.HasFlag(PuzzleFlag.NightMana))
								stones.Add(Library.CreateCard<cardstone3>());
							gameRun.AddDeckCards(stones.SampleManyOrAll(card.Config.Value2 ?? 2, gameRun.CardRng), true);
							break;
						default:
							break;
					}
					exhibit.FinalizeQuestByCardId(questCardId);
					exhibit.MarkQuestCompleted(questCardId);
					RecordRewardedQuestCompletion(questCardId);
					questProgressChanged = true;
				}
			}
		}

		exhibit.CleanupStaleQuestRequirements();

		if (questProgressChanged)
			PersistQuestProgress(gameRun, exhibit.PendingQuestProgress, syncToLiteShop: false, saveToDisk: false, questRequirements: exhibit.QuestRequirements, completedQuestCards: exhibit.CompletedQuestCards, writeToRunFlags: false, questModifiers: exhibit.PendingQuestModifiers);
	}

	public static void StationEntering(StationEventArgs args)
	{
		// this shit's restore support is moved to game restore prefix

		GameRunController gameRun = Singleton<GameMaster>.Instance.CurrentGameRun;
		// 1-0
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop == null || !shop.ChallengerModeEnabled)
			return;
		// every stage
		foreach (string itemId in shop.AllItems)
		{
			ShopItem item = shop.GetItem(itemId);
			if (item == null || item.CurrentTier <= 0)
				continue;

			switch (itemId)
			{
				case "alter.wings":
					GameMaster.Instance?.StartCoroutine(CoAddMapModeOverrider(gameRun, RogueliteCrosser.Instance));
					break;
			}
		}
	}

	// Replaces the identical quest19-advancing body shared by
	// ShopPanel_BuyCard_Quest_Patch and ShopPanel_BuyExhibit_Quest_Patch below
	// (they differ only in their guard condition, not in what happens once it's
	// satisfied).
	internal static void AdvanceQuest19(GameRunController gameRun)
	{
		exquesting exhibit = gameRun.Player.GetExhibit<exquesting>();
		var quest19 = Library.CreateCard<cardquest19>();
		if (exhibit.PendingQuestProgress.TryGetValue(quest19.Id, out int progress) && progress < quest19.Config.Value1)
		{
			exhibit.PendingQuestProgress[quest19.Id] = ++progress;
			if (exhibit.PendingQuestProgress[quest19.Id] >= quest19.Config.Value1)
			{
				if (!gameRun.Player.HasExhibit<Huiyuanka>())
					GameMaster.DebugGainExhibit(Library.CreateExhibit<Huiyuanka>());
				exhibit.FinalizeQuestByCardId(quest19.Id);
				exhibit.MarkQuestCompleted(quest19.Id);
				RecordRewardedQuestCompletion(quest19.Id);
			}
		}
	}

	// Replaces the 5 byte-identical cardquest26-30 case bodies in StationEnteredBlitz:
	// bump progress, and on reaching the goal, bump the modifier stack, finalize,
	// mark completed and record the reward.
	private static void AdvanceModifierQuest(exquesting exhibit, Card card, string questCardId, ref int progress, ref bool questProgressChanged)
	{
		exhibit.PendingQuestProgress[questCardId] = ++progress;
		questProgressChanged = true;
		if (progress >= card.Config.Value1)
		{
			exhibit.PendingQuestModifiers.TryGetValue(questCardId, out int stack);
			exhibit.PendingQuestModifiers[questCardId] = ++stack;
			exhibit.FinalizeQuestByCardId(questCardId);
			exhibit.MarkQuestCompleted(questCardId);
			RecordRewardedQuestCompletion(questCardId);
		}
	}

	public static void StationEnteredBlitz(StationEventArgs args)
	{
		// handle quest progress for station challenges that require entering stations
		GameRunController gameRun = GameMaster.Instance?.CurrentGameRun;
		if (gameRun == null)
			return;
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop == null || !gameRun.Player.HasExhibit<exquesting>())
			return;
		var exhibit = gameRun.Player.GetExhibit<exquesting>();

		// Prevent double-processing the same station (e.g. when restarting the stage)
		int stageIndex = gameRun.Stages?.IndexOf(gameRun.CurrentStage) ?? -1;
		int stationLevel = gameRun.CurrentStation?.Level ?? -1;
		if (shop.BPProgress != null
			&& shop.BPProgress.TryGetValue("stage", out int recordedStage)
			&& shop.BPProgress.TryGetValue("level", out int recordedLevel)
			&& recordedStage == stageIndex && recordedLevel == stationLevel)
		{
			return;
		}

		List<Card> stationQuests =
		[
			Library.CreateCard<cardquest4>(),
			Library.CreateCard<cardquest10>(),
			Library.CreateCard<cardquest26>(),
			Library.CreateCard<cardquest27>(),
			Library.CreateCard<cardquest28>(),
			Library.CreateCard<cardquest29>(),
			Library.CreateCard<cardquest30>(),
		];
		bool questProgressChanged = false;
		foreach (var card in stationQuests)
		{
			string questCardId = card?.Id;
			if (string.IsNullOrEmpty(questCardId))
				continue;

			if (exhibit.IsQuestCardSoldOut(questCardId) ||
				exhibit.IsQuestCardCompleted(questCardId))
				continue;

			if (!exhibit.PendingQuestProgress.TryGetValue(questCardId, out var progress))
				continue;

			if (progress >= card.Config.Value1)
				continue;

			switch (questCardId)
			{
				case nameof(cardquest4):
					if (gameRun.BaseDeck.Any(c => c.Id == nameof(cardgenji)))
					{
						exhibit.PendingQuestProgress[questCardId] = ++progress;
						questProgressChanged = true;

						if (progress >= card.Config.Value1)
						{
							gameRun.RemoveDeckCard(gameRun.BaseDeck.FirstOrDefault(c => c.Id == nameof(cardgenji)));
							gameRun.GainPower((int)card.Config.Value2);
							gameRun.Heal((int)card.Config.Value2);
							exhibit.FinalizeQuestByCardId(questCardId);
							exhibit.MarkQuestCompleted(questCardId);
							RecordRewardedQuestCompletion(questCardId);
						}
					}
					else
					{
						exhibit.FinalizeQuestByCardId(questCardId);
						exhibit.MarkQuestCompleted(questCardId);
					}
					break;
				case nameof(cardquest10):
					if (gameRun.BaseDeck.Count(c => c.Id == nameof(LBoL.EntityLib.Cards.Neutral.Black.Shadow)) >= ((cardquest10)card).Value20)
					{
						exhibit.PendingQuestProgress[questCardId] = ++progress;
						questProgressChanged = true;

						if (progress >= card.Config.Value1)
						{
							gameRun.RemoveDeckCards([.. gameRun.BaseDeck.Where(c => c.Id == nameof(LBoL.EntityLib.Cards.Neutral.Black.Shadow)).Take(((cardquest10)card).Value20)]);
							gameRun.Heal((int)card.Config.Value2);
							gameRun.GainMoney((int)card.Config.Value2 * 10, true, new VisualSourceData
							{
								SourceType = VisualSourceType.Entity,
								Source = exhibit,
							});
							exhibit.FinalizeQuestByCardId(questCardId);
							exhibit.MarkQuestCompleted(questCardId);
							RecordRewardedQuestCompletion(questCardId);
						}
					}
					else
					{
						exhibit.FinalizeQuestByCardId(questCardId);
						exhibit.MarkQuestCompleted(questCardId);
					}
					break;
				case nameof(cardquest26) when gameRun.CurrentStation.Type == StationType.Boss:
				case nameof(cardquest27) when gameRun.CurrentStation.Type == StationType.Boss:
				case nameof(cardquest28) when gameRun.CurrentStation.Type == StationType.Boss:
				case nameof(cardquest29) when gameRun.CurrentStation.Type == StationType.Boss:
				case nameof(cardquest30) when gameRun.CurrentStation.Type == StationType.Boss:
					AdvanceModifierQuest(exhibit, card, questCardId, ref progress, ref questProgressChanged);
					break;
				default:
					break;
			}
		}

		if (questProgressChanged)
			PersistQuestProgress(gameRun, exhibit.PendingQuestProgress, syncToLiteShop: true, saveToDisk: true, questRequirements: exhibit.QuestRequirements, completedQuestCards: exhibit.CompletedQuestCards, questModifiers: exhibit.PendingQuestModifiers);
	}

	public static void StationEntered(StationEventArgs args)
	{
		GameRunController gameRun = Singleton<GameMaster>.Instance.CurrentGameRun;
		if (!ReferenceEquals(CachedGameRun, gameRun))
		{
			CachedGameRun = gameRun;
			LastAppliedShopDiscountFactor = 1f;
			LastAppliedSeeOrder = 0;
			LastAppliedBlankCard = 0;
		}
		EnsureFreeChoiceFlag(gameRun);
		// reset stuff
		if (LastAppliedShopDiscountFactor > 0f)
			gameRun.ShopPriceMultiplier /= LastAppliedShopDiscountFactor;
		gameRun.CanViewDrawZoneActualOrder -= LastAppliedSeeOrder;
		if (LastAppliedBlankCard > 0)
			gameRun.RewardAndShopCardColorLimitFlag -= LastAppliedBlankCard;
		// 1-0
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop == null)
			return;

		if (!shop.ChallengerModeEnabled)
			return;

		// blue point progress save — create or overwrite keys
		int stageIndex = gameRun.Stages?.IndexOf(gameRun.CurrentStage) ?? -1;
		int stationLevel = gameRun.CurrentStation?.Level ?? -1;

		bool isFirstRunEntryStation = args.Station is EntryStation entryStation
			&& gameRun.Stages.IndexOf(entryStation.Stage) == 0
			&& stationLevel <= 0;
		if (isFirstRunEntryStation)
			ResetQuestStateForNewRun(gameRun, shop);

		// If LiteShop already recorded a different station than the one we're entering,
		// persist any runtime exquesting pending progress from the previous station so it
		// isn't lost when exquesting.SyncPendingQuestProgressFromPersistence runs for the new station.
		if (shop.BPProgress != null && gameRun?.Player?.HasExhibit<exquesting>() == true)
		{
			int recordedStage = int.MinValue, recordedLevel = int.MinValue;
			shop.BPProgress.TryGetValue("stage", out recordedStage);
			shop.BPProgress.TryGetValue("level", out recordedLevel);

			if (recordedStage != stageIndex || recordedLevel != stationLevel)
			{
				exquesting prevExhibit = gameRun.Player.GetExhibit<exquesting>();
				if (prevExhibit is { PendingQuestProgress.Count: > 0 })
				{
					// Persist runtime pending progress from the station we're leaving so it won't be
					// overwritten by the persistence sync on the station we're entering.
					PersistQuestProgress(gameRun, prevExhibit.PendingQuestProgress, syncToLiteShop: true, saveToDisk: false, questRequirements: prevExhibit.QuestRequirements, completedQuestCards: prevExhibit.CompletedQuestCards, writeToRunFlags: true, questModifiers: prevExhibit.PendingQuestModifiers);
				}
			}
		}

		shop.BPProgress ??= [];
		shop.BPProgress["stage"] = stageIndex;
		shop.BPProgress["level"] = stationLevel;

		MiniTracker.Instance.CustomGrSaveData.Save(0, false);
		ShopSaveLoader.Save();  //save progress on the spot

		if (isFirstRunEntryStation)
		{
			foreach (string itemId in shop.AllItems)
			{
				ShopItem item = shop.GetItem(itemId);
				if (item == null || item.CurrentTier <= 0)
					continue;

				switch (itemId)
				{
					case "init.hp":
						int newHp = toolbox.Round(((0.1 * item.CurrentTier) + 1) * gameRun.Player.MaxHp);
						gameRun.SetHpAndMaxHp(newHp, newHp);
						break;
					case "init.gold":
						gameRun.GainMoney(item.CurrentTier * item.Delta, true);
						break;
					case "init.card":
						GameMaster.Instance.StartCoroutine(DraftCardFromPrev(item.CurrentTier, gameRun));
						break;
					case "init.exhibit":
						GameMaster.Instance.StartCoroutine(DraftExhibitFromPrev(item.CurrentTier, gameRun));
						break;
					case "init.solo":
						GameMaster.Instance.StartCoroutine(GainQuestExhibit());
						break;
					case "discount.remove":
						gameRun.ShopRemoveCardCounter -= 1;
						break;
				}
			}
		}

		// every stage
		bool appliedShopDiscount = false;
		bool appliedSeeOrder = false;
		bool appliedBlankCard = false;
		foreach (string itemId in shop.AllItems)
		{
			ShopItem item = shop.GetItem(itemId);
			if (item == null || item.CurrentTier <= 0)
				continue;

			switch (itemId)
			{
				// discount.sc is no longer applied here -- see
				// UltimateSkill_PowerCost_Discount_Patch. Mutating
				// gameRun.Player.Us.Config.PowerCost directly was wrong: Config
				// comes from UltimateSkillConfig.FromId(id), a shared object cached
				// in a private static Dictionary populated once at game startup
				// (confirmed via decompile), not a fresh per-game-run copy. Every
				// UltimateSkill instance of the same skill, across every game run in
				// the same session, shared that one mutation target.
				case "discount.shop":
					if (!appliedShopDiscount)
					{
						float discountFactor = (float)Math.Max(0.0, 1.0 - (0.05 * item.CurrentTier));
						gameRun.ShopPriceMultiplier *= discountFactor;
						LastAppliedShopDiscountFactor = discountFactor;
						appliedShopDiscount = true;
					}
					break;
				case "battle.seedraw":
					if (!appliedSeeOrder)
					{
						gameRun.CanViewDrawZoneActualOrder += item.CurrentTier;
						LastAppliedSeeOrder = item.CurrentTier;
						appliedSeeOrder = true;
					}
					break;
				// case "alter.wings":
				// 	if (!gameRun._mapModeOverriders.Contains(RogueliteCrosser.Instance))
				// 	{
				// 		GameMaster.Instance?.StartCoroutine(CoAddMapModeOverrider(gameRun, RogueliteCrosser.Instance));
				// 	}
				// 	break;
				case "alter.blankcard":
					if (!appliedBlankCard)
					{
						gameRun.RewardAndShopCardColorLimitFlag += 1;
						LastAppliedBlankCard = 1;
						appliedBlankCard = true;
					}
					break;
			}
		}
		if (!appliedShopDiscount)
			LastAppliedShopDiscountFactor = 1f;
	}

	public static IEnumerator CoAddMapModeOverrider(GameRunController gameRun, IMapModeOverrider overrider)
	{
		// defer addition to avoid modifying enumerated collections during event dispatch
		yield return null;
		if (gameRun != null && !gameRun._mapModeOverriders.Contains(overrider))
			gameRun._mapModeOverriders.Add(overrider);
	}

}
[HarmonyPatch(typeof(GapStation), nameof(GapStation.DrinkTea))]
class GapStation_Gapple_Patch
{
	static void Postfix(GapStation __instance, DrinkTea drinkTea)
	{
		GameRunController gameRun = GameMaster.Instance.CurrentGameRun;
		if (ShopModHandlers.GetGapplePerHeal() > 0)
		{
			int toHeal = (drinkTea.Value + drinkTea.AdditionalHeal) / ShopModHandlers.GetGapplePerHeal(); // floor
			gameRun.SetHpAndMaxHp(gameRun.Player.Hp + toHeal, gameRun.Player.MaxHp + toHeal, true);
		}

		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop != null && gameRun.Player.HasExhibit<exquesting>())
		{
			exquesting exhibit = gameRun.Player.GetExhibit<exquesting>();
			var quest7 = Library.CreateCard<cardquest7>();
			if (exhibit.PendingQuestProgress.TryGetValue(quest7.Id, out int progress) && progress < quest7.Config.Value1)
			{
				exhibit.PendingQuestProgress[quest7.Id] = ++progress;
				if (exhibit.PendingQuestProgress[quest7.Id] >= quest7.Config.Value1)
				{
					gameRun.GainMoney((int)quest7.Config.Value2, true, new VisualSourceData
					{
						SourceType = VisualSourceType.Entity,
						Source = exhibit,
					});
					exhibit.FinalizeQuestByCardId(quest7.Id);
					exhibit.MarkQuestCompleted(quest7.Id);
					ShopModHandlers.RecordRewardedQuestCompletion(quest7.Id);
				}
			}
		}
	}

}
// Checklist G9: confirmed intentional, not a bug. Vanilla Card.Upgrade() throws
// when called on a card that's already upgraded (or otherwise not
// CanUpgradeAndPositive). cardquest18's completion (Card_Upgrade_Patch's
// Postfix right below) batch-upgrades a sampled set of BaseDeck cards via
// UpgradeDeckCards, which can call Upgrade() on a card mid-batch that was
// already upgraded earlier in that same batch (or by something else
// upgrading cards concurrently) -- this global prefix makes that a no-op
// instead of a throw. It's global (applies to every Upgrade() call, not
// just this quest's batch), which is broader than strictly necessary, but
// narrowing it to just the batch-upgrade call site was judged not worth
// the risk without live-game verification.
[HarmonyPatch(typeof(Card), nameof(Card.Upgrade))]
class Card_Upgrade_Patch_Shinmy
{
	static bool Prefix(Card __instance)
	{
		return __instance.CanUpgradeAndPositive;
	}

}

[HarmonyPatch(typeof(Card), nameof(Card.Upgrade))]
class Card_Upgrade_Patch
{
	static void Postfix(Card __instance)
	{
		GameRunController gameRun = GameMaster.Instance?.CurrentGameRun;
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (gameRun != null && shop != null && gameRun.Player.HasExhibit<exquesting>() && gameRun.BaseDeck.Contains(__instance))
		{
			exquesting exhibit = gameRun.Player.GetExhibit<exquesting>();
			var quest18 = Library.CreateCard<cardquest18>();
			if (exhibit.PendingQuestProgress.TryGetValue(quest18.Id, out int progress) && progress < quest18.Config.Value1)
			{
				exhibit.PendingQuestProgress[quest18.Id] = ++progress;
				if (exhibit.PendingQuestProgress[quest18.Id] >= quest18.Config.Value1)
				{
					List<Card> toUpgrade = [.. gameRun.BaseDeck.Where(c => c.CanUpgradeAndPositive).SampleManyOrAll(quest18.Config.Value2 ?? 3, gameRun.CardRng)];
					if (toUpgrade.Count > 0)
						gameRun.UpgradeDeckCards(toUpgrade, true);
					exhibit.FinalizeQuestByCardId(quest18.Id);
					exhibit.MarkQuestCompleted(quest18.Id);
					ShopModHandlers.RecordRewardedQuestCompletion(quest18.Id);
				}
			}
		}
	}

}

[HarmonyPatch(typeof(ShopPanel), nameof(ShopPanel.BuyCard))]
class ShopPanel_BuyCard_Quest_Patch
{
	static void Postfix(ShopPanel __instance, int index)
	{
		Card card = __instance.ShopStation?.ShopCards[index]?.Content;
		GameRunController gameRun = GameMaster.Instance?.CurrentGameRun;
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (card != null && gameRun != null && shop != null && !card.Config.RelativeEffects.Any(e => e == nameof(sequest)) && gameRun.Player.HasExhibit<exquesting>())
			ShopModHandlers.AdvanceQuest19(gameRun);
	}

}
[HarmonyPatch(typeof(ShopPanel), nameof(ShopPanel.BuyExhibit))]
class ShopPanel_BuyExhibit_Quest_Patch
{
	static void Postfix()
	{
		GameRunController gameRun = GameMaster.Instance?.CurrentGameRun;
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (gameRun != null && shop != null && gameRun.Player.HasExhibit<exquesting>())
			ShopModHandlers.AdvanceQuest19(gameRun);
	}
}
