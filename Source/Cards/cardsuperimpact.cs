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
using System.Linq;
using lvalonmima.StatusEffects;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardsuperimpactDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Red, ManaColor.Black };
			config.Cost = new ManaGroup() { Hybrid = 2, HybridColor = 7 };
			config.Rarity = Rarity.Common;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.SingleEnemy;

			config.Damage = 14;
			config.UpgradedDamage = 18;

			config.GunName = GunNameID.GetGunFromId(12140);
			config.GunNameBurst = GunNameID.GetGunFromId(12141);

			config.Value1 = 5;
			config.Value2 = 14;
			config.UpgradedValue2 = 18;

			config.RelativeEffects = new List<string>() { nameof(sesideload) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(sesideload) };

			config.RelativeKeyword = Keyword.Expel;
			config.UpgradedRelativeKeyword = Keyword.Expel;

			config.Illustrator = "カズハル／硝酸";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardsuperimpactDef))]
	public sealed class cardsuperimpact : lvalonmimaCard
	{
		bool localplaying = false;
		bool expelling = false;
		public override bool playing
		{
			get
			{
				return localplaying || expelling;
			}
		}
		public int Value10 => 10;
		public int svalue => 2;
		bool go = false;
		protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args)
		{
			expelling = true;
			try
			{
				if (go)
				{
					NotifyActivating();
					yield return SacrificeAction(Value10);
					yield return new GainPowerAction(Value2);
				}
			}
			finally
			{
				expelling = false;
			}
		}
		public override Interaction Precondition()
		{
			if (!Battle.Player.TryGetStatusEffect(out Charging se) || se.Level < svalue)
			{
				return null;
			}

			List<cardsuperimpact> list = Library.CreateCards<cardsuperimpact>(2, IsUpgraded).ToList();
			cardsuperimpact cardsuperimpact = list[0];
			cardsuperimpact cardsuperimpact2 = list[1];
			cardsuperimpact.ChoiceCardIndicator = 1;
			cardsuperimpact2.ChoiceCardIndicator = 2;
			cardsuperimpact.SetBattle(Battle);
			cardsuperimpact.Keywords = Keyword.None;
			cardsuperimpact2.SetBattle(Battle);
			cardsuperimpact2.Keywords = Keyword.None;
			return new MiniSelectCardInteraction(list, false, false, false);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			localplaying = true;
			try
			{
				go = false;
				MiniSelectCardInteraction miniSelectCardInteraction = (MiniSelectCardInteraction)precondition;
				Card card = (miniSelectCardInteraction != null) ? miniSelectCardInteraction.SelectedCard : null;
				if (card != null)
				{
					if (card.ChoiceCardIndicator == 2) // ExtraDescription2
					{
						go = true;
						if (Battle.Player.TryGetStatusEffect(out Charging se))
						{
							if (se.Level == svalue)
							{
								yield return new RemoveStatusEffectAction(se);
							}
							else
							{
								se.Level -= svalue;
							}
						}
					}
				}
				yield return AttackAction(selector);
				if (Battle.BattleShouldEnd)
				{
					yield break;
				}
				yield return BuffAction<Charging>(svalue);
				if (go)
				{
					yield return SacrificeAction(Value1);
				}
			}
			finally { localplaying = false; }
		}
	}
}


