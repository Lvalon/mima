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
using lvalonmima.SE.mburstmodifiers;
using LBoL.Core.Battle.BattleActions;
using LBoL.Base.Extensions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;

namespace lvalonmima.SE
{
    public sealed class magicalburstdef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(magicalburst);
        }

        public override LocalizationOption LoadLocalization()
        {
            return BepinexPlugin.sebatchloc.AddEntity(this);
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("semagicalburst.png", BepinexPlugin.embeddedSource);
        }

        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig statusEffectConfig = new StatusEffectConfig(
                Index: BepinexPlugin.sequenceTable.Next(typeof(CardConfig)),
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
                RelativeEffects: new List<string>() { nameof(extmpfiredef.extmpfire) },
                VFX: "Default",
                VFXloop: "Default",
                SFX: "Default",
                ImageId: null
            );
            return statusEffectConfig;
        }

        [EntityLogic(typeof(magicalburstdef))]
        public sealed class magicalburst : mimaextensions.mimase
        {
            public int burstdmgshow => (Battle == null) ? 0 : Convert.ToInt32(0);

            private bool dealdmgletsgo = false;
            private int fireneeded = 0;
            private int counterlost = 0;
            private int otcounter = 0;
            private float actualdmg = 0;
            public magicalburst() : base()
            {
                truecounter = 0;
            }
            public override bool ForceNotShowDownText => true;
            protected override void OnAdded(Unit unit)
            {
                HandleOwnerEvent(Owner.StatusEffectAdding, new GameEventHandler<StatusEffectApplyEventArgs>(OnStatusEffectAdding));
                ReactOwnerEvent(Owner.StatusEffectAdded, new EventSequencedReactor<StatusEffectApplyEventArgs>(OnStatusEffectAdded));
                ReactOwnerEvent(Owner.StatusEffectRemoved, new EventSequencedReactor<StatusEffectEventArgs>(OnStatusEffectRemoved));
                HandleOwnerEvent(Battle.CardUsing, new GameEventHandler<CardUsingEventArgs>(OnCardUsing));
                ReactOwnerEvent(Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(OnCardUsed));
                ReactOwnerEvent(Battle.AllEnemyTurnStarting, new EventSequencedReactor<GameEventArgs>(OnAllEnemyTurnStarting1));
                ReactOwnerEvent(Battle.AllEnemyTurnStarting, new EventSequencedReactor<GameEventArgs>(OnAllEnemyTurnStarting2));
            }
            private void OnStatusEffectAdding(StatusEffectApplyEventArgs args)
            {
                StatusEffect effect = args.Effect;
                if (effect is TempFirepower)
                {
                    fireneeded = effect.Level;
                    if (Owner.TryGetStatusEffect(out MeilingAbilitySe tmp))
                    {
                        fireneeded *= 2;
                    }
                    NotifyActivating();
                    React(new ApplyStatusEffectAction<extmpfiredef.extmpfire>(Owner, new int?(fireneeded), null, null, null, 0f, true));
                    args.CancelBy(this);
                }
            }
            private IEnumerable<BattleAction> OnStatusEffectAdded(StatusEffectApplyEventArgs args)
            {
                //check if effect isMBmod == true
                if (args.Effect is mimaextensions.mimase mimase && MBmods.Contains(mimase.Id))
                {
                    NotifyChanged();
                    dmgcalc();
                }
                if (args.Effect.Id == "magicalburst")
                {
                    NotifyChanged();
                    dmgcalc();
                }
                if (Owner.TryGetStatusEffect(out fastburstdef.fastburst tmp) && tmp is mimaextensions.mimase fastburst)
                {
                    dealdmg(fastburst.Level * 20);
                }
                yield break;
            }
            private IEnumerable<BattleAction> OnStatusEffectRemoved(StatusEffectEventArgs args)
            {
                if (args.Effect is mimaextensions.mimase mimase && MBmods.Contains(mimase.Id))
                {
                    NotifyChanged();
                    dmgcalc();
                }
                yield break;
            }
            private void OnCardUsing(CardUsingEventArgs args)
            {
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
                if (Owner.TryGetStatusEffect(out everlastingmagicdef.everlastingmagic everlastingmagic) && everlastingmagic.Level < 5)
                {
                    if (everlastingmagic.Level == 1) {
                        truecounter += Convert.ToInt32(Math.Round(Level * 0.5, MidpointRounding.AwayFromZero)); //fucking round up if .5 fu
                    }
                    else {
                        truecounter += Level;
                    }
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
            }
            private IEnumerable<BattleAction> OnAllEnemyTurnStarting1(GameEventArgs args)
            {
                if (Owner.TryGetStatusEffect(out accumulationdef.accumulation accumulation))
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
                if (Owner == null || Battle.BattleShouldEnd)
                {
                    yield break;
                }
                if (dealdmgletsgo == true)
                {
                    if (Owner.TryGetStatusEffect(out splitburstdef.splitburst splitburstfirst))
                    {
                        if (Owner.TryGetStatusEffect(out concentratedburstdef.concentratedburst concentratedfirst)) { }
                        else
                        {
                            for (int i = splitburstfirst.Level; i >= 0; i--)
                            {
                                if (!Battle.BattleShouldEnd)
                                {
                                    yield return new DamageAction(Owner, Battle.EnemyGroup.Alives, DamageInfo.Attack(Convert.ToInt32(actualdmg / (splitburstfirst.Level + 1)), true), "JunkoLunatic", GunType.Single);
                                }
                            }
                        }
                    }
                    if (Owner.TryGetStatusEffect(out concentratedburstdef.concentratedburst concentratedburst))
                    {
                        int enemycount = 0;
                        foreach (Unit target in Battle.AllAliveEnemies)
                        {
                            enemycount++;
                        }
                        int conmultiplied = enemycount * concentratedburst.Level;
                        if (Owner.TryGetStatusEffect(out splitburstdef.splitburst splitburst))
                        {
                            int consplit = conmultiplied * (1 + splitburst.Level);
                            //idk why i did this static one, will remain here just in case
                            //int staticsplitlvl = splitburst.Level;
                            for (int i = consplit; i > 0; i--)
                            {
                                if (!Battle.BattleShouldEnd)
                                {
                                    yield return new DamageAction(Owner, Battle.EnemyGroup.Alives.SampleOrDefault(Battle.GameRun.BattleRng), DamageInfo.Attack(Convert.ToInt32(actualdmg / (splitburst.Level + 1)), true), "JunkoLunaticHit", GunType.Single);
                                }
                            }

                        }
                        else
                        {
                            for (int i = conmultiplied; i > 0; i--)
                            {
                                if (!Battle.BattleShouldEnd)
                                {
                                    yield return new DamageAction(Owner, Battle.EnemyGroup.Alives.SampleOrDefault(Battle.GameRun.BattleRng), DamageInfo.Attack(actualdmg, true), "JunkoLunaticHit", GunType.Single);
                                }
                            }
                        }
                    }
                    else if (Owner.TryGetStatusEffect(out splitburstdef.splitburst _)) { }
                    else
                    {
                        if (!Battle.BattleShouldEnd)
                        { yield return new DamageAction(Owner, Battle.EnemyGroup.Alives, DamageInfo.Attack(actualdmg, true), "JunkoLunatic", GunType.Single); }
                    }
                    if (Owner.TryGetStatusEffect(out everlastingmagicdef.everlastingmagic tmp)) { }
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
                if (Owner == null || Battle.BattleShouldEnd)
                {
                    yield break;
                }
                if (dealdmgletsgo == true)
                {
                    if (Owner.TryGetStatusEffect(out splitburstdef.splitburst splitburstfirst))
                    {
                        if (Owner.TryGetStatusEffect(out concentratedburstdef.concentratedburst concentratedfirst)) { }
                        else
                        {
                            for (int i = splitburstfirst.Level; i >= 0; i--)
                            {
                                if (!Battle.BattleShouldEnd)
                                {
                                    yield return new DamageAction(Owner, Battle.EnemyGroup.Alives, DamageInfo.Attack(Convert.ToInt32(actualdmg / (splitburstfirst.Level + 1)), true), "JunkoLunatic", GunType.Single);
                                }
                            }
                        }
                    }
                    if (Owner.TryGetStatusEffect(out concentratedburstdef.concentratedburst concentratedburst))
                    {
                        int enemycount = 0;
                        foreach (Unit target in Battle.AllAliveEnemies)
                        {
                            enemycount++;
                        }
                        int conmultiplied = enemycount * concentratedburst.Level;
                        if (Owner.TryGetStatusEffect(out splitburstdef.splitburst splitburst))
                        {
                            int consplit = conmultiplied * (1 + splitburst.Level);
                            //idk why i did this static one, will remain here just in case
                            //int staticsplitlvl = splitburst.Level;
                            for (int i = consplit; i > 0; i--)
                            {
                                if (!Battle.BattleShouldEnd)
                                {
                                    yield return new DamageAction(Owner, Battle.EnemyGroup.Alives.SampleOrDefault(Battle.GameRun.BattleRng), DamageInfo.Attack(Convert.ToInt32(actualdmg / (splitburst.Level + 1)), true), "JunkoLunaticHit", GunType.Single);
                                }
                            }

                        }
                        else
                        {
                            for (int i = conmultiplied; i > 0; i--)
                            {
                                if (!Battle.BattleShouldEnd)
                                {
                                    yield return new DamageAction(Owner, Battle.EnemyGroup.Alives.SampleOrDefault(Battle.GameRun.BattleRng), DamageInfo.Attack(actualdmg, true), "JunkoLunaticHit", GunType.Single);
                                }
                            }
                        }
                    }
                    else if (Owner.TryGetStatusEffect(out splitburstdef.splitburst _)) { }
                    else
                    {
                        if (!Battle.BattleShouldEnd)
                        { yield return new DamageAction(Owner, Battle.EnemyGroup.Alives, DamageInfo.Attack(actualdmg, true), "JunkoLunatic", GunType.Single); }
                    }
                    if (Owner.TryGetStatusEffect(out everlastingmagicdef.everlastingmagic tmp2)) { }
                    else
                    {
                        truecounter -= (int)actualdmg;
                        actualdmg = 0;
                        yield return new ApplyStatusEffectAction<magicalburst>(Owner, 0, null, null, null, 0f, false);
                    }
                    dealdmgletsgo = false;
                    if (Owner.TryGetStatusEffect(out fastburstdef.fastburst effect))
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