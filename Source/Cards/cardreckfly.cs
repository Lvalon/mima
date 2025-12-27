using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Base.Extensions;
using LBoL.Core;
using System;
using lvalonmima.StatusEffects;
using LBoL.Core.Units;
using LBoL.Presentation;
using lvalonmima.SFX.Template;
using lvalonmima.SFX;

namespace lvalonmima.Cards
{
	public sealed class cardreckflyDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Colorless, ManaColor.Black };
			config.Cost = new ManaGroup() { Black = 3, Colorless = 2 };
			config.Rarity = Rarity.Rare;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.RandomEnemy;
			config.FindInBattle = false;

			config.Mana = new ManaGroup() { Colorless = 1 };

			config.Damage = 0;

			config.Value1 = 6;
			config.Value2 = 1;
			config.UpgradedValue2 = 2;

			config.Illustrator = "ツバネ";

			config.RelativeEffects = new List<string>() { nameof(seunder) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(seunder) };

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardreckflyDef))]
	public sealed class cardreckfly : lvalonmimaCard.trigger25card
	{
		int i = 0;
		protected override void EnterBattle2(BattleController battle)
		{
			HandleBattleEvent(Battle.Player.Dying, OnDying, GameEventPriority.Highest + 7);
		}

		private void OnDying(DieEventArgs args)
		{
			i++;
		}

		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			i = 0;
			Random r = new Random();
			while (i < Value2)
			{
				Unit unit = Battle.AllAliveUnits.Sample(GameRun.BattleRng);
				yield return DamageAction.LoseLife(unit, Value1, GunNameID.GetGunFromId(15110 + r.Next(0, 4)));
				if (r.Next(0, 5) == 1)
				{
					AudioManager.GuardedGetInstance().FixedPlaySfx(lvalonmimaSFXTemplate.GetSfxId<mimametalpipeDef>());
				}
				if (BepinexPlugin.u25)
				{
					yield return new GainManaAction(Mana);
				}
				if (unit != Battle.Player && (!unit.IsAlive || unit.Hp == 0))
				{
					i++;
				}
				else if (unit.HasStatusEffect<seabyss>() && unit.Hp == 0)
				{
					i++;
				}
			}
			yield return new RemoveCardAction(this);
		}
	}
}


