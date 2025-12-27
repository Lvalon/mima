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
	public sealed class cardimplodemagicDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Red, ManaColor.Green };
			config.Cost = new ManaGroup() { Red = 1, Green = 1 };
			config.Rarity = Rarity.Common;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.AllEnemies;

			config.Damage = 8;

			config.GunName = GunNameID.GetGunFromId(4522);
			config.GunNameBurst = GunNameID.GetGunFromId(4521);

			config.Value1 = 8;
			config.Value2 = 1;

			config.RelativeEffects = new List<string>() { nameof(semburst), nameof(sesideload) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(semburst), nameof(sesideload) };

			config.Keywords = Keyword.FollowCard;
			config.UpgradedKeywords = Keyword.FollowCard;

			config.UpgradedRelativeKeyword = Keyword.Expel;

			config.Illustrator = "hachi (8bit canvas)";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardimplodemagicDef))]
	public sealed class cardimplodemagic : lvalonmimaCard
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
		public int svalue => 1;
		bool go = false;
		public override int AdditionalDamage
		{
			get
			{
				if (Battle != null && go && IsUpgraded && Battle.Player.TryGetStatusEffect(out semburst se))
				{
					return se.Count;
				}
				return 0;
			}
		}
		public override DamageInfo Damage
		{
			get
			{
				if (Battle != null && go)
				{
					return DamageInfo.Attack(RawDamage, true);
				}
				return DamageInfo.Attack(RawDamage, IsAccuracy);
			}
		}
		protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args)
		{
			expelling = true;
			try
			{
				if (IsUpgraded)
				{
					NotifyActivating();
					GameRun.SetHpAndMaxHp(Battle.Player.Hp + Value2, Battle.Player.MaxHp + Value2, true);
				}
				yield break;
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

			List<cardimplodemagic> list = Library.CreateCards<cardimplodemagic>(2, IsUpgraded).ToList();
			cardimplodemagic cardimplodemagic = list[0];
			cardimplodemagic cardimplodemagic2 = list[1];
			cardimplodemagic.ChoiceCardIndicator = 1;
			cardimplodemagic2.ChoiceCardIndicator = 2;
			cardimplodemagic.SetBattle(Battle);
			cardimplodemagic.Keywords = Keyword.None;
			cardimplodemagic2.SetBattle(Battle);
			cardimplodemagic2.Keywords = Keyword.None;
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
				yield return BuffAction<semburst>(Value1, 0, 0);
				if (Battle.BattleShouldEnd) { yield break; }
				yield return AttackAction(selector);
			}
			finally
			{
				localplaying = false;
			}
		}
	}
}


