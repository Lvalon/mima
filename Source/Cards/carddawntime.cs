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
	public sealed class carddawntimeDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Blue, ManaColor.Black, ManaColor.Green, ManaColor.Colorless };
			config.Cost = new ManaGroup() { Blue = 1, Black = 2, Green = 2, Colorless = 1 };
			config.Rarity = Rarity.Rare;

			config.FindInBattle = false;

			config.Type = CardType.Skill;
			config.TargetType = TargetType.Self;
			config.Keywords = Keyword.Exile | Keyword.Ethereal;
			config.UpgradedKeywords = Keyword.Exile | Keyword.Ethereal;
			config.RelativeKeyword = Keyword.TempMorph;
			config.UpgradedRelativeKeyword = Keyword.TempMorph;
			config.RelativeEffects = new List<string>() { nameof(ExtraTurn), nameof(seunder) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(ExtraTurn), nameof(seunder) };
			config.Mana = new ManaGroup() { Any = 0 };
			config.Value1 = 7;
			config.UpgradedValue1 = 10;

			config.Illustrator = "camellia";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(carddawntimeDef))]
	public sealed class carddawntime : lvalonmimaCard.trigger10card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return PerformAction.Effect(base.Battle.Player, "ExtraTime");
			yield return PerformAction.Sfx("ExtraTurnLaunch");
			yield return PerformAction.Animation(base.Battle.Player, "spell", 1.6f);
			yield return BuffAction<ExtraTurn>(1);
			yield return BuffAction<sedawntime>(BepinexPlugin.u10 ? 1 : 0, 0, 0, Value1);
			yield return new RequestEndPlayerTurnAction();
			yield break;
		}
	}
}


