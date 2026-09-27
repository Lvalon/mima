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

namespace lvalonmima.Cards;

public sealed class cardtwilightsparkDef : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig();
		config.Colors = [ManaColor.Red, ManaColor.Colorless];
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

		config.RelativeEffects = config.UpgradedRelativeEffects = [nameof(sesideload), nameof(Burst), nameof(seunder)];

		config.Keywords = config.UpgradedKeywords = Keyword.Accuracy | Keyword.Exile | Keyword.Ethereal;

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
			if (Battle == null) return 0;
			int burstmult = Battle.Player.TryGetStatusEffect(out Burst se) ? se.DamageRate : 1;
			return toolbox.Round((1 + (Battle.BattleMana.Amount - Cost.Amount) * 1.0 * Value2 / 100) * Value1 * (BepinexPlugin.u10 ? burstmult : 1));
		}
	}
	public int showbase2
	{
		get
		{
			if (Battle == null) return 0;
			int burstmult = Battle.Player.TryGetStatusEffect(out Burst se) ? se.DamageRate : 1;
			return toolbox.Round((1 + Battle.BattleMana.Amount * 1.0 * Value2 / 100) * Value1 * (BepinexPlugin.u10 ? burstmult : 1));
		}
	}
	public string showtextfs
	{
		get
		{
			if (Battle == null) return " ";
			return " (" + showbase + ")";
		}
	}
	public string showtextfs2
	{
		get
		{
			if (Battle == null) return " ";
			return " (" + showbase2 + ")";
		}
	}
	public string showtext
	{
		get
		{
			if (Battle == null) return "";
			return " (" + showbase + ") ";
		}
	}
	public string showtext2
	{
		get
		{
			if (Battle == null) return "";
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
			yield return BuffAction<Charging>(Value4);
		if (Battle.Player.TryGetStatusEffect(out Burst se))
		{
			MiniSelectCardInteraction interaction = CreateChoicePair<cardtwilightspark>();
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
				// Round the whole product once, matching showbase/showbase2's rounding
				// order, so the previewed number and the real damage agree; the previous
				// code rounded (bonus * manamult) first and multiplied by tmp separately,
				// which could drift from the preview, and then discarded bonus entirely
				// when recomputing sum for the final DamageAction below.
				sum = toolbox.Round(bonus * manamult * tmp);
				foreach (Unit unit in Battle.AllAliveEnemies.ToList())
				{
					if (unit.Hp < sum)
					{
						yield return new ForceKillAction(Battle.Player, unit);
					}
				}
			}
		}
		yield return new DamageAction(Battle.Player, Battle.AllAliveEnemies, DamageInfo.Attack(sum, Damage.IsAccuracy), GunName);
	}
}
