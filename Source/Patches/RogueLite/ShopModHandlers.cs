using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
using LBoL.EntityLib.Exhibits.Common;
using LBoL.EntityLib.StatusEffects.Marisa;
using LBoL.Presentation;
using LBoL.Presentation.UI.Panels;
using LBoLEntitySideloader.CustomHandlers;
using lvalonmima.Cards;
using lvalonmima.Exhibits;
using lvalonmima.StatusEffects;

namespace lvalonmima.Source.Patches
{
	public class ShopModHandlers
	{
		private static readonly Dictionary<string, int> BaseScPowerCosts = new Dictionary<string, int>();
		private const string FreeChoiceBlockedFlag = "shop.freechoice.blocked";
		private const string RemoveDiscountAppliedPrefix = "shop.remove.applied:";
		private const string QuestProgressFlagPrefix = "exquesting.quest:";
		private const string QuestRequirementFlagPrefix = "exquesting.requirement:";
		private const string QuestCompletedFlagPrefix = "exquesting.completed:";
		private const string QuestModifierFlagPrefix = "exquesting.modifier:";
		private static GameRunController CachedGameRun;
		private static float LastAppliedShopDiscountFactor = 1f;
		private static int LastAppliedSeeOrder = 0;
		private static HashSet<string> battleChallenges = new HashSet<string>();
		private static List<Card> quest5ToRmv;
		private static int quest15played;
		private static bool isTurn1;
		private static bool isTurn1A;
		private static bool turn1Drawn;
		private static bool turn1DrawnA;
		private static bool quest16active;

		public static Dictionary<string, int> ReadQuestProgressFromRun(GameRunController gameRun)
		{
			var result = new Dictionary<string, int>(StringComparer.Ordinal);
			if (gameRun?.ExtraFlags == null)
			{
				BepinexPlugin.log.LogInfo("[EXQUESTING SAVE] ReadQuestProgressFromRun: no GameRun/ExtraFlags.");
				return result;
			}

			foreach (string flag in gameRun.ExtraFlags)
			{
				if (string.IsNullOrEmpty(flag) || !flag.StartsWith(QuestProgressFlagPrefix, StringComparison.Ordinal))
					continue;

				string payload = flag[QuestProgressFlagPrefix.Length..];
				int split = payload.LastIndexOf('=');
				if (split <= 0 || split >= payload.Length - 1)
					continue;

				string cardId = payload[..split];
				string progressText = payload[(split + 1)..];
				if (string.IsNullOrEmpty(cardId) || !int.TryParse(progressText, out int progress))
					continue;

				result[cardId] = progress;
			}

			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] ReadQuestProgressFromRun: entries={result.Count} [{FormatQuestProgress(result)}]");

			return result;
		}

		public static void PersistQuestProgress(GameRunController gameRun, IDictionary<string, int> pendingQuestProgress, bool syncToLiteShop, bool saveToDisk)
		{
			PersistQuestProgress(gameRun, pendingQuestProgress, syncToLiteShop, saveToDisk, null, null, writeToRunFlags: true, questModifiers: null);
		}

		public static Dictionary<string, string> ReadQuestRequirementsFromRun(GameRunController gameRun)
		{
			var result = new Dictionary<string, string>(StringComparer.Ordinal);
			if (gameRun?.ExtraFlags == null)
			{
				BepinexPlugin.log.LogInfo("[EXQUESTING SAVE] ReadQuestRequirementsFromRun: no GameRun/ExtraFlags.");
				return result;
			}

			foreach (string flag in gameRun.ExtraFlags)
			{
				if (string.IsNullOrEmpty(flag) || !flag.StartsWith(QuestRequirementFlagPrefix, StringComparison.Ordinal))
					continue;

				string payload = flag[QuestRequirementFlagPrefix.Length..];
				int split = payload.LastIndexOf('=');
				if (split <= 0 || split >= payload.Length - 1)
					continue;

				string cardId = payload[..split];
				string encodedRequirement = payload[(split + 1)..];
				if (string.IsNullOrEmpty(cardId) || string.IsNullOrEmpty(encodedRequirement))
					continue;

				result[cardId] = encodedRequirement;
			}

			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] ReadQuestRequirementsFromRun: entries={result.Count}");

			return result;
		}

		public static HashSet<string> ReadCompletedQuestCardsFromRun(GameRunController gameRun)
		{
			var result = new HashSet<string>(StringComparer.Ordinal);
			if (gameRun?.ExtraFlags == null)
			{
				BepinexPlugin.log.LogInfo("[EXQUESTING SAVE] ReadCompletedQuestCardsFromRun: no GameRun/ExtraFlags.");
				return result;
			}

			foreach (string flag in gameRun.ExtraFlags)
			{
				if (string.IsNullOrEmpty(flag) || !flag.StartsWith(QuestCompletedFlagPrefix, StringComparison.Ordinal))
					continue;

				string questCardId = flag[QuestCompletedFlagPrefix.Length..];
				if (string.IsNullOrEmpty(questCardId))
					continue;

				result.Add(questCardId);
			}

			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] ReadCompletedQuestCardsFromRun: entries={result.Count} [{string.Join(", ", result)}]");
			return result;
		}

		public static Dictionary<string, int> ReadQuestModifiersFromRun(GameRunController gameRun)
		{
			var result = new Dictionary<string, int>(StringComparer.Ordinal);
			if (gameRun?.ExtraFlags == null)
			{
				BepinexPlugin.log.LogInfo("[EXQUESTING SAVE] ReadQuestModifiersFromRun: no GameRun/ExtraFlags.");
				return result;
			}

			foreach (string flag in gameRun.ExtraFlags)
			{
				if (string.IsNullOrEmpty(flag) || !flag.StartsWith(QuestModifierFlagPrefix, StringComparison.Ordinal))
					continue;

				string payload = flag[QuestModifierFlagPrefix.Length..];
				int split = payload.LastIndexOf('=');
				if (split <= 0 || split >= payload.Length - 1)
					continue;

				string cardId = payload[..split];
				string stackText = payload[(split + 1)..];
				if (string.IsNullOrEmpty(cardId) || !int.TryParse(stackText, out int stack))
					continue;

				result[cardId] = stack;
			}

			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] ReadQuestModifiersFromRun: entries={result.Count} [{string.Join(", ", result.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}]");
			return result;
		}
		public static Dictionary<string, int> ReadQuestProgressFromLiteShop()
		{
			var result = new Dictionary<string, int>(StringComparer.Ordinal);
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop?.QuestProgress == null)
			{
				BepinexPlugin.log.LogInfo("[EXQUESTING SAVE] ReadQuestProgressFromLiteShop: no profile or quest progress.");
				return result;
			}

			foreach (var kvp in shop.QuestProgress)
			{
				if (string.IsNullOrEmpty(kvp.Key))
					continue;

				result[kvp.Key] = kvp.Value;
			}

			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] ReadQuestProgressFromLiteShop: entries={result.Count} [{FormatQuestProgress(result)}]");
			return result;
		}

		public static Dictionary<string, string> ReadQuestRequirementsFromLiteShop()
		{
			var result = new Dictionary<string, string>(StringComparer.Ordinal);
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop?.QuestRequirements == null)
			{
				BepinexPlugin.log.LogInfo("[EXQUESTING SAVE] ReadQuestRequirementsFromLiteShop: no profile or quest requirements.");
				return result;
			}

			foreach (var kvp in shop.QuestRequirements)
			{
				if (string.IsNullOrEmpty(kvp.Key) || string.IsNullOrEmpty(kvp.Value))
					continue;

				result[kvp.Key] = kvp.Value;
			}

			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] ReadQuestRequirementsFromLiteShop: entries={result.Count}");
			return result;
		}

		public static HashSet<string> ReadCompletedQuestCardsFromLiteShop()
		{
			var result = new HashSet<string>(StringComparer.Ordinal);
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop?.QuestCompletedCards == null)
			{
				BepinexPlugin.log.LogInfo("[EXQUESTING SAVE] ReadCompletedQuestCardsFromLiteShop: no profile or completed quests.");
				return result;
			}

			foreach (string questCardId in shop.QuestCompletedCards)
			{
				if (!string.IsNullOrEmpty(questCardId))
				{
					result.Add(questCardId);
				}
			}

			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] ReadCompletedQuestCardsFromLiteShop: entries={result.Count} [{string.Join(", ", result)}]");
			return result;
		}

		public static Dictionary<string, int> ReadQuestModifiersFromLiteShop()
		{
			var result = new Dictionary<string, int>(StringComparer.Ordinal);
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop?.QuestModifiers == null)
			{
				BepinexPlugin.log.LogInfo("[EXQUESTING SAVE] ReadQuestModifiersFromLiteShop: no profile or quest modifiers.");
				return result;
			}

			foreach (var kvp in shop.QuestModifiers)
			{
				if (string.IsNullOrEmpty(kvp.Key))
					continue;

				result[kvp.Key] = kvp.Value;
			}

			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] ReadQuestModifiersFromLiteShop: entries={result.Count} [{string.Join(", ", result.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}]");
			return result;
		}

		public static void PersistQuestProgress(GameRunController gameRun, IDictionary<string, int> pendingQuestProgress, bool syncToLiteShop, bool saveToDisk, IDictionary<string, string> questRequirements, ISet<string> completedQuestCards = null, bool writeToRunFlags = true, IDictionary<string, int> questModifiers = null)
		{
			HashSet<string> completedToPersist = completedQuestCards == null
				? new HashSet<string>(StringComparer.Ordinal)
				: new HashSet<string>(completedQuestCards.Where(id => !string.IsNullOrEmpty(id)), StringComparer.Ordinal);

			Dictionary<string, int> modifiersToPersist = null;
			if (questModifiers != null)
			{
				modifiersToPersist = new Dictionary<string, int>(StringComparer.Ordinal);
				foreach (var kvp in questModifiers)
				{
					if (string.IsNullOrEmpty(kvp.Key))
						continue;
					modifiersToPersist[kvp.Key] = kvp.Value;
				}
			}

			IEnumerable<string> completedForLog = completedQuestCards != null
				? (IEnumerable<string>)completedQuestCards
				: Array.Empty<string>();
			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] PersistQuestProgress: syncToLiteShop={syncToLiteShop}, saveToDisk={saveToDisk}, writeToRunFlags={writeToRunFlags}, entries={(pendingQuestProgress?.Count ?? 0)} [{FormatQuestProgress(pendingQuestProgress)}], reqEntries={(questRequirements?.Count ?? 0)}, completedEntries={(completedQuestCards?.Count ?? 0)} [{string.Join(", ", completedForLog)}], completedPersisted={completedToPersist.Count}, modifierEntries={(modifiersToPersist?.Count ?? -1)}");

			if (writeToRunFlags && gameRun?.ExtraFlags != null)
			{
				gameRun.ExtraFlags.RemoveWhere(flag =>
					!string.IsNullOrEmpty(flag) &&
					flag.StartsWith(QuestProgressFlagPrefix, StringComparison.Ordinal));

				gameRun.ExtraFlags.RemoveWhere(flag =>
					!string.IsNullOrEmpty(flag) &&
					flag.StartsWith(QuestRequirementFlagPrefix, StringComparison.Ordinal));

				gameRun.ExtraFlags.RemoveWhere(flag =>
					!string.IsNullOrEmpty(flag) &&
					flag.StartsWith(QuestCompletedFlagPrefix, StringComparison.Ordinal));

				// remove old modifier flags as well
				gameRun.ExtraFlags.RemoveWhere(flag =>
					!string.IsNullOrEmpty(flag) &&
					flag.StartsWith(QuestModifierFlagPrefix, StringComparison.Ordinal));

				if (pendingQuestProgress != null)
				{
					foreach (var kvp in pendingQuestProgress)
					{
						if (string.IsNullOrEmpty(kvp.Key))
							continue;
						gameRun.ExtraFlags.Add($"{QuestProgressFlagPrefix}{kvp.Key}={kvp.Value}");
					}
				}

				if (questRequirements != null)
				{
					foreach (var kvp in questRequirements)
					{
						if (string.IsNullOrEmpty(kvp.Key) || string.IsNullOrEmpty(kvp.Value))
							continue;
						gameRun.ExtraFlags.Add($"{QuestRequirementFlagPrefix}{kvp.Key}={kvp.Value}");
					}
				}

				if (completedToPersist.Count > 0)
				{
					foreach (string questCardId in completedToPersist)
					{
						gameRun.ExtraFlags.Add($"{QuestCompletedFlagPrefix}{questCardId}");
					}
				}

				// persist modifiers to run flags when requested
				Dictionary<string, int> modifiersToWrite = questModifiers != null
					? new Dictionary<string, int>(questModifiers, StringComparer.Ordinal)
					: gameRun?.Player?.GetExhibit<exquesting>()?.PendingQuestModifiers;

				if (modifiersToWrite != null)
				{
					foreach (var kvp in modifiersToWrite)
					{
						if (string.IsNullOrEmpty(kvp.Key))
							continue;
						gameRun.ExtraFlags.Add($"{QuestModifierFlagPrefix}{kvp.Key}={kvp.Value}");
					}
				}
			}

			if (syncToLiteShop)
			{
				var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
				if (shop != null)
				{
					shop.QuestProgress = pendingQuestProgress == null
						? new Dictionary<string, int>(StringComparer.Ordinal)
						: new Dictionary<string, int>(pendingQuestProgress, StringComparer.Ordinal);

					shop.QuestRequirements = questRequirements == null
						? new Dictionary<string, string>(StringComparer.Ordinal)
						: new Dictionary<string, string>(questRequirements, StringComparer.Ordinal);

					shop.QuestCompletedCards = new HashSet<string>(completedToPersist, StringComparer.Ordinal);

					if (modifiersToPersist != null)
					{
						shop.QuestModifiers = new Dictionary<string, int>(modifiersToPersist, StringComparer.Ordinal);
					}
					else
					{
						var mods = gameRun?.Player?.GetExhibit<exquesting>()?.PendingQuestModifiers;
						if (mods != null)
							shop.QuestModifiers = new Dictionary<string, int>(mods, StringComparer.Ordinal);
						else if (shop.QuestModifiers == null)
							shop.QuestModifiers = new Dictionary<string, int>(StringComparer.Ordinal);
					}
				}
			}

			if (saveToDisk)
			{
				MiniTracker.Instance?.CustomGrSaveData?.Save(0, false);
				ShopSaveLoader.Save();
			}
		}

		public static void QueueResolveCompletedQuestEffectsOnStationEnter(GameRunController gameRun, exquesting exhibit)
		{
			if (gameRun == null || exhibit == null || exhibit.CompletedQuestCards == null || exhibit.CompletedQuestCards.Count == 0)
			{
				BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] QueueResolveCompletedQuestEffectsOnStationEnter skipped gameRunNull={gameRun == null} exhibitNull={exhibit == null} completedCount={(exhibit?.CompletedQuestCards?.Count ?? 0)}");
				return;
			}

			HashSet<string> completedSnapshot = new HashSet<string>(exhibit.CompletedQuestCards, StringComparer.Ordinal);
			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] QueueResolveCompletedQuestEffectsOnStationEnter queued completed=[{string.Join(", ", completedSnapshot)}]");
			GameMaster.Instance?.StartCoroutine(CoResolveCompletedQuestEffectsOnStationEnter(gameRun, exhibit, completedSnapshot));
		}

		private static IEnumerator CoResolveCompletedQuestEffectsOnStationEnter(GameRunController gameRun, exquesting exhibit, HashSet<string> completedSnapshot)
		{
			if (gameRun == null || exhibit == null || completedSnapshot == null || completedSnapshot.Count == 0)
				yield break;

			// Run after station enter initialization so vanilla restore/state hydration does not overwrite effects.
			yield return null;
			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] CoResolveCompletedQuestEffectsOnStationEnter begin completed=[{string.Join(", ", completedSnapshot)}], baseDeckCount={(gameRun.BaseDeck?.Count ?? -1)}");
			HashSet<string> consumedCompleted = new HashSet<string>(StringComparer.Ordinal);

			foreach (string questCardId in completedSnapshot)
			{
				if (string.IsNullOrEmpty(questCardId))
					continue;

				if (exhibit.IsFreshlyCompletedQuestCard(questCardId))
				{
					BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] CoResolveCompletedQuestEffectsOnStationEnter skip fresh completion quest={questCardId}; preserving for restart replay.");
					exhibit.ClearFreshQuestCompletion(questCardId);
					continue;
				}

				switch (questCardId)
				{
					case nameof(cardquest4):
						Card card = Library.CreateCard<cardquest4>();
						Card genji = null;
						for (int attempt = 0; attempt < 60; attempt++)
						{
							genji = gameRun.BaseDeck?.FirstOrDefault(c => c.Id == nameof(cardgenji));
							if (genji != null)
							{
								if (attempt > 0)
								{
									BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] CoResolveCompletedQuestEffectsOnStationEnter quest4 found cardgenji after retries attempt={attempt}");
								}
								break;
							}

							if (attempt < 59)
							{
								yield return null;
							}
						}
						if (genji != null && card != null)
						{
							int hpBefore = gameRun.Player?.Hp ?? -1;
							int maxHpBefore = gameRun.Player?.MaxHp ?? -1;
							int powerBefore = gameRun.Player?.Power ?? -1;
							gameRun.RemoveDeckCard(genji);
							gameRun.GainPower((int)card.Config.Value2);
							gameRun.Heal((int)card.Config.Value2);
							BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] CoResolveCompletedQuestEffectsOnStationEnter quest4 applied value2={card.Config.Value2} hp={hpBefore}->{gameRun.Player?.Hp ?? -1} maxHp={maxHpBefore}->{gameRun.Player?.MaxHp ?? -1} power={powerBefore}->{gameRun.Player?.Power ?? -1}");
						}
						else
						{
							BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] CoResolveCompletedQuestEffectsOnStationEnter quest4 no-op genjiMissing={genji == null} cardNull={card == null}; treating as already resolved.");
						}
						consumedCompleted.Add(questCardId);
						break;
					case nameof(cardquest10):
						cardquest10 card2 = Library.CreateCard<cardquest10>();
						if (card2 != null)
						{
							int requiredShadows = card2.Value20;
							for (int attempt = 0; attempt < 60; attempt++)
							{
								int shadowCount = gameRun.BaseDeck?.Count(c => c.Id == nameof(LBoL.EntityLib.Cards.Neutral.Black.Shadow)) ?? 0;
								if (shadowCount >= requiredShadows)
								{
									if (attempt > 0)
									{
										BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] CoResolveCompletedQuestEffectsOnStationEnter quest10 found shadows after retries attempt={attempt} shadows={shadowCount}");
									}
									break;
								}

								if (attempt < 59)
								{
									yield return null;
								}
							}

							List<Card> shadows = gameRun.BaseDeck
								.Where(c => c.Id == nameof(LBoL.EntityLib.Cards.Neutral.Black.Shadow))
								.Take(card2.Value20)
								.ToList();
							if (shadows.Count >= card2.Value20)
							{
								gameRun.RemoveDeckCards(shadows);
								int hpBefore = gameRun.Player?.Hp ?? -1;
								int maxHpBefore = gameRun.Player?.MaxHp ?? -1;
								int moneyBefore = gameRun.Money;
								gameRun.Heal((int)card2.Config.Value2);
								gameRun.GainMoney((int)card2.Config.Value2 * 10, true, new VisualSourceData
								{
									SourceType = VisualSourceType.Entity,
									Source = exhibit,
								});
								BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] CoResolveCompletedQuestEffectsOnStationEnter quest10 applied shadowsRemoved={shadows.Count} value2={card2.Config.Value2} hp={hpBefore}->{gameRun.Player?.Hp ?? -1} maxHp={maxHpBefore}->{gameRun.Player?.MaxHp ?? -1} money={moneyBefore}->{gameRun.Money}");
							}
							else
							{
								BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] CoResolveCompletedQuestEffectsOnStationEnter quest10 no-op insufficient shadows after retries shadows={shadows.Count} required={card2.Value20}; treating as already resolved.");
							}
						}
						consumedCompleted.Add(questCardId);
						break;
				}
			}

			if (consumedCompleted.Count > 0)
			{
				foreach (string questCardId in consumedCompleted)
				{
					exhibit.ClearQuestCompleted(questCardId);
					foreach (var kvp in exhibit.RolledQuestCards)
					{
						Card slotCard = kvp.Value;
						if (slotCard != null && string.Equals(slotCard.Id, questCardId, StringComparison.Ordinal))
						{
							exhibit.SoldOutQuestSlots.Remove(kvp.Key);
						}
					}
				}

				BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] CoResolveCompletedQuestEffectsOnStationEnter consumed completed=[{string.Join(", ", consumedCompleted)}], remainingCompleted=[{string.Join(", ", exhibit.CompletedQuestCards)}], soldOutCount={exhibit.SoldOutQuestSlots.Count}");
			}

			// Persist post-resolution state to avoid regression on immediate reload.
			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] CoResolveCompletedQuestEffectsOnStationEnter persisting pendingEntries={(exhibit.PendingQuestProgress?.Count ?? 0)} completedEntries={(exhibit.CompletedQuestCards?.Count ?? 0)}");
			PersistQuestProgress(gameRun, exhibit.PendingQuestProgress, syncToLiteShop: true, saveToDisk: true, questRequirements: exhibit.QuestRequirements, completedQuestCards: exhibit.CompletedQuestCards, questModifiers: exhibit.PendingQuestModifiers);
		}

		private static string FormatQuestProgress(IEnumerable<KeyValuePair<string, int>> data)
		{
			if (data == null)
				return "";

			return string.Join(", ", data
				.Where(kvp => !string.IsNullOrEmpty(kvp.Key))
				.Select(kvp => $"{kvp.Key}={kvp.Value}"));
		}

		public static void DeckCardsAdded(CardsEventArgs args)
		{
			GameRunController gameRun = GameMaster.Instance.CurrentGameRun;
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop == null || !gameRun.Player.HasExhibit<exquesting>())
				return;
			var exhibit = gameRun.Player.GetExhibit<exquesting>();
			bool questProgressChanged = false;

			List<Card> reqQuests = new List<Card>()
			{
				Library.CreateCard<cardquest2>(),
				Library.CreateCard<cardquest13>(),
				Library.CreateCard<cardquest20>(),
			};
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
								List<Card> stones = new List<Card>()
								{
									Library.CreateCard<cardstone1>(),
									// Library.CreateCard<cardstone2>(),
									Library.CreateCard<cardstone4>(),
								};
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
						questProgressChanged = true;
					}
				}
			}

			exhibit.CleanupStaleQuestRequirements();

			if (questProgressChanged)
			{
				PersistQuestProgress(gameRun, exhibit.PendingQuestProgress, syncToLiteShop: false, saveToDisk: false, questRequirements: exhibit.QuestRequirements, completedQuestCards: exhibit.CompletedQuestCards, writeToRunFlags: false, questModifiers: exhibit.PendingQuestModifiers);
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
				BepinexPlugin.log.LogInfo($"[Lvalon's Roguelite Shop] StationEnteredBlitz: skipping already-processed station stage={stageIndex} level={stationLevel}");
				return;
			}

			List<Card> stationQuests = new List<Card>()
			{
				Library.CreateCard<cardquest4>(),
				Library.CreateCard<cardquest10>(),
			};
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
					case nameof(cardquest4) when gameRun.BaseDeck.Any(c => c.Id == nameof(cardgenji)):
						exhibit.PendingQuestProgress[questCardId] = ++progress;
						questProgressChanged = true;

						if (progress >= card.Config.Value1)
						{
							gameRun.RemoveDeckCard(gameRun.BaseDeck.FirstOrDefault(c => c.Id == nameof(cardgenji)));
							gameRun.GainPower((int)card.Config.Value2);
							gameRun.Heal((int)card.Config.Value2);
							exhibit.FinalizeQuestByCardId(questCardId);
							exhibit.MarkQuestCompleted(questCardId);
						}
						break;
					case nameof(cardquest10) when gameRun.BaseDeck.Count(c => c.Id == nameof(LBoL.EntityLib.Cards.Neutral.Black.Shadow)) >= ((cardquest10)card).Value20:
						exhibit.PendingQuestProgress[questCardId] = ++progress;
						questProgressChanged = true;

						if (progress >= card.Config.Value1)
						{
							gameRun.RemoveDeckCards(gameRun.BaseDeck.Where(c => c.Id == nameof(LBoL.EntityLib.Cards.Neutral.Black.Shadow)).Take(((cardquest10)card).Value20).ToList());
							gameRun.Heal((int)card.Config.Value2);
							gameRun.GainMoney((int)card.Config.Value2 * 10, true, new VisualSourceData
							{
								SourceType = VisualSourceType.Entity,
								Source = exhibit,
							});
							exhibit.FinalizeQuestByCardId(questCardId);
							exhibit.MarkQuestCompleted(questCardId);
						}
						break;
					default:
						break;
				}
			}

			if (questProgressChanged)
			{
				PersistQuestProgress(gameRun, exhibit.PendingQuestProgress, syncToLiteShop: true, saveToDisk: true, questRequirements: exhibit.QuestRequirements, completedQuestCards: exhibit.CompletedQuestCards, questModifiers: exhibit.PendingQuestModifiers);
			}
		}

		public static void StationEntered(StationEventArgs args)
		{
			GameRunController gameRun = Singleton<GameMaster>.Instance.CurrentGameRun;
			if (!ReferenceEquals(CachedGameRun, gameRun))
			{
				CachedGameRun = gameRun;
				LastAppliedShopDiscountFactor = 1f;
				LastAppliedSeeOrder = 0;
			}
			// reset stuff
			if (LastAppliedShopDiscountFactor > 0f)
				gameRun.ShopPriceMultiplier /= LastAppliedShopDiscountFactor;
			if (gameRun?.Player?.Us?.Config != null)
				gameRun.Player.Us.Config.PowerCost = GetBaseScPowerCost(gameRun);
			gameRun.CanViewDrawZoneActualOrder -= LastAppliedSeeOrder;
			// 1-0
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop == null)
				return;

			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] StationEntered: stage={gameRun.Stages?.IndexOf(gameRun.CurrentStage) ?? -1}, level={gameRun.CurrentStation?.Level ?? -1}, shopQuestEntries={shop.QuestProgress?.Count ?? 0} [{FormatQuestProgress(shop.QuestProgress)}]");

			if (!shop.ChallengerModeEnabled)
				return;

			// blue point progress save — create or overwrite keys
			int stageIndex = gameRun.Stages?.IndexOf(gameRun.CurrentStage) ?? -1;
			int stationLevel = gameRun.CurrentStation?.Level ?? -1;

			bool isFirstRunEntryStation = args.Station is EntryStation entryStation
				&& gameRun.Stages.IndexOf(entryStation.Stage) == 0
				&& stationLevel <= 0;
			if (isFirstRunEntryStation)
			{
				ResetQuestStateForNewRun(gameRun, shop);
			}

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
					if (prevExhibit != null && prevExhibit.PendingQuestProgress != null && prevExhibit.PendingQuestProgress.Count > 0)
					{
						// Persist runtime pending progress from the station we're leaving so it won't be
						// overwritten by the persistence sync on the station we're entering.
						PersistQuestProgress(gameRun, prevExhibit.PendingQuestProgress, syncToLiteShop: true, saveToDisk: false, questRequirements: prevExhibit.QuestRequirements, completedQuestCards: prevExhibit.CompletedQuestCards, writeToRunFlags: true, questModifiers: prevExhibit.PendingQuestModifiers);
						BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] StationEntered persisted runtime pending from previous station: [{FormatQuestProgress(prevExhibit.PendingQuestProgress)}]");
					}
				}
			}

			shop.BPProgress ??= new Dictionary<string, int>();
			shop.BPProgress["stage"] = stageIndex;
			shop.BPProgress["level"] = stationLevel;

			BepinexPlugin.log.LogInfo($"[Lvalon's Roguelite Shop] Station entered: stageIndex={stageIndex}, stationLevel={stationLevel}");

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
							BepinexPlugin.log.LogInfo($"[Lvalon's Roguelite Shop] Applied starting Remove discount: {gameRun.ShopRemoveCardCounter}");
							break;
					}
				}
			}

			// every stage
			bool appliedShopDiscount = false;
			bool appliedScDiscount = false;
			bool appliedSeeOrder = false;
			foreach (string itemId in shop.AllItems)
			{
				ShopItem item = shop.GetItem(itemId);
				if (item == null || item.CurrentTier <= 0)
					continue;

				switch (itemId)
				{
					case "discount.sc":
						if (!appliedScDiscount)
						{
							double discountMultiplier = Math.Max(0.0, 1.0 - (0.1 * item.CurrentTier));
							gameRun.Player.Us.Config.PowerCost = toolbox.Round(gameRun.Player.Us.Config.PowerCost * discountMultiplier);
							BepinexPlugin.log.LogInfo($"[Lvalon's Roguelite Shop] Applied SC discount multiplier: {gameRun.Player.Us.Config.PowerCost}");
							appliedScDiscount = true;
						}
						break;
					case "discount.shop":
						if (!appliedShopDiscount)
						{
							float discountFactor = (float)Math.Max(0.0, 1.0 - (0.05 * item.CurrentTier));
							gameRun.ShopPriceMultiplier *= discountFactor;
							LastAppliedShopDiscountFactor = discountFactor;
							BepinexPlugin.log.LogInfo($"[Lvalon's Roguelite Shop] Applied Shop discount factor: {gameRun.ShopPriceMultiplier}");
							appliedShopDiscount = true;
						}
						break;
					case "battle.seedraw":
						if (!appliedSeeOrder)
						{
							gameRun.CanViewDrawZoneActualOrder += item.CurrentTier;
							LastAppliedSeeOrder = item.CurrentTier;
							BepinexPlugin.log.LogInfo($"[Lvalon's Roguelite Shop] Applied See Order: {gameRun.CanViewDrawZoneActualOrder}");
							appliedSeeOrder = true;
						}
						break;
				}
			}
			if (!appliedShopDiscount)
				LastAppliedShopDiscountFactor = 1f;
		}

		private static void ResetQuestStateForNewRun(GameRunController gameRun, LiteShop shop)
		{
			if (shop != null)
			{
				shop.QuestProgress = new Dictionary<string, int>(StringComparer.Ordinal);
				shop.QuestRequirements = new Dictionary<string, string>(StringComparer.Ordinal);
				shop.QuestCompletedCards = new HashSet<string>(StringComparer.Ordinal);
				shop.QuestModifiers = new Dictionary<string, int>(StringComparer.Ordinal);
			}

			if (gameRun?.ExtraFlags != null)
			{
				gameRun.ExtraFlags.RemoveWhere(flag =>
					!string.IsNullOrEmpty(flag)
					&& (flag.StartsWith(QuestProgressFlagPrefix, StringComparison.Ordinal)
						|| flag.StartsWith(QuestRequirementFlagPrefix, StringComparison.Ordinal)
						|| flag.StartsWith(QuestCompletedFlagPrefix, StringComparison.Ordinal)));
			}

			BepinexPlugin.log.LogInfo("[EXQUESTING SAVE] ResetQuestStateForNewRun: cleared progress/requirements/completed for first station of new run.");
		}

		internal static int GetUpgradeDiscount(GameRunController gameRun)
		{
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop == null || !shop.ChallengerModeEnabled)
				return 0;
			ShopItem item = shop.GetItem("discount.upgrade");
			if (item == null || item.CurrentTier <= 0)
				return 0;
			return 25 * item.CurrentTier;
		}

		internal static bool HasTeaSync()
		{
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop == null || !shop.ChallengerModeEnabled)
				return false;
			ShopItem item = shop.GetItem("feature.teasync");
			return item != null && item.CurrentTier > 0;
		}
		internal static int GetGapplePerHeal()
		{
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop == null || !shop.ChallengerModeEnabled)
				return -1;
			ShopItem item = shop.GetItem("feature.gapple");
			if (item == null || item.CurrentTier <= 0)
				return -1;
			return item.Initial + (item.Delta * (item.CurrentTier - 1));
		}

		internal static int GetSponsorGold()
		{
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop == null || !shop.ChallengerModeEnabled)
				return -1;
			ShopItem item = shop.GetItem("feature.sponsor");
			if (item == null || item.CurrentTier <= 0)
				return -1;
			return item.Initial + item.Delta * item.CurrentTier;
		}

		internal static bool HasFreeChoice()
		{
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop == null || !shop.ChallengerModeEnabled)
				return false;
			ShopItem item = shop.GetItem("alter.freechoice");
			return item != null && item.CurrentTier > 0;
		}

		internal static int GetBaseScPowerCost(GameRunController gameRun)
		{
			string playerId = gameRun?.Player?.Id ?? string.Empty;
			if (playerId.Length == 0 || gameRun?.Player?.Us?.Config == null)
				return 0;
			if (!BaseScPowerCosts.TryGetValue(playerId, out int baseCost))
			{
				baseCost = gameRun.Player.Us.Config.PowerCost;
				BaseScPowerCosts[playerId] = baseCost;
			}
			return baseCost;
		}

		internal static int GetAppliedRemoveDiscount(GameRunController gameRun)
		{
			if (gameRun?.ExtraFlags == null)
				return 0;
			foreach (string flag in gameRun.ExtraFlags)
			{
				if (!flag.StartsWith(RemoveDiscountAppliedPrefix, StringComparison.Ordinal))
					continue;
				string value = flag[RemoveDiscountAppliedPrefix.Length..];
				if (int.TryParse(value, out int parsed))
					return parsed;
			}
			return 0;
		}

		internal static void SetAppliedRemoveDiscount(GameRunController gameRun, int value)
		{
			if (gameRun?.ExtraFlags == null)
				return;
			string existing = gameRun.ExtraFlags.FirstOrDefault(flag =>
				flag.StartsWith(RemoveDiscountAppliedPrefix, StringComparison.Ordinal));
			if (!string.IsNullOrEmpty(existing))
				gameRun.ExtraFlags.Remove(existing);
			gameRun.ExtraFlags.Add(RemoveDiscountAppliedPrefix + value);
		}

		internal static void MarkFreeChoiceBlocked(GameRunController gameRun)
		{
			if (gameRun?.ExtraFlags == null)
				return;
			gameRun.ExtraFlags.Add(FreeChoiceBlockedFlag);
		}

		internal static bool IsFreeChoiceBlocked(GameRunController gameRun)
		{
			return gameRun?.ExtraFlags?.Contains(FreeChoiceBlockedFlag) == true;
		}

		private static IEnumerator DraftCardFromPrev(int num, GameRunController GameRun)
		{
			GameRunController gameRun = GameRun;
			var historyLast = GameMaster.GetGameRunHistory()?.LastOrDefault();
			if (historyLast == null)
				yield break;
			var prevIds = historyLast?.Cards?.Select(rec => rec.Id).ToArray() ?? Array.Empty<string>();
			var filteredCards = prevIds
				.Select(id => toolbox.createcardwithid(id))
				.Where(card => card != null && card.CardType == CardType.Ability)
				.ToArray();
			if (filteredCards.Length == 0)
				yield break;
			//GameRun.UpgradeNewDeckCardOnFlags(array1);
			SelectCardInteraction interaction = new SelectCardInteraction(0, Math.Min(filteredCards.Length, num), filteredCards)
			{
				Source = null,
				CanCancel = false,
				Description = GetLocalizedText($"{LocalisationKeys.ShopPrefix}{LocalisationKeys.InitPrefix}card")
			};
			yield return GameRun.InteractionViewer.View(interaction);
			if (interaction.SelectedCards == null || interaction.SelectedCards.Count == 0)
				yield break;
			GameRun.AddDeckCards(interaction.SelectedCards, true, null);
		}

		private static IEnumerator DraftExhibitFromPrev(int num, GameRunController gameRun)
		{
			var historyLast = GameMaster.GetGameRunHistory()?.LastOrDefault()?.Exhibits ?? Array.Empty<string>();
			Stage stage = gameRun.CurrentStage;
			for (int i = 0; i < num; i++)
			{
				var exhibit = gameRun.RollNormalExhibit(
					gameRun.ExhibitRng,
					new ExhibitWeightTable(new RarityWeightTable(0.5f, 0.33f, 0.17f, 0f), AppearanceWeightTable.NotInShop),
					new Func<Exhibit>(stage.GetSentinelExhibit),
					c => c.Rarity != Rarity.Mythic && c.Rarity != Rarity.Mythic && !historyLast.Contains(c.Id));
				if (exhibit != null)
					GameMaster.DebugGainExhibit(exhibit);
			}
			yield break;
		}
		private static IEnumerator GainQuestExhibit()
		{
			yield return null;
			GameMaster.DebugGainExhibit(Library.CreateExhibit<exquesting>());
			yield break;
		}

		public static void addreactors()
		{
			CHandlerManager.RegisterBattleEventHandler(b => b.BattleStarting, addbattlereactor, null, (GameEventPriority)int.MinValue);
		}

		private static void addbattlereactor(GameEventArgs args)
		{
			battleChallenges = new HashSet<string>();
			quest5ToRmv = new List<Card>();
			GameRunController gamerun = GameMaster.Instance.CurrentGameRun;
			PlayerUnit player = gamerun.Battle.Player;
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();

			exquesting exhibit = null;
			if (player.HasExhibit<exquesting>())
			{
				exhibit = player.GetExhibit<exquesting>();
				foreach (string id in exhibit.PendingQuestProgress.Keys)
				{
					switch (id)
					{
						case nameof(cardquest5):
							battleChallenges.Add(id);
							player.HandleBattleEvent(gamerun.Battle.CardUsing, args =>
							{
								if (args.Card.InstinctActive && args.Card != gamerun.Battle.HandZone.Last()) quest5ToRmv.Add(args.Card);
							});
							player.ReactBattleEvent(gamerun.Battle.CardUsed, args =>
							{
								if (quest5ToRmv.Contains(args.Card))
								{
									quest5ToRmv.Remove(args.Card);
									if (gamerun.Battle.EnumerateAllCardsButExile().Contains(args.Card))
									{
										return new List<BattleAction>() { new ExileCardAction(args.Card) };
									}
								}
								return Enumerable.Empty<BattleAction>();
							});
							break;
						case nameof(cardquest6):
							foreach (EnemyUnit enemy in gamerun.Battle.AllAliveEnemies.Where(e => e.HasStatusEffect<Servant>()))
							{
								battleChallenges.Add(id);
								gamerun.Battle.React(new ApplyStatusEffectAction<sehaunted>(enemy, 0), exhibit, ActionCause.Exhibit);
							}
							player.ReactBattleEvent(gamerun.Battle.EnemySpawned, args =>
							{
								if (args.Unit.HasStatusEffect<Servant>())
								{
									battleChallenges.Add(id);
									return new List<BattleAction>() { new ApplyStatusEffectAction<sehaunted>(args.Unit, 0) };
								}
								return Enumerable.Empty<BattleAction>();
							});
							break;
						case nameof(cardquest9):
							foreach (EnemyUnit enemy in gamerun.Battle.AllAliveEnemies)
								enemy.ReactBattleEvent(enemy.DamageReceived, args => OnQuest9(args, gamerun, exhibit));
							player.HandleBattleEvent(gamerun.Battle.EnemySpawned
							, args => args.Unit.ReactBattleEvent(args.Unit.DamageReceived, damageArgs => OnQuest9(damageArgs, gamerun, exhibit)));
							break;
						case nameof(cardquest11):
							battleChallenges.Add(id);
							player.ReactBattleEvent(player.DamageReceived, args =>
							{
								cardquest11 quest11 = Library.CreateCard<cardquest11>();
								List<BattleAction> actions = new List<BattleAction>();
								if (args.DamageInfo.Damage > 0 && args.Source != player)
								{
									if (gamerun.Money > args.DamageInfo.Damage * quest11.Value2)
									{
										actions.Add(new LoseMoneyAction(toolbox.Round(args.DamageInfo.Damage * quest11.Value2)));
									}
									else
									{
										int remainder = toolbox.Round(args.DamageInfo.Damage * quest11.Value2) - gamerun.Money;
										if (gamerun.Money > 0)
										{
											actions.Add(new LoseMoneyAction(gamerun.Money));
										}
										actions.Add(DamageAction.LoseLife(player, remainder));
									}
								}
								return actions;
							});
							break;
						case nameof(cardquest14):
							battleChallenges.Add(id);
							player.HandleBattleEvent(gamerun.Battle.Reshuffled, args => { battleChallenges.Remove(id); });
							break;
						case nameof(cardquest15):
							battleChallenges.Add(id);
							quest15played = 0;
							player.HandleBattleEvent(gamerun.Battle.CardUsed, args => { quest15played++; });
							player.HandleBattleEvent(player.TurnStarting, args => { quest15played = 0; });
							break;
						case nameof(cardquest16):
							quest16active = true;
							player.HandleBattleEvent(gamerun.Battle.CardDrawn, args =>
							{
								if (args.Cause != ActionCause.TurnStart  //draw in turn
								&& (args.Cause != ActionCause.Card || !(args.ActionSource is Card card && card.IsReplenish) || gamerun.Battle.Player.IsInTurn))
								{
									turn1Drawn = true;
									quest16active = false;
								}
							});
							player.HandleBattleEvent(player.TurnStarted, args => { if (player.TurnCounter == 1) { isTurn1 = true; turn1Drawn = false; } });
							player.HandleBattleEvent(player.TurnEnded, args =>
							{
								if (!isTurn1 //turn 1 ending without ability
								|| turn1Drawn || !gamerun.Battle.DrawZone.Any(c => c.CardType == CardType.Ability))
									quest16active = false;

								if (gamerun.Battle.Player.HasExhibit<exquesting>() && quest16active)
								{
									exquesting exhibit = gamerun.Battle.Player.GetExhibit<exquesting>();
									var quest16 = Library.CreateCard<cardquest16>();
									if (exhibit.PendingQuestProgress.TryGetValue(quest16.Id, out int progress) && progress < quest16.Config.Value1)
									{
										exhibit.PendingQuestProgress[quest16.Id] = ++progress;
										if (exhibit.PendingQuestProgress[quest16.Id] >= quest16.Config.Value1)
										{
											exhibit.PendingQuestModifiers.TryGetValue(quest16.Id, out int stack);
											exhibit.PendingQuestModifiers[quest16.Id] = ++stack;
											exhibit.FinalizeQuestByCardId(quest16.Id);
											exhibit.MarkQuestCompleted(quest16.Id);
										}
									}
								}
								isTurn1 = false;
							});
							break;
						case nameof(cardquest17):
							battleChallenges.Add(id);
							player.HandleBattleEvent(player.TurnStarted, args =>
							{
								if (player.TurnCounter == 1)
								{
									gamerun.Battle.React(new ApplyStatusEffectAction<seplayleft>(player, 0), exhibit, ActionCause.Exhibit);
								}
							});
							break;
						case nameof(cardquest21):
							battleChallenges.Add(id);
							cardquest21 quest21 = Library.CreateCard<cardquest21>();
							gamerun.Battle.React(new ApplyStatusEffectAction<seattackplayed>(player, quest21.Value9), exhibit, ActionCause.Exhibit);
							gamerun.Battle.React(new ApplyStatusEffectAction<sedefenseplayed>(player, quest21.Value9), exhibit, ActionCause.Exhibit);
							gamerun.Battle.React(new ApplyStatusEffectAction<seskillplayed>(player, quest21.Value9), exhibit, ActionCause.Exhibit);
							gamerun.Battle.React(new ApplyStatusEffectAction<seabilityplayed>(player, quest21.Value9), exhibit, ActionCause.Exhibit);
							gamerun.Battle.React(new ApplyStatusEffectAction<sefriendplayed>(player, quest21.Value9), exhibit, ActionCause.Exhibit);
							gamerun.Battle.React(new ApplyStatusEffectAction<sestatusplayed>(player, quest21.Value9), exhibit, ActionCause.Exhibit);
							gamerun.Battle.React(new ApplyStatusEffectAction<semisfortuneplayed>(player, quest21.Value9), exhibit, ActionCause.Exhibit);
							gamerun.Battle.React(new ApplyStatusEffectAction<setoolplayed>(player, quest21.Value9), exhibit, ActionCause.Exhibit);
							break;
						case nameof(cardquest22):
							battleChallenges.Add(id);
							cardquest22 quest22 = Library.CreateCard<cardquest22>();
							gamerun.Battle.React(new ApplyStatusEffectAction<ManaFreezed>(player, quest22.Mana2.Total), exhibit, ActionCause.Exhibit);
							break;
						case nameof(cardquest23):
							gamerun.Battle.React(new ApplyStatusEffectAction<selifediff>(player, gamerun.Battle.Player.Hp), exhibit, ActionCause.Exhibit);
							break;
						case nameof(cardquest24):
							foreach (EnemyUnit enemy in gamerun.Battle.AllAliveEnemies)
							{
								gamerun.Battle.React(new ApplyStatusEffectAction<seholddamage>(enemy, 0), exhibit, ActionCause.Exhibit);
							}
							player.ReactBattleEvent(gamerun.Battle.EnemySpawned, args =>
							{
								return new List<BattleAction>() { new ApplyStatusEffectAction<seholddamage>(args.Unit, 0) };
							});
							break;
						default:
							break;
					}
				}
			}

			//quest 16
			player.HandleBattleEvent(gamerun.Battle.CardDrawn, args =>
			{
				if (args.Cause != ActionCause.TurnStart  //draw in turn
				&& (args.Cause != ActionCause.Card || !(args.ActionSource is Card card && card.IsReplenish) || gamerun.Battle.Player.IsInTurn))
				{
					turn1DrawnA = true;
				}
			});
			player.HandleBattleEvent(player.TurnStarted, args => { if (player.TurnCounter == 1) { isTurn1A = true; turn1DrawnA = false; } });
			player.HandleBattleEvent(player.TurnEnded, args =>
			{
				if (isTurn1A && !turn1DrawnA && gamerun.Battle.DrawZone.Any(c => c.CardType == CardType.Ability)
				&& shop != null && shop.QuestModifiers.TryGetValue(nameof(cardquest16), out int stack))
				{
					for (int i = 0; i < stack; i++)
					{
						Card toPlay = gamerun.Battle.DrawZone.Where(c => c.CardType == CardType.Ability).SampleOrDefault(gamerun.BattleCardRng);
						if (toPlay != null && toPlay.Zone == CardZone.Draw)
						{
							gamerun.Battle.React(new PlayCardAction(toPlay), exhibit, ActionCause.Exhibit);
						}
					}
				}
				isTurn1A = false;
			}, GameEventPriority.ConfigDefault + 1); //presumably slower than quest16 completion

			//quest 17
			player.HandleBattleEvent(player.TurnStarted, args =>
			{
				if (player.TurnCounter == 1 && shop != null && shop.QuestModifiers.TryGetValue(nameof(cardquest17), out int stack))
				{
					for (int i = 0; i < stack; i++)
					{
						Card toChange = gamerun.Battle.HandZone.FirstOrDefault(c => !c.IsForbidden && c.CanUse);
						if (toChange != null && !toChange.IsXCost)
							toChange.SetTurnCost(new ManaGroup { Any = 0 });
					}
				}
			});

			//quest 22
			player.ReactBattleEvent(player.TurnStarted, args =>
			{
				if (shop != null && shop.QuestModifiers.TryGetValue(nameof(cardquest22), out int stack))
				{
					return new List<BattleAction>() { new GainManaAction(new ManaGroup { Philosophy = stack }) };
				}
				return Enumerable.Empty<BattleAction>();
			});

			//quest 24
			player.ReactBattleEvent(player.DamageDealt, args =>
			{
				if (args.Source == player && args.Target != player && args.DamageInfo.Damage > 0 && shop != null && shop.QuestModifiers.TryGetValue(nameof(cardquest24), out int stack))
				{
					int toDeal = toolbox.Round(0.1f * args.DamageInfo.Damage * stack);
					if (toDeal > 0)
						return new List<BattleAction>() { new ApplyStatusEffectAction<sedelaydamage>(args.Target, toDeal) };
				}
				return Enumerable.Empty<BattleAction>();
			});

			player.ReactBattleEvent(gamerun.Battle.BattleStarted, args => OnBattleStarted(args, gamerun.Battle));
			player.ReactBattleEvent(gamerun.Battle.BattleEnding, args => OnBattleEnding(args, gamerun.Battle));
			player.ReactBattleEvent(gamerun.Battle.BattleEnded, args => OnBattleEnded(args, gamerun.Battle));
			player.ReactBattleEvent(gamerun.Battle.Player.TurnStarted, args => OnPlayerTurnStarted(args, gamerun.Battle));

			player.ReactBattleEvent(gamerun.Battle.EnemyDied, args => OnEnemyDied(args, gamerun.Battle));
			player.ReactBattleEvent(gamerun.Battle.Player.DamageReceived, args => OnPlayerDamageReceived(args, gamerun.Battle));
		}
		private static IEnumerable<BattleAction> OnQuest9(DamageEventArgs args, GameRunController gamerun, exquesting exhibit)
		{
			Card card = Library.CreateCard<cardquest9>();
			if (args.DamageInfo.DamageType == DamageType.Attack || args.DamageInfo.Damage < card.Config.Value1)
				yield break;

			if (exhibit.PendingQuestProgress.TryGetValue(card.Id, out int progress) && progress < card.Config.Value1)
				exhibit.PendingQuestProgress[card.Id] = ++progress;
			if (exhibit.PendingQuestProgress[card.Id] >= card.Config.Value1)
			{
				SelectCardInteraction interaction = new SelectCardInteraction(0, card.Config.Value2 ?? 1, Library.CreateCards<IceWing>(2), SelectedCardHandling.DoNothing)
				{
					CanCancel = true,
					Description = TypeFactory<Card>.LocalizeProperty(card.Id, "Name", true, true)
				};
				yield return new InteractionAction(interaction, false);
				IReadOnlyList<Card> selectedCards = interaction.SelectedCards;
				if (selectedCards != null)
				{
					gamerun.AddDeckCards(selectedCards, true);
				}
				exhibit.FinalizeQuestByCardId(card.Id);
				exhibit.MarkQuestCompleted(card.Id);
			}
		}

		private static IEnumerable<BattleAction> OnPlayerDamageReceived(DamageEventArgs args, BattleController battle)
		{
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop == null || !battle.Player.HasExhibit<exquesting>() || !args.DamageInfo.IsGrazed)
				yield break;
			exquesting exhibit = battle.Player.GetExhibit<exquesting>();
			var quest8 = Library.CreateCard<cardquest8>();
			if (exhibit.PendingQuestProgress.TryGetValue(quest8.Id, out int progress) && progress < quest8.Config.Value1)
			{
				exhibit.PendingQuestProgress[quest8.Id] = ++progress;
				if (exhibit.PendingQuestProgress[quest8.Id] >= quest8.Config.Value1)
				{
					if (!battle.Player.HasExhibit<LouguanJian>())
						GameMaster.DebugGainExhibit(Library.CreateExhibit<LouguanJian>());
					exhibit.FinalizeQuestByCardId(quest8.Id);
					exhibit.MarkQuestCompleted(quest8.Id);
				}
			}
		}

		private static IEnumerable<BattleAction> OnBattleEnded(GameEventArgs args, BattleController battle)
		{
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop == null || !battle.Player.HasExhibit<exquesting>())
				yield break;
			var exhibit = battle.Player.GetExhibit<exquesting>();

			foreach (BattleAction ba in HandleEndBattleChallenges(args, battle, exhibit))
			{
				yield return ba;
			}

			// handle dynamic ending req: quest 23
			if (battle.Player.TryGetStatusEffect(out selifediff se) && exhibit.PendingQuestProgress.TryGetValue(nameof(cardquest23), out int progress))
			{
				bool shouldFinish = false;
				if (progress == 0) // init
				{
					if (se.Count > se.Level)
						exhibit.PendingQuestProgress[nameof(cardquest23)] += 1;
					if (se.Count < se.Level)
						exhibit.PendingQuestProgress[nameof(cardquest23)] -= 1;
				}
				else if (progress > 0)
				{
					if (se.Count > se.Level)
						exhibit.PendingQuestProgress[nameof(cardquest23)] += 1;
					if (se.Count < se.Level)
					{
						yield return new GainMoneyAction(Library.CreateCard<cardquest23>().Value2 * exhibit.PendingQuestProgress[nameof(cardquest23)]);
						shouldFinish = true;
					}
				}
				else if (progress < 0)
				{
					if (se.Count < se.Level)
						exhibit.PendingQuestProgress[nameof(cardquest23)] -= 1;
					if (se.Count > se.Level)
					{
						battle.GameRun.GainMaxHp(Library.CreateCard<cardquest23>().Value1 * -exhibit.PendingQuestProgress[nameof(cardquest23)]);
						shouldFinish = true;
					}
				}

				if (shouldFinish)
				{
					exhibit.FinalizeQuestByCardId(nameof(cardquest23));
					exhibit.MarkQuestCompleted(nameof(cardquest23));
				}
			}

			exhibit.UnlockCompletedQuestSlots();
			exhibit.FlushCompletedQuestStateAfterFullSave();
			exhibit.RefreshRolledQuestRequirementsForSave();
			exhibit.CleanupStaleQuestRequirements();

			exhibit.RollQuestCards(preserveAcceptedSlots: true);
			PersistQuestProgress(battle?.GameRun, exhibit.PendingQuestProgress, syncToLiteShop: true, saveToDisk: true, questRequirements: exhibit.QuestRequirements, completedQuestCards: exhibit.CompletedQuestCards, questModifiers: exhibit.PendingQuestModifiers);
		}

		private static IEnumerable<BattleAction> HandleEndBattleChallenges(GameEventArgs args, BattleController battle, exquesting exhibit)
		{
			GameRunController gameRun = battle.GameRun;
			List<Card> challengeQuests = new List<Card>();
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			foreach (string cards in battleChallenges)
			{
				Card card = Library.CreateCard(cards);
				if (card != null)
				{
					challengeQuests.Add(card);
				}
				else
				{
					BepinexPlugin.log.LogError($"[Lvalon's Roguelite Shop] HandleEndBattleChallenges: Failed to create card for id {cards}");
				}
			}

			HashSet<Card> willFinish = new HashSet<Card>();
			List<Card> prematureRemove = new List<Card>();

			foreach (var card in challengeQuests) // check which one will finish first
			{
				string questCardId = card?.Id;
				if (string.IsNullOrEmpty(questCardId))
					continue;

				if (exhibit.IsQuestCardSoldOut(questCardId) ||
					exhibit.IsQuestCardCompleted(questCardId))
					continue;

				if (!exhibit.PendingQuestProgress.TryGetValue(questCardId, out var progress))
					continue;

				if (!battleChallenges.Contains(card.Id))
					continue;

				if (progress + 1 >= card.Config.Value1)
				{
					willFinish.Add(card);
				}

				if (card.Id == nameof(cardquest15)) //handle unconditional effects
				{
					cardquest15 quest15 = Library.CreateCard<cardquest15>();
					if (battle.EnumerateAllCardsButExile().Count() < quest15.Value2)
						yield return new DamageAction(battle.Player, new List<Unit> { battle.Player }, DamageInfo.HpLose(quest15.Value2, true));
					if (!battle.EnumerateAllCards().Any(c => c.CardType == CardType.Ability))
						yield return new LosePowerAction(battle.Player.Power);
					if (quest15played > quest15.Value10)
						yield return new LoseMoneyAction(battle.GameRun.Money);
				}
			}

			foreach (Card card in willFinish) // resolve rewards
			{
				exhibit.PendingQuestProgress.TryGetValue(card.Id, out var progress);
				switch (card.Id)
				{
					case nameof(cardquest3):
						yield return new GainMoneyAction((int)card.Config.Value2);
						break;
					case nameof(cardquest5):
						Card[] array = battle.GameRun.RollCards(battle.GameRun.CardRng, new CardWeightTable(new RarityWeightTable(1f, 0.8f, 0f, 0f), OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), card.Config.Value2 ?? 20, false, false, null);
						SelectCardInteraction interaction = new SelectCardInteraction(1, 1, array, SelectedCardHandling.DoNothing)
						{
							CanCancel = false,
							Description = TypeFactory<Card>.LocalizeProperty(card.Id, "Name", true, true)
						};
						yield return new InteractionAction(interaction, false);
						IReadOnlyList<Card> selectedCards = interaction.SelectedCards;
						if (selectedCards != null)
						{
							battle.GameRun.AddDeckCards(selectedCards, true);
						}
						break;
					case nameof(cardquest6):
						Card[] array2 = battle.GameRun.RollCardsWithoutManaLimit(battle.GameRun.CardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot, false), card.Config.Value2 ?? 3, false, false, config => config.Owner == nameof(lvalonmima));
						foreach (Card c in array2.Where(c => c.CanUpgradeAndPositive))
						{
							c.Upgrade();
						}
						SelectCardInteraction interaction2 = new SelectCardInteraction(0, 1, array2, SelectedCardHandling.DoNothing)
						{
							CanCancel = true,
							Description = TypeFactory<Card>.LocalizeProperty(card.Id, "Name", true, true)
						};
						yield return new InteractionAction(interaction2, false);
						IReadOnlyList<Card> selectedCards2 = interaction2.SelectedCards;
						if (selectedCards2 != null)
						{
							battle.GameRun.AddDeckCards(selectedCards2, true);
						}
						break;
					case nameof(cardquest12):
						SelectCardInteraction interaction3 = new SelectCardInteraction(0, 1, gameRun.BaseDeck.Where(card => !card.Unremovable && card.Config.Rarity != Rarity.Rare), SelectedCardHandling.DoNothing)
						{
							CanCancel = true,
							Description = TypeFactory<Card>.LocalizeProperty(card.Id, "Name", true, true)
						};
						yield return new InteractionAction(interaction3, false);
						IReadOnlyList<Card> selectedCards3 = interaction3.SelectedCards;
						if (selectedCards3 != null)
						{
							List<Rarity> allowed = new List<Rarity>() { Rarity.Uncommon, Rarity.Rare, Rarity.Mythic };
							if (selectedCards3[0].Config.Rarity == Rarity.Uncommon)
								allowed = new List<Rarity>() { Rarity.Rare };
							if (selectedCards3[0].Config.Rarity == Rarity.Common)
								allowed = new List<Rarity>() { Rarity.Uncommon };
							Card toAdd = battle.GameRun.RollCards(battle.GameRun.CardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot), 1, false, false, config => allowed.Contains(config.Rarity))[0];
							if (toAdd != null)
							{
								battle.GameRun.RemoveDeckCard(selectedCards3[0]);
								battle.GameRun.AddDeckCard(toAdd, true);
							}
						}
						break;
					case nameof(cardquest14):
						if (battle.DrawZone.Count() == 0)
						{
							gameRun.GainMaxHp(gameRun.BaseDeck.Count());
							List<Card> array3 = gameRun.BaseDeck.Where(c => c.CanUpgradeAndPositive).ToList();
							if (array3.Count > 0)
							{
								SelectCardInteraction interaction4 = new SelectCardInteraction(0, 1, array3, SelectedCardHandling.DoNothing)
								{
									CanCancel = true,
									Description = TypeFactory<Card>.LocalizeProperty(card.Id, "Name", true, true)
								};
								yield return new InteractionAction(interaction4, false);
								if (interaction4?.SelectedCards?[0] != null)
									gameRun.UpgradeDeckCard(interaction4.SelectedCards[0], true);
							}
						}
						else
						{
							prematureRemove.Add(card);
						}
						break;
					case nameof(cardquest15):
						cardquest15 quest15 = Library.CreateCard<cardquest15>();
						gameRun.GainMaxHp(quest15.Value2);
						gameRun.SetHpAndMaxHp(gameRun.Player.MaxHp, gameRun.Player.MaxHp, true);
						int toGain = gameRun.BaseDeck.Count(c => c.CardType == CardType.Ability) * quest15.Value2;
						if (toGain > 0)
							yield return new GainPowerAction(toGain);
						break;
					case nameof(cardquest21):
						yield return new GainMoneyAction((int)card.Config.Value2);
						break;
					default:
						break;
				}
			}

			challengeQuests.RemoveAll(c => prematureRemove.Contains(c));

			foreach (Card card in challengeQuests) // resolve append/finish
			{
				bool ok = exhibit.PendingQuestProgress.TryGetValue(card.Id, out var progress);
				if (!ok)
				{
					BepinexPlugin.log.LogError($"[Lvalon's Roguelite Shop] HandleEndBattleChallenges: Failed to get progress for card {card.Id}");
					continue;
				}
				exhibit.PendingQuestProgress[card.Id] = ++progress;
				if (progress >= card.Config.Value1)
				{
					switch (card.Id) // add perma effs
					{
						case nameof(cardquest17):
							exhibit.PendingQuestModifiers.TryGetValue(card.Id, out int stack);
							exhibit.PendingQuestModifiers[card.Id] = ++stack;
							break;
						case nameof(cardquest22):
							exhibit.PendingQuestModifiers.TryGetValue(card.Id, out int stack22);
							exhibit.PendingQuestModifiers[card.Id] = ++stack22;
							break;
						default:
							break;
					}
					exhibit.FinalizeQuestByCardId(card.Id);
					exhibit.MarkQuestCompleted(card.Id);
				}
			}
			yield break;
		}

		private static IEnumerable<BattleAction> OnEnemyDied(DieEventArgs args, BattleController battle)
		{
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop == null || !battle.Player.HasExhibit<exquesting>() || args.Unit.HasStatusEffect<Servant>())
				yield break;
			exquesting exhibit = battle.Player.GetExhibit<exquesting>();
			var quest1 = Library.CreateCard<cardquest1>();
			if (exhibit.PendingQuestProgress.TryGetValue(quest1.Id, out int progress) && progress < quest1.Config.Value1)
			{
				exhibit.PendingQuestProgress[quest1.Id] = ++progress;
				if (exhibit.PendingQuestProgress[quest1.Id] >= quest1.Config.Value1)
				{
					yield return new GainMoneyAction((int)quest1.Config.Value2);
					exhibit.FinalizeQuestByCardId(quest1.Id);
					exhibit.MarkQuestCompleted(quest1.Id);
				}
			}
		}

		private static IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args, BattleController battle)
		{
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop == null)
				yield break;
			if (!shop.ChallengerModeEnabled)
				yield break;
			foreach (string itemId in shop.AllItems)
			{
				ShopItem item = shop.GetItem(itemId);
				if (item == null || item.CurrentTier <= 0)
					continue;

				switch (itemId)
				{
					case "init.fp":
						yield return new ApplyStatusEffectAction<Firepower>(battle.Player, item.CurrentTier);
						break;
					case "init.sp":
						yield return new ApplyStatusEffectAction<Spirit>(battle.Player, item.CurrentTier);
						break;
					case "battle.block":
						yield return new CastBlockShieldAction(battle.Player, new BlockInfo(item.CurrentTier * 2, BlockShieldType.Normal), true);
						break;
					case "battle.graze":
						yield return new ApplyStatusEffectAction<Graze>(battle.Player, item.CurrentTier);
						break;
					case "battle.rolldiscard":
						if (battle.DrawZone.Count() > 0)
						{
							foreach (BattleAction ba in Rerolldiscard(battle)) yield return ba;
						}
						break;
				}
			}
			if (!battle.Player.HasExhibit<exquesting>())
				yield break;
			var exhibit = battle.Player.GetExhibit<exquesting>();
			foreach (string id in exhibit.PendingQuestProgress.Keys)
			{
				switch (id)
				{
					case nameof(cardquest3):
						battleChallenges.Add(id);
						yield return new ApplyStatusEffectAction<FirepowerNegative>(battle.Player, 1);
						yield return new ApplyStatusEffectAction<SpiritNegative>(battle.Player, 1);
						break;
					case nameof(cardquest12):
						battleChallenges.Add(id);
						cardquest12 quest12 = Library.CreateCard<cardquest12>();
						List<Card> toConvert = battle.EnumerateAllCards().SampleManyOrAll(quest12.Value30, battle.GameRun.BattleCardRng).ToList();
						foreach (Card card in toConvert)
						{
							Card toAdd = battle.RollCards(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot), 1, config => config.Rarity == card.Config.Rarity)[0];
							if (card != null && toAdd != null && battle.EnumerateAllCards().Contains(card))
								yield return new TransformCardAction(card, toAdd);
						}
						break;
					default:
						break;
				}
			}
			yield break;
		}

		private static IEnumerable<BattleAction> OnBattleEnding(GameEventArgs args, BattleController battle)
		{
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop == null)
				yield break;
			if (!shop.ChallengerModeEnabled)
				yield break;

			foreach (string itemId in shop.AllItems)
			{
				ShopItem item = shop.GetItem(itemId);
				if (item == null || item.CurrentTier <= 0)
					continue;

				switch (itemId)
				{
					case "battle.heal":
						if (battle.Player.IsAlive)
							yield return new HealAction(battle.Player, battle.Player, item.CurrentTier);
						break;
				}
			}

			if (!battle.Player.HasExhibit<exquesting>())
				yield break;
			var exhibit = battle.Player.GetExhibit<exquesting>();

			foreach (string id in exhibit.PendingQuestProgress.Keys)
			{
				switch (id)
				{
					case nameof(cardquest21):
						cardquest21 card = Library.CreateCard<cardquest21>();
						StatusEffect atk = battle.Player.GetStatusEffect<seattackplayed>();
						StatusEffect def = battle.Player.GetStatusEffect<sedefenseplayed>();
						StatusEffect skill = battle.Player.GetStatusEffect<seskillplayed>();
						StatusEffect ability = battle.Player.GetStatusEffect<seabilityplayed>();
						StatusEffect friend = battle.Player.GetStatusEffect<sefriendplayed>();
						StatusEffect status = battle.Player.GetStatusEffect<sestatusplayed>();
						StatusEffect misfortune = battle.Player.GetStatusEffect<semisfortuneplayed>();
						StatusEffect tool = battle.Player.GetStatusEffect<setoolplayed>();
						if (atk != null && atk.Count != card.Value9
						&& def != null && def.Count != card.Value9
						&& skill != null && skill.Count != card.Value9
						&& ability != null && ability.Count != card.Value9
						&& friend != null && friend.Count != card.Value9
						&& status != null && status.Count != card.Value9
						&& misfortune != null && misfortune.Count != card.Value9
						&& tool != null && tool.Count != card.Value9)
							battleChallenges.Remove(id);
						break;
					default:
						break;
				}
			}
		}

		private static IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args, BattleController battle)
		{
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop == null || !shop.ChallengerModeEnabled)
				yield break;
			foreach (string itemId in shop.AllItems)
			{
				ShopItem item = shop.GetItem(itemId);
				if (item == null || item.CurrentTier <= 0)
					continue;

				switch (itemId)
				{
					case "battle.hacks":
						foreach (BattleAction ba in Tryhacking(battle, item)) yield return ba;
						break;
				}
			}
		}
		private static string GetLocalizedText(string key)
		{
			var locale = LBoL.Core.Localization.CurrentLocale;
			if (LocalisationKeys.LocTable.TryGetValue((locale, key), out var text))
				return text;
			if (LocalisationKeys.LocTable.TryGetValue((Locale.En, key), out var fallback))
				return fallback;
			return key;
		}
		internal static IEnumerable<BattleAction> Rerolldiscard(BattleController battle)
		{
			List<cardrerolldiscard> list = Library.CreateCards<cardrerolldiscard>(2).ToList();
			cardrerolldiscard cardrerolldiscard = list[0];
			cardrerolldiscard cardrerolldiscard2 = list[1];
			cardrerolldiscard.ChoiceCardIndicator = 1;
			cardrerolldiscard2.ChoiceCardIndicator = 2;
			cardrerolldiscard.SetBattle(battle);
			cardrerolldiscard2.SetBattle(battle);
			MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(list, false, false, false) { Description = GetLocalizedText($"{LocalisationKeys.ShopPrefix}{LocalisationKeys.BattlePrefix}rolldiscard") };
			yield return new InteractionAction(interaction);
			Card card = interaction?.SelectedCard;
			if (card != null && card.ChoiceCardIndicator == 2) // ExtraDescription2
			{
				List<Card> list2 = battle.DrawZone.Reverse().Concat(battle.HandZone).ToList();
				foreach (Card item2 in list2)
				{
					if (item2.Zone == CardZone.Draw || item2.Zone == CardZone.Hand)
					{
						yield return new MoveCardAction(item2, CardZone.Discard);
					}
				}
			}
		}
		internal static IEnumerable<BattleAction> Tryhacking(BattleController battle, ShopItem item)
		{
			if (battle.Player.TurnCounter != 1)
				yield break;

			float chance = Math.Min(1f, item.Delta * item.CurrentTier / 100f);
			var rng = GameMaster.Instance.CurrentGameRun.BattleRng;
			if (rng.NextFloat() >= chance)
				yield break;

			Card selected = null;
			int abilityCount = 0;
			foreach (var c in battle.EnumerateAllCards())
			{
				if (c.CardType != CardType.Ability)
					continue;
				abilityCount++;

				if (rng.NextFloat() < 1f / abilityCount)
					selected = c;
			}
			if (selected != null)
			{
				yield return new PlayCardAction(selected);
			}
		}
	}

	[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.UpgradeDeckCardPrice), MethodType.Getter)]
	class GameRunController_UpgradeDeckCardPrice_Patch
	{
		static void Postfix(GameRunController __instance, ref int __result)
		{
			int discount = ShopModHandlers.GetUpgradeDiscount(__instance);
			if (discount <= 0)
				return;
			int adjusted = __result - discount;
			__result = adjusted < 0 ? 0 : adjusted;
		}
	}

	[HarmonyPatch(typeof(GapOptionsPanel), nameof(GapOptionsPanel.OptionClicked))]
	class GapOptionsPanel_TeaSync_Patch
	{
		static void Postfix(GapOptionsPanel __instance, GapOption option)
		{
			if (option == null || option.Type != GapOptionType.UpgradeCard)
				return;

			if (ShopModHandlers.GetSponsorGold() > 0)
			{
				GameMaster.Instance.CurrentGameRun.GainMoney(ShopModHandlers.GetSponsorGold(), true);
			}

			if (!ShopModHandlers.HasTeaSync())
				return;
			GapStation gapStation = Traverse.Create(__instance).Field("_gapStation").GetValue<GapStation>();
			if (gapStation == null)
				return;
			DrinkTea drinkTea = gapStation.GapOptions?.OfType<DrinkTea>().FirstOrDefault();
			if (drinkTea == null)
				return;
			gapStation.DrinkTea(drinkTea);
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
					}
				}
			}
		}
	}

	[HarmonyPatch(typeof(GameMaster), nameof(GameMaster.RequestAbandonGameRun))]
	class GameMaster_RequestAbandonGameRun_FreeChoice_Patch
	{
		static void Prefix()
		{
			GameRunController gameRun = Singleton<GameMaster>.Instance?.CurrentGameRun;
			if (gameRun == null)
				return;
			ShopModHandlers.MarkFreeChoiceBlocked(gameRun);
		}
	}

	[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.LeaveBattle))]
	class GameRunController_LeaveBattle_FreeChoice_Patch
	{
		static void Postfix(GameRunController __instance)
		{
			if (__instance?.Player?.IsDead != true)
				return;
			ShopModHandlers.MarkFreeChoiceBlocked(__instance);
		}
	}

	[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.CanEnterTrueEnding))]
	class GameRunController_CanEnterTrueEnding_FreeChoice_Patch
	{
		static void Postfix(GameRunController __instance, ref bool __result)
		{
			if (!ShopModHandlers.HasFreeChoice())
				return;
			if (ShopModHandlers.IsFreeChoiceBlocked(__instance))
				return;
			__result = true;
		}
	}

	[HarmonyPatch(typeof(LBoL.Core.Dialogs.DialogFunctions), nameof(LBoL.Core.Dialogs.DialogFunctions.HasTrueEndProvider))]
	class DialogFunctions_HasTrueEndProvider_FreeChoice_Patch
	{
		static void Postfix(LBoL.Core.Dialogs.DialogFunctions __instance, ref bool __result)
		{
			if (!ShopModHandlers.HasFreeChoice())
				return;
			GameRunController gameRun = __instance.GetGameRun();
			__result = !ShopModHandlers.IsFreeChoiceBlocked(gameRun);
		}
	}

	[HarmonyPatch(typeof(LBoL.Core.Dialogs.DialogFunctions), nameof(LBoL.Core.Dialogs.DialogFunctions.IsTrueEndBlocked))]
	class DialogFunctions_IsTrueEndBlocked_FreeChoice_Patch
	{
		static void Postfix(LBoL.Core.Dialogs.DialogFunctions __instance, ref bool __result)
		{
			if (!ShopModHandlers.HasFreeChoice())
				return;
			GameRunController gameRun = __instance.GetGameRun();
			__result = ShopModHandlers.IsFreeChoiceBlocked(gameRun);
		}
	}

	[HarmonyPatch(typeof(LBoL.Core.Dialogs.DialogFunctions), nameof(LBoL.Core.Dialogs.DialogFunctions.TrueEndProviderName))]
	class DialogFunctions_TrueEndProviderName_FreeChoice_Patch
	{
		static void Postfix(LBoL.Core.Dialogs.DialogFunctions __instance, ref string __result)
		{
			if (!ShopModHandlers.HasFreeChoice())
				return;
			GameRunController gameRun = __instance.GetGameRun();
			if (ShopModHandlers.IsFreeChoiceBlocked(gameRun))
				return;
			if (gameRun.TrueEndingProviders != null && gameRun.TrueEndingProviders.Count > 0)
				return;
			__result = "Free Choice";
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
						List<Card> toUpgrade = gameRun.BaseDeck.Where(c => c.CanUpgradeAndPositive).SampleManyOrAll(quest18.Config.Value2 ?? 3, gameRun.CardRng).ToList();
						if (toUpgrade.Count > 0)
							gameRun.UpgradeDeckCards(toUpgrade, true);
						exhibit.FinalizeQuestByCardId(quest18.Id);
						exhibit.MarkQuestCompleted(quest18.Id);
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
					}
				}
			}
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
					}
				}
			}
		}
	}
}

