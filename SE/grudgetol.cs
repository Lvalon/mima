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
    public sealed class grudgetoldef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(grudgetol);
        }

        public override LocalizationOption LoadLocalization()
        {
            return BepinexPlugin.sebatchloc.AddEntity(this);
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("segrudgetol.png", BepinexPlugin.embeddedSource);
        }

        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig statusEffectConfig = new StatusEffectConfig(
                Index: BepinexPlugin.sequenceTable.Next(typeof(CardConfig)),
                Id: "",
                Order: 999,
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

        [EntityLogic(typeof(grudgetoldef))]
        public sealed class grudgetol : StatusEffect
        {
            private bool thisround = false;
            public int loselife => GameRun == null ? 2 : (Level >= 27) ? Convert.ToInt32(Math.Pow(2, 27)) : Convert.ToInt32(Math.Pow(2, Level));
            private float loselifeinternal => GameRun == null
                        ? 50
                        : (Level >= 27) ? 100 - (100 / Convert.ToInt32(Math.Pow(2, 27))) : 100 - (100 / Convert.ToInt32(Math.Pow(2, Level)));
            protected override void OnAdded(Unit unit)
            {
                HandleOwnerEvent(Owner.DamageReceiving, new GameEventHandler<DamageEventArgs>(OnDamageReceiving));
                ReactOwnerEvent(Owner.StatusEffectAdded, new EventSequencedReactor<StatusEffectApplyEventArgs>(OnStatusEffectAdded));
                ReactOwnerEvent(Battle.RoundEnding, new EventSequencedReactor<GameEventArgs>(OnRoundEnding));
                ReactOwnerEvent(Owner.TurnStarting, new EventSequencedReactor<UnitEventArgs>(OnTurnStarting));
            }
            private void OnDamageReceiving(DamageEventArgs args)
            {
                DamageInfo damageInfo = args.DamageInfo;
                damageInfo.Damage = damageInfo.Amount * ((100f - loselifeinternal) / 100f);
                args.DamageInfo = damageInfo;
                args.AddModifier(this);
            }
            private IEnumerable<BattleAction> OnStatusEffectAdded(StatusEffectApplyEventArgs args)
            {
                if (args.Effect.Id == nameof(karmanationdef.karmanation))
                {
                    thisround = true;
                }
                BepinexPlugin.log.LogDebug(args.Effect.Id);
                BepinexPlugin.log.LogDebug(nameof(karmanationdef.karmanation));
                yield break;
            }
            private IEnumerable<BattleAction> OnTurnStarting(GameEventArgs args)
            {
                thisround = false;
                yield break;
            }
            private IEnumerable<BattleAction> OnRoundEnding(GameEventArgs args)
            {
                if (!thisround)
                {
                    int num = Level - 1;
                    Level = num;
                    if (Level == 0)
                    {
                        NotifyActivating();
                        yield return new RemoveStatusEffectAction(this, true);
                    }
                }
                yield break;
            }
        }
    }
}
