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
using System;
using System.Collections.Generic;
using UnityEngine;
using static lvalonmima.BepinexPlugin;
using static lvalonmima.SE.mburstmodifiers.accumulationdef;
using static lvalonmima.SE.mburstmodifiers.fastburstdef;
using static lvalonmima.SE.mburstmodifiers.everlastingmagicdef;
using static lvalonmima.SE.mburstmodifiers.concentratedburstdef;
using static lvalonmima.SE.mburstmodifiers.splitburstdef;
using LBoL.Core.Battle.BattleActions;
using LBoL.Base.Extensions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;
using static lvalonmima.SE.extmpfiredef;

namespace lvalonmima.SE
{
    public sealed class magicalburstdef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(magicalburst);
        }

        public override LocalizationOption LoadLocalization() => sebatchloc.AddEntity(this);

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("semagicalburst.png", embeddedSource);
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
                HasCount: true,
                CountStackType: StackType.Keep,
                LimitStackType: StackType.Keep,
                ShowPlusByLimit: false,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { nameof(extmpfire) },
                VFX: "Default",
                VFXloop: "Default",
                SFX: "Default"
            );
            return statusEffectConfig;
        }

        [EntityLogic(typeof(magicalburstdef))]
        public sealed class magicalburst : mimaextensions.mimase
        {
            public int burstdmgshow { get { return (Battle == null) ? 0 : Convert.ToInt32(0); } }
            bool dealdmgletsgo = false;
            int fireneeded = 0;
            int counterlost = 0;
            int otcounter = 0;
            float actualdmg = 0;
            public magicalburst() : base()
            {
                truecounter = 0;
            }
            public override bool ForceNotShowNumber
            {
                get
                {
                    return true;
                }
            }
            protected override void OnAdded(Unit unit)
            {
                base.HandleOwnerEvent<StatusEffectApplyEventArgs>(base.Owner.StatusEffectAdding, new GameEventHandler<StatusEffectApplyEventArgs>(this.OnStatusEffectAdding));
                ReactOwnerEvent(Owner.StatusEffectAdded, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnStatusEffectAdded));
                ReactOwnerEvent(Owner.StatusEffectRemoved, new EventSequencedReactor<StatusEffectEventArgs>(this.OnStatusEffectRemoved));
                HandleOwnerEvent(Battle.CardUsing, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsing));
                ReactOwnerEvent(Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
                ReactOwnerEvent(Battle.AllEnemyTurnStarting, new EventSequencedReactor<GameEventArgs>(OnAllEnemyTurnStarting1));
                ReactOwnerEvent(Battle.AllEnemyTurnStarting, new EventSequencedReactor<GameEventArgs>(this.OnAllEnemyTurnStarting2));
            }
            private void OnStatusEffectAdding(StatusEffectApplyEventArgs args)
            {
                StatusEffect effect = args.Effect;
                if (effect is TempFirepower)
                {
                    fireneeded = effect.Level;
                    if (Owner.TryGetStatusEffect<MeilingAbilitySe>(out var tmp))
                    {
                        fireneeded *= 2;
                    }
                    base.NotifyActivating();
                    React(new ApplyStatusEffectAction<extmpfire>(base.Owner, new int?(fireneeded), null, null, null, 0f, true));
                    args.CancelBy(this);
                }
            }
            private IEnumerable<BattleAction> OnStatusEffectAdded(StatusEffectApplyEventArgs args)
            {
                //check if effect isMBmod == true
                if (args.Effect is mimaextensions.mimase mimase && mimase.isMBmod == true)
                {
                    this.NotifyChanged();
                    dmgcalc();
                }
                if (args.Effect.Id == "magicalburst")
                {
                    this.NotifyChanged();
                    dmgcalc();
                }
                if (Owner.TryGetStatusEffect<fastburst>(out var tmp) && tmp is mimaextensions.mimase fastburst)
                {
                    dealdmg(fastburst.Level * 20);
                }
                yield break;
            }
            private IEnumerable<BattleAction> OnStatusEffectRemoved(StatusEffectEventArgs args)
            {
                if (args.Effect is mimaextensions.mimase mimase && mimase.isMBmod == true)
                {
                    this.NotifyChanged();
                    dmgcalc();
                }
                yield break;
            }
            private void OnCardUsing(CardUsingEventArgs args)
            {
                //if (Owner.TryGetStatusEffect<fastburst>(out var tmp) && tmp is mimaextensions.mimase fastburst)
                //{
                //    dealdmg(fastburst.Level * 20);
                //}
            }
            private void dealdmg(int consume100)
            {
                if (consume100 > 0 && truecounter > 0)
                {
                    NotifyActivating();
                    actualdmg = consume100 * truecounter / 100;
                    dealdmgletsgo = true;
                }
            }
            private void dmgcalc()
            {
                //everlasting reduction
                if (Owner.TryGetStatusEffect<everlastingmagic>(out var everlastingmagic) && everlastingmagic.Level < 5)
                {
                    truecounter += Convert.ToInt32(Level * everlastingmagic.Level / 5);
                }
                else { truecounter += Level; }

                //check if counter is decreased
                if (truecounter < otcounter)
                {
                    counterlost += otcounter - truecounter;
                }

                if (truecounter > 999) { Count = 999; }
                else { Count = truecounter; }
                otcounter = truecounter;
                Level = 0;
                //truecounter = burstdmg;
            }
            private IEnumerable<BattleAction> OnAllEnemyTurnStarting1(GameEventArgs args)
            {
                if (Owner.TryGetStatusEffect<accumulation>(out var accumulation))
                {
                    accumulation.Level -= 1;
                    if (accumulation.Level == 0)
                    {
                        yield return new RemoveStatusEffectAction(accumulation, true);
                        yield break;
                    }
                }
                else if (truecounter > 0)
                {
                    NotifyActivating();
                    dealdmg(100);
                }
                yield break;
            }
            private IEnumerable<BattleAction> OnAllEnemyTurnStarting2(GameEventArgs args)
            {
                if (base.Owner == null || base.Battle.BattleShouldEnd)
                {
                    yield break;
                }
                if (dealdmgletsgo == true)
                {
                    if (Owner.TryGetStatusEffect<splitburst>(out var splitburstfirst))
                    {
                        if (Owner.TryGetStatusEffect<concentratedburst>(out var concentratedfirst)) { }
                        else
                        {
                            for (int i = splitburstfirst.Level; i >= 0; i--)
                            {
                                if (!Battle.BattleShouldEnd)
                                {
                                    yield return new DamageAction(base.Owner, Battle.EnemyGroup.Alives, DamageInfo.Attack(Convert.ToInt32(actualdmg / (splitburstfirst.Level + 1)), true), "JunkoLunatic", GunType.Single);
                                }
                            }
                        }
                    }
                    if (Owner.TryGetStatusEffect<concentratedburst>(out var concentratedburst))
                    {
                        int enemycount = 0;
                        foreach (Unit target in base.Battle.AllAliveEnemies)
                        {
                            enemycount++;
                        }
                        int conmultiplied = enemycount * concentratedburst.Level;
                        if (Owner.TryGetStatusEffect<splitburst>(out var splitburst))
                        {
                            int consplit = conmultiplied * (1 + splitburst.Level);
                            //idk why i did this static one, will remain here just in case
                            //int staticsplitlvl = splitburst.Level;
                            for (int i = consplit; i > 0; i--)
                            {
                                if (!Battle.BattleShouldEnd)
                                {
                                    yield return new DamageAction(base.Owner, Battle.EnemyGroup.Alives.SampleOrDefault(base.Battle.GameRun.BattleRng), DamageInfo.Attack(Convert.ToInt32(actualdmg / (splitburst.Level + 1)), true), "JunkoLunaticHit", GunType.Single);
                                }
                            }

                        }
                        else
                        {
                            for (int i = conmultiplied; i > 0; i--)
                            {
                                if (!Battle.BattleShouldEnd)
                                {
                                    yield return new DamageAction(base.Owner, Battle.EnemyGroup.Alives.SampleOrDefault(base.Battle.GameRun.BattleRng), DamageInfo.Attack(actualdmg, true), "JunkoLunaticHit", GunType.Single);
                                }
                            }
                        }
                    }
                    else if (Owner.TryGetStatusEffect<splitburst>(out var _)) { }
                    else
                    {
                        if (!Battle.BattleShouldEnd)
                        { yield return new DamageAction(base.Owner, Battle.EnemyGroup.Alives, DamageInfo.Attack(actualdmg, true), "JunkoLunatic", GunType.Single); }
                    }
                    if (Owner.TryGetStatusEffect<everlastingmagic>(out var tmp)) { }
                    else
                    {
                        truecounter -= (int)actualdmg;
                        actualdmg = 0;
                        yield return new ApplyStatusEffectAction<magicalburst>(Owner, 0, null, null, null, 0f, false);
                    }
                }
                dealdmgletsgo = false;
                dmgcalc();
                yield break;
            }
            private IEnumerable<BattleAction> OnCardUsed(GameEventArgs args)
            {
                if (base.Owner == null || base.Battle.BattleShouldEnd)
                {
                    yield break;
                }
                if (dealdmgletsgo == true)
                {
                    if (Owner.TryGetStatusEffect<splitburst>(out var splitburstfirst))
                    {
                        if (Owner.TryGetStatusEffect<concentratedburst>(out var concentratedfirst)) { }
                        else
                        {
                            for (int i = splitburstfirst.Level; i >= 0; i--)
                            {
                                if (!Battle.BattleShouldEnd)
                                {
                                    yield return new DamageAction(base.Owner, Battle.EnemyGroup.Alives, DamageInfo.Attack(Convert.ToInt32(actualdmg / (splitburstfirst.Level + 1)), true), "JunkoLunatic", GunType.Single);
                                }
                            }
                        }
                    }
                    if (Owner.TryGetStatusEffect<concentratedburst>(out var concentratedburst))
                    {
                        int enemycount = 0;
                        foreach (Unit target in base.Battle.AllAliveEnemies)
                        {
                            enemycount++;
                        }
                        int conmultiplied = enemycount * concentratedburst.Level;
                        if (Owner.TryGetStatusEffect<splitburst>(out var splitburst))
                        {
                            int consplit = conmultiplied * (1 + splitburst.Level);
                            //idk why i did this static one, will remain here just in case
                            //int staticsplitlvl = splitburst.Level;
                            for (int i = consplit; i > 0; i--)
                            {
                                if (!Battle.BattleShouldEnd)
                                {
                                    yield return new DamageAction(base.Owner, Battle.EnemyGroup.Alives.SampleOrDefault(base.Battle.GameRun.BattleRng), DamageInfo.Attack(Convert.ToInt32(actualdmg / (splitburst.Level + 1)), true), "JunkoLunaticHit", GunType.Single);
                                }
                            }

                        }
                        else
                        {
                            for (int i = conmultiplied; i > 0; i--)
                            {
                                if (!Battle.BattleShouldEnd)
                                {
                                    yield return new DamageAction(base.Owner, Battle.EnemyGroup.Alives.SampleOrDefault(base.Battle.GameRun.BattleRng), DamageInfo.Attack(actualdmg, true), "JunkoLunaticHit", GunType.Single);
                                }
                            }
                        }
                    }
                    else if (Owner.TryGetStatusEffect<splitburst>(out var _)) { }
                    else
                    {
                        if (!Battle.BattleShouldEnd)
                        { yield return new DamageAction(base.Owner, Battle.EnemyGroup.Alives, DamageInfo.Attack(actualdmg, true), "JunkoLunatic", GunType.Single); }
                    }
                    if (Owner.TryGetStatusEffect<everlastingmagic>(out var tmp2)) { }
                    else
                    {
                        truecounter -= (int)actualdmg;
                        actualdmg = 0;
                        yield return new ApplyStatusEffectAction<magicalburst>(Owner, 0, null, null, null, 0f, false);
                    }
                    dealdmgletsgo = false;
                    if (Owner.TryGetStatusEffect<fastburst>(out var effect))
                    {
                        yield return new RemoveStatusEffectAction(effect, true);
                    }
                }
                dmgcalc();
                yield break;
            }
        }
    }
}