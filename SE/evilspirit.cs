using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using UnityEngine;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace lvalonmima.SE
{
    public sealed class evilspiritdef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(evilspirit);
        }

        public override LocalizationOption LoadLocalization()
        {
            return BepinexPlugin.sebatchloc.AddEntity(this);
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("seevilspirit.png", BepinexPlugin.embeddedSource);
        }

        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig statusEffectConfig = new StatusEffectConfig(
                Index: BepinexPlugin.sequenceTable.Next(typeof(CardConfig)),
                Id: "",
                Order: 20,
                Type: StatusEffectType.Special,
                IsVerbose: true,
                IsStackable: true,
                StackActionTriggerLevel: null,
                HasLevel: true,
                LevelStackType: StackType.Add,
                HasDuration: false,
                DurationStackType: null,
                DurationDecreaseTiming: DurationDecreaseTiming.Custom,
                HasCount: true,
                CountStackType: StackType.Keep,
                LimitStackType: StackType.Keep,
                ShowPlusByLimit: false,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { nameof(karmanationdef.karmanation), nameof(grudgetoldef.grudgetol) },
                VFX: "Default",
                VFXloop: "Default",
                SFX: "Default"
            );
            return statusEffectConfig;
        }

        [EntityLogic(typeof(evilspiritdef))]
        public sealed class evilspirit : StatusEffect
        {
            private bool thisround = false;
            //set up triggers to give a fuck on
            //also vfx/sfx
            //they worked
            protected override void OnAdded(Unit unit)
            {
                Count = 1;
                if (unit.Hp > 66) { GameRun.SetHpAndMaxHp(66, 66, false); }
                else { GameRun.SetHpAndMaxHp(unit.Hp, 66, false); }
                ReactOwnerEvent(Battle.RoundEnding, new EventSequencedReactor<GameEventArgs>(OnRoundEnding));
                ReactOwnerEvent<StatusEffectApplyEventArgs>(Battle.Player.StatusEffectAdded, new EventSequencedReactor<StatusEffectApplyEventArgs>(OnStatusEffectAdded));
                HandleOwnerEvent(Owner.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(OnDamageDealing));
                HandleOwnerEvent(Owner.Dying, new GameEventHandler<DieEventArgs>(OnDying));
                HandleOwnerEvent(Owner.TurnStarting, new GameEventHandler<UnitEventArgs>(OnOwnerTurnStarting));
                ReactOwnerEvent(Battle.BattleEnding, new EventSequencedReactor<GameEventArgs>(OnBattleEnding));
                React(PerformAction.Effect(unit, "JunkoNightmare", 0f, "JunkoNightmare", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
                React(PerformAction.Effect(unit, "JunkoNightmare", 1f, "", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
                React(PerformAction.Effect(unit, "JunkoNightmare", 2f, "", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
                React(PerformAction.Effect(unit, "JinziMirror", 3f, "", 1f, PerformAction.EffectBehavior.Add, 0f));
                //}
            }
            private void OnDying(DieEventArgs args)
            {
                if (args.ActionSource != this)
                {
                    NotifyActivating();
                    React(PerformAction.Effect(Owner, "JinziMirror", 3f, "", 1f, PerformAction.EffectBehavior.Add, 0f));
                    if (Battle.Player.BaseName == Owner.BaseName)
                    {
                        React(PerformAction.Effect(Owner, "JunkoNightmare", 0f, "JunkoNightmare", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
                        GameRun.SetHpAndMaxHp(Owner.MaxHp, Owner.MaxHp, false);
                        args.CancelBy(this);
                    }
                    else
                    {
                        React(PerformAction.Effect(Owner, "JunkoNightmare", 0f, "JunkoNightmare", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
                        GameRun.SetEnemyHpAndMaxHp(Owner.MaxHp, Owner.MaxHp, (EnemyUnit)Owner, false);
                        args.CancelBy(this);
                    }
                    if (GameRun.Battle != null)
                    {
                        React(new ApplyStatusEffectAction<karmanationdef.karmanation>(Owner, 1, null, null, null, 0f, true));
                        React(new ApplyStatusEffectAction<grudgetoldef.grudgetol>(Owner, 1, null, null, null, 0f, true));
                    }
                    if (GameRun.Battle != null && thisround == false)
                    {
                        Level++;
                        thisround = true;
                        Count = 0;
                    }
                }
                return;
            }

            //round end lose level, kill if level=0
            //it works
            private IEnumerable<BattleAction> OnRoundEnding(GameEventArgs args)
            {
                thisround = false;
                Count = 1;
                int num = Level - 1;
                Level = num;
                if (Level == 0)
                {
                    NotifyActivating();
                    yield return new ForceKillAction(Owner, Owner);
                    Level++;
                    yield break;
                }
                yield break;
            }
            //heal to full after battle
            //it works
            private IEnumerable<BattleAction> OnBattleEnding(GameEventArgs args)
            {
                if (Owner.IsAlive)
                {
                    NotifyActivating();
                    yield return new HealAction(Owner, Owner, Owner.MaxHp, HealType.Normal, 0.2f);
                }
                yield break;
            }

            //effect readd
            private void OnOwnerTurnStarting(UnitEventArgs args)
            {
                React(PerformAction.Effect(Owner, "JinziMirror", 3f, "", 1f, PerformAction.EffectBehavior.Add, 0f));
            }

            private IEnumerable<BattleAction> OnStatusEffectAdded(StatusEffectApplyEventArgs args)
            {
                yield break;
            }
            private void OnDamageDealing(DamageDealingEventArgs args)
            {
                DamageInfo damageInfo = args.DamageInfo;
                if (damageInfo.DamageType == DamageType.Attack)
                {
                    damageInfo.Damage = damageInfo.Amount * (0.25f);
                    args.DamageInfo = damageInfo;
                    args.AddModifier(this);
                }
            }
        }
    }
}
