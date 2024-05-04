using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core;
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
    public sealed class startturnloselifedef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(startturnloselife);
        }

        public override LocalizationOption LoadLocalization()
        {
            return BepinexPlugin.sebatchloc.AddEntity(this);
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("sestartturnloselife.png", BepinexPlugin.embeddedSource);
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
                RelativeEffects: new List<string>() { },
                VFX: "Default",
                VFXloop: "Default",
                SFX: "Default"
            );
            return statusEffectConfig;
        }

        [EntityLogic(typeof(startturnloselifedef))]
        public sealed class startturnloselife : mimaextensions.mimase
        {
            protected override void OnAdded(Unit unit)
            {
                ReactOwnerEvent(Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(OnTurnStarted));
            }
            private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
            {
                if (Battle.BattleShouldEnd)
                {
                    yield break;
                }
                NotifyActivating();
                yield return new DamageAction(Owner, Owner, DamageInfo.HpLose((float)Level, false), "Instant", GunType.Single);
                yield break;
            }
        }
    }
}
