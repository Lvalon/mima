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
using LBoL.Core.Units;

namespace lvalonmima.SE.mburstmodifiers
{
    public sealed class concentratedburstdef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(concentratedburst);
        }

        public override LocalizationOption LoadLocalization()
        {
            return BepinexPlugin.sebatchloc.AddEntity(this);
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("seconcentratedburst.png", BepinexPlugin.embeddedSource);
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

        [EntityLogic(typeof(concentratedburstdef))]
        public sealed class concentratedburst : mimaextensions.mimase
        {
            public concentratedburst() : base()
            {
                truecounter = 0;
            }

            private int enemycount = 0;
            //set up triggers to give a fuck on
            //also vfx/sfx
            //they worked
            protected override void OnAdded(Unit unit)
            {
                foreach (Unit target in Battle.AllAliveEnemies)
                {
                    enemycount++;
                }
                ReactOwnerEvent(Battle.EnemyDied, new EventSequencedReactor<DieEventArgs>(OnEnemyDied));
            }
            public int showenemycount => Battle == null ? 1 : enemycount;
            private IEnumerable<BattleAction> OnEnemyDied(DieEventArgs arg)
            {
                NotifyChanged();
                enemycount = 0;
                foreach (Unit target in Battle.AllAliveEnemies)
                {
                    enemycount++;
                }
                yield break;
            }
        }
    }
}
