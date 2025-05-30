﻿using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Randoms;

namespace lvalonmima.NotImages.Uncommon
{
    public sealed class cardmarisaskilldef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardmarisaskill);
        }

        public override CardImages LoadCardImages()
        {
            CardImages imgs = new CardImages(BepinexPlugin.embeddedSource);
            imgs.AutoLoad(this, extension: ".png");
            return imgs;
        }

        public override LocalizationOption LoadLocalization()
        {
            return BepinexPlugin.cardbatchloc.AddEntity(this);
        }

        public override CardConfig MakeConfig()
        {
            CardConfig cardConfig = new CardConfig(
               Index: BepinexPlugin.sequenceTable.Next(typeof(CardConfig)),
               Id: "",
               Order: 10,
               AutoPerform: true,
               Perform: new string[0][],
               GunName: "Simple1",
               GunNameBurst: "Simple1",
               DebugLevel: 0,
               Revealable: false,
               IsPooled: true,
               FindInBattle: true,
               HideMesuem: false,
               IsUpgradable: true,
               Rarity: Rarity.Uncommon,
               Type: CardType.Skill,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { ManaColor.Black, ManaColor.Red },
               IsXCost: false,
               Cost: new ManaGroup() { Black = 1, Red = 1 },
               UpgradedCost: new ManaGroup() { Any = 1, Hybrid = 1, HybridColor = 7 },
               MoneyCost: null,
               Damage: null,
               UpgradedDamage: null,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: 1,
               UpgradedValue1: null,
               Value2: 3,
               UpgradedValue2: 5,
               Mana: new ManaGroup() { Any = 0 },
               UpgradedMana: null,
               Scry: null,
               UpgradedScry: null,
               ToolPlayableTimes: null,
               Loyalty: null,
               UpgradedLoyalty: null,
               PassiveCost: null,
               UpgradedPassiveCost: null,
               ActiveCost: null,
               UpgradedActiveCost: null,
               UltimateCost: null,
               UpgradedUltimateCost: null,
               Kicker: null,
               UpgradedKicker: null,
               ActiveCost2: null,
               UpgradedActiveCost2: null,

               Keywords: Keyword.Exile,
               UpgradedKeywords: Keyword.Exile,
               EmptyDescription: false,
               RelativeKeyword: Keyword.TempMorph,
               UpgradedRelativeKeyword: Keyword.TempMorph,

               RelativeEffects: new List<string>() { },
               UpgradedRelativeEffects: new List<string>() { },
               RelativeCards: new List<string>() { },
               UpgradedRelativeCards: new List<string>() { },
               Owner: "Mima",
               ImageId: "",
               UpgradeImageId: "",
               Unfinished: false,
               Illustrator: "Dairi",
               SubIllustrator: new List<string>() { }
            );
            return cardConfig;
        }

        [EntityLogic(typeof(cardmarisaskilldef))]
        public sealed class cardmarisaskill : mimaextensions.mimacard
        {
            //discover 2/3 marisa skill cards
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {

                //value2 is cards shown, value1 is cards selectable
                //mana is temporary mana cost
                List<Card> list = new List<Card>();
                list = Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.OnlySkill), Value2, (config) => config.Owner == "Marisa" && !config.Keywords.HasFlag(Keyword.Forbidden)).ToList();
                SelectCardInteraction interaction = new SelectCardInteraction(0, Value1, list, SelectedCardHandling.DoNothing)
                {
                    Source = this
                };
                yield return new InteractionAction(interaction, false);
                IReadOnlyList<Card> selectedCards = interaction.SelectedCards;

                if (selectedCards != null)
                {
                    foreach (Card card in selectedCards)
                    {
                        card.SetTurnCost(Mana);
                        card.IsEthereal = true;
                        card.IsExile = true;
                    }
                    yield return new AddCardsToHandAction(selectedCards);
                }
                yield break;
            }
        }
    }
}
