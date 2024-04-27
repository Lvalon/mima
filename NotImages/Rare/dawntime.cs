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
using static lvalonmima.SE.sedawntimedef;
using Unity.IO.LowLevel.Unsafe;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.EntityLib.Cards.Character.Sakuya;
using LBoL.Core.Randoms;

namespace lvalonmima.NotImages.Rare
{
    public sealed class carddawntimedef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(carddawntime);
        }

        public override CardImages LoadCardImages()
        {
            var imgs = new CardImages(embeddedSource);
            imgs.AutoLoad(this, extension: ".png");
            return imgs;
        }

        public override LocalizationOption LoadLocalization() => cardbatchloc.AddEntity(this);

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
               FindInBattle: true,
               HideMesuem: false,
               IsUpgradable: true,
               Rarity: Rarity.Rare,
               Type: CardType.Ability,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { ManaColor.Red, ManaColor.White, ManaColor.Blue, ManaColor.Green, ManaColor.Black },
               IsXCost: false,
               Cost: new ManaGroup() { Any = 1, Red = 1, White = 1, Blue = 1, Green = 1, Black = 1 },
               UpgradedCost: null,
               MoneyCost: null,
               Damage: null,
               UpgradedDamage: null,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: 7,
               UpgradedValue1: 10,
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

               Keywords: Keyword.Ethereal,
               UpgradedKeywords: Keyword.Ethereal,
               EmptyDescription: false,
               RelativeKeyword: Keyword.TempMorph,
               UpgradedRelativeKeyword: Keyword.TempMorph,

               RelativeEffects: new List<string>() { nameof(ExtraTurn), nameof(sedawntime) },
               UpgradedRelativeEffects: new List<string>() { nameof(ExtraTurn), nameof(sedawntime) },
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

        [EntityLogic(typeof(carddawntimedef))]
        public sealed class carddawntime : mimaextensions.mimacard
        {
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                yield return PerformAction.Effect(base.Battle.Player, "ExtraTime", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
                yield return PerformAction.Sfx("ExtraTurnLaunch", 0f);
                yield return PerformAction.Animation(base.Battle.Player, "spell", 1.6f, null, 0f, -1);
                yield return base.BuffAction<ExtraTurn>(1, 0, 0, 0, 0.2f);
                yield return base.BuffAction<sedawntime>(Value1, 0, 0, 0, 0.2f);
                yield return new RequestEndPlayerTurnAction();
                yield break;
            }
        }
    }
}
