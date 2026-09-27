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

public partial class ShopModHandlers
{
	private static HashSet<string> battleChallenges = [];

	private static List<Card> quest5ToRmv;

	private static int quest15played;

	private static bool isTurn1;

	private static bool isTurn1A;

	private static bool turn1Drawn;

	private static bool turn1DrawnA;

	private static bool quest16active;

	private static int quest30TriggerTurn;

	public static void addreactors()
	{
		CHandlerManager.RegisterBattleEventHandler(b => b.BattleStarting, addbattlereactor, null, (GameEventPriority)int.MinValue);
	}

	private static void addbattlereactor(GameEventArgs args)
	{
		battleChallenges = [];
		quest5ToRmv = [];
		GameRunController gamerun = GameMaster.Instance.CurrentGameRun;
		PlayerUnit player = gamerun.Battle.Player;
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();

		exquesting exhibit = null;
		if (player.HasExhibit<exquesting>())
		{
			exhibit = player.GetExhibit<exquesting>();
			var pendingKeys = exhibit.PendingQuestProgress != null ? exhibit.PendingQuestProgress.Keys.ToList() : [];
			foreach (string id in pendingKeys)
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
									return [new ExileCardAction(args.Card)];
								}
							}
							return [];
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
								return [new ApplyStatusEffectAction<sehaunted>(args.Unit, 0)];
							}
							return [];
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
							List<BattleAction> actions = [];
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
							&& (args.Cause != ActionCause.Card || args.ActionSource is not Card { IsReplenish: true } || gamerun.Battle.Player.IsInTurn))
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
										RecordRewardedQuestCompletion(quest16.Id);
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
							return [new ApplyStatusEffectAction<seholddamage>(args.Unit, 0)];
						});
						break;
					case nameof(cardquest25):
						gamerun.Battle.React(new ApplyStatusEffectAction<seabilityplayed>(player, 0), exhibit, ActionCause.Exhibit);
						gamerun.Battle.React(new ApplyStatusEffectAction<sedamagereceived>(player, 0), exhibit, ActionCause.Exhibit);
						gamerun.Battle.React(new ApplyStatusEffectAction<sehealreceived>(player, 0), exhibit, ActionCause.Exhibit);
						gamerun.Battle.React(new ApplyStatusEffectAction<seblockgained>(player, 0), exhibit, ActionCause.Exhibit);
						break;
					case nameof(cardquest26):
						player.HandleBattleEvent(player.DamageReceived, args =>
						{
							if (args.Source != player && args.DamageInfo.Damage > 0)
							{
								if (gamerun.Battle.Player.HasExhibit<exquesting>())
								{
									exquesting exhibit = gamerun.Battle.Player.GetExhibit<exquesting>();
									var quest26 = Library.CreateCard<cardquest26>();
									exhibit.FinalizeQuestByCardId(quest26.Id);
									exhibit.MarkQuestCompleted(quest26.Id);
								}
							}
						});
						break;
					case nameof(cardquest27):
						gamerun.Battle.React(new ApplyStatusEffectAction<sepie>(player, Library.CreateCard<cardquest27>().Value2), exhibit, ActionCause.Exhibit);
						break;
					case nameof(cardquest29):
						foreach (EnemyUnit enemy in gamerun.Battle.AllAliveEnemies)
						{
							gamerun.Battle.React(new ApplyStatusEffectAction<seharden>(enemy, 0), exhibit, ActionCause.Exhibit);
						}
						player.ReactBattleEvent(gamerun.Battle.EnemySpawned, args =>
						{
							return [new ApplyStatusEffectAction<seharden>(args.Unit, 0)];
						});
						break;
					case nameof(cardquest30):
						cardquest30 quest30 = Library.CreateCard<cardquest30>();
						quest30TriggerTurn = gamerun.BattleRng.NextInt(quest30.Value2, quest30.Value7); // this shits inclusive? wtf ok
						player.ReactBattleEvent(player.TurnStarted, args =>
						{
							List<BattleAction> actions = [];
							if (player.TurnCounter == quest30TriggerTurn)
							{
								actions.Add(new ApplyStatusEffectAction<ExtraTurn>(player, 1));
							}
							if (player.TurnCounter == quest30TriggerTurn + 1)
								actions.Add(new ApplyStatusEffectAction<selockinstance>(player, 0));
							if (player.TurnCounter == quest30TriggerTurn + 2)
							{
								foreach (EnemyUnit enemy in gamerun.Battle.AllAliveEnemies)
								{
									actions.Add(new ApplyStatusEffectAction<selockinstance>(enemy, 0));
								}
								player.ReactBattleEvent(gamerun.Battle.EnemySpawned, args =>
								{
									return [new ApplyStatusEffectAction<selockinstance>(args.Unit, 0)];
								});
							}
							return actions;
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
				&& (args.Cause != ActionCause.Card || args.ActionSource is not Card { IsReplenish: true } || gamerun.Battle.Player.IsInTurn))
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
				if (stack > 0)
				{
					List<Card> toPlay = [.. gamerun.Battle.DrawZone.Where(c => c.CardType == CardType.Ability)];
					foreach (Card card in toPlay)
					{
						if (card.Zone == CardZone.Draw && !gamerun.Battle.BattleShouldEnd)
						{
							gamerun.Battle.React(new PlayCardAction(card), exhibit, ActionCause.Exhibit);
						}
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
				return [new GainManaAction(new ManaGroup { Philosophy = stack })];
			return [];
		});

		//quest 24
		player.ReactBattleEvent(player.DamageDealt, args =>
		{
			if (args.Source == player && args.Target != player && args.DamageInfo.Damage > 0 && shop != null && shop.QuestModifiers.TryGetValue(nameof(cardquest24), out int stack))
			{
				int toDeal = toolbox.Round(0.2f * args.DamageInfo.Damage * stack);
				if (toDeal > 0)
					return [new ApplyStatusEffectAction<sedelaydamage>(args.Target, toDeal)];
			}
			return [];
		});

		//quest 25
		if (shop != null && shop.QuestModifiers.TryGetValue(nameof(cardquest25), out int stack25))
		{
			if (stack25 > 0)
				gamerun.Battle.React(new ApplyStatusEffectAction<sequest25>(player, stack25), exhibit, ActionCause.Exhibit);
		}

		//quest 26
		if (shop != null && shop.QuestModifiers.TryGetValue(nameof(cardquest26), out int stack26))
		{
			if (stack26 > 0)
				gamerun.Battle.React(new ApplyStatusEffectAction<sequest26>(player, stack26), exhibit, ActionCause.Exhibit);
		}

		//quest 27
		if (shop != null && shop.QuestModifiers.TryGetValue(nameof(cardquest27), out int stack27))
		{
			if (stack27 > 0)
				gamerun.Battle.React(new ApplyStatusEffectAction<sequest27>(player, stack27), exhibit, ActionCause.Exhibit);
		}

		//quest 28
		if (shop != null && shop.QuestModifiers.TryGetValue(nameof(cardquest28), out int stack28))
		{
			if (stack28 > 0)
				gamerun.Battle.React(new ApplyStatusEffectAction<sequest28>(player, stack28), exhibit, ActionCause.Exhibit);
		}

		//quest 29
		if (shop != null && shop.QuestModifiers.TryGetValue(nameof(cardquest29), out int stack29))
		{
			if (stack29 > 0)
				gamerun.Battle.React(new ApplyStatusEffectAction<sequest29>(player, stack29), exhibit, ActionCause.Exhibit);
		}

		//quest 30
		if (shop != null && shop.QuestModifiers.TryGetValue(nameof(cardquest30), out int stack30))
		{
			if (stack30 > 0)
				gamerun.Battle.React(new ApplyStatusEffectAction<SuperExtraTurn>(player, stack30), exhibit, ActionCause.Exhibit);
		}

		player.ReactBattleEvent(gamerun.Battle.BattleStarted, args => OnBattleStarted(args, gamerun.Battle));
		player.ReactBattleEvent(gamerun.Battle.BattleEnding, args => OnBattleEnding(args, gamerun.Battle));
		player.ReactBattleEvent(gamerun.Battle.BattleEnded, args => OnBattleEnded(args, gamerun.Battle), GameEventPriority.ConfigDefault + 100);
		player.ReactBattleEvent(gamerun.Battle.Player.TurnStarted, args => OnPlayerTurnStarted(args, gamerun.Battle));
		player.ReactBattleEvent(gamerun.Battle.Player.TurnEnded, args => OnPlayerTurnEnded(args, gamerun.Battle));

		player.ReactBattleEvent(gamerun.Battle.EnemyDied, args => OnEnemyDied(args, gamerun.Battle));
		player.ReactBattleEvent(gamerun.Battle.Player.DamageReceived, args => OnPlayerDamageReceived(args, gamerun.Battle));

		if (shop != null && shop.ChallengerModeEnabled && shop.GetItem("difficulty.ascension")?.CurrentTier > 0)
		{
			foreach (EnemyUnit enemy in gamerun.Battle.AllAliveEnemies)//.Where(e => !e.HasStatusEffect<Servant>()))
			{
				gamerun.Battle.React(ApplyAscensionBuff(enemy, gamerun.Battle), player, ActionCause.Player);
			}
			player.HandleBattleEvent(gamerun.Battle.EnemySpawned, args =>
			{
				if (args.Unit is EnemyUnit enemy)// && !enemy.HasStatusEffect<Servant>())
				{
					gamerun.Battle.React(ApplyAscensionBuff(enemy, gamerun.Battle), player, ActionCause.Player);
				}
			});
		}
	}

	private static BattleAction ApplyAscensionBuff(EnemyUnit enemy, BattleController battle)
	{
		return enemy switch
		{
			MaoyuBlack => new ApplyStatusEffectAction<seMaoyuBlack>(enemy, 1),
			MaoyuOrigin => new ApplyStatusEffectAction<seMaoyu>(enemy, 1),
			WhiteFairy => new ApplyStatusEffectAction<seWhiteFairy>(enemy, 1),
			Raven => new ApplyStatusEffectAction<seRaven>(enemy, 1),
			BlackFairy => new ApplyStatusEffectAction<seBlackFairy>(enemy, 1),
			Guihuo => new ApplyStatusEffectAction<seGuihuo>(enemy, 1),
			SickGirl => new ApplyStatusEffectAction<seSickGirl>(enemy, 1),
			YinyangyuRedOrigin => new ApplyStatusEffectAction<seRedOrb>(enemy, 1),
			YinyangyuBlueOrigin => new ApplyStatusEffectAction<seBlueOrb>(enemy, 1),
			FraudRabbit => new ApplyStatusEffectAction<seFraudRabbit>(enemy, 1),
			BatLord => new ApplyStatusEffectAction<seBatLord>(enemy, 1),
			BatOrigin => new ApplyStatusEffectAction<seBat>(enemy, 1),
			DollBase => new ApplyStatusEffectAction<seDoll>(enemy, 1),
			WaterGirl => new ApplyStatusEffectAction<seWaterGirl>(enemy, 1),
			ScoutOrigin => new ApplyStatusEffectAction<seScout>(enemy, 1),
			PurifierOrigin => new ApplyStatusEffectAction<sePurifier>(enemy, 1),
			TerminatorOrigin => new ApplyStatusEffectAction<seTerminator>(enemy, 1),
			LBoL.EntityLib.EnemyUnits.Normal.Yaoshi => new ApplyStatusEffectAction<seYaoshi>(enemy, 1),
			Fox => new ApplyStatusEffectAction<seFox>(enemy, 1),
			ShenlingWhite => new ApplyStatusEffectAction<seSPWhite>(enemy, 1),
			ShenlingPurple => new ApplyStatusEffectAction<seSPPurple>(enemy, 1),
			LoveGirl => new ApplyStatusEffectAction<seLoveGirl>(enemy, 1),
			YaTiangou => new ApplyStatusEffectAction<seRaven3>(enemy, 1),
			LangTiangou => new ApplyStatusEffectAction<seBigCrow>(enemy, 1),
			KanakoLimao => new ApplyStatusEffectAction<seKanako>(enemy, 1),
			SuwakoLimao => new ApplyStatusEffectAction<seSuwako>(enemy, 1),
			Aya => new ApplyStatusEffectAction<seAya>(enemy, 1),
			Sunny => new ApplyStatusEffectAction<seSunny>(enemy, 1),
			Star => new ApplyStatusEffectAction<seStar>(enemy, 1),
			Luna => new ApplyStatusEffectAction<seLuna>(enemy, 1),
			Rin => new ApplyStatusEffectAction<seRin>(enemy, 1),
			Youmu => new ApplyStatusEffectAction<seYoumu>(enemy, 1),
			Nitori => new ApplyStatusEffectAction<seNitori>(enemy, 1),
			Kokoro => new ApplyStatusEffectAction<seKoNu>(enemy, 1),
			Doremy => new ApplyStatusEffectAction<seDoremy>(enemy, 1),
			Clownpiece => new ApplyStatusEffectAction<seClownpiece>(enemy, 1),
			Siji => new ApplyStatusEffectAction<seEiki>(enemy, 1),
			Reimu => new ApplyStatusEffectAction<seReimu>(enemy, 1),
			Marisa => new ApplyStatusEffectAction<seMarisa>(enemy, 1),
			Sakuya => new ApplyStatusEffectAction<seSakuya>(enemy, 1),
			Cirno => new ApplyStatusEffectAction<seCirno>(enemy, 1),
			Koishi => new ApplyStatusEffectAction<sesbKoishi>(enemy, 1),
			Yuyuko => new ApplyStatusEffectAction<seYuyuko>(enemy, 1),
			Tianzi => new ApplyStatusEffectAction<seTenshi>(enemy, 1),
			Long => new ApplyStatusEffectAction<seMegumu>(enemy, 1),
			Remilia => new ApplyStatusEffectAction<seRemilia>(enemy, 1),
			Sanae => new ApplyStatusEffectAction<seSanae>(enemy, 1),
			Junko => new ApplyStatusEffectAction<seJunko>(enemy, 1),
			Seija => new ApplyStatusEffectAction<seSeija>(enemy, 1),
			_ => null,
		};
	}

	private static IEnumerable<BattleAction> OnQuest9(DamageEventArgs args, GameRunController gamerun, exquesting exhibit)
	{
		Card card = Library.CreateCard<cardquest9>();
		if (args.DamageInfo.DamageType == DamageType.Attack || args.DamageInfo.Damage < card.Config.Value1)
			yield break;

		if (exhibit.IsQuestCardCompleted(card.Id) || exhibit.IsQuestCardSoldOut(card.Id))
			yield break;

		if (!exhibit.PendingQuestProgress.TryGetValue(card.Id, out int progress))
			yield break;

		if (progress >= card.Config.Value1)
			yield break;

		progress++;
		exhibit.PendingQuestProgress[card.Id] = progress;

		if (progress >= card.Config.Value1)
		{
			SelectCardInteraction interaction = new(0, card.Config.Value2 ?? 1, Library.CreateCards<IceWing>(card.Config.Value2 ?? 1), SelectedCardHandling.DoNothing)
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
			RecordRewardedQuestCompletion(card.Id);
		}
	}

	private static IEnumerable<BattleAction> OnPlayerDamageReceived(DamageEventArgs args, BattleController battle)
	{
		yield return null;
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
				RecordRewardedQuestCompletion(quest8.Id);
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
			yield return ba;

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
				RecordRewardedQuestCompletion(nameof(cardquest23));
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
		List<Card> challengeQuests = [];
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		foreach (string cards in battleChallenges)
		{
			Card card = Library.CreateCard(cards);
			if (card != null)
			{
				challengeQuests.Add(card);
			}
		}

		HashSet<Card> willFinish = [];
		List<Card> prematureRemove = [];

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
				willFinish.Add(card);

			if (card.Id == nameof(cardquest15)) //handle unconditional effects
			{
				cardquest15 quest15 = Library.CreateCard<cardquest15>();
				if (battle.EnumerateAllCardsButExile().Count() < quest15.Value2)
					yield return new DamageAction(battle.Player, [battle.Player], DamageInfo.HpLose(quest15.Value2, true));
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
					SelectCardInteraction interaction = new(1, 1, array, SelectedCardHandling.DoNothing)
					{
						CanCancel = false,
						Description = TypeFactory<Card>.LocalizeProperty(card.Id, "Name", true, true)
					};
					yield return new InteractionAction(interaction, false);
					IReadOnlyList<Card> selectedCards = interaction.SelectedCards;
					if (selectedCards != null)
						battle.GameRun.AddDeckCards(selectedCards, true);
					break;
				case nameof(cardquest6):
					Card[] array2 = battle.GameRun.RollCardsWithoutManaLimit(battle.GameRun.CardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot, false), card.Config.Value2 ?? 3, false, false, config => config.Owner == nameof(lvalonmima));
					foreach (Card c in array2.Where(c => c.CanUpgradeAndPositive))
						c.Upgrade();
					SelectCardInteraction interaction2 = new(0, 1, array2, SelectedCardHandling.DoNothing)
					{
						CanCancel = true,
						Description = TypeFactory<Card>.LocalizeProperty(card.Id, "Name", true, true)
					};
					yield return new InteractionAction(interaction2, false);
					IReadOnlyList<Card> selectedCards2 = interaction2.SelectedCards;
					if (selectedCards2 != null)
						battle.GameRun.AddDeckCards(selectedCards2, true);
					break;
				case nameof(cardquest12):
					SelectCardInteraction interaction3 = new(0, 1, gameRun.BaseDeck.Where(card => !card.Unremovable && card.Config.Rarity != Rarity.Rare), SelectedCardHandling.DoNothing)
					{
						CanCancel = true,
						Description = TypeFactory<Card>.LocalizeProperty(card.Id, "Name", true, true)
					};
					yield return new InteractionAction(interaction3, false);
					IReadOnlyList<Card> selectedCards3 = interaction3.SelectedCards;
					if (selectedCards3 != null)
					{
						List<Rarity> allowed = [Rarity.Uncommon, Rarity.Rare, Rarity.Mythic];
						if (selectedCards3[0].Config.Rarity == Rarity.Uncommon)
							allowed = [Rarity.Rare];
						if (selectedCards3[0].Config.Rarity == Rarity.Common)
							allowed = [Rarity.Uncommon];
						Card toAdd = battle.GameRun.RollCard(battle.GameRun.CardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot), false, false, config => allowed.Contains(config.Rarity));
						if (toAdd != null)
						{
							battle.GameRun.RemoveDeckCard(selectedCards3[0]);
							battle.GameRun.AddDeckCard(toAdd, true);
						}
					}
					break;
				case nameof(cardquest14):
					if (battle.DrawZone.Count == 0)
					{
						gameRun.GainMaxHp(gameRun.BaseDeck.Count());
						List<Card> array3 = [.. gameRun.BaseDeck.Where(c => c.CanUpgradeAndPositive)];
						if (array3.Count > 0)
						{
							gameRun.UpgradeDeckCards(array3.SampleManyOrAll(toolbox.Round(array3.Count() * 1.0 / 2), gameRun.CardRng), true);
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
				continue;
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
				RecordRewardedQuestCompletion(card.Id);
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
				RecordRewardedQuestCompletion(quest1.Id);
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
					if (battle.DrawZone.Count > 0)
					{
						foreach (BattleAction ba in Rerolldiscard(battle)) yield return ba;
					}
					break;
			}
		}
		if (!battle.Player.HasExhibit<exquesting>())
			yield break;
		var exhibit = battle.Player.GetExhibit<exquesting>();
		var pendingKeys = exhibit.PendingQuestProgress != null ? exhibit.PendingQuestProgress.Keys.ToList() : [];
		foreach (string id in pendingKeys)
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
					List<Card> toConvert = [.. battle.EnumerateAllCards().SampleManyOrAll(quest12.Value30, battle.GameRun.BattleCardRng)];
					foreach (Card card in toConvert)
					{
						Card toAdd = battle.RollCard(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot), config => config.Rarity == card.Config.Rarity);
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

		var pendingKeys = exhibit.PendingQuestProgress != null ? exhibit.PendingQuestProgress.Keys.ToList() : [];
		foreach (string id in pendingKeys)
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
				case nameof(cardquest28):
					if (exhibit.PendingQuestProgress.ContainsKey(id)
					&& battle.BattleCardUsageHistory.Last() != null //not having played card doesnt remove
					&& battle.GameRun.GetDeckCardByInstanceId(battle.BattleCardUsageHistory.Last().InstanceId) != null) // flag existing
					{
						exhibit.PendingQuestProgress.Remove(id);
						// also clear any rolled slot that currently shows this quest so the slot is free for future rolls
						List<int> slotsToClear = [];
						foreach (var (key, value) in exhibit.RolledQuestCards ?? [])
						{
							if (value != null && string.Equals(value.Id, id, StringComparison.Ordinal))
								slotsToClear.Add(key);
						}
						foreach (int slot in slotsToClear)
						{
							exhibit.RolledQuestCards.Remove(slot);
						}
					}
					break;
				case nameof(cardquest30):
					if (exhibit.PendingQuestProgress.ContainsKey(id)
					&& (battle.Player.TurnCounter <= quest30TriggerTurn || (battle.Player.TurnCounter == quest30TriggerTurn && !battle.Player.IsInTurn))) // before extra turn
					{
						exhibit.PendingQuestProgress.Remove(id);
						// also clear any rolled slot that currently shows this quest so the slot is free for future rolls
						List<int> slotsToClear = [];
						foreach (var (key, value) in exhibit.RolledQuestCards ?? [])
						{
							if (value != null && string.Equals(value.Id, id, StringComparison.Ordinal))
								slotsToClear.Add(key);
						}
						foreach (int slot in slotsToClear)
						{
							exhibit.RolledQuestCards.Remove(slot);
						}
					}
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
				case "battle.block" when battle.Player.TurnCounter == 1:
					yield return new CastBlockShieldAction(battle.Player, new BlockInfo(item.CurrentTier * 2, BlockShieldType.Normal), true);
					break;
				case "battle.graze" when battle.Player.TurnCounter == 1:
					yield return new ApplyStatusEffectAction<Graze>(battle.Player, item.CurrentTier);
					break;
			}
		}
	}

	// battle.hacks ("End of turn": "At the end of the Challenger's first turn,
	// there is a {0}% chance to play 1 random Ability card") fires here, not in
	// OnPlayerTurnStarted -- it was previously applied at the *start* of turn 1
	// instead of the end, contrary to its own description (checklist G16).
	private static IEnumerable<BattleAction> OnPlayerTurnEnded(UnitEventArgs args, BattleController battle)
	{
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop == null || !shop.ChallengerModeEnabled)
			yield break;

		ShopItem item = shop.GetItem("battle.hacks");
		if (item == null || item.CurrentTier <= 0)
			yield break;

		foreach (BattleAction ba in Tryhacking(battle, item)) yield return ba;
	}

	private static string GetLocalizedText(string key) => LocalisationKeys.Get(key);

	internal static IEnumerable<BattleAction> Rerolldiscard(BattleController battle)
	{
		List<cardrerolldiscard> list = [.. Library.CreateCards<cardrerolldiscard>(2)];
		cardrerolldiscard cardrerolldiscard = list[0];
		cardrerolldiscard cardrerolldiscard2 = list[1];
		cardrerolldiscard.ChoiceCardIndicator = 1;
		cardrerolldiscard2.ChoiceCardIndicator = 2;
		cardrerolldiscard.SetBattle(battle);
		cardrerolldiscard2.SetBattle(battle);
		MiniSelectCardInteraction interaction = new(list, false, false, false) { Description = GetLocalizedText($"{LocalisationKeys.ShopPrefix}{LocalisationKeys.BattlePrefix}rolldiscard") };
		yield return new InteractionAction(interaction);
		Card card = interaction?.SelectedCard;
		if (card != null && card.ChoiceCardIndicator == 2) // ExtraDescription2
		{
			List<Card> list2 = [.. battle.DrawZone.Reverse(), .. battle.HandZone];
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
			yield return new PlayCardAction(selected);
	}

}
[HarmonyPatch(typeof(Seija), nameof(Seija.BuffAndClear))]
class Seija_BuffAndClear_Patch
{
	static bool Prefix(Seija __instance, ref IEnumerable<BattleAction> __result)
	{
		if (__instance.HasStatusEffect<seSeija>())
		{
			__result = BuffAndClearPrefix(__instance);
			return false;
		}
		return true;
	}

	private static IEnumerable<BattleAction> BuffAndClearPrefix(Seija seija)
	{
		yield return PerformAction.Spell(seija, "逆转攻势");
		yield return PerformAction.Animation(seija, "spell", 1f);
		yield return new CastBlockShieldAction(seija, 0, seija.Defend, BlockShieldType.Normal, cast: false);
		if (seija.ItemCount < 6)
		{
			bool hasGrail = seija.HasStatusEffect<HolyGrailSe>() || seija._pool.Contains(typeof(HolyGrailSe));
			bool hasPyramid = seija.HasStatusEffect<QiannianShenqiSe>() || seija._pool.Contains(typeof(QiannianShenqiSe));
			if (hasGrail && hasPyramid)
			{
				if (!seija.HasStatusEffect<InfinityGemsSe>())
					seija._pool.Add(typeof(InfinityGemsSe));
				if (!seija.HasStatusEffect<SakuraWandSe>())
					seija._pool.Add(typeof(SakuraWandSe));
			}
			else
			{
				if (!seija.HasStatusEffect<HolyGrailSe>() && !seija._pool.Contains(typeof(HolyGrailSe)))
					seija._pool.Add(typeof(HolyGrailSe));
				if (!seija.HasStatusEffect<QiannianShenqiSe>() && !seija._pool.Contains(typeof(QiannianShenqiSe)))
					seija._pool.Add(typeof(QiannianShenqiSe));
			}
			yield return seija.RandomBuff();
		}
		else
		{
			yield return new ApplyStatusEffectAction<DragonBallSe>(seija, null, null, null, null, 1f);
		}

		int itemCount = seija.ItemCount + 1;
		seija.ItemCount = itemCount;
		itemCount = seija.BigRoundCount + 1;
		seija.BigRoundCount = itemCount;
	}
}
