using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static lvalonmima.BepinexPlugin;
using LBoL.Core.Battle.BattleActions;
using LBoL.Base.Extensions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace lvalonmima.SE
{
    public sealed class RepeatActionSeDef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(RepeatActionSe);
        }

        public override LocalizationOption LoadLocalization() => sebatchloc.AddEntity(this);

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("RepeatActionSe.png", embeddedSource);
        }
        public override StatusEffectConfig MakeConfig()
        {
            var statusEffectConfig = new StatusEffectConfig(
                Index: sequenceTable.Next(typeof(StatusEffectConfig)),
                Id: "",
                Order: 10,
                Type: StatusEffectType.Positive,
                IsVerbose: false,
                IsStackable: true,
                StackActionTriggerLevel: null,
                HasLevel: true,
                LevelStackType: StackType.Add,
                HasDuration: false,
                DurationStackType: StackType.Add,
                DurationDecreaseTiming: DurationDecreaseTiming.Custom,
                HasCount: false,
                CountStackType: StackType.Keep,
                LimitStackType: StackType.Keep,
                ShowPlusByLimit: false,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { },
                VFX: "Default",
                VFXloop: "Default",
                SFX: "Default"
            );
            return statusEffectConfig;
        }
        [EntityLogic(typeof(RepeatActionSeDef))]
        public sealed class RepeatActionSe : mimaextensions.mimase
        {
            private bool Temp = false;
            private Card card;
            private int? activeCost;
            private int? upgradedActiveCost;
            protected override void OnAdded(Unit unit)
            {
                ReactOwnerEvent(Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(OnCardUsed));
                ReactOwnerEvent(Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(OnPlayerTurnStarted));
            }
            private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
            {
                card = args.Card;
                yield break;
            }
            private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
            {
                if (card != null && card.Battle == null)
                {
                    card.EnterBattle(Battle);
                    Temp = true;
                }
                if (card.CardType == CardType.Friend)
                {
                    activeCost = card.Config.ActiveCost;
                    upgradedActiveCost = card.Config.UpgradedActiveCost;
                    card.Config.ActiveCost = 0;
                    card.Config.UpgradedActiveCost = 0;
                }
                if (card != null && card.Battle != null && card.GameRun != null)
                {
                    UnitSelector unitSelector = new UnitSelector(Battle.AllAliveEnemies.Sample(GameRun.BattleRng));
                    for (int i = 0; i < Level; i++)
                    {
                        if (!unitSelector.SelectedEnemy.IsAlive)
                        {
                            unitSelector = new UnitSelector(Battle.AllAliveEnemies.Sample(GameRun.BattleRng));
                        }
                        List<DamageAction> damageActions = new List<DamageAction>();
                        switch (card.Config.TargetType)
                        {
                            case TargetType.Nobody:
                                foreach (BattleAction battleAction in card.GetActions(UnitSelector.Nobody, ManaGroup.Empty, card.Precondition(), damageActions, card.Summoning))
                                {
                                    DamageAction damageAction = battleAction as DamageAction;
                                    if (damageAction != null)
                                    {
                                        damageActions.Add(damageAction);
                                    }
                                    yield return battleAction;
                                }
                                break;
                            case TargetType.SingleEnemy:
                                foreach (BattleAction battleAction in card.GetActions(unitSelector, ManaGroup.Empty, card.Precondition(), damageActions, card.Summoning))
                                {
                                    DamageAction damageAction = battleAction as DamageAction;
                                    if (damageAction != null)
                                    {
                                        damageActions.Add(damageAction);
                                    }
                                    yield return battleAction;
                                }
                                break;
                            case TargetType.AllEnemies:
                                foreach (BattleAction battleAction in card.GetActions(UnitSelector.AllEnemies, ManaGroup.Empty, card.Precondition(), damageActions, card.Summoning))
                                {
                                    DamageAction damageAction = battleAction as DamageAction;
                                    if (damageAction != null)
                                    {
                                        damageActions.Add(damageAction);
                                    }
                                    yield return battleAction;
                                }
                                break;
                            case TargetType.RandomEnemy:
                                foreach (BattleAction battleAction in card.GetActions(UnitSelector.RandomEnemy, ManaGroup.Empty, card.Precondition(), damageActions, card.Summoning))
                                {
                                    DamageAction damageAction = battleAction as DamageAction;
                                    if (damageAction != null)
                                    {
                                        damageActions.Add(damageAction);
                                    }
                                    yield return battleAction;
                                }
                                break;
                            case TargetType.Self:
                                foreach (BattleAction battleAction in card.GetActions(UnitSelector.Self, ManaGroup.Empty, card.Precondition(), damageActions, card.Summoning))
                                {
                                    DamageAction damageAction = battleAction as DamageAction;
                                    if (damageAction != null)
                                    {
                                        damageActions.Add(damageAction);
                                    }
                                    yield return battleAction;
                                }
                                break;
                            case TargetType.All:
                                foreach (BattleAction battleAction in card.GetActions(UnitSelector.All, ManaGroup.Empty, card.Precondition(), damageActions, card.Summoning))
                                {
                                    DamageAction damageAction = battleAction as DamageAction;
                                    if (damageAction != null)
                                    {
                                        damageActions.Add(damageAction);
                                    }
                                    yield return battleAction;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    Battle.PlayerTurnShouldEnd = false;
                }
                if (card.CardType == CardType.Friend)
                {
                    card.Config.ActiveCost = activeCost;
                    card.Config.UpgradedActiveCost = upgradedActiveCost;
                }
                if (Temp)
                {
                    if (card.Battle != null)
                    {
                        card.LeaveBattle();
                    }
                    Temp = false;
                }
            }
            protected override void OnRemoved(Unit unit)
            {
                card = null;
            }
        }
    }
}
