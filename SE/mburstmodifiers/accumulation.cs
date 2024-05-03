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
using static lvalonmima.SE.mburstmodifiers.everlastingmagicdef;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace lvalonmima.SE.mburstmodifiers
{
    public sealed class accumulationdef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(accumulation);
        }

        public override LocalizationOption LoadLocalization() => sebatchloc.AddEntity(this);

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("seaccumulation.png", embeddedSource);
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

        [EntityLogic(typeof(accumulationdef))]
        public sealed class accumulation : mimaextensions.mimase
        {
            public accumulation() : base()
            {
                isMBmod = true;
                truecounter = 0;
            }
            //set up triggers to give a fuck on
            //also vfx/sfx
            //they worked
            protected override void OnAdded(Unit unit)
            {
                if (Owner.TryGetStatusEffect<everlastingmagic>(out var everlastingmagic))
                { React(new RemoveStatusEffectAction(this, true, 0.1f)); }
                ReactOwnerEvent(base.Owner.StatusEffectAdded, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnStatusEffectAdded));
            }
            private IEnumerable<BattleAction> OnStatusEffectAdded(StatusEffectApplyEventArgs args)
            {
                if (args.Effect is everlastingmagic)
                {
                    this.NotifyChanged();
                    yield return new RemoveStatusEffectAction(this, true, 0.1f);
                }
                yield break;
            }
        }
    }
}
