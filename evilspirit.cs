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

namespace lvalonmima
{
    public sealed class evilspiritdef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(evilspirit);
        }

        public override LocalizationOption LoadLocalization()
        {
            var loc = new GlobalLocalization(BepinexPlugin.embeddedSource);
            loc.LocalizationFiles.AddLocaleFile(LBoL.Core.Locale.En, "SEEn.yaml");
            return loc;
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("seevilspirit.png", embeddedSource);
        }

        public override StatusEffectConfig MakeConfig()
        {
            var statusEffectConfig = new StatusEffectConfig(
                Index: 8745,
                Id: "",
                Order: 1,
                Type: StatusEffectType.Positive,
                IsVerbose: true,
                IsStackable: true,
                StackActionTriggerLevel: null,
                HasLevel: true,
                LevelStackType: StackType.Add,
                HasDuration: false,
                DurationStackType: StackType.Add,
                DurationDecreaseTiming: DurationDecreaseTiming.Custom,
                HasCount: false,
                CountStackType: StackType.Keep,
                LimitStackType: StackType.Keep,
                ShowPlusByLimit: false,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { "DroneBlock" },
                VFX: "Default",
                VFXloop: "Default",
                SFX: "Default"
            );
            return statusEffectConfig;
        }

        [EntityLogic(typeof(evilspiritdef))]
        public sealed class evilspirit : StatusEffect
        {

            //set up triggers to give a fuck on
            //also vfx/sfx
            //they worked
            protected override void OnAdded(Unit unit)
            {
                base.HandleOwnerEvent<DieEventArgs>(base.Owner.Dying, new GameEventHandler<DieEventArgs>(this.OnDying));
                base.ReactOwnerEvent<GameEventArgs>(base.Battle.RoundEnding, new EventSequencedReactor<GameEventArgs>(this.OnRoundEnding));
                base.ReactOwnerEvent<GameEventArgs>(base.Battle.BattleEnding, new EventSequencedReactor<GameEventArgs>(this.OnBattleEnding));
                this.React(PerformAction.Effect(unit, "JunkoNightmare", 0f, "JunkoNightmare", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
                this.React(PerformAction.Effect(unit, "JunkoNightmare", 1f, "", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
                this.React(PerformAction.Effect(unit, "JunkoNightmare", 2f, "", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
                this.React(PerformAction.Effect(unit, "JinziMirror", 3f, "", 1f, PerformAction.EffectBehavior.Add, 0f));
            }

            //heal to full, gain 1 level 3 defense matrix on death
            //IT WORKED LETS FUCKING GO
            private void OnDying(DieEventArgs args)
            {
                if (args.ActionSource != this)
                {
                    base.NotifyActivating();
                    //int num = args.Unit.MaxHp;
                    if (Battle.Player.BaseName == Owner.BaseName)
                    {
                        base.GameRun.SetHpAndMaxHp(base.Owner.MaxHp, base.Owner.MaxHp, false);
                        args.CancelBy(this);
                        this.React(new ApplyStatusEffectAction<DroneBlock>(this.Owner, 3));
                    }
                    else
                    {
                        base.GameRun.SetEnemyHpAndMaxHp(base.Owner.MaxHp, base.Owner.MaxHp, (EnemyUnit)Owner, false);
                        args.CancelBy(this);
                        this.React(new ApplyStatusEffectAction<DroneBlock>(this.Owner, 3));
                    }
                    if (base.GameRun.Battle != null)
                    {
                        base.Level++;
                    }
                }
                return;
            }

            //round end lose level, kill if level=0
            //it works
            private IEnumerable<BattleAction> OnRoundEnding(GameEventArgs args)
            {
                if (base.IsAutoDecreasing)
                {
                    int num = base.Level - 1;
                    base.Level = num;
                    if (base.Level == 0)
                    {
                        base.NotifyActivating();
                        yield return new ForceKillAction(base.Owner, base.Owner);
                        yield return new RemoveStatusEffectAction(this, true);
                        //args.ActionSource != this
                        yield break;
                    }
                }
                else
                {
                    base.IsAutoDecreasing = true;
                }
                yield break;
            }
            //heal to full after battle
            //it works
            private IEnumerable<BattleAction> OnBattleEnding(GameEventArgs args)
            {
                if (base.Owner.IsAlive)
                {
                    base.NotifyActivating();
                    yield return new HealAction(base.Owner, base.Owner, base.Owner.MaxHp, HealType.Normal, 0.2f);
                }
                yield break;
            }
        }
    }
}
