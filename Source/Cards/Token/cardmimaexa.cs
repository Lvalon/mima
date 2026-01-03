using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Battle.BattleActions;
using LBoL.Base.Extensions;
using LBoL.Core;
using System.Linq;
using lvalonmima.Source.Packs;

namespace lvalonmima.Cards
{
	public sealed class cardmimaexaDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.IsPooled = false;
			config.Colors = new List<ManaColor>() { ManaColor.Black, ManaColor.Green, ManaColor.Colorless };
			config.Cost = new ManaGroup() { Black = 2, Green = 2, Colorless = 1 };
			config.IsUpgradable = false;
			config.Rarity = Rarity.Rare;

			config.Keywords = Keyword.Unremovable;
			config.UpgradedKeywords = Keyword.Unremovable;

			config.Type = CardType.Ability;
			config.TargetType = TargetType.Nobody;
			config.Value1 = 12;

			config.Illustrator = "camellia";

			config.Pack = nameof(packtrumpDef)[..^3];

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardmimaexaDef))]
	public sealed class cardmimaexa : lvalonmimaCard
	{
		protected override void EnterBattle2(BattleController battle)
		{
			ReactBattleEvent(Battle.BattleStarted, OnBattleStarted, GameEventPriority.Highest + 100);
		}

		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			if (Battle.EnumerateAllCards().Count(c => c.CardType == CardType.Ability && c != this) > 0)
			{
				foreach (Card card in Battle.EnumerateAllCards().Where(c => c.CardType == CardType.Ability && c != this).ToList())
				{
					if (Battle.BattleShouldEnd) { yield break; }
					yield return new PlayCardAction(card);
				}
				if (Battle.BattleShouldEnd) { yield break; }
			}
			yield return new PlayCardAction(this);
		}
	}
}


