using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Presentation;
using LBoLEntitySideloader.Resource;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using static lvalonmima.SE.transcendeddef;
using static UnityEngine.UI.GridLayoutGroup;

namespace lvalonmima
{
    public abstract class toolbox
    {

        static public Card[] RollCardsCustom(RandomGen rng, CardWeightTable weightTable, int count, ManaGroup? manaLimit = null, bool colorLimit = false, bool applyFactors = false, bool battleRolling = false, bool ensureCount = false, Predicate<Card> filter = null)
        {
            var gr = GameMaster.Instance?.CurrentGameRun;
            if (gr == null)
                throw new InvalidOperationException("Rolling cards when run is not started.");

            var innitialPool = gr.CreateValidCardsPool(weightTable, manaLimit, colorLimit, applyFactors, battleRolling, null);

            var filteredPool = new UniqueRandomPool<Card>();

            foreach (var e in innitialPool)
            {
                var card = Library.CreateCard(e.Elem);
                if (filter(card))
                {
                    card.GameRun = gr;
                    filteredPool.Add(card, e.Weight);
                }
            }

            return filteredPool.SampleMany(rng, count, ensureCount);
        }

        //ONREVIVE

        //private int oghp;
        //private int ogmax;
        //private int ogdmg;

        //protected override void OnEnterBattle(BattleController battle)
        //{
        //    base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageTaking));
        //    base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageReceived, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageReceived));
        //}

        //private void OnPlayerDamageTaking(DamageEventArgs args)
        //{
        //    oghp = Owner.Hp;
        //    ogmax = Owner.MaxHp;
        //    ogdmg = args.DamageInfo.Damage.RoundToInt();
        //}

        //private void OnPlayerDamageReceived(DamageEventArgs args)
        //{
        //    if (Owner.Hp == Owner.MaxHp && ogdmg > 0 && ((oghp - ogdmg != Owner.Hp - ogdmg) || ogdmg >= ogmax))
        //    {
        //        base.NotifyActivating();
        //    }
        //}

        //private void OnPlayerDamageTaking(DamageEventArgs args)
        //{
        //    oghp = base.Battle.Player.Hp;
        //    ogmax = base.Battle.Player.MaxHp;
        //    ogdmg = args.DamageInfo.Damage.RoundToInt();
        //}
        //private void OnPlayerDamageReceived(DamageEventArgs args)
        //{
        //    if (base.Battle.Player.Hp == base.Battle.Player.MaxHp && ogdmg > 0 && ((oghp - ogdmg != base.Battle.Player.Hp - ogdmg) || ogdmg >= ogmax))
        //    {
        //        base.NotifyActivating();
        //    }
        //}
    }
}