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
using static lvalonmima.Exhibit.mimaadef;
using LBoL.Core.Cards;

namespace lvalonmima.Exhibit
{
    public sealed class mimabdef : ExhibitTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(mimab);
        }

        public override LocalizationOption LoadLocalization()
        {
            return toolbox.locex();
        }

        public override ExhibitSprites LoadSprite()
        {
            // embedded resource folders are separated by a dot
            // this means exhibit image name must be exhibitid.png
            var folder = "Resources.";
            var exhibitSprites = new ExhibitSprites();
            Func<string, Sprite> wrap = (s) => ResourceLoader.LoadSprite(folder + GetId() + s + ".png", embeddedSource);
            exhibitSprites.main = wrap("");
            return exhibitSprites;
        }

        public override ExhibitConfig MakeConfig()
        {
            var exhibitConfig = new ExhibitConfig(
                Index: 0,
                Id: "",
                Order: 10,
                IsDebug: false,
                IsPooled: false,
                IsSentinel: false,
                Revealable: false,
                Appearance: AppearanceType.Nowhere,
                Owner: "", //"Mima"
                LosableType: ExhibitLosableType.DebutLosable,
                Rarity: Rarity.Shining,
                Value1: 2,
                Value2: 10,
                Value3: 1,
                Mana: new ManaGroup() { Black = 1 },
                BaseManaRequirement: null,
                BaseManaColor: ManaColor.Black,
                BaseManaAmount: 0,
                HasCounter: false,
                InitialCounter: null,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { "Firepower", "evilspirit" },
                RelativeCards: new List<string>() { }
            );
            return exhibitConfig;
        }

        [EntityLogic(typeof(mimabdef))]
        public sealed class mimab : ShiningExhibit
        {
            protected override void OnEnterBattle()
            {
                base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
                //base.HandleGameRunEvent(Owner.Dying, new GameEventHandler<DieEventArgs>(OnDying));
            }

            private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
            {
                base.NotifyActivating();
                yield return new ApplyStatusEffectAction<evilspirit>(base.Owner, new int?(base.Value1), null, null, null, 0f, true);
                //yield return new DamageAction(base.Owner, base.Battle.EnemyGroup.Alives, DamageInfo.Attack((float)base.Value2, false), "ExhFeixiang", GunType.Single);
                yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Value3), null, null, null, 0f, true);
                yield break;
            }


            private void OnDying(DieEventArgs args)
            {
                //base.NotifyActivating();
                //foreach (var se in Owner.StatusEffects.Where(se => se.Id == "evilspirit"))
                //{
                //React(new DamageAction(base.Owner, base.Battle.EnemyGroup.Alives, DamageInfo.Attack((float)base.Value2, false), "ExhFeixiang", GunType.Single));
                //React(new ApplyStatusEffectAction<Burst>(base.Owner, new int?(base.Value2), null, null, null, 0f, true));
                //}
                return;
            }
        }
    }
}
