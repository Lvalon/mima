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
using static lvalonmima.SE.transmigrateddef;
using Unity.IO.LowLevel.Unsafe;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.Core.Randoms;

namespace lvalonmima.NotImages.Offcolor
{
    public sealed class carddoublefalldef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(carddoublefall);
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
               Rarity: Rarity.Common,
               Type: CardType.Attack,
               TargetType: TargetType.SingleEnemy,
               Colors: new List<ManaColor>() { ManaColor.Black },
               IsXCost: false,
               Cost: new ManaGroup() { Any = 1, Black = 1 },
               UpgradedCost: null,
               MoneyCost: null,
               Damage: 3,
               UpgradedDamage: 4,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: 1,
               UpgradedValue1: null,
               Value2: 2,
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

               RelativeEffects: new List<string>() { "transmigrated" },
               UpgradedRelativeEffects: new List<string>() { "transmigrated" },
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

        [EntityLogic(typeof(carddoublefalldef))]
        public sealed class carddoublefall : Card
        {
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                EnemyUnit enemy = selector.SelectedEnemy; //value1 = apply status level
                yield return new ApplyStatusEffectAction<transmigrated>(enemy, Value1, null, null, null, 0f, true);
                base.CardGuns = new Guns(base.GunName, base.Value2, true); //value2 = atk instances
                foreach (GunPair gunPair in base.CardGuns.GunPairs)
                {
                    yield return base.AttackAction(selector, gunPair);
                }
                List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
                yield break;
            }
        }
    }
}
