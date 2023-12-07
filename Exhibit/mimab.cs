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
using LBoL.EntityLib.StatusEffects.Enemy.SeijaItems;

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
                Value2: 2,
                Value3: 10,
                Mana: new ManaGroup() { Black = 1 },
                BaseManaRequirement: null,
                BaseManaColor: ManaColor.Black,
                BaseManaAmount: 1,
                HasCounter: false,
                InitialCounter: null,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { "evilspirit", "Firepower" },
                RelativeCards: new List<string>() { }
            );
            return exhibitConfig;
        }

        [EntityLogic(typeof(mimabdef))]
        public sealed class mimab : ShiningExhibit
        {
            private int oghp;
            private int ogdmg;

            protected override void OnEnterBattle()
            {
                base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
                base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageTaking));
                base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageReceived, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageReceived));
            }

            private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
            {
                base.NotifyActivating();
                yield return new ApplyStatusEffectAction<evilspirit>(base.Owner, new int?(base.Value1), null, null, null, 0f, true);
                yield break;
            }

            private void OnPlayerDamageTaking(DamageEventArgs args)
            {
                oghp = Owner.Hp;
                ogdmg = args.DamageInfo.Damage.RoundToInt();
            }

            private void OnPlayerDamageReceived(DamageEventArgs args)
            {
                if (Owner.Hp >= oghp && ogdmg > 0 && oghp <= ogdmg)
                {
                    base.NotifyActivating();
                    React(new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Value2), null, null, null, 0f, true));
                    React(new DamageAction(base.Owner, base.Battle.EnemyGroup.Alives, DamageInfo.Attack((float)base.Value3, false), "InfinityGemsSe1", GunType.Single));//ExhFeixiang
                }
            }

            private void OnDying(DieEventArgs args)
            {
                return;
            }
        }
    }
}
