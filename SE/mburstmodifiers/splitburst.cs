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
using static lvalonmima.SE.karmanationdef;
using static lvalonmima.SE.magicalburstdef;
using LBoL.Core.Battle.BattleActions;
using LBoL.Base.Extensions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoL.EntityLib.StatusEffects.Koishi;
using static UnityEngine.GraphicsBuffer;
using System.Linq;
using static lvalonmima.SE.mburstmodifiers.everlastingmagicdef;

namespace lvalonmima.SE.mburstmodifiers
{
    public sealed class splitburstdef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(splitburst);
        }

        public override LocalizationOption LoadLocalization()
        {
            return toolbox.locse();
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("sesplitburst.png", embeddedSource);
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
                RelativeEffects: new List<string>() { "magicalburst" },
                VFX: "Default",
                VFXloop: "Default",
                SFX: "Default"
            );
            return statusEffectConfig;
        }

        [EntityLogic(typeof(splitburstdef))]
        public sealed class splitburst : mimaextensions.mimase
        {
            public splitburst() : base()
            {
                isMBmod = true;
            }
            //set up triggers to give a fuck on
            //also vfx/sfx
            //they worked
            protected override void OnAdded(Unit unit)
            {
            }
            public int splitdmg
            {
                get
                {
                    return (GameRun == null) ? 50 : Convert.ToInt32(100 / (Level + 1));
                }
            }
        }
    }
}
