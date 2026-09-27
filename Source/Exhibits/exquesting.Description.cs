using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Exhibits.Common;
using LBoL.Presentation;
using LBoLEntitySideloader.Attributes;
using lvalonmima.Cards;
using lvalonmima.Source.Patches;
using lvalonmima.StatusEffects;

namespace lvalonmima.Exhibits;

public sealed partial class exquesting : Exhibit
{

	protected override string GetBaseDescription()
	{
		return base.GetBaseDescription() + QuestDescriptions() + BuffDescriptions();
	}

	private string BuffDescriptions()
	{
		var buffs = PendingQuestModifiers;
		if (buffs == null || buffs.Count == 0)
			return string.Empty;
		StringBuilder sb = new();
		foreach (var (key, value) in buffs)
		{
			Card buffCard = Library.TryCreateCard(key, false);
			if (buffCard == null)
				continue;
			sb.Append("\n").Append(value + "× ").Append(ResolveQuestExtraDescription(buffCard, 2));
		}
		if (!string.IsNullOrWhiteSpace(sb.ToString()))
			return StringDecorator.Decorate("\n" + sb.ToString());
		return StringDecorator.Decorate(sb.ToString());
	}

	private string QuestDescriptions()
	{
		StringBuilder sb = new();
		foreach (var (key, value) in PendingQuestProgress)
		{
			int progress = value;
			var card = Library.TryCreateCard(key, false);
			if (card == null)
				continue;
			card.GameRun = GameMaster.Instance?.CurrentGameRun;

			if (card.Config.Rarity == Rarity.Rare)
			{
				sb.Append("\n").Append(ResolveQuestExtraDescription(card, 0));
				continue;
			}

			int goal = card.Config.Value1 ?? -1;
			if (goal == -1)
				continue;
			string prog = " (|c:" + progress + "| / |c:" + goal + "|)";

			if (card.Id == nameof(cardquest23))
			{
				if (progress > 0)
					prog = " (|f:" + progress + "|)";
				if (progress < 0)
					prog = " (|u:" + progress + "|)";
				if (progress == 0)
					prog = " (|" + progress + "|)"; ;
			}

			prog = StringDecorator.Decorate(prog);
			string extraDescription = ResolveQuestExtraDescription(card, 1); // desc 1 for condition
			if (string.IsNullOrEmpty(extraDescription))
				continue;
			sb.Append("\n").Append(extraDescription).Append(prog); // effect + progress
		}
		if (!string.IsNullOrWhiteSpace(sb.ToString()))
			return StringDecorator.Decorate("\n" + sb.ToString());
		return StringDecorator.Decorate(sb.ToString());
	}

	private static string ResolveQuestExtraDescription(Card card, int desc)
	{
		if (card == null || string.IsNullOrEmpty(card.Id))
			return string.Empty;

		try
		{
			string field = desc != 0 ? "ExtraDescription" + desc : "Description";
			string rawText = TypeFactory<Card>.LocalizeProperty(card.Id, field, true, true);
			if (string.IsNullOrEmpty(rawText))
				return string.Empty;
			return rawText.RuntimeFormat(card.FormatWrapper);
		}
		catch (Exception)
		{
			return string.Empty;
		}
	}
}
