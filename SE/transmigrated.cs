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

namespace lvalonmima.SE
{
    public sealed class transmigrateddef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(transmigrated);
        }

        public override LocalizationOption LoadLocalization()
        {
            return toolbox.locse();
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("setransmigrated.png", embeddedSource);
        }

        public override StatusEffectConfig MakeConfig()
        {
            var statusEffectConfig = new StatusEffectConfig(
                Index: 0,
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

        [EntityLogic(typeof(transmigrateddef))]
        public sealed class transmigrated : StatusEffect
        {
            public int Value
            {
                get
                {
                    GameRunController gameRun = base.GameRun;
                    if (gameRun == null)
                    {
                        return 50;
                    }
                    return 50;// + ((base.Owner is PlayerUnit) ? gameRun.PlayerVulnerableExtraPercentage : gameRun.EnemyVulnerableExtraPercentage);
                }
            }

            //set up triggers to give a fuck on
            //they worked
            protected override void OnAdded(Unit unit)
            {
                HandleOwnerEvent<DamageDealingEventArgs>(unit.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
                HandleOwnerEvent<DamageEventArgs>(unit.DamageReceiving, new GameEventHandler<DamageEventArgs>(this.OnDamageReceiving));
                ReactOwnerEvent(Battle.RoundEnding, new EventSequencedReactor<GameEventArgs>(OnRoundEnding));
                React(PerformAction.Effect(unit, "JinHua", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
            }

            private void OnDamageDealing(DamageDealingEventArgs args)
            {
                if (args.DamageInfo.DamageType == DamageType.Attack)
                {
                    args.DamageInfo = args.DamageInfo.IncreaseBy(5);
                    args.AddModifier(this);
                }
            }

            private void OnDamageReceiving(DamageEventArgs args)
            {
                DamageInfo damageInfo = args.DamageInfo;
                if (damageInfo.DamageType == DamageType.Attack)
                {
                    damageInfo.Damage = damageInfo.Amount * (1f + (float)this.Value / 100f);
                    args.DamageInfo = damageInfo;
                    args.AddModifier(this);
                }
            }

            private IEnumerable<BattleAction> OnRoundEnding(GameEventArgs args)
            {
                int num = Level - 1;
                Level = num;
                if (Level == 0)
                {
                    NotifyActivating();
                    yield return new RemoveStatusEffectAction(this, true);
                    yield break;
                }
                yield break;
            }
        }
    }
}
