using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoLEntitySideloader.CustomKeywords;
using LBoL.Presentation;
using lvalonmima.Common;
using lvalonmima.Exhibits;

namespace lvalonmima.Cards.Template;

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
			text += RawExtraDescription1;

		if (HasExtraDescription2)
		{
			if (text.Length > 0)
			{
				text += "\n";
			}
			text += RawExtraDescription2;
		}

		if (quest != null && quest != -1 && Config.Rarity != LBoL.Base.Rarity.Rare)
		{
			if (text.Length > 0)
			{
				text += "\n";
			}
			text += "(|c:" + progress + "| / |c:" + Value1 + "|)";
		}

		if (text.Length == 0)
			return base.GetBaseDescription();

		return StringDecorator.Decorate(FollowByDetailIcon(text));
	}
}
public class lvalonmimaCard : Card
{
	private int _playingDepth;
	private int _expellingDepth;

	// Cards migrated onto RunAsPlaying/RunAsExpelling (Phase 2) no longer override this;
	// it reflects their _playingDepth/_expellingDepth instead. Cards not yet migrated
	// still override it with their own localplaying/expelling fields, which is why this
	// stays virtual. Nothing sets `playing` directly (confirmed: no `.playing =` outside
	// those local-flag overrides), so this is safe to compute.
	public virtual bool playing => _playingDepth > 0 || _expellingDepth > 0;

	// Replaces the localplaying/expelling bool-and-try/finally pair repeated across
	// ~22 cards (e.g. cardsymmconv, cardbiglaser, cardabyssdweller).
	protected IEnumerable<BattleAction> RunAsPlaying(Func<IEnumerable<BattleAction>> body)
	{
		_playingDepth++;
		try
		{
			foreach (BattleAction action in body()) yield return action;
		}
		finally { _playingDepth--; }
	}

	protected IEnumerable<BattleAction> RunAsExpelling(Func<IEnumerable<BattleAction>> body)
	{
		_expellingDepth++;
		try
		{
			foreach (BattleAction action in body()) yield return action;
		}
		finally { _expellingDepth--; }
	}

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
	// Builds the paired "choose one of two" cards used by the choice-card cards
	// (twilightspark, ignition, implodemagic, smashpass, superimpact). Only the
	// setup is shared: some cards yield the InteractionAction directly from Actions,
	// others return the interaction from Precondition() and read it back via the
	// `precondition` parameter, so the run/select step itself stays at the call site.
	protected MiniSelectCardInteraction CreateChoicePair<TSelf>() where TSelf : lvalonmimaCard
	{
		List<TSelf> list = [.. Library.CreateCards<TSelf>(2, IsUpgraded)];
		list[0].ChoiceCardIndicator = 1;
		list[1].ChoiceCardIndicator = 2;
		foreach (TSelf c in list)
		{
			c.SetBattle(Battle);
			c.Keywords = Keyword.None;
		}
		return new MiniSelectCardInteraction(list, false, false, false);
	}

	// Replaces the `se.Level == amount ? RemoveStatusEffectAction : se.Level -= amount`
	// pair duplicated in cardimplodemagic and cardsuperimpact.
	protected IEnumerable<BattleAction> SpendCharging(StatusEffect charging, int amount)
	{
		if (charging.Level == amount)
			yield return new RemoveStatusEffectAction(charging);
		else
			charging.Level -= amount;
	}

	// Replaces the identical `AddCardsToDrawZoneAction(Library.CreateCards<cardpurediamond>(n, false), ...)`
	// line repeated in 9 cards (birdq, dpillar, entertainment, entrance, fromhell,
	// mountain, mexploit, starliege, wheresleep).
	protected AddCardsToDrawZoneAction AddPureDiamondsToDraw(int count) =>
		new(Library.CreateCards<cardpurediamond>(count, false), DrawZoneTarget.Random, AddCardsType.Normal);

	// Replaces `Library.CreateCard<cardquestN>()` used only to read Config values
	// (Value1/Value2/etc.) — avoids instantiating a full card entity just to read
	// its static config. Not for call sites that read post-Initialize instance state.
	protected static CardConfig QuestConfig<T>() where T : Card => CardConfig.FromId(typeof(T).Name);

	protected virtual IEnumerable<BattleAction> RemoveSelf()
	{
		Card deckCardByInstanceId = GameRun.GetDeckCardByInstanceId(InstanceId);
		if (deckCardByInstanceId != null)
			GameRun.RemoveDeckCard(deckCardByInstanceId, false);
		if (Battle.BattleShouldEnd) yield break;
		yield return new RemoveCardAction(this);
	}

	protected virtual void EnterBattle2(BattleController battle)
	{
	}

	public class trigger50card : lvalonmimaCard
	{
		public override bool Triggered => MimaHp.Below50;
	}
	public class trigger25card : lvalonmimaCard
	{
		public override bool Triggered => MimaHp.Below25;
	}
	public class trigger10card : lvalonmimaCard
	{
		public override bool Triggered => MimaHp.Below10;
	}
	protected virtual int BaseValue3 { get; set; } = 0;
	protected virtual int BaseUpgradedValue3 { get; set; } = 0;
	public int Value3 => IsUpgraded ? BaseUpgradedValue3 : BaseValue3;

	// Read by YAML descriptions as {hp10ns}/{hp25ns}/{hp50ns}/{hp10bs}/{hp25bs}/{hp50bs}
	// (see DirResources/Cards*.yaml), so these names and their fallback text must stay.
	// hp50/hp25/hp10 and hp50fs/hp25fs/hp10fs were unreferenced by any YAML or card and
	// have been removed.
	public string hp10ns => MimaHp.Text(10, " (", ")", "");
	public string hp25ns => MimaHp.Text(25, " (", ")", "");
	public string hp50ns => MimaHp.Text(50, " (", ")", "");
	public string hp10bs => MimaHp.Text(10, " (", ") ", " ");
	public string hp25bs => MimaHp.Text(25, " (", ") ", " ");
	public string hp50bs => MimaHp.Text(50, " (", ") ", " ");
}
