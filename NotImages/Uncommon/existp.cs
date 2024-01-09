using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using static lvalonmima.BepinexPlugin;
using LBoL.Core.Units;
using System.Xml.Linq;
using static lvalonmima.SE.evilspiritdef;
using Unity.IO.LowLevel.Unsafe;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.Core.Randoms;
using LBoL.EntityLib.Cards;

namespace lvalonmima.NotImages.Uncommon
{
    public sealed class cardexistpdef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardexistp);
        }

        public override CardImages LoadCardImages()
        {
            var imgs = new CardImages(embeddedSource);
            imgs.AutoLoad(this, extension: ".png");
            return imgs;
        }

        public override LocalizationOption LoadLocalization()
        {
            return toolbox.loccard();
        }

        public override CardConfig MakeConfig()
        {
            var cardConfig = new CardConfig(
               Index: sequenceTable.Next(typeof(CardConfig)),
               Id: "",
               Order: 10,
               AutoPerform: true,
               Perform: new string[0][],
               GunName: "Simple1",
               GunNameBurst: "Simple1",
               DebugLevel: 0,
               Revealable: false,
               IsPooled: true,
               HideMesuem: false,
               IsUpgradable: true,
               Rarity: Rarity.Uncommon,
               Type: CardType.Skill,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { ManaColor.Black, ManaColor.Green },
               IsXCost: false,
               Cost: new ManaGroup() { Black = 1, Green = 1 },
               UpgradedCost: new ManaGroup() { Any = 2 },
               MoneyCost: null,
               Damage: null,
               UpgradedDamage: null,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: 2,
               UpgradedValue1: 3,
               Value2: 1,
               UpgradedValue2: null,
               Mana: null,
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

               Keywords: Keyword.Exile,
               UpgradedKeywords: Keyword.Exile,
               EmptyDescription: false,
               RelativeKeyword: Keyword.Instinct | Keyword.Exile | Keyword.Ethereal,
               UpgradedRelativeKeyword: Keyword.Instinct | Keyword.Exile | Keyword.Ethereal,

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

        [EntityLogic(typeof(cardexistpdef))]
        public sealed class cardexistp : mimaextensions.mimacard
        {
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                List<Card> list = new List<Card>();
                //List<Card> list = Battle.RollCards(new CardWeightTable(RarityWeightTable.AllOnes, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot), Value1, (CardConfig config) => config.Id != base.Id && (config.RelativeKeyword.HasFlag(Keyword.Instinct) || config.UpgradedRelativeKeyword.HasFlag(Keyword.Instinct) || config.Keywords.HasFlag(Keyword.Instinct) || config.UpgradedKeywords.HasFlag(Keyword.Instinct))).ToList();
                //foreach (Card card in list)
                //{
                //    card.IsEthereal = true;
                //    card.IsExile = true;
                //}
                //yield return new AddCardsToHandAction(list);
                //yield return new GainManaAction(Mana);
                list = Battle.RollCards(new CardWeightTable(RarityWeightTable.AllOnes, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot), Value1, (CardConfig config) => config.Id != base.Id && (config.RelativeKeyword.HasFlag(Keyword.Instinct) || config.UpgradedRelativeKeyword.HasFlag(Keyword.Instinct) || config.Keywords.HasFlag(Keyword.Instinct) || config.UpgradedKeywords.HasFlag(Keyword.Instinct))).ToList();
                SelectCardInteraction interaction = new SelectCardInteraction(Value2, Value2, list, SelectedCardHandling.DoNothing)
                {
                    Source = this
                };
                yield return new InteractionAction(interaction, false);
                IReadOnlyList<Card> selectedCards = interaction.SelectedCards;

                if (selectedCards != null)
                {
                    foreach (Card card in selectedCards)
                    {
                        card.IsEthereal = true;
                        card.IsExile = true;
                        //yield return new AddCardsToHandAction(card);
                    }
                    yield return new AddCardsToHandAction(selectedCards);
                }
                yield break;
            }
        }
    }
}
