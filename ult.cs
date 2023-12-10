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
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static lvalonmima.BepinexPlugin;
using static lvalonmima.SE.evilspiritdef;
using Unity.IO.LowLevel.Unsafe;
using System.Collections;

namespace lvalonmima
{
    public sealed class ultadef : UltimateSkillTemplate
    {
        public override IdContainer GetId() => nameof(ulta);

        public override LocalizationOption LoadLocalization()
        {
            return toolbox.locult();
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("ulta.png", embeddedSource);
        }

        public override UltimateSkillConfig MakeConfig()
        {
            var config = new UltimateSkillConfig(
                Id: "",
                Order: 10,
                PowerCost: 80,
                PowerPerLevel: 80,
                MaxPowerLevel: 2,
                RepeatableType: UsRepeatableType.OncePerBattle,
                Damage: 15,
                Value1: 999,
                Value2: 3,
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
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
            {
                yield return new DamageAction(base.Owner, base.Owner, DamageInfo.HpLose(Value1, true), "Instant", GunType.Single);
                int atktimeleft = Value2;
                while ( atktimeleft > 0 && !base.Battle.BattleShouldEnd)
                {
                    yield return new DamageAction(base.Owner, selector.GetEnemy(base.Battle), this.Damage, base.GunName, GunType.Single);
                    atktimeleft--;
                }
            }
        }
    }

    public sealed class ultbdef : UltimateSkillTemplate
    {
        public override IdContainer GetId() => nameof(ultb);

        public override LocalizationOption LoadLocalization()
        {
            return toolbox.locult();
        }

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
                MaxPowerLevel: 2,
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
                base.TargetType = TargetType.RandomEnemy;
            }
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
            {
                yield return new DamageAction(base.Owner, base.Owner, DamageInfo.HpLose(Value1, true), "Instant", GunType.Single);
                if (base.Battle.BattleShouldEnd)
                {
                    yield break;
                }
                yield return new DamageAction(base.Owner, base.Owner, DamageInfo.HpLose(Value1, true), "Instant", GunType.Single);
                if (base.Battle.BattleShouldEnd)
                {
                    yield break;
                }
            }
        }
    }
}
