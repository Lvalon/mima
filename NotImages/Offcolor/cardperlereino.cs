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

namespace lvalonmima.NotImages.Offcolor
{
    public sealed class cardperlereinodef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardperlereino);
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
               Cost: new ManaGroup() { Any = 2, White = 1, Blue = 1, Black = 1 },
               UpgradedCost: new ManaGroup() { White = 1, Blue = 1, Black = 1 },
               MoneyCost: null,
               Damage: null,
               UpgradedDamage: null,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: null,
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

               RelativeEffects: new List<string>() { "evilspirit", "Invincible" },
               UpgradedRelativeEffects: new List<string>() { "evilspirit", "Invincible" },
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

        [EntityLogic(typeof(cardperlereinodef))]
        public sealed class cardperlereino : Card
        {
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                //yield return base.BuffAction<Invincible>(0, base.Value1, 0, 0, 0.2f);
                //yield return base.BuffAction<evilspirit>(0, base.Value1, 0, 0, 0.2f);
                log.LogDebug("actions entered");
                int buff = 0;
                foreach (Unit target in selector.GetUnits(base.Battle))
                {
                    log.LogDebug(target);
                    foreach (StatusEffect status in target.StatusEffects.Where(status => status.Id != "evilspirit"))
                    {
                        log.LogDebug("second forloop entered");
                        new RemoveStatusEffectAction(status, true);
                        buff++;
                        log.LogDebug(status);
                    }
                }
                yield return base.BuffAction<Invincible>(0, buff, 0, 0, 0.2f);
                yield return base.BuffAction<evilspirit>(buff, 0, 0, 0, 0.2f);
                yield break;
            }
        }
    }
}
