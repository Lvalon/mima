using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;
using LBoL.EntityLib.Cards.Character.Cirno;
using LBoL.Core.Battle;
using LBoL.Core;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using System.Linq;

namespace lvalonmima.Cards
{
	public sealed class cardstone2Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Black, ManaColor.Green };
			config.Rarity = Rarity.Rare;

			config.Owner = null;
			config.IsPooled = false;
			config.HideMesuem = true;
			config.Cost = new ManaGroup() { Any = 0 };
			config.Value1 = 11;
			config.Type = CardType.Ability;
			config.TargetType = TargetType.Nobody;
			config.IsUpgradable = false;

			config.RelativeEffects = new List<string>() { nameof(seistone) };

			config.Illustrator = "おしゃむ";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardstone2Def))]
	public sealed class cardstone2 : lvalonmimaCard
	{
		protected override void EnterBattle2(BattleController battle)
		{
			ReactBattleEvent(Battle.BattleStarted, OnBattleStarted);
		}

		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			if (!Battle.EnumerateAllCards().Contains(this))
				yield break;
			yield return new PlayCardAction(this);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ApplyStatusEffectAction<sestone2>(Battle.Player, Value1);
		}
	}
}


