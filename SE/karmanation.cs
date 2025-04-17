using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections.Generic;
using UnityEngine;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace lvalonmima.SE
{
    public sealed class karmanationdef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(karmanation);
        }

        public override LocalizationOption LoadLocalization()
        {
            return BepinexPlugin.sebatchloc.AddEntity(this);
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("sekarmanation.png", BepinexPlugin.embeddedSource);
        }

        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig statusEffectConfig = new StatusEffectConfig(
                Index: BepinexPlugin.sequenceTable.Next(typeof(CardConfig)),
                Id: "",
                Order: 20,
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
                SFX: "Default",
                ImageId: null
            );
            return statusEffectConfig;
        }

        [EntityLogic(typeof(karmanationdef))]
        public sealed class karmanation : mimaextensions.mimase
        {

            //set up triggers to give a fuck on
            //also vfx/sfx
            //they worked
            protected override void OnAdded(Unit unit)
            {
                HandleOwnerEvent(Owner.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(OnDamageDealing));
                React(PerformAction.Effect(unit, "JunkoNightmare", 0f, "JunkoNightmare", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
                React(PerformAction.Effect(unit, "JunkoNightmare", 1f, "", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
                React(PerformAction.Effect(unit, "JunkoNightmare", 2f, "", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
            }
            public int dmgscale => GameRun.Battle == null ? 2 : (Level >= 27) ? Convert.ToInt32(Math.Pow(2, 27)) : Convert.ToInt32(Math.Pow(2, Level));

            private void OnDamageDealing(DamageDealingEventArgs args)
            {
                if (args.DamageInfo.DamageType == DamageType.Attack)
                {
                    args.DamageInfo = args.DamageInfo.MultiplyBy((Level >= 27) ? Convert.ToInt32(Math.Pow(2, 27)) : Convert.ToInt32(Math.Pow(2, Level)));
                    args.AddModifier(this);
                    if (args.Cause != ActionCause.OnlyCalculate)
                    {
                        NotifyActivating();
                    }
                }
            }
        }
    }
}
