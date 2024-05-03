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
using static lvalonmima.BepinexPlugin;

namespace lvalonmima
{
    public sealed class ultadef : UltimateSkillTemplate
    {
        public override IdContainer GetId() => nameof(ulta);

        public override LocalizationOption LoadLocalization() => ultbatchloc.AddEntity(this);

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("ulta.png", embeddedSource);
        }

        public override UltimateSkillConfig MakeConfig()
        {
            var config = new UltimateSkillConfig(
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
                base.TargetType = TargetType.RandomEnemy;
                base.GunName = "Butterfly1";
            }
            public int hplosing
            {
                get
                {
                    return Value1;
                }
            }

            protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
            {
                yield return PerformAction.Spell(Battle.Player, "ulta");
                yield return new DamageAction(base.Owner, base.Owner, DamageInfo.HpLose(hplosing, true), "Instant", GunType.Single);
                int atktimeleft = Value2;
                while (atktimeleft > 0 && !base.Battle.BattleShouldEnd)
                {
                    atktimeleft--;
                    yield return new DamageAction(base.Owner, selector.GetEnemy(base.Battle), this.Damage, base.GunName, GunType.Single);
                }
                yield break;
            }
        }
    }

    public sealed class ultbdef : UltimateSkillTemplate
    {
        public override IdContainer GetId() => nameof(ultb);

        public override LocalizationOption LoadLocalization() => ultbatchloc.AddEntity(this);

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("ultb.png", embeddedSource);
        }

        public override UltimateSkillConfig MakeConfig()
        {
            var config = new UltimateSkillConfig(
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
                base.TargetType = TargetType.Self;
            }
            public int hplosing
            {
                get
                {
                    return Value1;
                }
            }
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
            {
                yield return PerformAction.Spell(Battle.Player, "ultb");
                yield return new DamageAction(base.Owner, base.Owner, DamageInfo.HpLose(hplosing, true), "Instant", GunType.Single);
                yield break;
            }
        }
    }
}
