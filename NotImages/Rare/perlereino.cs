using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Units;

namespace lvalonmima.NotImages.Rare
{
    public sealed class cardperlereinodef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardperlereino);
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
               Colors: new List<ManaColor>() { ManaColor.Blue, ManaColor.Black, ManaColor.Green, ManaColor.White },
               IsXCost: false,
               Cost: new ManaGroup() { Any = 1, White = 1, Blue = 1, Green = 1, Black = 1 },
               UpgradedCost: new ManaGroup() { White = 1, Blue = 1, Green = 1, Black = 1 },
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

               Keywords: Keyword.Exile,
               UpgradedKeywords: Keyword.Exile,
               EmptyDescription: false,
               RelativeKeyword: Keyword.None,
               UpgradedRelativeKeyword: Keyword.None,

               RelativeEffects: new List<string>() { nameof(SE.evilspiritdef.evilspirit), nameof(InvincibleEternal) },
               UpgradedRelativeEffects: new List<string>() { nameof(SE.evilspiritdef.evilspirit), nameof(InvincibleEternal) },
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

        [EntityLogic(typeof(cardperlereinodef))]
        public sealed class cardperlereino : mimaextensions.mimacard
        {
            private string exDescription1 => LocalizeProperty("exDescription1", true, true);
            protected override string GetBaseDescription()
            {
                return !Active ? exDescription1 : base.GetBaseDescription();
            }
            private bool Active => Battle != null ? !Battle.BattleCardUsageHistory.Any((Card card) => card is cardperlereino) : true;
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                if (Active)
                {
                    int buff = 0;
                    foreach (Unit target in selector.GetUnits(Battle))
                    {
                        foreach (StatusEffect status in (from se in target.StatusEffects
                                                         where se.Id != "evilspirit" && se.Id != "SijiZui" && target != Battle.Player
                                                         select se).ToList())
                        {
                            yield return new RemoveStatusEffectAction(status, true, 0.1f);
                            buff++;
                        }
                    }
                    if (buff > 0)
                    {
                        yield return BuffAction<InvincibleEternal>(1, 0, 0, 0, 0.2f);
                        yield return BuffAction<SE.evilspiritdef.evilspirit>(buff, 0, 0, 0, 0.2f);
                    }
                    yield break;
                }

            }
        }
    }
}
