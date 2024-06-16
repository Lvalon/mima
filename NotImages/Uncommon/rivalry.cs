using LBoL.Base;
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
    public sealed class cardrivalrydef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardrivalry);
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
               Colors: new List<ManaColor>() { ManaColor.White, ManaColor.Red },
               IsXCost: false,
               Cost: new ManaGroup() { Any = 2, Hybrid = 1, HybridColor = 2 },
               UpgradedCost: new ManaGroup() { Any = 1, Hybrid = 1, HybridColor = 2 },
               MoneyCost: null,
               Damage: null,
               UpgradedDamage: null,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: 2,
               UpgradedValue1: null,
               Value2: 1,
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

               Keywords: Keyword.Exile | Keyword.Ethereal,
               UpgradedKeywords: Keyword.Exile | Keyword.Ethereal,
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

        [EntityLogic(typeof(cardrivalrydef))]
        public sealed class cardrivalry : mimaextensions.mimacard
        {
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                List<Card> list = new List<Card>();

                list = Battle.RollCards(new CardWeightTable(RarityWeightTable.OnlyUncommon, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.CanBeLoot), Value1, (CardConfig config) => config.Type == CardType.Ability).ToList();
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
                        card.SetTurnCost(Mana);
                        card.IsEthereal = true;
                    }
                    yield return new AddCardsToHandAction(selectedCards);
                }
                yield break;
            }
        }
    }
}
