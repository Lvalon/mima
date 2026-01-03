using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using lvalonmima.StatusEffects;
using lvalonmima.Source.Packs;

namespace lvalonmima.Cards
{
	public sealed class cardmimaexbDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.IsPooled = false;
			config.Colors = new List<ManaColor>() { ManaColor.Colorless };
			config.IsUpgradable = false;
			config.Cost = new ManaGroup() { Colorless = 5 };
			config.Rarity = Rarity.Rare;
			config.Type = CardType.Ability;
			config.TargetType = TargetType.Nobody;
			config.RelativeKeyword = Keyword.Purified;
			config.RelativeEffects = new List<string>() { nameof(seabyss) };

			config.Pack = nameof(packtrumpDef)[..^3];

			config.Keywords = Keyword.Unremovable;
			config.UpgradedKeywords = Keyword.Unremovable;

			config.Value1 = 12;
			config.Value2 = 1;

			config.Illustrator = "あい";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardmimaexbDef))]
	public sealed class cardmimaexb : lvalonmimaCard
	{
		protected override void EnterBattle2(BattleController battle)
		{
			ReactBattleEvent(Battle.BattleStarted, OnBattleStarted, GameEventPriority.Highest + 100);
		}

		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			// if (Battle.EnumerateAllCards().Count(c => !c.IsPurified && c != this) > 0)
			// {
			// 	foreach (Card card in Battle.EnumerateAllCards().Where(c => !c.IsPurified && c != this && !c.IsXCost).ToList())
			// 	{
			// 		if (Battle.BattleShouldEnd) { yield break; }
			// 		card.IsPurified = true;
			// 	}
			// }
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new ApplyStatusEffectAction<semimaexb>(Battle.Player, 1, 0, 0, 0);
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new ApplyStatusEffectAction<seabyss>(Battle.Player, 1, 0, 0, 0);
			yield return new PlayCardAction(this);
		}
	}
}


