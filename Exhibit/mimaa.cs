using System;
using System.Collections.Generic;
using System.Text;
using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using LBoLEntitySideloader.Attributes;
using static lvalonmima.BepinexPlugin;
using Mono.Cecil;
using UnityEngine;
using LBoL.Base;
using LBoL.EntityLib.Exhibits;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
using LBoL.Core;
using LBoL.Core.Units;
using static lvalonmima.SE.evilspiritdef;
using LBoL.Base.Extensions;
using LBoL.EntityLib.StatusEffects.Enemy;
using System.Linq;

namespace lvalonmima.Exhibit
{
    internal class mimaadef : ExhibitTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(mimaa);
        }

        public override LocalizationOption LoadLocalization()
        {
            return toolbox.locex();
        }

        public override ExhibitSprites LoadSprite()
        {
            // embedded resource folders are separated by a dot
            // this means exhibit image name must be exhibitid.png
            var folder = "";
            var exhibitSprites = new ExhibitSprites();
            Func<string, Sprite> wrap = (s) => ResourceLoader.LoadSprite(folder + GetId() + s + ".png", embeddedSource);
            exhibitSprites.main = wrap("");
            return exhibitSprites;
        }

        public override ExhibitConfig MakeConfig()
        {
            var exhibitConfig = new ExhibitConfig(
                Index: sequenceTable.Next(typeof(ExhibitConfig)),
                Id: "",
                Order: 10,
                IsDebug: false,
                IsPooled: true,
                IsSentinel: false,
                Revealable: false,
                Appearance: AppearanceType.Nowhere,
                Owner: "", //"Mima"
                LosableType: ExhibitLosableType.DebutLosable,
                Rarity: Rarity.Shining,
                Value1: 2,
                Value2: 1,
                Value3: 1,
                Mana: new ManaGroup() { Blue = 1 },
                BaseManaRequirement: null,
                BaseManaColor: ManaColor.Blue,
                BaseManaAmount: 0,
                HasCounter: false,
                InitialCounter: 0,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { "GuangxueMicai", "evilspirit", "Burst" },
                RelativeCards: new List<string>() { }
            );
            return exhibitConfig;
        }

        [EntityLogic(typeof(mimaa))]
        public sealed class mimaa : ShiningExhibit
        {
            protected override void OnEnterBattle()
            {
                base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
                base.HandleGameRunEvent(Owner.Dying, new GameEventHandler<DieEventArgs>(OnDying));
            }

            private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
            {
                base.NotifyActivating();
                yield return new ApplyStatusEffectAction<evilspirit>(base.Owner, new int?(base.Value1), null, null, null, 0f, true);
                yield break;
            }

            private void OnDying(DieEventArgs args)
            {
                base.NotifyActivating();
                //foreach (StatusEffect effect in (from se in this.Owner.StatusEffects
                //                                 where se.Id == "evilspirit"
                //                                 select se).ToList<StatusEffect>())
                //{
                    React(new ApplyStatusEffectAction<Burst>(Owner, base.Value2));
                    React(new ApplyStatusEffectAction<GuangxueMicai>(Owner, base.Value3));
                //}
                return;
            }
        }
    }
}
