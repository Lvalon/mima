using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using UnityEngine;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace lvalonmima.SE.mburstmodifiers
{
    public sealed class retributiondef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(retribution);
        }

        public override LocalizationOption LoadLocalization()
        {
            return BepinexPlugin.sebatchloc.AddEntity(this);
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("seretribution.png", BepinexPlugin.embeddedSource);
        }

        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig statusEffectConfig = new StatusEffectConfig(
                Index: BepinexPlugin.sequenceTable.Next(typeof(CardConfig)),
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
                RelativeEffects: new List<string>() { nameof(magicalburstdef.magicalburst) },
                VFX: "Default",
                VFXloop: "Default",
                SFX: "Default"
            );
            return statusEffectConfig;
        }

        [EntityLogic(typeof(retributiondef))]
        public sealed class retribution : mimaextensions.mimase
        {
            public retribution() : base()
            {
                isMBmod = false;
                truecounter = 0;
            }
            protected override void OnAdded(Unit unit)
            {
                ReactOwnerEvent(Owner.DamageDealt, new EventSequencedReactor<DamageEventArgs>(OnDamageDealt));
            }
            private IEnumerable<BattleAction> OnDamageDealt(DamageEventArgs args)
            {
                if (Battle.BattleShouldEnd)
                {
                    yield break;
                }
                if (args.ActionSource is StatusEffect statusEffect && statusEffect is magicalburstdef.magicalburst)
                {
                    DamageInfo damageInfo = args.DamageInfo;
                    if (damageInfo.Damage > 0f)
                    {
                        NotifyActivating();
                        yield return DamageAction.LoseLife(Owner, Level, "Cold2");
                    }
                }
                yield break;
            }
        }
    }
}
