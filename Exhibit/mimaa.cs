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
using LBoL.Core.Randoms;
using static lvalonmima.SE.transmigrateddef;

namespace lvalonmima.Exhibit
{
    public sealed class mimaadef : ExhibitTemplate
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
                Owner: "Mima", //"Mima"
                LosableType: ExhibitLosableType.DebutLosable,
                Rarity: Rarity.Shining,
                Value1: 2,
                Value2: 2,
                Value3: 1,
                Mana: new ManaGroup() { Blue = 1 },
                BaseManaRequirement: null,
                BaseManaColor: ManaColor.Blue,
                BaseManaAmount: 1,
                HasCounter: false,
                InitialCounter: null,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { "evilspirit", "transmigrated" },
                RelativeCards: new List<string>() { }
            );
            return exhibitConfig;
        }

        [EntityLogic(typeof(mimaadef))]
        public sealed class mimaa : ShiningExhibit
        {
            private int oghp;
            private int ogmax;
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
                ogmax = Owner.MaxHp;
                ogdmg = args.DamageInfo.Damage.RoundToInt();
            }

            private void OnPlayerDamageReceived(DamageEventArgs args)
            {
                if (Owner.Hp == Owner.MaxHp && ogdmg > 0 && ((oghp - ogdmg != Owner.Hp - ogdmg) || ogdmg >= ogmax))
                {
                    base.NotifyActivating();
                    React(new ApplyStatusEffectAction<transmigrated>(base.Owner, new int?(base.Value2), null, null, null, 0f, true));
                }
            }
        }
    }
}
