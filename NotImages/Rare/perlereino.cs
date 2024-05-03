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
using static lvalonmima.BepinexPlugin;
using LBoL.Core.Units;
using static lvalonmima.SE.evilspiritdef;

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

               RelativeEffects: new List<string>() { nameof(evilspirit), nameof(InvincibleEternal) },
               UpgradedRelativeEffects: new List<string>() { nameof(evilspirit), nameof(InvincibleEternal) },
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
            private string exDescription1
            {
                get
                {
                    return this.LocalizeProperty("exDescription1", true, true);
                }
            }
            protected override string GetBaseDescription()
            {
                if (!this.Active)
                {
                    return this.exDescription1;
                }
                return base.GetBaseDescription();
            }
            private bool Active
            {
                get
                {
                    if (base.Battle != null)
                    {
                        return !base.Battle.BattleCardUsageHistory.Any((Card card) => card is cardperlereino);
                    }
                    return true;
                }
            }
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                if (this.Active)
                {
                    int buff = 0;
                    foreach (Unit target in selector.GetUnits(Battle))
                    {
                        //backup for anti-levelling

                        //foreach (StatusEffect status in (from se in target.StatusEffects
                        //                                 where se.Id != "evilspirit" && se.HasLevel
                        //                                 select se).ToList())
                        //{
                        //    yield return new ApplyStatusEffectAction(status.GetType() ,target, new int?(-status.Level), null, null, null, 0f, true);
                        //}

                        //if (!seExceptions.Contains(status.Id))
                        //{
                        //}

                        foreach (StatusEffect status in (from se in target.StatusEffects
                                                         where se.Id != "evilspirit" && se.Id != "SijiZui" && target != base.Battle.Player
                                                         select se).ToList())
                        {
                            yield return new RemoveStatusEffectAction(status, true, 0.1f);
                            buff++;
                        }
                    }
                    if (buff > 0)
                    {
                        yield return BuffAction<InvincibleEternal>(1, 0, 0, 0, 0.2f);
                        yield return BuffAction<evilspirit>(buff, 0, 0, 0, 0.2f);
                    }
                    yield break;
                }

            }
        }
    }
}
