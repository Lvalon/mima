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
using LBoL.Core.Battle.BattleActions;
using LBoL.Base.Extensions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;
using System.Linq;
using LBoL.Core.Battle.Interactions;
using LBoL.Presentation.UI.Panels;

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
            return toolbox.locse();
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("seextmpfire.png", embeddedSource);
        }

        public override StatusEffectConfig MakeConfig()
        {
            var statusEffectConfig = new StatusEffectConfig(
                Index: 0,
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
                SFX: "Default"
            );
            return statusEffectConfig;
        }

        [EntityLogic(typeof(extmpfiredef))]
        public sealed class extmpfire : StatusEffect
        {
            //set up triggers to give a fuck on
            //they worked
            protected override void OnAdded(Unit unit)
            {
                base.HandleOwnerEvent<DamageDealingEventArgs>(unit.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
                ReactOwnerEvent(Battle.RoundEnded, new EventSequencedReactor<GameEventArgs>(OnRoundEnded));
            }
            private void OnDamageDealing(DamageDealingEventArgs args)
            {
                if (args.DamageInfo.DamageType == DamageType.Attack)
                {
                    args.DamageInfo = args.DamageInfo.IncreaseBy(base.Level);
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
