using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using LBoL.Core;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using System.Collections.Generic;
using UnityEngine;

namespace lvalonmima
{
    public sealed class ultadef : UltimateSkillTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(ulta);
        }

        public override LocalizationOption LoadLocalization()
        {
            return BepinexPlugin.ultbatchloc.AddEntity(this);
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("ulta.png", BepinexPlugin.embeddedSource);
        }

        public override UltimateSkillConfig MakeConfig()
        {
            UltimateSkillConfig config = new UltimateSkillConfig(
                Id: "",
                Order: 10,
                PowerCost: 100,
                PowerPerLevel: 100,
                MaxPowerLevel: 2,
                RepeatableType: UsRepeatableType.FreeToUse,
                Damage: 6,
                Value1: 33,
                Value2: 6,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { },
                RelativeCards: new List<string>() { }
                );

            return config;
        }

        [EntityLogic(typeof(ultadef))]
        public sealed class ulta : UltimateSkill
        {
            public ulta()
            {
                TargetType = TargetType.RandomEnemy;
                GunName = "Butterfly1";
            }
            public int hplosing => Value1;

            protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
            {
                yield return PerformAction.Spell(Battle.Player, "ulta");
                yield return new DamageAction(Owner, Owner, DamageInfo.HpLose(hplosing, true), "Instant", GunType.Single);
                int atktimeleft = Value2;
                while (atktimeleft > 0 && !Battle.BattleShouldEnd)
                {
                    atktimeleft--;
                    yield return new DamageAction(Owner, selector.GetEnemy(Battle), Damage, GunName, GunType.Single);
                }
                yield break;
            }
        }
    }

    public sealed class ultbdef : UltimateSkillTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(ultb);
        }

        public override LocalizationOption LoadLocalization()
        {
            return BepinexPlugin.ultbatchloc.AddEntity(this);
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("ultb.png", BepinexPlugin.embeddedSource);
        }

        public override UltimateSkillConfig MakeConfig()
        {
            UltimateSkillConfig config = new UltimateSkillConfig(
                Id: "",
                Order: 10,
                PowerCost: 120,
                PowerPerLevel: 120,
                MaxPowerLevel: 3,
                RepeatableType: UsRepeatableType.FreeToUse,
                Damage: 0,
                Value1: 666,
                Value2: 0,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { },
                RelativeCards: new List<string>() { }
                );

            return config;
        }

        [EntityLogic(typeof(ultbdef))]
        public sealed class ultb : UltimateSkill
        {
            public ultb()
            {
                TargetType = TargetType.Self;
            }
            public int hplosing => Value1;
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
            {
                yield return PerformAction.Spell(Battle.Player, "ultb");
                yield return new DamageAction(Owner, Owner, DamageInfo.HpLose(hplosing, true), "Instant", GunType.Single);
                yield break;
            }
        }
    }
}
