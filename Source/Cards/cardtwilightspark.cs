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
using LBoL.Core.Units;

namespace lvalonmima.Cards
{
	public sealed class cardtwilightsparkDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Red, ManaColor.Colorless };
			config.Cost = new ManaGroup() { Any = 2, Red = 2, Colorless = 2 };
			config.Rarity = Rarity.Rare;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.AllEnemies;

			config.Damage = 60;
			config.UpgradedDamage = 72;

			config.GunName = GunNameID.GetGunFromId(12180);
			config.GunNameBurst = GunNameID.GetGunFromId(12181);

			config.Value1 = 60;
			config.UpgradedValue1 = 72;
			config.Value2 = 20;

			config.Mana = new ManaGroup() { Any = 1 };

			config.RelativeEffects = new List<string>() { nameof(sesideload), nameof(Burst), nameof(seunder) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(sesideload), nameof(Burst), nameof(seunder) };

			config.Keywords = Keyword.Accuracy | Keyword.Exile | Keyword.Ethereal;
			config.UpgradedKeywords = Keyword.Accuracy | Keyword.Exile | Keyword.Ethereal;

			config.Illustrator = "美しあくま";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardtwilightsparkDef))]
	public sealed class cardtwilightspark : lvalonmimaCard.trigger10card
	{
		public int Value4 => 4;
		int manaleft = 0;
		double manamult = 0;
		int bonus = 1;
		int sum = 0;
		public int showbase
		{
			get
			{
				if (Battle == null) { return 0; }
				int burstmult = Battle.Player.TryGetStatusEffect(out Burst se) ? se.DamageRate : 1;
				return toolbox.Round((1 + (Battle.BattleMana.Amount - Cost.Amount) * 1.0 * Value2 / 100) * Value1 * (BepinexPlugin.u10 ? burstmult : 1));
			}
		}
		public int showbase2
		{
			get
			{
				if (Battle == null) { return 0; }
				int burstmult = Battle.Player.TryGetStatusEffect(out Burst se) ? se.DamageRate : 1;
				return toolbox.Round((1 + Battle.BattleMana.Amount * 1.0 * Value2 / 100) * Value1 * (BepinexPlugin.u10 ? burstmult : 1));
			}
		}
		public string showtextfs
		{
			get
			{
				if (Battle == null) { return " "; }
				return " (" + showbase + ")";
			}
		}
		public string showtextfs2
		{
			get
			{
				if (Battle == null) { return " "; }
				return " (" + showbase2 + ")";
			}
		}
		public string showtext
		{
			get
			{
				if (Battle == null) { return ""; }
				return " (" + showbase + ") ";
			}
		}
		public string showtext2
		{
			get
			{
				if (Battle == null) { return ""; }
				return " (" + showbase2 + ") ";
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			manaleft = 0;
			manamult = 1;
			bonus = 1;
			sum = Value1;
			if (IsUpgraded)
			{
				yield return BuffAction<Charging>(Value4);
			}
			if (Battle.Player.TryGetStatusEffect(out Burst se))
			{
				List<cardtwilightspark> list = Library.CreateCards<cardtwilightspark>(2, IsUpgraded).ToList();
				cardtwilightspark cardtwilightspark = list[0];
				cardtwilightspark cardtwilightspark2 = list[1];
				cardtwilightspark.ChoiceCardIndicator = 1;
				cardtwilightspark2.ChoiceCardIndicator = 2;
				cardtwilightspark.SetBattle(Battle);
				cardtwilightspark.Keywords = Keyword.None;
				cardtwilightspark2.SetBattle(Battle);
				cardtwilightspark2.Keywords = Keyword.None;
				MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(list, false, false, false);
				yield return new InteractionAction(interaction);
				Card card = interaction?.SelectedCard;
				if (card != null && card.ChoiceCardIndicator == 2) // ExtraDescription2
				{
					if (BepinexPlugin.u10)
					{
						bonus = se.DamageRate;
					}
					else
					{
						yield return new RemoveStatusEffectAction(se);
					}
					manaleft = Battle.BattleMana.Amount;
					manamult += manaleft * 1.0 * Value2 / 100;
					yield return new ExileManyCardAction(Battle.HandZone.Where(c => c != this));
					yield return new LoseManaAction(Battle.BattleMana);
					int tmp = sum;
					sum *= toolbox.Round(bonus * manamult);
					foreach (Unit unit in Battle.AllAliveEnemies)
					{
						if (unit.Hp < sum && Battle.AllAliveEnemies.Count() > 0)
						{
							yield return new ForceKillAction(Battle.Player, unit);
						}
					}
					sum = toolbox.Round(tmp * manamult);
				}
			}
			yield return new DamageAction(Battle.Player, Battle.AllAliveEnemies, DamageInfo.Attack(sum, Damage.IsAccuracy), GunName);
		}
	}
}


