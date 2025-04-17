using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using UnityEngine;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace lvalonmima.SE
{
    public sealed class extmpfiredef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(extmpfire);
        }

        public override LocalizationOption LoadLocalization()
        {
            return BepinexPlugin.sebatchloc.AddEntity(this);
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("seextmpfire.png", BepinexPlugin.embeddedSource);
        }

        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig statusEffectConfig = new StatusEffectConfig(
                Index: BepinexPlugin.sequenceTable.Next(typeof(CardConfig)),
                Id: "",
                Order: 3,
                Type: StatusEffectType.Positive,
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
                RelativeEffects: new List<string>() { },
                VFX: "Default",
                VFXloop: "Default",
                SFX: "Default",
                ImageId: null
            );
            return statusEffectConfig;
        }

        [EntityLogic(typeof(extmpfiredef))]
        public sealed class extmpfire : mimaextensions.mimase
        {
            //set up triggers to give a fuck on
            //they worked
            protected override void OnAdded(Unit unit)
            {
                HandleOwnerEvent(unit.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(OnDamageDealing));
                ReactOwnerEvent(Battle.RoundEnded, new EventSequencedReactor<GameEventArgs>(OnRoundEnded));
            }
            private void OnDamageDealing(DamageDealingEventArgs args)
            {
                if (args.DamageInfo.DamageType == DamageType.Attack)
                {
                    args.DamageInfo = args.DamageInfo.IncreaseBy(Level);
                    args.AddModifier(this);
                }
            }
            private IEnumerable<BattleAction> OnRoundEnded(GameEventArgs args)
            {
                NotifyActivating();
                yield return new RemoveStatusEffectAction(this, true);
                yield break;
            }
        }
    }
}
