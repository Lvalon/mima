using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core;
using System;

namespace lvalonmima.Cards
{
	public sealed class cardpwaveDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Colorless };
			config.Cost = new ManaGroup() { Any = 1, Colorless = 1 };
			config.Rarity = Rarity.Common;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.AllEnemies;

			config.Keywords = Keyword.Accuracy;
			config.UpgradedKeywords = Keyword.Accuracy;

			config.Damage = 12;

			config.GunName = GunNameID.GetGunFromId(25130);
			config.GunNameBurst = GunNameID.GetGunFromId(25131);

			config.Value1 = 6;
			config.UpgradedValue1 = 3;

			config.Value2 = 1;

			config.Illustrator = "灯跡（ヒセキ）";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardpwaveDef))]
	public sealed class cardpwave : lvalonmimaCard
	{
		public override int AdditionalDamage
		{
			get
			{
				if (Battle != null)
				{
					return (int)Math.Floor((Battle.Player.MaxHp - Battle.Player.Hp) * 1.0 / Value1);
				}
				return 0;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return AttackAction(selector);
		}
	}
}


