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
    public sealed class evilspiritdef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(evilspirit);
        }

        public override LocalizationOption LoadLocalization()
        {
            //var loc = new GlobalLocalization(BepinexPlugin.embeddedSource);
            //loc.LocalizationFiles.AddLocaleFile(LBoL.Core.Locale.En, "SEEn.yaml");
            //return loc;
            return toolbox.locse();
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("seevilspirit.png", embeddedSource);
        }

        public override StatusEffectConfig MakeConfig()
        {
            var statusEffectConfig = new StatusEffectConfig(
                Index: 87450,
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
                HandleOwnerEvent(Owner.Dying, new GameEventHandler<DieEventArgs>(OnDying));
                ReactOwnerEvent(Battle.RoundEnding, new EventSequencedReactor<GameEventArgs>(OnRoundEnding));
                ReactOwnerEvent(Battle.BattleEnding, new EventSequencedReactor<GameEventArgs>(OnBattleEnding));
                React(PerformAction.Effect(unit, "JunkoNightmare", 0f, "JunkoNightmare", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
                React(PerformAction.Effect(unit, "JunkoNightmare", 1f, "", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
                React(PerformAction.Effect(unit, "JunkoNightmare", 2f, "", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
                React(PerformAction.Effect(unit, "JinziMirror", 3f, "", 1f, PerformAction.EffectBehavior.Add, 0f));
            }

            //heal to full, gain 1 level 3 defense matrix on death
            //IT WORKED LETS FUCKING GO
            private void OnDying(DieEventArgs args)
            {
                if (args.ActionSource != this)
                {
                    NotifyActivating();
                    if (Battle.Player.BaseName == Owner.BaseName)
                    {
                        React(PerformAction.Effect(Owner, "JunkoNightmare", 0f, "JunkoNightmare", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
                        GameRun.SetHpAndMaxHp(Owner.MaxHp, Owner.MaxHp, false);
                        args.CancelBy(this);
                        React(new ApplyStatusEffectAction<DroneBlock>(Owner, 3));
                    }
                    else
                    {
                        React(PerformAction.Effect(Owner, "JunkoNightmare", 0f, "JunkoNightmare", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
                        GameRun.SetEnemyHpAndMaxHp(Owner.MaxHp, Owner.MaxHp, (EnemyUnit)Owner, false);
                        args.CancelBy(this);
                        React(new ApplyStatusEffectAction<DroneBlock>(Owner, 3));
                    }
                    if (GameRun.Battle != null)
                    {
                        Level++;
                    }
                }
                return;
            }

            //round end lose level, kill if level=0
            //it works
            private IEnumerable<BattleAction> OnRoundEnding(GameEventArgs args)
            {
                int num = Level - 1;
                Level = num;
                if (Level == 0)
                {
                    NotifyActivating();
                    yield return new ForceKillAction(Owner, Owner);
                    yield return new RemoveStatusEffectAction(this, true);
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
        }
    }
}
