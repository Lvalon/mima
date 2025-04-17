using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle;
using LBoL.Core;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections.Generic;
using UnityEngine;
using LBoL.Core.Units;

namespace lvalonmima.SE.mburstmodifiers
{
    public sealed class fastburstdef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(fastburst);
        }

        public override LocalizationOption LoadLocalization()
        {
            return BepinexPlugin.sebatchloc.AddEntity(this);
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("sefastburst.png", BepinexPlugin.embeddedSource);
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
                RelativeEffects: new List<string>() { nameof(magicalburstdef.magicalburst) },
                VFX: "Default",
                VFXloop: "Default",
                SFX: "Default",
                ImageId: null
            );
            return statusEffectConfig;
        }

        [EntityLogic(typeof(fastburstdef))]
        public sealed class fastburst : mimaextensions.mimase
        {
            public fastburst() : base()
            {
                truecounter = 0;
            }
            public int showfastburst => GameRun.Battle == null ? 20 : (Level > 5) ? Convert.ToInt32(5 * 20) : Convert.ToInt32(Level * 20);
            //set up triggers to give a fuck on
            //also vfx/sfx
            //they worked
            protected override void OnAdded(Unit unit)
            {
                ReactOwnerEvent(Owner.StatusEffectAdded, new EventSequencedReactor<StatusEffectApplyEventArgs>(OnStatusEffectAdded));
                if (Level > 5) { NotifyChanged(); Level = 5; }
            }
            private IEnumerable<BattleAction> OnStatusEffectAdded(StatusEffectApplyEventArgs args)
            {
                if (Level > 5)
                {
                    NotifyChanged();
                    Level = 5;
                }
                yield break;
            }
        }
    }
}
