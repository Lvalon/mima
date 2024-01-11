using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static lvalonmima.BepinexPlugin;
using static lvalonmima.SE.karmanationdef;
using static lvalonmima.SE.magicalburstdef;
using LBoL.Core.Battle.BattleActions;
using LBoL.Base.Extensions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoL.EntityLib.StatusEffects.Koishi;
using static UnityEngine.GraphicsBuffer;
using System.Linq;
using LBoL.EntityLib.StatusEffects.Others;

namespace lvalonmima.SE.mburstmodifiers
{
    public sealed class implosiondef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(implosion);
        }

        public override LocalizationOption LoadLocalization() => sebatchloc.AddEntity(this);

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("seimplosion.png", embeddedSource);
        }

        public override StatusEffectConfig MakeConfig()
        {
            var statusEffectConfig = new StatusEffectConfig(
                Index: sequenceTable.Next(typeof(CardConfig)),
                Id: "",
                Order: 1,
                Type: StatusEffectType.Special,
                IsVerbose: true,
                IsStackable: true,
                StackActionTriggerLevel: null,
                HasLevel: true,
                LevelStackType: StackType.Add,
                HasDuration: false,
                DurationStackType: null,
                DurationDecreaseTiming: DurationDecreaseTiming.Custom,
                HasCount: false,
                CountStackType: StackType.Keep,
                LimitStackType: StackType.Keep,
                ShowPlusByLimit: false,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { "magicalburst" },
                VFX: "Default",
                VFXloop: "Default",
                SFX: "Default"
            );
            return statusEffectConfig;
        }

        [EntityLogic(typeof(implosiondef))]
        public sealed class implosion : mimaextensions.mimase
        {
            public implosion() : base()
            {
                truecounter = 0;
            }
            protected override void OnAdded(Unit unit)
            {
                ReactOwnerEvent(Owner.DamageDealt, new EventSequencedReactor<DamageEventArgs>(this.OnDamageDealt));
            }
            private IEnumerable<BattleAction> OnDamageDealt(DamageEventArgs args)
            {
                if (base.Battle.BattleShouldEnd)
                {
                    yield break;
                }
                if (args.Target.IsAlive)
                {
                    DamageInfo damageInfo = args.DamageInfo;
                    if (args.ActionSource.Id == "magicalburst" && damageInfo.Damage > 0f)
                    {
                        base.NotifyActivating();
                        yield return DamageAction.LoseLife(args.Target, (int)(args.DamageInfo.Damage * Level), "Cold2");
                    }
                }
                yield break;
            }
        }
    }
}
