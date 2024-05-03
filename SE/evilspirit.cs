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
using static lvalonmima.BepinexPlugin;
using static lvalonmima.SE.karmanationdef;
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

        public override LocalizationOption LoadLocalization() => sebatchloc.AddEntity(this);

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("seevilspirit.png", embeddedSource);
        }

        public override StatusEffectConfig MakeConfig()
        {
            var statusEffectConfig = new StatusEffectConfig(
                Index: sequenceTable.Next(typeof(CardConfig)),
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
                RelativeEffects: new List<string>() { "karmanation" },
                VFX: "Default",
                VFXloop: "Default",
                SFX: "Default"
            );
            return statusEffectConfig;
        }

        [EntityLogic(typeof(evilspiritdef))]
        public sealed class evilspirit : StatusEffect
        {
            bool thisround = false;
            //set up triggers to give a fuck on
            //also vfx/sfx
            //they worked
            protected override void OnAdded(Unit unit)
            {
                Count = 1;
                if (unit.Id == "Mima")
                {
                    if (unit.Hp > 66) { GameRun.SetHpAndMaxHp(66, 66, false); }
                    else { GameRun.SetHpAndMaxHp(unit.Hp, 66, false); }
                }
                ReactOwnerEvent(Battle.RoundEnding, new EventSequencedReactor<GameEventArgs>(OnRoundEnding));
                ReactOwnerEvent<StatusEffectApplyEventArgs>(base.Battle.Player.StatusEffectAdded, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnStatusEffectAdded));
                //base.HandleOwnerEvent<DamageEventArgs>(unit.DamageReceiving, new GameEventHandler<DamageEventArgs>(this.OnDamageReceiving));
                base.HandleOwnerEvent<DamageDealingEventArgs>(base.Owner.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
                //if (unit.Id != "Mima") { React(new ForceKillAction(Owner, Owner)); }
                //else
                //{
                    HandleOwnerEvent(Owner.Dying, new GameEventHandler<DieEventArgs>(OnDying));
                    base.HandleOwnerEvent<UnitEventArgs>(base.Owner.TurnStarting, new GameEventHandler<UnitEventArgs>(this.OnOwnerTurnStarting));
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
                        //React(new ApplyStatusEffectAction<DroneBlock>(Owner, 3));
                    }
                    else
                    {
                        React(PerformAction.Effect(Owner, "JunkoNightmare", 0f, "JunkoNightmare", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
                        GameRun.SetEnemyHpAndMaxHp(Owner.MaxHp, Owner.MaxHp, (EnemyUnit)Owner, false);
                        args.CancelBy(this);
                        //React(new ApplyStatusEffectAction<DroneBlock>(Owner, 3));
                    }
                    if (GameRun.Battle != null) { React(new ApplyStatusEffectAction<karmanation>(base.Owner, 1, null, null, null, 0f, true)); }
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
                    //GameRun.GainMaxHp(1, true, true);
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
                //if (Level == 0)
                //{
                //    NotifyActivating();
                //    yield return new ForceKillAction(Owner, Owner);
                //    yield return new RemoveStatusEffectAction(this, true);
                //}
                yield break;
            }
            //private void OnDamageReceiving(DamageEventArgs args)
            //{
            //    DamageInfo damageInfo = args.DamageInfo;
            //    if (damageInfo.DamageType == DamageType.Attack)
            //    {
            //        damageInfo.Damage = damageInfo.Amount * 10f;
            //        args.DamageInfo = damageInfo;
            //        args.AddModifier(this);
            //    }
            //}
            private void OnDamageDealing(DamageDealingEventArgs args)
            {
                DamageInfo damageInfo = args.DamageInfo;
                if (damageInfo.DamageType == DamageType.Attack)
                {
                    //if ((Owner.TryGetStatusEffect<transcended>(out var tmp) || Owner.TryGetStatusEffect<theabyss>(out var tmp2)) && args.ActionSource is StatusEffect statusEffect && (statusEffect is magicalburst))
                    //{
                    //    damageInfo.Damage = damageInfo.Amount * (0.5f);
                    //    args.DamageInfo = damageInfo;
                    //    args.AddModifier(this);
                    //}
                    //else
                    //{
                    damageInfo.Damage = damageInfo.Amount * (0.25f);
                    args.DamageInfo = damageInfo;
                    args.AddModifier(this);
                    //}
                }
                //if (args.DamageInfo.DamageType == DamageType.Attack)
                //{
                //    args.DamageInfo = args.DamageInfo.MultiplyBy(0.25f);
                //    args.AddModifier(this);
                //    if (args.Cause != ActionCause.OnlyCalculate)
                //    {
                //        base.NotifyActivating();
                //    }
                //}
            }
        }
    }
}
