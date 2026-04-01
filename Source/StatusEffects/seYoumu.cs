using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Others;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seYoumuDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(seYoumuDef))]
	public sealed class seYoumu : StatusEffect
	{
		Dictionary<Card, (int, CardZone)> cards = new Dictionary<Card, (int, CardZone)>();
		protected override void OnAdded(Unit unit)
		{
			cards = new Dictionary<Card, (int, CardZone)>();
			ReactOwnerEvent(Battle.Player.DamageReceived, OnDmgReceived);
			ReactOwnerEvent(Battle.RoundEnded, OnRoundEnded);
		}

		private IEnumerable<BattleAction> OnRoundEnded(GameEventArgs args)
		{
			bool moved = false;
			foreach (Card card in cards.Keys.ToList())
			{
				if (cards.TryGetValue(card, out var kvp))
				{
					cards[card] = (kvp.Item1 - 1, kvp.Item2);
					if (kvp.Item1 == 0)
					{
						if (!moved)
						{
							NotifyActivating();
							moved = true;
						}
						if (card.Zone == CardZone.Exile)
						{
							if (kvp.Item2 == CardZone.Draw)
							{
								yield return new MoveCardToDrawZoneAction(card, DrawZoneTarget.Random);
							}
							else
							{
								yield return new MoveCardAction(card, kvp.Item2);
							}
						}
						cards.Remove(card);
					}
				}
			}
		}

		private IEnumerable<BattleAction> OnDmgReceived(DamageEventArgs args)
		{
			if (args.DamageInfo.IsGrazed)
			{
				Card sampled = Battle.EnumerateAllCardsButExile().SampleOrDefault(GameRun.EnemyBattleRng);
				if (sampled != null)
				{
					NotifyActivating();
					if (cards.ContainsKey(sampled))
						cards.Remove(sampled);
					cards.Add(sampled, (1, sampled.Zone));
					yield return new ExileCardAction(sampled);
				}
			}
		}
	}
}