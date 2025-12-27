using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using System.Linq;
using lvalonmima.StatusEffects;
using LBoL.Core.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardrewindDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Black, ManaColor.Green };
			config.Cost = new ManaGroup() { Any = 2, Black = 1, Green = 1 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Self;
			config.Keywords = Keyword.Exile;
			config.UpgradedKeywords = Keyword.Exile | Keyword.Retain;
			config.RelativeEffects = new List<string>() { nameof(ExtraTurn) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(ExtraTurn) };

			config.Illustrator = "カタケイ";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardrewindDef))]
	public sealed class cardrewind : lvalonmimaCard
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			foreach (Card card in Battle.HandZone.Where(c => c != this).ToList())
			{
				if (Battle.BattleShouldEnd) { yield break; }
				yield return new MoveCardToDrawZoneAction(card, DrawZoneTarget.Top);
			}
			yield return PerformAction.Effect(base.Battle.Player, "ExtraTime");
			yield return PerformAction.Sfx("ExtraTurnLaunch");
			yield return PerformAction.Animation(base.Battle.Player, "spell", 1.6f);
			yield return BuffAction<ExtraTurn>(1);
			yield return BuffAction<serewind>();
			yield return new RequestEndPlayerTurnAction();
			yield break;
		}
	}
}


