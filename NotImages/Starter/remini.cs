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

namespace lvalonmima.NotImages.Starter
{
    public sealed class cardreminidef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardremini);
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
               Rarity: Rarity.Common,
               Type: CardType.Skill,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { ManaColor.Blue },
               IsXCost: false,
               Cost: new ManaGroup() { Blue = 1 },
               UpgradedCost: new ManaGroup() { Any = 1 },
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

               Keywords: Keyword.None,
               UpgradedKeywords: Keyword.None,
               EmptyDescription: false,
               RelativeKeyword: Keyword.None,
               UpgradedRelativeKeyword: Keyword.None,

               RelativeEffects: new List<string>() { nameof(SE.magicalburstdef.magicalburst) },
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

        [EntityLogic(typeof(cardreminidef))]
        public sealed class cardremini : mimaextensions.mimacard
        {
            public int Value3 => 2;
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                if (Battle.Player.TryGetStatusEffect(out SE.magicalburstdef.magicalburst tmp) && tmp is mimaextensions.mimase magicalburst && !IsUpgraded)
                {
                    int counttmp = magicalburst.truecounter - Value3;

                    if (counttmp >= 0)
                    {
                        magicalburst.truecounter -= Value3;
                        yield return BuffAction<SE.magicalburstdef.magicalburst>(0, 1, 0, 0, 0.2f);
                    }
                    else { magicalburst.truecounter = 0; yield return BuffAction<SE.magicalburstdef.magicalburst>(0, 1, 0, 0, 0.2f); }
                }
                List<Card> list = Battle.RollCards(new CardWeightTable(RarityWeightTable.NoneRare, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.OnlySkill), Value2, (config) => !config.Keywords.HasFlag(Keyword.Forbidden) && config.Id != Id).ToList();
                if (list.Count > 0)
                {
                    SelectCardInteraction interaction = new SelectCardInteraction(Value1, Value1, list, SelectedCardHandling.DoNothing)
                    {
                        Source = this
                    };
                    yield return new InteractionAction(interaction, false);
                    Card card = interaction.SelectedCards.FirstOrDefault();
                    if (card != null)
                    {
                        yield return new AddCardsToHandAction(new Card[]
                        {
                        card
                        });
                    }
                }
                yield break;
            }
        }
    }
}
