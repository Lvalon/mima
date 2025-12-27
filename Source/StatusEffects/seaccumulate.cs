using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;
using lvalonmima.Cards;

namespace lvalonmima.StatusEffects
{
	public sealed class seaccumulateDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			config.Keywords = Keyword.Overdraft | Keyword.Purify;
			config.HasCount = true;
			config.CountStackType = StackType.Add;
			config.RelativeEffects = new List<string>() { nameof(seunder) };
			return config;
		}
	}

	[EntityLogic(typeof(seaccumulateDef))]
	public sealed class seaccumulate : sehl25
	{
		public ManaGroup Mana
		{
			get
			{
				if (Owner == null)
				{
					return ManaGroup.Whites(1) + ManaGroup.Greens(1);
				}
				else
				{
					return ManaGroup.Whites(Level) + ManaGroup.Greens(Level);
				}
			}
		}
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.Player.TurnStarted, OnTurnStarted);
			ReactOwnerEvent(Battle.BattleEnding, OnBattleEnding);
		}

		private IEnumerable<BattleAction> OnBattleEnding(GameEventArgs args)
		{
			NotifyActivating();
			yield return new GainMoneyAction(Count);
		}

		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			NotifyActivating();
			yield return new GainManaAction(Mana);
			if (!BepinexPlugin.u25)
			{
				yield return new LockRandomTurnManaAction(Level);
				if (Battle.BattleMana.HasTrivial)
				{
					yield return ConvertManaAction.Purify(Battle.BattleMana, Level);
				}
			}
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<cardpurediamond>(Level, false), DrawZoneTarget.Random, AddCardsType.Normal);
		}
	}
}