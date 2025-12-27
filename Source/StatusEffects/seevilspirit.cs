using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.Presentation;
using LBoL.Presentation.Bullet;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.Units;
using LBoLEntitySideloader.Attributes;
using lvalonmima.Exhibits;
using lvalonmima.GunName;
using lvalonmima.SFX;
using lvalonmima.SFX.Template;
using UnityEngine;

namespace lvalonmima.StatusEffects
{
	public sealed class seevilspiritDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			return config;
		}
	}

	[EntityLogic(typeof(seevilspiritDef))]
	public sealed class seevilspirit : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		int bscapbase = 6;
		public int bscap
		{
			get
			{
				if (GameRun != null && GameRun.Player.HasExhibit<exmimab>())
				{
					return toolbox.Round(bscapbase / 2);
				}
				return bscapbase;
			}
		}
		int blockshield = 0;
		bool triggered = false;
		bool immune = false;
		int immunetype = -1; //0 normal, 1 half
		int increaseamount = 1;
		protected override void OnAdded(Unit unit)
		{
			blockshield = 0;
			triggered = false;
			immune = false;
			immunetype = -1;
			increaseamount = 1;
			while (Battle.Player.MaxHp < 6)
			{
				GameRun.SetHpAndMaxHp(Owner.MaxHp * 2, Owner.MaxHp * 2, true);
			}
			immune = Battle.Player.HasExhibit<exmimab>();
			blockshield = 0;
			HandleOwnerEvent(Battle.Player.BlockShieldGaining, OnBlockShieldGaining, GameEventPriority.Lowest);
			HandleOwnerEvent(Battle.Player.Dying, OnDying, GameEventPriority.Highest + 10);
			HandleOwnerEvent(Battle.Player.Dying, OnDyingPrio, GameEventPriority.Highest + 1);
			HandleOwnerEvent(Battle.RoundEnded, OnRoundEnded);
			HandleOwnerEvent(Battle.BattleEnding, OnRoundEnded);
		}
		private void revive(int hp)
		{
			foreach (EnemyUnit unit in Battle.AllAliveEnemies)
			{
				if (unit.Hp <= Owner.MaxHp || unit.MaxHp <= Owner.MaxHp)
				{
					React(new ForceKillAction(Owner, unit));
				}
				else
				{
					GameRun.SetEnemyHpAndMaxHp(unit.Hp - Owner.MaxHp, unit.MaxHp - Owner.MaxHp, unit, true);
				}
			}
			GameRun.SetHpAndMaxHp(hp, hp, true);
		}
		private void performeff(int hp, int dmg)
		{
			int gunid = 15160;
			double maxhp = lvalonmimaLoadouts.maxhp;
			int[] thresholds = { 25, 50, 100, 200, 300 };
			gunid += thresholds.Count(t => Owner.MaxHp >= toolbox.Round(maxhp * t / 100));

			Gun gun = GunManager.CreateGun(GunNameID.GetGunFromId(gunid));
			gun.Target = GameMaster.PlayerView;
			gun.Targets = new List<UnitView> { GameMaster.PlayerView };
			GunManager.GunShoot(gun);
			//GameMaster.PlayerView.PerformShoot(GunNameID.GetGunFromId(gunid));

			foreach (Unit unit in Battle.AllAliveUnits.Concat(new List<Unit> { Battle.Player }).Distinct())
			{
				if (Battle.BattleShouldEnd || Battle.AllAliveUnits.Count() == 0 || unit == null) { break; }
				UnitView target = GameDirector.GetUnit(unit);
				bool isplayer = unit == Battle.Player;
				Color32 color = new Color32(0xC8, 0x50, 0xC8, 0xFF);
				if (unit != Battle.Player)
				{
					color = new Color32(0x37, 0x81, 0xE1, 0xFF);
				}
				PopupHud.Instance.PopupFromScene(isplayer ? hp : dmg, color, target.transform.position);
				AudioManager.GuardedGetInstance().FixedPlaySfx(lvalonmimaSFXTemplate.GetSfxId<mimadeathDef>());
			}
		}

		private void OnBlockShieldGaining(BlockShieldEventArgs args)
		{
			if (args.Cause != ActionCause.OnlyCalculate)
			{
				blockshield += Math.Min(toolbox.Round(args.Block + args.Shield), bscap);
				args.CancelBy(this);
				NotifyActivating();
				React(new DamageAction(Owner, new List<Unit> { Owner }, DamageInfo.Reaction(blockshield)));
				blockshield = 0;
			}
		}
		private void OnDyingPrio(DieEventArgs args)
		{
			immune = Battle.Player.HasExhibit<exmimab>();
			if (triggered && Owner.Hp == 0 && immune)
			{
				NotifyActivating();
				args.CancelBy(this);
				return;
			}
		}
		private void OnDying(DieEventArgs args)
		{
			triggered = true;
			immune = Battle.Player.HasExhibit<exmimab>();
			if (Owner == Battle.Player)
			{
				//add maximum life
				if (args.DieSource == Owner || args.ActionSource == Owner
				|| args.DieSource is Card || args.ActionSource is Card
				|| (args.DieSource is Exhibit exhibit && exhibit.Owner == Owner) || (args.ActionSource is Exhibit exhibit2 && exhibit2.Owner == Owner)
				|| (args.DieSource is StatusEffect se && se.Owner == Owner) || (args.ActionSource is StatusEffect se2 && se2.Owner == Owner)
				|| (args.DieSource is UltimateSkill us && us.Owner == Owner) || (args.ActionSource is UltimateSkill us2 && us2.Owner == Owner))
				{
					if (Owner.MaxHp >= 6)
					{
						NotifyActivating();
						if (immune)
						{
							if (immunetype == -1)
							{
								immunetype = 0;
								Highlight = true;
							}
						}
						else
						{
							performeff(Owner.MaxHp + increaseamount, Owner.MaxHp);
							revive(Owner.MaxHp + increaseamount);
						}
						args.CancelBy(this);
					}
				}
				else if (toolbox.Round(Owner.MaxHp * 1.0 / 2) >= 6)
				{
					NotifyActivating();
					if (immune)
					{
						if (immunetype == -1)
						{
							immunetype = 1;
						}
					}
					else
					{
						performeff(toolbox.Round(Owner.MaxHp * 1.0 / 2), Owner.MaxHp);
						revive(toolbox.Round(Owner.MaxHp * 1.0 / 2));
					}
					args.CancelBy(this);
				}
			}
		}
		private void OnRoundEnded(GameEventArgs args)
		{
			triggered = false;
			if (immune)
			{
				if (immunetype == 0)
				{
					NotifyActivating();
					performeff(Owner.MaxHp + increaseamount, Owner.MaxHp);
					revive(Owner.MaxHp + increaseamount);
				}
				if (immunetype == 1)
				{
					int newhp = toolbox.Round(Owner.MaxHp * 1.0 / 2);
					if (newhp >= 6)
					{
						NotifyActivating();
						performeff(newhp, Owner.MaxHp);
						revive(newhp);
					}
					else
					{
						React(new ForceKillAction(Owner, Owner));
					}
				}
				immunetype = -1;
				Highlight = false;
			}
		}
	}
}