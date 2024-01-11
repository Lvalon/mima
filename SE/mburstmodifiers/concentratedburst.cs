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

namespace lvalonmima.SE.mburstmodifiers
{
    public sealed class concentratedburstdef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(concentratedburst);
        }

        public override LocalizationOption LoadLocalization() => sebatchloc.AddEntity(this);

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("seconcentratedburst.png", embeddedSource);
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

        [EntityLogic(typeof(concentratedburstdef))]
        public sealed class concentratedburst : mimaextensions.mimase
        {
            public concentratedburst() : base()
            {
                isMBmod = true;
                truecounter = 0;
            }
            int enemycount = 0;
            //set up triggers to give a fuck on
            //also vfx/sfx
            //they worked
            protected override void OnAdded(Unit unit)
            {
                foreach (Unit target in base.Battle.AllAliveEnemies)
                {
                        enemycount++;
                }
                ReactOwnerEvent(base.Battle.EnemyDied, new EventSequencedReactor<DieEventArgs>(this.OnEnemyDied));
            }
            public int showenemycount
            {
                get
                {
                    if (base.Battle == null) { return 1; }
                    else { return enemycount; }
                }
            }
            private IEnumerable<BattleAction> OnEnemyDied(DieEventArgs arg)
            {
                base.NotifyChanged();
                enemycount = 0;
                foreach (Unit target in base.Battle.AllAliveEnemies)
                {
                    enemycount++;
                }
                yield break;
            }
        }
    }
}
