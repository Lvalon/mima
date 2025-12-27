using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;

namespace lvalonmima.Cards
{
	public sealed class cardmimabDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.IsPooled = false;

			config.Colors = new List<ManaColor>() { ManaColor.Black, ManaColor.Colorless };
			config.Cost = new ManaGroup() { Any = 2 };
			config.Rarity = Rarity.Common;

			config.Type = CardType.Attack;
			config.TargetType = TargetType.SingleEnemy;

			config.Damage = 10;
			config.UpgradedDamage = 14;

			config.GunName = GunNameID.GetGunFromId(25010);
			config.GunNameBurst = GunNameID.GetGunFromId(25011);

			//The card is treated as a basic card. 
			config.Keywords = Keyword.Basic;
			config.UpgradedKeywords = Keyword.Basic;

			config.Illustrator = "Barudo";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardmimabDef))]
	public sealed class cardmimab : lvalonmimaCard
	{
		//By default, if config.Damage / config.Block / config.Shield are set:
		//The card will deal damage or gain Block/Barrier without having to set anything.
		//Here, this is is equivalent to the following code.

		/*protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return DefenseAction(true); 
        }*/
	}
}


