using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using lvalonmima.StatusEffects;
using LBoL.Core.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardmicrocosmosDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Red, ManaColor.Colorless };
			config.Cost = new ManaGroup() { Any = 2, Red = 1, Colorless = 2 };
			config.Rarity = Rarity.Rare;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Self;
			config.Keywords = Keyword.Exile;
			config.UpgradedKeywords = Keyword.Exile;
			config.RelativeEffects = new List<string>() { nameof(ExtraTurn), nameof(seabyss), nameof(semburst), nameof(Charging), nameof(seunder) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(ExtraTurn), nameof(seabyss), nameof(semburst), nameof(Charging), nameof(seunder) };

			config.Value1 = 0;
			config.UpgradedValue1 = 1;

			config.Illustrator = "camellia";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardmicrocosmosDef))]
	public sealed class cardmicrocosmos : lvalonmimaCard.trigger50card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			int extra = BepinexPlugin.u50 ? 1 : 0;
			yield return PerformAction.Effect(base.Battle.Player, "ExtraTime");
			yield return PerformAction.Sfx("ExtraTurnLaunch");
			yield return PerformAction.Animation(base.Battle.Player, "spell", 1.6f);
			yield return BuffAction<ExtraTurn>(1);
			yield return BuffAction<semicrocosmos>(Value1 + extra, 0, 0);
			yield return new RequestEndPlayerTurnAction();
			yield break;
		}
	}
}


