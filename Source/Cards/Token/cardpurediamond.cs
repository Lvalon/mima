using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;

namespace lvalonmima.Cards
{
	public sealed class cardpurediamondDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.IsPooled = false;
			config.Colors = new List<ManaColor>() { ManaColor.Colorless };
			config.Cost = new ManaGroup() { Any = 3 };
			config.IsUpgradable = false;
			config.Rarity = Rarity.Rare;

			config.Type = CardType.Status;
			config.TargetType = TargetType.Nobody;
			config.Damage = 5;
			config.Value1 = 5;
			config.Mana = new ManaGroup() { Philosophy = 1 };

			config.Keywords = Keyword.Forbidden | Keyword.Replenish;
			config.UpgradedKeywords = Keyword.Forbidden | Keyword.Replenish;

			config.Illustrator = "半熟とまと";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardpurediamondDef))]
	public sealed class cardpurediamond : lvalonmimaCard
	{
		public override IEnumerable<BattleAction> OnDraw()
		{
			return EnterHandReactor();
		}

		public override IEnumerable<BattleAction> OnMove(CardZone srcZone, CardZone dstZone)
		{
			return dstZone != CardZone.Hand ? null : EnterHandReactor();
		}

		protected override void EnterBattle2(BattleController battle)
		{
			if (Zone == CardZone.Hand)
			{
				React(EnterHandReactor());
			}
			ReactBattleEvent(Battle.CardExiled, OnCardExiled);
		}

		private IEnumerable<BattleAction> OnCardExiled(CardEventArgs args)
		{
			if (args.Card == this)
			{
				if (Battle.BattleShouldEnd) { yield break; }
				yield return new GainManaAction(Mana);
				if (Battle.BattleShouldEnd) { yield break; }
				yield return new DamageAction(Battle.Player, Battle.AllAliveUnits, DamageInfo.Reaction(Value1), GunNameID.GetGunFromId(400), GunType.Single);
			}
		}
		private IEnumerable<BattleAction> EnterHandReactor()
		{
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new ExileCardAction(this);
		}
	}
}


