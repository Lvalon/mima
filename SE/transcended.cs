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
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace lvalonmima.SE
{
    public sealed class transcendeddef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(transcended);
        }

        public override LocalizationOption LoadLocalization()
        {
            return BepinexPlugin.sebatchloc.AddEntity(this);
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("setranscended.png", BepinexPlugin.embeddedSource);
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
                RelativeEffects: new List<string>() { },
                VFX: "Default",
                VFXloop: "Default",
                SFX: "Default",
                ImageId: null
            );
            return statusEffectConfig;
        }

        [EntityLogic(typeof(transcendeddef))]
        public sealed class transcended : mimaextensions.mimase
        {
            public ManaGroup Mana => ManaGroup.Philosophies(1);

            private int cardused = 0;
            //set up triggers to give a fuck on
            //they worked
            protected override void OnAdded(Unit unit)
            {
                ReactOwnerEvent(Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(OnCardUsed));
                ReactOwnerEvent(Battle.RoundEnding, new EventSequencedReactor<GameEventArgs>(OnRoundEnding));

                React(PerformAction.Effect(unit, "JingHua", 0f, "", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
            }

            private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
            {
                if (args.Card.CardType == CardType.Skill)
                {
                    cardused = 1;
                    NotifyActivating();
                    yield return new GainManaAction(Mana);
                }
                yield break;
            }

            private IEnumerable<BattleAction> OnRoundEnding(GameEventArgs args)
            {
                if (cardused == 1)
                {
                    int num = Level - 1;
                    Level = num;
                    cardused = 0;
                }
                if (Level == 0)
                {
                    NotifyActivating();
                    yield return new RemoveStatusEffectAction(this, true);
                    yield break;
                }
                yield break;
            }
        }
    }
}
