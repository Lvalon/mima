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
using UnityEngine;
using static lvalonmima.BepinexPlugin;
using LBoL.Core.Units;
using System.Xml.Linq;
using static lvalonmima.SE.evilspiritdef;
using Unity.IO.LowLevel.Unsafe;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.Core.Randoms;

namespace lvalonmima.NotImages.Offcolor
{
    public sealed class marisaskilldef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(marisaskill);
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
               Rarity: Rarity.Rare,
               Type: CardType.Skill,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { ManaColor.Blue, ManaColor.Black, ManaColor.White },
               IsXCost: false,
               Cost: new ManaGroup() { Any = 2, Blue = 1, Black = 1, White = 1 },
               UpgradedCost: new ManaGroup() { Blue = 1, Black = 1, White = 1 },
               MoneyCost: null,
               Damage: null,
               UpgradedDamage: null,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: 2,
               UpgradedValue1: null,
               Value2: null,
               UpgradedValue2: null,
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

               Keywords: Keyword.None,
               UpgradedKeywords: Keyword.None,
               EmptyDescription: false,
               RelativeKeyword: Keyword.None,
               UpgradedRelativeKeyword: Keyword.None,

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

        [EntityLogic(typeof(marisaskilldef))]
        public sealed class marisaskill : Card
        {
            //discover 2/3 marisa skill cards
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                int check = 0;
                while (check == 0)
                {
                    Card One = base.Battle.RollCard(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.OnlySkill), delegate (CardConfig config)
                    {
                        string owner = config.Owner;
                        if (owner != null && owner == "Marisa")
                        {
                            check = 1;
                            return false;
                        }
                    });
                }
                while (check == 1)
                {
                    Card Two = base.Battle.RollCard(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.OnlySkill), delegate (CardConfig config)
                    {
                        string owner = config.Owner;
                        if (owner != null && owner == "Marisa")
                        {
                            check = 2;
                            return false;
                        }
                    });
                }
                while (check == 2)
                {
                    Card Three = base.Battle.RollCard(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.OnlySkill), delegate (CardConfig config)
                    {
                        string owner = config.Owner;
                        if (owner != null && owner == "Marisa")
                        {
                            check = 3;
                            return false;
                        }
                    });
                }

                //value1 is cards selectable
                //mana is temporary mana cost
                //Card[] array = new Card[] { One }, new Card[] { Two }, new Card[] { Three };
                Card[] array = One, Two, Three;
                foreach (Card card in array)
                {
                    card.SetTurnCost(base.Mana);
                    card.IsEthereal = true;
                    card.IsExile = true;
                }
                SelectCardInteraction interaction = new SelectCardInteraction(0, base.Value1, array, SelectedCardHandling.DoNothing)
                {
                    Source = this
                };
                yield return new InteractionAction(interaction, false);
                IReadOnlyList<Card> selectedCards = interaction.SelectedCards;
                if (selectedCards.Count > 0)
                {
                    yield return new AddCardsToHandAction(selectedCards);
                }
                yield break;
            }
        }
    }
}
