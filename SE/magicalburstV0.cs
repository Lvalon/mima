//using LBoL.Base;
//using LBoL.ConfigData;
//using LBoL.Core.Battle;
//using LBoL.Core.Cards;
//using LBoL.Core;
//using LBoL.Core.StatusEffects;
//using LBoLEntitySideloader;
//using LBoLEntitySideloader.Attributes;
//using LBoLEntitySideloader.Entities;
//using LBoLEntitySideloader.Resource;
//using Mono.Cecil;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using UnityEngine;
//using static lvalonmima.BepinexPlugin;
//using static lvalonmima.SE.karmanationdef;
//using static lvalonmima.SE.mburstmodifiers.accumulationdef;
//using static lvalonmima.SE.mburstmodifiers.fastburstdef;
//using static lvalonmima.SE.mburstmodifiers.retributiondef;
//using static lvalonmima.SE.mburstmodifiers.everlastingmagicdef;
//using static lvalonmima.SE.mburstmodifiers.concentratedburstdef;
//using static lvalonmima.SE.mburstmodifiers.splitburstdef;
//using LBoL.Core.Battle.BattleActions;
//using LBoL.Base.Extensions;
//using LBoL.Core.Units;
//using LBoL.EntityLib.StatusEffects.Enemy;
//using System.Linq;
//using LBoL.EntityLib.StatusEffects.Cirno;
//using static UnityEngine.TouchScreenKeyboard;

//namespace lvalonmima.SE
//{
//    public sealed class magicalburstV0def : StatusEffectTemplate
//    {
//        public override IdContainer GetId()
//        {
//            return nameof(magicalburstV0);
//        }

//        public override LocalizationOption LoadLocalization()
//        {
//            return toolbox.locse();
//        }

//        public override Sprite LoadSprite()
//        {
//            return ResourceLoader.LoadSprite("semagicalburstV0.png", embeddedSource);
//        }

//        public override StatusEffectConfig MakeConfig()
//        {
//            var statusEffectConfig = new StatusEffectConfig(
//                Index: sequenceTable.Next(typeof(CardConfig)),
//                Id: "",
//                Order: 1,
//                Type: StatusEffectType.Special,
//                IsVerbose: true,
//                IsStackable: true,
//                StackActionTriggerLevel: null,
//                HasLevel: true,
//                LevelStackType: StackType.Add,
//                HasDuration: false,
//                DurationStackType: null,
//                DurationDecreaseTiming: DurationDecreaseTiming.Custom,
//                HasCount: true,
//                CountStackType: StackType.Keep,
//                LimitStackType: StackType.Keep,
//                ShowPlusByLimit: false,
//                Keywords: Keyword.None,
//                RelativeEffects: new List<string>() { },
//                VFX: "Default",
//                VFXloop: "Default",
//                SFX: "Default"
//            );
//            return statusEffectConfig;
//        }

//        [EntityLogic(typeof(magicalburstV0def))]
//        public sealed class magicalburstV0 : StatusEffect
//        {
//            int burstdmg = 0;
//            int burstdmglost = 0;
//            int truecount = 0;
//            int oldcount = 0;
//            int fastburstmodifier = 0;
//            float actualdmg = 0;
//            bool accumulationreal = false;
//            bool accrealthisturn = false;
//            bool fastburstreal = false;
//            bool retributionreal = false;
//            bool everlastingreal = false;
//            bool dealdmgletsgo = false;

//            public int burstdmgshow { get { return (Battle == null) ? 0 : Convert.ToInt32(burstdmg); } }
//            protected override void OnAdded(Unit unit)
//            {
//                //if (Owner.TryGetStatusEffect<everlastingmagic>(out var everlasting)) { everlastingreal = true; }
//                //if (Owner.TryGetStatusEffect<accumulation>(out var accumulation))
//                //{
//                //    if (Owner.TryGetStatusEffect<everlastingmagic>(out var everlasting2)) { }
//                //    else { accumulationreal = true; }
//                //}
//                //if (Owner.TryGetStatusEffect<fastburst>(out var fastburst)) { fastburstreal = true; }
//                //if (Owner.TryGetStatusEffect<retribution>(out var retribution)) { retributionreal = true; }
//                //Count = 0;
//                //burstdmgcalc();
//                //ReactOwnerEvent(base.Owner.StatusEffectAdded, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnStatusEffectAdded));
//                //ReactOwnerEvent(base.Owner.StatusEffectRemoved, new EventSequencedReactor<StatusEffectEventArgs>(this.OnStatusEffectRemoved));
//                //HandleOwnerEvent(base.Battle.CardUsing, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsing));
//                //ReactOwnerEvent(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
//                ReactOwnerEvent(Battle.AllEnemyTurnStarting, new EventSequencedReactor<GameEventArgs>(OnAllEnemyTurnStarting1));
//                base.ReactOwnerEvent<GameEventArgs>(base.Battle.AllEnemyTurnStarting, new EventSequencedReactor<GameEventArgs>(this.OnAllEnemyTurnStarting2));
//            }
//            private IEnumerable<BattleAction> OnStatusEffectRemoved(StatusEffectEventArgs args)
//            {
//                if (args.Effect is accumulation)
//                {
//                    this.NotifyChanged();
//                    accumulationreal = false;
//                    burstdmgcalc();
//                }
//                if (args.Effect is retribution)
//                {
//                    this.NotifyChanged();
//                    accumulationreal = false;
//                    burstdmgcalc();
//                }
//                if (args.Effect is everlastingmagic)
//                {
//                    if (Owner.TryGetStatusEffect<accumulation>(out var accumulation))
//                    { accumulationreal = true; }
//                    this.NotifyChanged();
//                    everlastingreal = false;
//                    burstdmgcalc();
//                }
//                yield break;
//            }
//            private IEnumerable<BattleAction> OnStatusEffectAdded(StatusEffectApplyEventArgs args)
//            {
//                if (args.Effect is accumulation)
//                {
//                    if (Owner.TryGetStatusEffect<everlastingmagic>(out var everlasting2)) { }
//                    else
//                    {
//                        this.NotifyChanged();
//                        accumulationreal = true;
//                        burstdmgcalc();
//                    }
//                }
//                if (args.Effect is retribution)
//                {
//                    this.NotifyChanged();
//                    retributionreal = true;
//                    burstdmgcalc();
//                }
//                if (args.Effect is everlastingmagic)
//                {
//                    this.NotifyChanged();
//                    everlastingreal = true;
//                    accumulationreal = false;
//                    burstdmgcalc();
//                }
//                if (args.Effect is magicalburstV0 || args.Effect is concentratedburst)
//                {
//                    this.NotifyChanged();
//                    burstdmgcalc();
//                }
//                yield break;
//            }
//            private void OnCardUsing(CardUsingEventArgs args)
//            {
//                if (Owner.TryGetStatusEffect<fastburst>(out var fastburst))
//                {
//                    int fastburstlvl = fastburst.Level;
//                    if (fastburstlvl >= 5) { fastburstmodifier = 100; }
//                    else { fastburstmodifier = fastburst.Level * 20; }
//                    fastburstreal = false;
//                    dealdmg(fastburstmodifier);
//                }
//            }
//            private IEnumerable<BattleAction> OnAllEnemyTurnStarting1(GameEventArgs args)
//            {
//                if (accumulationreal == true)
//                {
//                    if (Owner.TryGetStatusEffect<accumulation>(out var accumulation))
//                    {
//                        accumulation.Level -= 1;
//                        accrealthisturn = true;
//                        if (accumulation.Level == 0)
//                        {
//                            yield return new RemoveStatusEffectAction(accumulation, true);
//                            yield break;
//                        }
//                    }
//                }
//                if (accumulationreal == false)
//                {
//                    NotifyActivating();
//                    dealdmg(100);
//                }
//                yield break;
//            }
//            private void burstdmgcalc()
//            {
//                if (everlastingreal == false) { burstdmg += Level; }
//                if (everlastingreal == true)
//                {
//                    if (Owner.TryGetStatusEffect<everlastingmagic>(out var everlastingmagic))
//                    {
//                        int everlastinglvl = everlastingmagic.Level;
//                        if (everlastinglvl <= 5) { burstdmg += Convert.ToInt32(Level * everlastingmagic.Level / 5); }
//                    }
//                }
//                //check if burstdmg is decreased by self consumption
//                if (burstdmg < truecount && retributionreal == true)
//                {
//                    burstdmglost += truecount - burstdmg;
//                }

//                //check if count decreased manually: count updates burstdmg
//                if (Count < burstdmg && Level == 0 && truecount != burstdmg)
//                {
//                    burstdmg -= oldcount - Count;
//                    burstdmglost += oldcount - Count;
//                }

//                if (burstdmg > 999) { Count = 999; oldcount = 999; }
//                else { Count = burstdmg; oldcount = burstdmg; }
//                Level = 0;
//                truecount = burstdmg;
//            }
//            private void dealdmg(int usepercentage)
//            {
//                if (usepercentage > 0 && burstdmg > 0)
//                {
//                    NotifyActivating();
//                    actualdmg = usepercentage * burstdmg / 100;
//                    //yield return new DamageAction(base.Owner, Battle.AllAliveEnemies, DamageInfo.Attack(actualdmg, true), "鬼气狂澜", GunType.Middle);
//                    //React(new DamageAction(base.Owner, Battle.AllAliveEnemies, DamageInfo.Attack(actualdmg, true), "鬼气狂澜", GunType.Middle));

//                    dealdmgletsgo = true;
//                }
//            }
//            private IEnumerable<BattleAction> OnAllEnemyTurnStarting2(GameEventArgs args)
//            {
//                if (base.Owner == null || base.Battle.BattleShouldEnd)
//                {
//                    yield break;
//                }
//                if (dealdmgletsgo == true)
//                {
//                    if (Owner.TryGetStatusEffect<splitburst>(out var splitburstfirst))
//                    {
//                        if (Owner.TryGetStatusEffect<concentratedburst>(out var concentratedfirst)) { }
//                        else
//                        {
//                            int staticsplitlvl = splitburstfirst.Level;
//                            for (int i = splitburstfirst.Level; i >= 0; i--)
//                            {
//                                yield return new DamageAction(base.Owner, Battle.EnemyGroup.Alives, DamageInfo.Attack(Convert.ToInt32(actualdmg / (staticsplitlvl + 1)), true), "JunkoLunatic", GunType.Single);
//                                log.LogDebug(staticsplitlvl);
//                            }
//                        }

//                    }
//                    if (Owner.TryGetStatusEffect<concentratedburst>(out var concentratedburst))
//                    {
//                        int enemycount = 0;
//                        foreach (Unit target in base.Battle.AllAliveEnemies)
//                        {
//                            enemycount++;
//                        }
//                        int conmultiplied = enemycount * concentratedburst.Level;
//                        if (Owner.TryGetStatusEffect<splitburst>(out var splitburst))
//                        {
//                            int consplit = conmultiplied * (1 + splitburst.Level);
//                            int staticsplitlvl = splitburst.Level;
//                            for (int i = consplit; i > 0; i--)
//                            {
//                                yield return new DamageAction(base.Owner, Battle.EnemyGroup.Alives.SampleOrDefault(base.Battle.GameRun.BattleRng), DamageInfo.Attack(Convert.ToInt32(actualdmg / (staticsplitlvl + 1)), true), "JunkoLunaticHit", GunType.Single);
//                            }

//                        }
//                        else
//                        {
//                            for (int i = conmultiplied; i > 0; i--)
//                            {
//                                yield return new DamageAction(base.Owner, Battle.EnemyGroup.Alives.SampleOrDefault(base.Battle.GameRun.BattleRng), DamageInfo.Attack(actualdmg, true), "JunkoLunaticHit", GunType.Single);
//                            }
//                        }
//                    }
//                    else
//                    {
//                        if (Owner.TryGetStatusEffect<splitburst>(out var splitburst)) { }
//                        else { yield return new DamageAction(base.Owner, Battle.EnemyGroup.Alives, DamageInfo.Attack(actualdmg, true), "JunkoLunatic", GunType.Single); }
//                    }
//                }
//                dealdmgletsgo = false;
//                if (everlastingreal == false && accrealthisturn == false)
//                {
//                    burstdmg = 0;
//                }
//                burstdmgcalc();
//                //if (Owner.TryGetStatusEffect<retribution>(out var retribution))
//                //{
//                //retribution.Level = burstdmglost;
//                if (burstdmglost > 0) { yield return DamageAction.LoseLife(base.Owner, burstdmglost, "Cold2"); }
//                burstdmglost = 0;
//                //}
//                yield break;
//            }
//            private IEnumerable<BattleAction> OnCardUsed(GameEventArgs args)
//            {
//                if (base.Owner == null || base.Battle.BattleShouldEnd)
//                {
//                    yield break;
//                }
//                if (dealdmgletsgo == true)
//                {
//                    if (Owner.TryGetStatusEffect<splitburst>(out var splitburstfirst))
//                    {
//                        if (Owner.TryGetStatusEffect<concentratedburst>(out var concentratedfirst)) { }
//                        else
//                        {
//                            for (int i = splitburstfirst.Level; i > 0; i--)
//                            {
//                                yield return new DamageAction(base.Owner, Battle.EnemyGroup.Alives, DamageInfo.Attack(Convert.ToInt32((1 / splitburstfirst.Level + 1) * actualdmg), true), "JunkoLunatic", GunType.Single);
//                            }
//                        }

//                    }
//                    if (Owner.TryGetStatusEffect<concentratedburst>(out var concentratedburst))
//                    {
//                        int enemycount = 0;
//                        foreach (Unit target in base.Battle.AllAliveEnemies)
//                        {
//                            enemycount++;
//                        }
//                        int conmultiplied = enemycount * concentratedburst.Level;
//                        if (Owner.TryGetStatusEffect<splitburst>(out var splitburst))
//                        {
//                            int consplit = conmultiplied * splitburst.Level;
//                            for (int i = consplit; i > 0; i--)
//                            {
//                                yield return new DamageAction(base.Owner, Battle.EnemyGroup.Alives.SampleOrDefault(base.Battle.GameRun.BattleRng), DamageInfo.Attack(Convert.ToInt32((1 / splitburst.Level + 1) * actualdmg), true), "JunkoLunaticHit", GunType.Single);
//                            }

//                        }
//                        else
//                        {
//                            for (int i = conmultiplied; i > 0; i--)
//                            {
//                                yield return new DamageAction(base.Owner, Battle.EnemyGroup.Alives.SampleOrDefault(base.Battle.GameRun.BattleRng), DamageInfo.Attack(actualdmg, true), "JunkoLunaticHit", GunType.Single);
//                            }
//                        }
//                    }
//                    else
//                    {
//                        yield return new DamageAction(base.Owner, Battle.EnemyGroup.Alives, DamageInfo.Attack(actualdmg, true), "JunkoLunatic", GunType.Single);
//                    }
//                }
//                dealdmgletsgo = false;
//                burstdmg -= (int)actualdmg;
//                burstdmgcalc();
//                //if (Owner.TryGetStatusEffect<retribution>(out var retribution))
//                //{
//                //    retribution.Level = burstdmglost;
//                if (burstdmglost > 0) { yield return DamageAction.LoseLife(base.Owner, burstdmglost, "Cold2"); }
//                burstdmglost = 0;
//                //}
//                yield break;
//            }
//        }
//    }
//}