using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardcdarknessDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Colorless, ManaColor.Black };
			config.Cost = new ManaGroup() { Black = 2, Colorless = 2 };
			config.Rarity = Rarity.Rare;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.AllEnemies;
			config.FindInBattle = false;

			config.GunName = GunNameID.GetGunFromId(4522);
			config.GunNameBurst = GunNameID.GetGunFromId(4522);

			config.Damage = 1;
			config.UpgradedKeywords = Keyword.Retain;

			config.RelativeKeyword = Keyword.Purify;

			config.Value1 = 1;

			config.Illustrator = "くまばち";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardcdarknessDef))]
	public sealed class cardcdarkness : lvalonmimaCard
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (Battle.BattleMana.HasTrivial && !IsUpgraded)
			{
				yield return ConvertManaAction.Purify(Battle.BattleMana, Battle.BattleMana.Amount);
			}
			if (Battle.BattleShouldEnd) { yield break; }
			yield return AttackAction(selector);
			if (Battle.BattleShouldEnd) { yield break; }
			yield return BuffAction<secdarkness>(Value1);
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new RemoveCardAction(this);
		}
	}
}


