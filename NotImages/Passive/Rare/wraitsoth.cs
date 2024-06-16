using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;

namespace lvalonmima.NotImages.Passive.Rare
{
    public sealed class cardpassivewraitsothdef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardpassivewraitsoth);
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
               FindInBattle: false,
               HideMesuem: false,
               IsUpgradable: false,
               Rarity: Rarity.Rare,
               Type: CardType.Ability,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { ManaColor.Red, ManaColor.Green },
               IsXCost: false,
               Cost: new ManaGroup() { },
               UpgradedCost: new ManaGroup() { },
               MoneyCost: null,
               Damage: null,
               UpgradedDamage: null,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: 1,
               UpgradedValue1: null,
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

               Keywords: Keyword.Forbidden,
               UpgradedKeywords: Keyword.None,
               EmptyDescription: false,
               RelativeKeyword: Keyword.None,
               UpgradedRelativeKeyword: Keyword.None,

               RelativeEffects: new List<string>() { nameof(SE.sepassivedef.sepassive), nameof(SE.mburstmodifiers.everlastingmagicdef.everlastingmagic) },
               UpgradedRelativeEffects: new List<string>() { nameof(SE.sepassivedef.sepassive), nameof(SE.mburstmodifiers.everlastingmagicdef.everlastingmagic) },
               RelativeCards: new List<string>() { },
               UpgradedRelativeCards: new List<string>() { },
               Owner: "Mima",
               ImageId: "",
               UpgradeImageId: "",
               Unfinished: false,
               Illustrator: "Lvalon",
               SubIllustrator: new List<string>() { }
            );
            return cardConfig;
        }

        [EntityLogic(typeof(cardpassivewraitsothdef))]
        public sealed class cardpassivewraitsoth : mimaextensions.mimacard
        {
            public cardpassivewraitsoth() : base()
            {
                ispassive = true;
            }
            public int gainred => Value1 * 50;
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                if (Battle.Player.TryGetStatusEffect(out SE.magicalburstdef.magicalburst tmp) && tmp is mimaextensions.mimase magicalburst)
                {
                    magicalburst.truecounter = 0;
                    yield return BuffAction<SE.magicalburstdef.magicalburst>(0, 1, 0, 0, 0.2f);
                }
                yield return BuffAction<SE.mburstmodifiers.everlastingmagicdef.everlastingmagic>(Value1, 0, 0, 0, 0.2f);
                yield break;
            }

        }
    }
}
