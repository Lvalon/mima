using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seabyssDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Keywords = Keyword.Purified | Keyword.Morph;
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(seabyssDef))]
	public sealed class seabyss : StatusEffect
	{
		public ManaGroup Mana => new ManaGroup() { Colorless = 2 };
		protected override void OnAdded(Unit unit)
		{
			Highlight = true;
			ReactOwnerEvent(Battle.Player.TurnEnded, OnTurnEnded);
			ReactOwnerEvent(Battle.Player.TurnStarted, OnTurnStarted);
			React(FirstGain(base.Level));
		}

		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			NotifyActivating();
			yield return new GainManaAction(Mana);
			foreach (Card card in Battle.EnumerateAllCards().Where(card => card.Zone == CardZone.Hand && card.IsPurified && !card.IsXCost))
			{
				card.BaseCost = new ManaGroup();
			}
			foreach (Card card in Battle.EnumerateAllCards().Where(card => card.Zone == CardZone.Hand && !card.IsPurified && !card.IsXCost))
			{
				card.NotifyChanged();
				card.IsPurified = true;
			}
		}

		public override bool Stack(StatusEffect other)
		{
			bool num = base.Stack(other);
			if (num)
			{
				React(StackGain(other.Level));
			}
			return num;
		}
		public IEnumerable<BattleAction> FirstGain(int level = 1)
		{
			NotifyActivating();
			if (Owner.MaxHp <= level)
			{
				yield return new ForceKillAction(Owner, Owner);
			}
			else if (Owner == Battle.Player)
			{
				GameRun.LoseMaxHp(level, true);
				if (Owner.HasStatusEffect<seevilspirit>() && Owner.MaxHp < 6)
				{
					yield return new ForceKillAction(Owner, Owner);
				}
			}

			if (Battle.BattleShouldEnd) { yield break; }

			yield return new GainManaAction(Mana * level);
			foreach (Card card in Battle.EnumerateAllCards().Where(card => card.Zone == CardZone.Hand && (card.IsPurified || level > 1) && !card.IsXCost))
			{
				card.BaseCost = new ManaGroup();
			}
			foreach (Card card in Battle.EnumerateAllCards().Where(card => card.Zone == CardZone.Hand && !card.IsPurified && !card.IsXCost))
			{
				card.NotifyChanged();
				card.IsPurified = true;
			}
		}

		public IEnumerable<BattleAction> StackGain(int level = 1)
		{
			NotifyActivating();
			if (Owner.MaxHp <= level)
			{
				yield return new ForceKillAction(Owner, Owner);
			}
			else if (Owner == Battle.Player)
			{
				GameRun.LoseMaxHp(level, true);
				if (Owner.HasStatusEffect<seevilspirit>() && Owner.MaxHp < 6)
				{
					yield return new ForceKillAction(Owner, Owner);
				}
			}

			if (Battle.BattleShouldEnd) { yield break; }

			yield return new GainManaAction(Mana * level);
			foreach (Card card in Battle.EnumerateAllCards().Where(card => card.Zone == CardZone.Hand && (card.IsPurified || level > 1) && !card.IsXCost))
			{
				card.BaseCost = new ManaGroup();
			}
			foreach (Card card in Battle.EnumerateAllCards().Where(card => card.Zone == CardZone.Hand && !card.IsPurified && !card.IsXCost))
			{
				card.NotifyChanged();
				card.IsPurified = true;
			}
		}
		private IEnumerable<BattleAction> OnTurnEnded(UnitEventArgs args)
		{
			if (Level <= 1)
			{
				yield return new RemoveStatusEffectAction(this);
			}
			else
			{
				Level--;
			}
			yield break;
		}
	}
}