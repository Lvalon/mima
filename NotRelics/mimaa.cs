using System;
using System.Collections.Generic;
using System.Text;
using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using LBoLEntitySideloader.Attributes;
using static lvalonmima.BepinexPlugin;
using Mono.Cecil;
using UnityEngine;
using LBoL.Base;
using LBoL.EntityLib.Exhibits;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
using LBoL.Core;
using LBoL.Core.Units;
using static lvalonmima.SE.evilspiritdef;
using LBoL.Base.Extensions;
using LBoL.EntityLib.StatusEffects.Enemy;
using System.Linq;
using LBoL.Core.Randoms;
using static lvalonmima.SE.transcendeddef;
using static lvalonmima.SE.magicalburstdef;
using LBoL.Core.Stations;

namespace lvalonmima.NotRelics
{
    public sealed class mimaadef : ExhibitTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(mimaa);
        }

        public override LocalizationOption LoadLocalization()
        {
            return toolbox.locex();
        }

        public override ExhibitSprites LoadSprite()
        {
            // embedded resource folders are separated by a dot
            // this means exhibit image name must be exhibitid.png
            var folder = "Resources.";
            var exhibitSprites = new ExhibitSprites();
            Func<string, Sprite> wrap = (s) => ResourceLoader.LoadSprite(folder + GetId() + s + ".png", embeddedSource);
            exhibitSprites.main = wrap("");
            return exhibitSprites;
        }

        public override ExhibitConfig MakeConfig()
        {
            var exhibitConfig = new ExhibitConfig(
                Index: sequenceTable.Next(typeof(CardConfig)),
                Id: "",
                Order: 10,
                IsDebug: false,
                IsPooled: false,
                IsSentinel: false,
                Revealable: false,
                Appearance: AppearanceType.Nowhere,
                Owner: "Mima", //"Mima"
                LosableType: ExhibitLosableType.CantLose,
                Rarity: Rarity.Mythic,
                Value1: 3,
                Value2: 1,
                Value3: 2,
                Mana: new ManaGroup() { Philosophy = 1 },
                BaseManaRequirement: null,
                BaseManaColor: null,
                BaseManaAmount: 0,
                HasCounter: false,
                InitialCounter: null,
                Keywords: Keyword.Philosophy,
                RelativeEffects: new List<string>() { "evilspirit", "transcended" },
                RelativeCards: new List<string>() { }
            );
            return exhibitConfig;
        }

        [EntityLogic(typeof(mimaadef))]
        public sealed class mimaa : ShiningExhibit
        {
            private int oghp;
            private int ogmax;
            private int ogdmg;

            protected override void OnAdded(PlayerUnit player)
            {
                GameRunController gameRun = base.GameRun;
                int rewardAndShopCardColorLimitFlag = gameRun.RewardAndShopCardColorLimitFlag + 1;
                gameRun.RewardAndShopCardColorLimitFlag = rewardAndShopCardColorLimitFlag;
                //base.GameRun.AdditionalRewardCardCount -= base.Value2;
                //base.HandleGameRunEvent<StationEventArgs>(base.GameRun.StationRewardGenerating, delegate (StationEventArgs args)
                //{
                //    Station station = args.Station;
                //    if (station.Type == StationType.Enemy || station.Type == StationType.EliteEnemy || station.Type == StationType.Boss)
                //    {
                //        base.NotifyActivating();
                //        station.Rewards.Add(station.Stage.GetEnemyCardReward());
                //    }
                //});
            }
            protected override void OnEnterBattle()
            {
                base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
                base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageTaking));
                base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageReceived, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageReceived));
                base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
                base.ReactBattleEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
            }

            private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
            {
                if (base.Owner.IsInTurn && args.Card.CardType == CardType.Skill)
                {
                    base.NotifyActivating();
                    yield return new ApplyStatusEffectAction<magicalburst>(base.Owner, new int?(base.Value3), null, null, null, 0f, true);
                }
                yield break;
            }

            private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
            {
                base.NotifyActivating();
                yield return new ApplyStatusEffectAction<evilspirit>(base.Owner, new int?(base.Value1), null, null, null, 0f, true);
                yield break;
            }

            private void OnPlayerDamageTaking(DamageEventArgs args)
            {
                oghp = Owner.Hp;
                ogmax = Owner.MaxHp;
                ogdmg = args.DamageInfo.Damage.RoundToInt();
            }

            private void OnPlayerDamageReceived(DamageEventArgs args)
            {
                if (Owner.Hp == Owner.MaxHp && ogdmg > 0 && ((oghp - ogdmg != Owner.Hp - ogdmg) || ogdmg >= ogmax))
                {
                    base.NotifyActivating();
                    React(new ApplyStatusEffectAction<transcended>(base.Owner, new int?(base.Value2), null, null, null, 0f, true));
                }
            }

            private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
            {
                base.NotifyActivating();
                ManaGroup value = ManaGroup.Single(ManaColors.Colors.Sample(base.GameRun.BattleRng));
                yield return new GainManaAction(value);
                yield break;
            }
        }
    }
}
