using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle;
using LBoL.Core;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using UnityEngine;
using static lvalonmima.BepinexPlugin;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using static lvalonmima.SE.magicalburstdef;

namespace lvalonmima.SE
{
    public sealed class seshepherdgdef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(seshepherdg);
        }

        public override LocalizationOption LoadLocalization() => sebatchloc.AddEntity(this);

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("seshepherdg.png", embeddedSource);
        }

        public override StatusEffectConfig MakeConfig()
        {
            var statusEffectConfig = new StatusEffectConfig(
                Index: sequenceTable.Next(typeof(CardConfig)),
                Id: "",
                Order: 1,
                Type: StatusEffectType.Positive,
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

        [EntityLogic(typeof(seshepherdgdef))]
        public sealed class seshepherdg : mimaextensions.mimase
        {
            protected override void OnAdded(Unit unit)
            {
                base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted));
            }
            private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
            {
                if (base.Owner == null || base.Battle.BattleShouldEnd)
                {
                    yield break;
                }
                base.NotifyActivating();
                yield return DamageAction.LoseLife(base.Owner, base.Level, "Poison");
                yield return new ApplyStatusEffectAction<magicalburst>(base.Owner, new int?(base.Level), null, null, null, 0f, true);
                yield break;
            }
        }
    }
}
