using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle;
using LBoL.Core;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using UnityEngine;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.ExtraTurn;

namespace lvalonmima.SE
{
    public sealed class exfireonskilldef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(exfireonskill);
        }

        public override LocalizationOption LoadLocalization()
        {
            return BepinexPlugin.sebatchloc.AddEntity(this);
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("seexfireonskill.png", BepinexPlugin.embeddedSource);
        }

        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig statusEffectConfig = new StatusEffectConfig(
                Index: BepinexPlugin.sequenceTable.Next(typeof(CardConfig)),
                Id: "",
                Order: 1,
                Type: StatusEffectType.Special,
                IsVerbose: true,
                IsStackable: true,
                StackActionTriggerLevel: null,
                HasLevel: true,
                LevelStackType: StackType.Add,
                HasDuration: false,
                DurationStackType: null,
                DurationDecreaseTiming: DurationDecreaseTiming.Custom,
                HasCount: false,
                CountStackType: StackType.Keep,
                LimitStackType: StackType.Keep,
                ShowPlusByLimit: false,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { nameof(extmpfiredef.extmpfire) },
                VFX: "Default",
                VFXloop: "Default",
                SFX: "Default"
            );
            return statusEffectConfig;
        }

        [EntityLogic(typeof(exfireonskilldef))]
        public sealed class exfireonskill : mimaextensions.mimaexpartner
        {
            protected override void OnAdded(Unit unit)
            {
                ThisTurnActivating = false;
                HandleOwnerEvent(Battle.Player.TurnStarting, delegate (UnitEventArgs _)
                {
                    if (Battle.Player.IsExtraTurn && !Battle.Player.IsSuperExtraTurn && Battle.Player.GetStatusEffectExtend<ExtraTurnPartner>() == this)
                    {
                        ThisTurnActivating = true;
                    }
                });
                ReactOwnerEvent(Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(OnCardUsed));
                ReactOwnerEvent(Battle.Player.TurnEnding, new EventSequencedReactor<GameEventArgs>(OnPlayerTurnEnding));
            }

            private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
            {
                if (args.Card.CardType == CardType.Skill && ThisTurnActivating)
                {
                    NotifyActivating();
                    yield return new ApplyStatusEffectAction<extmpfiredef.extmpfire>(Owner, new int?(Level), null, null, null, 0.2f, true);
                }
                yield break;
            }

            private IEnumerable<BattleAction> OnPlayerTurnEnding(GameEventArgs args)
            {
                if (ThisTurnActivating)
                {
                    NotifyActivating();
                    yield return new RemoveStatusEffectAction(this, true);
                    yield break;
                }
            }
        }
    }
}
