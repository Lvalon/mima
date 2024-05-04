using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using System.Linq;

namespace lvalonmima.NotImages.Uncommon
{
    public sealed class cardwheresleepdef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardwheresleep);
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
               Cost: new ManaGroup() { Any = 0 },
               UpgradedCost: null,
               MoneyCost: null,
               Damage: null,
               UpgradedDamage: null,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: 4,
               UpgradedValue1: 6,
               Value2: 4,
               UpgradedValue2: 6,
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

               Keywords: Keyword.Forbidden,
               UpgradedKeywords: Keyword.Forbidden,
               EmptyDescription: false,
               RelativeKeyword: Keyword.None,
               UpgradedRelativeKeyword: Keyword.None,

               RelativeEffects: new List<string>() { nameof(SE.magicalburstdef.magicalburst) },
               UpgradedRelativeEffects: new List<string>() { nameof(SE.magicalburstdef.magicalburst) },
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

        [EntityLogic(typeof(cardwheresleepdef))]
        public sealed class cardwheresleep : mimaextensions.mimacard
        {
            protected override void OnEnterBattle(BattleController battle)
            {
                ReactBattleEvent(Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(OnBattleStarted));
            }

            // Token: 0x06000CC9 RID: 3273 RVA: 0x00017DC5 File Offset: 0x00015FC5
            private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
            {
                if (this == Battle.EnumerateAllCards().First((Card card) => card.Id is nameof(cardwheresleep)))
                {
                    List<Card> list = (from card in Battle.DrawZone
                                       where card.Id is nameof(cardwheresleep)
                                       select card).ToList();
                    int value1 = list.Sum((Card card) => card.Value1);
                    int value2 = list.Sum((Card card) => card.Value2);
                    yield return new ExileManyCardAction(list);
                    yield return SacrificeAction(value1);
                    yield return BuffAction<SE.magicalburstdef.magicalburst>(value2, 0, 0, 0, 0.2f);
                }
                yield break;
            }
        }
    }
}
