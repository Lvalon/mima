using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoLEntitySideloader.CustomKeywords;
using LBoL.Presentation;
using lvalonmima.Exhibits;

namespace lvalonmima.Cards.Template
{
	public class questCard : lvalonmimaCard
	{
		public override void Initialize()
		{
			base.Initialize();
			this.AddCustomKeyword(lvalonmimakeyword.Quest);
		}

		protected override string GetBaseDescription()
		{
			int? quest = GameMaster.Instance?.CurrentGameRun?.Player?.GetExhibit<exquesting>()?.PendingQuestProgress?.GetValueOrDefault(Id, -1);
			int progress = quest ?? 0;

			string text = BaseDescription;
			if (!string.IsNullOrEmpty(text))
				text += "\n";

			if (HasExtraDescription1)
			{
				text += RawExtraDescription1;
			}

			if (HasExtraDescription2)
			{
				if (text.Length > 0)
				{
					text += "\n";
				}
				text += RawExtraDescription2;
			}

			if (quest != null && quest != -1)
			{
				if (text.Length > 0)
				{
					text += "\n";
				}
				text += "(|c:" + progress + "| / |c:" + Value1 + "|)";
			}

			if (text.Length == 0)
			{
				return base.GetBaseDescription();
			}

			return StringDecorator.Decorate(FollowByDetailIcon(text));
		}
	}
	public class lvalonmimaCard : Card
	{
		public virtual bool playing { get; set; } = false;
		protected override void OnEnterBattle(BattleController battle)
		{
			EnterBattle2(battle);
			ReactBattleEvent(Battle.EnemyDied, OnExpeltmp);
		}

		protected virtual IEnumerable<BattleAction> OnExpeltmp(DieEventArgs args)
		{
			if ((args.DieSource == this || playing) && !args.Unit.HasStatusEffect<Servant>())
			{
				foreach (BattleAction ba in OnExpel(args)) yield return ba;
			}
			yield break;
		}
		protected virtual IEnumerable<BattleAction> OnExpel(DieEventArgs args)
		{
			yield break;
		}
		protected virtual IEnumerable<BattleAction> RemoveSelf()
		{
			Card deckCardByInstanceId = GameRun.GetDeckCardByInstanceId(InstanceId);
			if (deckCardByInstanceId != null)
			{
				GameRun.RemoveDeckCard(deckCardByInstanceId, false);
			}
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new RemoveCardAction(this);
		}

		protected virtual void EnterBattle2(BattleController battle)
		{
		}

		public class trigger50card : lvalonmimaCard
		{
			public override bool Triggered => BepinexPlugin.u50;
		}
		public class trigger25card : lvalonmimaCard
		{
			public override bool Triggered => BepinexPlugin.u25;
		}
		public class trigger10card : lvalonmimaCard
		{
			public override bool Triggered => BepinexPlugin.u10;
		}
		protected virtual int BaseValue3 { get; set; } = 0;
		protected virtual int BaseUpgradedValue3 { get; set; } = 0;
		public int Value3
		{
			get
			{
				if (this.IsUpgraded)
				{
					return BaseUpgradedValue3;
				}
				return BaseValue3;
			}
		}
		public int hp50
		{
			get
			{
				var player = GameMaster.Instance.CurrentGameRun?.Player;
				if (player != null)
				{
					return toolbox.hpfrompercent(GameMaster.Instance.CurrentGameRun.Player, 50);
				}
				return 0;
			}
		}
		public int hp25
		{
			get
			{
				var player = GameMaster.Instance.CurrentGameRun?.Player;
				if (player != null)
				{
					return toolbox.hpfrompercent(GameMaster.Instance.CurrentGameRun.Player, 25);
				}
				return 0;
			}
		}
		public int hp10
		{
			get
			{
				var player = GameMaster.Instance.CurrentGameRun?.Player;
				if (player != null)
				{
					return toolbox.hpfrompercent(GameMaster.Instance.CurrentGameRun.Player, 10);
				}
				return 0;
			}
		}
		public string hp50fs
		{
			get
			{
				var player = GameMaster.Instance.CurrentGameRun?.Player;
				if (player != null)
				{
					return " (" + toolbox.hpfrompercent(GameMaster.Instance.CurrentGameRun.Player, 50) + ")";
				}
				return " ";
			}
		}
		public string hp25fs
		{
			get
			{
				var player = GameMaster.Instance.CurrentGameRun?.Player;
				if (player != null)
				{
					return " (" + toolbox.hpfrompercent(GameMaster.Instance.CurrentGameRun.Player, 25) + ")";
				}
				return " ";
			}
		}
		public string hp10fs
		{
			get
			{
				var player = GameMaster.Instance.CurrentGameRun?.Player;
				if (player != null)
				{
					return " (" + toolbox.hpfrompercent(GameMaster.Instance.CurrentGameRun.Player, 10) + ")";
				}
				return " ";
			}
		}
		public string hp10ns
		{
			get
			{
				var player = GameMaster.Instance.CurrentGameRun?.Player;
				if (player != null)
				{
					return " (" + toolbox.hpfrompercent(GameMaster.Instance.CurrentGameRun.Player, 10) + ")";
				}
				return "";
			}
		}
		public string hp25ns
		{
			get
			{
				var player = GameMaster.Instance.CurrentGameRun?.Player;
				if (player != null)
				{
					return " (" + toolbox.hpfrompercent(GameMaster.Instance.CurrentGameRun.Player, 25) + ")";
				}
				return "";
			}
		}
		public string hp50ns
		{
			get
			{
				var player = GameMaster.Instance.CurrentGameRun?.Player;
				if (player != null)
				{
					return " (" + toolbox.hpfrompercent(GameMaster.Instance.CurrentGameRun.Player, 50) + ")";
				}
				return "";
			}
		}
		public string hp10bs
		{
			get
			{
				var player = GameMaster.Instance.CurrentGameRun?.Player;
				if (player != null)
				{
					return " (" + toolbox.hpfrompercent(GameMaster.Instance.CurrentGameRun.Player, 10) + ") ";
				}
				return " ";
			}
		}
		public string hp25bs
		{
			get
			{
				var player = GameMaster.Instance.CurrentGameRun?.Player;
				if (player != null)
				{
					return " (" + toolbox.hpfrompercent(GameMaster.Instance.CurrentGameRun.Player, 25) + ") ";
				}
				return " ";
			}
		}
		public string hp50bs
		{
			get
			{
				var player = GameMaster.Instance.CurrentGameRun?.Player;
				if (player != null)
				{
					return " (" + toolbox.hpfrompercent(GameMaster.Instance.CurrentGameRun.Player, 50) + ") ";
				}
				return " ";
			}
		}
	}
}