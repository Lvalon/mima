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
using UnityEngine.UI;
using LBoLEntitySideloader.Resource;
using System.Collections;
using lvalonmima.Patches;

namespace lvalonmima.StatusEffects
{
	public sealed class seevilspiritDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			config.HasCount = true;
			return config;
		}
	}

	[EntityLogic(typeof(seevilspiritDef))]
	public sealed class seevilspirit : StatusEffect
	{
		public readonly static string evilBarGoName = "EvilSpiritBar";

		public void UpdateOrCreateEvilBar(int counterValue, bool gibberish = true)
		{
			if (Owner?.View is UnitView view && Battle.Player != null && Battle.Player.HasExhibit<exmimab>())
			{
				var sw = view._statusWidget;
				if (sw == null)
					return;
				var hpBar = sw.hpBar;
				if (hpBar == null)
					return;
				var hpBarGo = hpBar.gameObject;

				var eBarGo = hpBarGo.transform.Find(evilBarGoName)?.gameObject;
				if (eBarGo == null)
				{
					var src = hpBarGo.transform.Find("HealthBarHealth")?.gameObject;
					if (src == null)
						return;
					eBarGo = UnityEngine.Object.Instantiate(src, hpBarGo.transform, worldPositionStays: true);
					eBarGo.name = evilBarGoName;
					var img = eBarGo.GetComponent<Image>();
					if (img != null)
					{
						Sprite s = ResourceLoader.LoadSprite("PurpleBar.png", BepinexPlugin.directorySource);
						if (s != null)
							img.sprite = s;
						img.color = new Color(1f, 1f, 1f, 0.5f);
					}
				}

				var eBarImage = eBarGo.GetComponent<Image>();
				var pulse = eBarGo.GetComponent<EvilBarPulse>() ?? eBarGo.AddComponent<EvilBarPulse>();
				pulse.Initialize(eBarImage);
				int capped = Mathf.Clamp(counterValue, 0, Owner.MaxHp);
				if (eBarImage != null)
				{
					float fillFrac = Owner.MaxHp > 0 ? (capped / (float)Owner.MaxHp) : 0f;
					if (gibberish)
					{
						performeff2(fillFrac);
					}
					eBarImage.fillAmount = Mathf.Clamp01(fillFrac);
					pulse.SetFillFraction(eBarImage.fillAmount);
				}

				if (counterValue == 0)
				{
					UnityEngine.Object.Destroy(pulse);
					UnityEngine.Object.Destroy(eBarGo);
					return;
				}
			}
		}

		// MonoBehaviour to pulse the evil HP bar. Faster pulsing as fill approaches 1.0.
		private sealed class EvilBarPulse : MonoBehaviour
		{
			private static EvilBarPulse _instance;
			Image _img;
			Color _baseColor = Color.white;
			float _minSpeed = 0.25f;
			float _maxSpeed = 1.5f;
			float _speed = 1f;
			float _amplitude = 0.18f;
			float _alphaBase = 0.01f;
			float fill;

			public static EvilBarPulse Instance
			{
				get
				{
					if (_instance == null)
					{
						GameObject obj = new GameObject("EvilBarPulse");
						_instance = obj.AddComponent<EvilBarPulse>();
						obj.hideFlags = HideFlags.HideAndDontSave;
					}
					return _instance;
				}
			}

			public void Initialize(Image img)
			{
				_img = img;
				if (_img != null)
					_baseColor = _img.color;
			}

			public void SetFillFraction(float fill)
			{
				this.fill = fill;
				_speed = Mathf.Lerp(_minSpeed, _maxSpeed, Mathf.Clamp01(fill));
				_amplitude = 0.08f + 0.22f * Mathf.Clamp01(fill);
				_alphaBase = 0.35f + 0.25f * Mathf.Clamp01(fill);
			}

			void Update()
			{
				if (_img == null || GameMaster.Instance?.CurrentGameRun?.Battle == null) return;
				float t = Time.time * _speed * Mathf.PI * 2f;
				float s = Mathf.Sin(t) * 0.5f + 0.5f; // 0..1
				float brightness = 1f + _amplitude * s;
				var c = _baseColor;
				c.r = Mathf.Clamp01(c.r * brightness);
				c.g = Mathf.Clamp01(c.g * brightness);
				c.b = Mathf.Clamp01(c.b * brightness);
				c.a = Mathf.Clamp01(_alphaBase + 0.15f * s);
				_img.color = c;
				var scale = 1f + 0.3f * s * (0.5f * fill + 1);
				transform.localScale = new Vector3(1f, scale, 1f);
			}
		}

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
			Count = 1;
			blockshield = 0;
			triggered = false;
			immune = false;
			immunetype = -1;
			increaseamount = 1;
			if (Battle.Player.MaxHp < 6)
			{
				GameRun.SetHpAndMaxHp(Owner.MaxHp * 2, Owner.MaxHp * 2, true);
			}
			immune = Battle.Player.HasExhibit<exmimab>();
			blockshield = 0;
			HandleOwnerEvent(Battle.Player.BlockShieldGaining, OnBlockShieldGaining, GameEventPriority.Lowest);
			ReactOwnerEvent(Owner.DamageReceived, OnDmgReceived, GameEventPriority.Lowest);
			HandleOwnerEvent(Battle.Player.Dying, OnDying, GameEventPriority.Highest + 10);
			HandleOwnerEvent(Battle.Player.Dying, OnDyingPrio, GameEventPriority.Highest + 1);
			HandleOwnerEvent(Battle.RoundEnded, OnRoundEnded);
			HandleOwnerEvent(Battle.BattleEnding, OnRoundEnded);
		}

		private IEnumerable<BattleAction> OnDmgReceived(DamageEventArgs args)
		{
			// update alternate HP bar from exhibit counter when damaged
			int counter = 0;
			if (Battle != null && Battle.Player != null && Battle.Player.HasExhibit<exmimab>())
			{
				counter = Battle.Player.GetExhibit<exmimab>().Counter;
			}
			if (Owner.Hp == 0)
			{
				UpdateOrCreateEvilBar(counter);
			}

			if (Owner.MaxHp < 6)
			{
				yield return new ForceKillAction(Owner, Owner);
			}
		}

		private void revive(int hp)
		{
			if (hp < 6) { return; }
			foreach (EnemyUnit unit in Battle.AllAliveEnemies)
			{
				int lifelosing = toolbox.Round(hp * 1.0 / 2);
				if (unit.Hp <= lifelosing || unit.MaxHp <= lifelosing)
				{
					React(new ForceKillAction(Owner, unit));
				}
				else
				{
					if (unit.Hp - lifelosing <= 0) { continue; }
					GameRun.SetEnemyHpAndMaxHp(unit.Hp - lifelosing, unit.MaxHp - lifelosing, unit, true);
				}
			}
			GameRun.SetHpAndMaxHp(hp, hp, true);
			UpdateOrCreateEvilBar(0, false);
		}
		private void performeff(int hp, int dmg)
		{
			int dmgloc = dmg;
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
					dmgloc = toolbox.Round(dmg * 1.0 / 2);
				}
				PopupHud.Instance.PopupFromScene(isplayer ? hp : dmgloc, color, target.transform.position);
				AudioManager.GuardedGetInstance().FixedPlaySfx(lvalonmimaSFXTemplate.GetSfxId<mimadeathDef>());
			}
		}
		public void performeff2(float count)
		{
			if (count <= 0f)
				return;

			count = Mathf.Clamp01(count);
			float durationSeconds = 0.1f + 0.25f * count;

			CoroutineRunner.Instance.StartCoroutine(
				PerformEff2Coroutine(durationSeconds)
			);
		}

		private IEnumerator PerformEff2Coroutine(float durationSeconds)
		{
			float elapsed = 0f;

			UnitView target = GameDirector.GetUnit(Battle.Player);

			while (elapsed < durationSeconds)
			{
				// Color color = Color.HSVToRGB(
				// UnityEngine.Random.value,
				// UnityEngine.Random.Range(0.8f, 1f),
				// UnityEngine.Random.Range(0.85f, 1f)
				// );
				Color color = Color.HSVToRGB(1, 1, 1); // handled in custom game event man

				Camera cam = Camera.main;
				float scale = cam != null ? cam.orthographicSize : 5f;

				float radiusBias = Mathf.Pow(UnityEngine.Random.value, 0.25f);
				Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;

				Vector2 chaosOffset2D =
					direction
					* radiusBias
					* scale
					* durationSeconds;

				PopupHud.Instance.PopupFromScene(
					int.MinValue,
					color,
					target.transform.position + (Vector3)chaosOffset2D
				);

				yield return null;
				elapsed += Time.deltaTime;
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
			if (Owner.MaxHp < 6) { return; }
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
			if (Owner.MaxHp < 6) { return; }
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
							if (Count > 0)
							{
								performeff(Owner.MaxHp + increaseamount, Owner.MaxHp);
								revive(Owner.MaxHp + increaseamount);
								Count--;
							}
							else
							{
								performeff(Owner.MaxHp, Owner.MaxHp);
								revive(Owner.MaxHp);
							}
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
					if (Count > 0)
					{
						performeff(Owner.MaxHp + increaseamount, Owner.MaxHp);
						revive(Owner.MaxHp + increaseamount);
						Count--;
					}
					else
					{
						performeff(Owner.MaxHp, Owner.MaxHp);
						revive(Owner.MaxHp);
					}
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
		protected override void OnRemoved(Unit unit)
		{
			UpdateOrCreateEvilBar(0);
		}
	}
}