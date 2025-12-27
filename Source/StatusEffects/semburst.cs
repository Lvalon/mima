using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class semburstDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.HasCount = true;
			return config;
		}
	}

	[EntityLogic(typeof(semburstDef))]
	public sealed class semburst : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		int truecount = 0;
		protected override void OnAdded(Unit unit)
		{
			truecount = Level;
			Count = truecount;
			Level = 0;
			HandleOwnerEvent(Battle.Player.StatusEffectAdded, OnSEAdded);
			ReactOwnerEvent(Battle.Player.TurnEnded, OnTurnEnded, GameEventPriority.Highest);
		}

		private void OnSEAdded(StatusEffectApplyEventArgs args)
		{
			truecount += Level;
			Count = truecount;
			Level = 0;
		}

		private IEnumerable<BattleAction> OnTurnEnded(UnitEventArgs args)
		{
			NotifyActivating();
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new DamageAction(Owner, Battle.EnemyGroup.Alives, DamageInfo.Attack(toolbox.Round(truecount), true), "JunkoLunatic", GunType.Single);
			if (Battle.Player.TryGetStatusEffect(out sewraitsoth se) && Battle.EnumerateAllCardsButExile().Count() > 0 && Battle.AllAliveEnemies.Count() > 0)
			{
				SelectCardInteraction interaction = new SelectCardInteraction(0, se.Level, Battle.EnumerateAllCardsButExile())
				{
					Source = this
				};
				yield return new InteractionAction(interaction);
				IReadOnlyList<Card> cards = interaction.SelectedCards;
				if (cards.Count > 0)
				{
					foreach (Card card in cards)
					{
						if (!card.IsPurified && !card.IsXCost)
						{
							card.NotifyChanged();
							card.IsPurified = true;
						}
						if (card.Zone != CardZone.Hand && BepinexPlugin.u25)
						{
							yield return new MoveCardAction(card, CardZone.Hand);
						}
					}
				}
			}
			if (!Battle.Player.HasStatusEffect<sewraitsoth>())
			{
				yield return new RemoveStatusEffectAction(this);
			}
		}
	}
}