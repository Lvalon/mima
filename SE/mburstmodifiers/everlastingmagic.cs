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
using static lvalonmima.SE.mburstmodifiers.accumulationdef;

namespace lvalonmima.SE.mburstmodifiers
{
    public sealed class everlastingmagicdef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(everlastingmagic);
        }

        public override LocalizationOption LoadLocalization() => sebatchloc.AddEntity(this);

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("seeverlastingmagic.png", embeddedSource);
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

        [EntityLogic(typeof(everlastingmagicdef))]
        public sealed class everlastingmagic : mimaextensions.mimase
        {
            public everlastingmagic() : base()
            {
                isMBmod = true;
                truecounter = 0;
            }
            public int showeverlasting
            {
                get
                {
                    if (GameRun == null) { return 80; }
                    //else { return (Level >= 5) ? 0 : Convert.ToInt32(100 - (Level * 20)); }
                    else { return (Level > 5) ? 100 : Convert.ToInt32(Level * 20); }
                }
            }
            protected override void OnAdded(Unit unit)
            {
                ReactOwnerEvent<StatusEffectApplyEventArgs>(base.Owner.StatusEffectAdded, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnStatusEffectAdded));
                if (Level > 5) { base.NotifyChanged(); Level = 5; }
            }
            private IEnumerable<BattleAction> OnStatusEffectAdded(StatusEffectApplyEventArgs args)
            {
                if (Level > 5)
                {
                    base.NotifyChanged();
                    Level = 5;
                }
                yield break;
            }
        }
    }
}
