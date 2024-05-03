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
using static lvalonmima.BepinexPlugin;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.ExtraTurn;
using static lvalonmima.SE.extmpfiredef;

namespace lvalonmima.SE
{
    public sealed class exfireonskilldef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(exfireonskill);
        }

        public override LocalizationOption LoadLocalization() => sebatchloc.AddEntity(this);

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("seexfireonskill.png", embeddedSource);
        }

        public override StatusEffectConfig MakeConfig()
        {
            var statusEffectConfig = new StatusEffectConfig(
                Index: sequenceTable.Next(typeof(CardConfig)),
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
                RelativeEffects: new List<string>() { nameof(extmpfire) },
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
                base.ThisTurnActivating = false;
                base.HandleOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, delegate (UnitEventArgs _)
                {
                    if (base.Battle.Player.IsExtraTurn && !base.Battle.Player.IsSuperExtraTurn && base.Battle.Player.GetStatusEffectExtend<ExtraTurnPartner>() == this)
                    {
                        base.ThisTurnActivating = true;
                    }
                });
                base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
                ReactOwnerEvent(Battle.Player.TurnEnding, new EventSequencedReactor<GameEventArgs>(OnPlayerTurnEnding));
            }

            private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
            {
                if (args.Card.CardType == CardType.Skill && base.ThisTurnActivating)
                {
                    NotifyActivating();
                    yield return new ApplyStatusEffectAction<extmpfire>(base.Owner, new int?(base.Level), null, null, null, 0.2f, true);
                }
                yield break;
            }

            private IEnumerable<BattleAction> OnPlayerTurnEnding(GameEventArgs args)
            {
                if (base.ThisTurnActivating)
                {
                    NotifyActivating();
                    yield return new RemoveStatusEffectAction(this, true);
                    yield break;
                }
            }
        }
    }
}
