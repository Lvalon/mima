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
using static MonoMod.Cil.RuntimeILReferenceBag.FastDelegateInvokers;

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
            return toolbox.locse();
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("sefastburst.png", embeddedSource);
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

        [EntityLogic(typeof(fastburstdef))]
        public sealed class fastburst : mimaextensions.mimase
        {
            public fastburst() : base()
            {
                isMBmod = true;
                truecounter = 0;
            }
            public int showfastburst
            {
                get
                {
                    if (GameRun == null) { return 20; }
                    else { return (Level > 5) ? Convert.ToInt32(5 * 20) : Convert.ToInt32(Level * 20); }
                }
            }
            bool holup = false;
            //set up triggers to give a fuck on
            //also vfx/sfx
            //they worked
            protected override void OnAdded(Unit unit)
            {
                //ReactOwnerEvent(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
                ReactOwnerEvent<StatusEffectApplyEventArgs>(base.Owner.StatusEffectAdded, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnStatusEffectAdded));
                if (Level > 5) { base.NotifyChanged(); Level = 5; }
            }

            //private IEnumerable<BattleAction> OnCardUsed(GameEventArgs args)
            //{
            //    if (Owner.TryGetStatusEffect<magicalburst>(out var effect))
            //    {
            //        NotifyActivating();
            //        yield return new RemoveStatusEffectAction(this, true);
            //    }
            //    yield break;
            //}

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
