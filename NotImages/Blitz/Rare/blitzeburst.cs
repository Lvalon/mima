using LBoL.Base;
using LBoL.Core.Cards;
using LBoL.Core.Battle;
using LBoL.ConfigData;
using LBoL.Presentation;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using lvalonmima.SE;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using System.Collections;
using LBoL.Core.Battle.BattleActions;

namespace lvalonmima.NotImages.Blitz.Rare
{
    public sealed class blitzeburstdef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(blitzeburst);
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
               IsPooled: false,
               FindInBattle: false,
               HideMesuem: false,
               IsUpgradable: false,
               Rarity: Rarity.Rare,
               Type: CardType.Skill,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { ManaColor.Black },
               IsXCost: false,
               Cost: new ManaGroup() { },
               UpgradedCost: null,
               MoneyCost: null,
               Damage: 10,
               UpgradedDamage: null,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: null,
               UpgradedValue1: null,
               Value2: null,
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

               RelativeEffects: new List<string>() { nameof(seblitzdef.seblitz), nameof(Burst) },
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

        [EntityLogic(typeof(blitzeburstdef))]
        public sealed class blitzeburst : mimaextensions.mimacard.blitzcard
        {
            public static void onconfirm(Card card) {
                GameRunController gamerun = GameMaster.Instance?.CurrentGameRun;
                BattleController battle = gamerun.Battle;
                battle.RequestDebugAction(new ApplyStatusEffectAction<Burst>(battle.Player, 1, null, null, null, 0f, true).SetCause(ActionCause.Card),"blitzeburst: enter burst");
                battle.RequestDebugAction(new DamageAction(battle.Player, battle.EnemyGroup.Alives, DamageInfo.Attack(10, true), "JunkoLunatic", GunType.Single).SetCause(ActionCause.Card), "blitzeburst: deal dmg");
                gamerun.RemoveDeckCard(card);
            }
        }
    }
}
