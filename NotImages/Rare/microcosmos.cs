using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using LBoL.EntityLib.StatusEffects.ExtraTurn;

namespace lvalonmima.NotImages.Rare
{
    public sealed class cardmicrocosmosdef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardmicrocosmos);
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
               Rarity: Rarity.Rare,
               Type: CardType.Skill,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { ManaColor.Blue, ManaColor.Red },
               IsXCost: false,
               Cost: new ManaGroup() { Any = 2, Blue = 1, Red = 1 },
               UpgradedCost: new ManaGroup() { Any = 3 },
               MoneyCost: null,
               Damage: null,
               UpgradedDamage: null,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: 3,
               UpgradedValue1: 5,
               Value2: null,
               UpgradedValue2: null,
               Mana: new ManaGroup() { Any = 1 },
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
               RelativeKeyword: Keyword.None,
               UpgradedRelativeKeyword: Keyword.None,

               RelativeEffects: new List<string>() { nameof(ExtraTurn), nameof(SE.exfireonskilldef.exfireonskill), nameof(TimeIsLimited) },
               UpgradedRelativeEffects: new List<string>() { nameof(ExtraTurn), nameof(SE.exfireonskilldef.exfireonskill), nameof(TimeIsLimited) },
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

        [EntityLogic(typeof(cardmicrocosmosdef))]
        public sealed class cardmicrocosmos : mimaextensions.mimacard
        {
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                yield return PerformAction.Effect(Battle.Player, "ExtraTime", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
                yield return PerformAction.Sfx("ExtraTurnLaunch", 0f);
                yield return PerformAction.Animation(Battle.Player, "spell", 1.6f, null, 0f, -1);
                yield return BuffAction<ExtraTurn>(1, 0, 0, 0, 0.2f);
                yield return BuffAction<SE.exfireonskilldef.exfireonskill>(Value1, 0, 0, 0, 0.2f);
                yield return DebuffAction<TimeIsLimited>(Battle.Player, 1, 0, 0, 0, true, 0.2f);
                yield return new RequestEndPlayerTurnAction();
                yield break;
            }
        }
    }
}
