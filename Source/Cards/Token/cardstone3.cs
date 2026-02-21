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
	public sealed class cardstone3Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Blue, ManaColor.Black };
			config.Rarity = Rarity.Rare;

			config.Owner = null;
			config.IsPooled = false;
			config.HideMesuem = true;
			config.Cost = new ManaGroup() { Any = 0 };
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Nobody;
			config.IsUpgradable = false;

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(seistone) };

			config.Illustrator = "一碗清汤荞麦面丶";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardstone3Def))]
	public sealed class cardstone3 : lvalonmimaCard
	{
		protected override void EnterBattle2(BattleController battle)
		{
			ReactBattleEvent(Battle.BattleStarted, OnBattleStarted);
		}

		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			if (!Battle.EnumerateAllCardsButExile().Contains(this))
				yield break;
			yield return new ExileCardAction(this);
			if (Battle.BattleShouldEnd) { yield break; }
			if (!Battle.EnumerateAllCardsButExile().Any(c => c.Id.ToLowerInvariant().Contains("nightmana")))
				yield break;
			yield return new ExileManyCardAction(Battle.EnumerateAllCardsButExile().Where(c => c.Id.ToLowerInvariant().Contains("nightmana")));
		}
	}
}


