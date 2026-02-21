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
using System;

namespace lvalonmima.Cards
{
	public sealed class cardgenjiDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Owner = null;
			config.IsPooled = false;
			config.Colors = new List<ManaColor>() { ManaColor.Green };
			config.Cost = new ManaGroup() { Any = 0 };
			config.Rarity = Rarity.Common;
			config.IsUpgradable = false;
			config.Loyalty = 9;

			config.Keywords = Keyword.Forbidden;

			config.Type = CardType.Friend;
			config.HideMesuem = true;
			config.TargetType = TargetType.Nobody;

			config.Illustrator = "Men-dont-scream";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardgenjiDef))]
	public sealed class cardgenji : lvalonmimaCard
	{
		protected override void EnterBattle2(BattleController battle)
		{
			HandleBattleEvent(Battle.Predraw, OnPredraw);
		}

		private void OnPredraw(CardEventArgs args)
		{
			if (Zone == CardZone.Hand && args.Cause != ActionCause.TurnStart && (args.Cause != ActionCause.Card || !(args.ActionSource is Card card && card.IsReplenish) || Battle.Player.IsInTurn))
			{
				args.CancelBy(this);
				NotifyActivating();
			}
		}
	}
}


