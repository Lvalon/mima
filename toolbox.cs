using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using System.Linq;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.Units;
using LBoL.Presentation;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace lvalonmima;

public abstract class toolbox
{
	public static string gibberish()
	{
		System.Random rng = new();
		float t = Mathf.Pow((float)rng.NextDouble(), 2.5f);
		int length = Mathf.RoundToInt(Mathf.Lerp(3, 24, t));
		var sb = new System.Text.StringBuilder();

		for (int i = 0; i < length; i++)
		{
			int roll = rng.Next(3);

			if (roll == 0)
				sb.Append((char)rng.Next(33, 127));
			else if (roll == 1)
				sb.Append((char)rng.Next(0x2200, 0x22FF));
			else
				sb.Append((char)rng.Next(0x25A0, 0x25FF));
		}

		return sb.ToString();
	}


	/// <summary>
	/// round away from zero
	/// </summary>
	public static int Round(double i)
	{
		return Convert.ToInt32(Math.Round(i, MidpointRounding.AwayFromZero));
	}
	public static int Round(float i)
	{
		return Convert.ToInt32(Math.Round(i, MidpointRounding.AwayFromZero));
	}
	public static int hpfrompercent(Unit unit, int percent, bool maxhp = true)
	{
		if (unit == null)
			return 0;
		return Convert.ToInt32(Math.Round((double)(maxhp ? unit.MaxHp : unit.Hp) * percent / 100, MidpointRounding.AwayFromZero));
	}
	// RollCardsCustomIgnore, RollCardsCustom and RepeatableAllCards (near-identical
	// variants of this) were never called anywhere and have been removed.
	static public Card[] UniqueAllCards(RandomGen rng, CardWeightTable weightTable, int count, bool ensureCount = false, Predicate<Card> filter = null)
	{
		GameRunController gr = (GameMaster.Instance?.CurrentGameRun) ?? throw new InvalidOperationException("Rolling cards when run is not started.");
		UniqueRandomPool<Type> innitialPool = CreateAllCardsPool(weightTable, null);

		UniqueRandomPool<Card> filteredPool = [];

		foreach (RandomPoolEntry<Type> e in innitialPool)
		{
			Card card = Library.CreateCard(e.Elem);
			if (filter?.Invoke(card) ?? true)
			{
				card.GameRun = gr;
				filteredPool.Add(card, e.Weight);
			}
		}

		return filteredPool.SampleMany(rng, count, ensureCount);
	}

	static public UniqueRandomPool<Type> CreateAllCardsPool(CardWeightTable weightTable, [MaybeNull] Predicate<CardConfig> filter = null)
	{
		var gr = GameMaster.Instance.CurrentGameRun;
		var charExSet = new HashSet<string>(gr.Player.Exhibits.Where(e => e.OwnerId != null).Select(e => e.OwnerId));
		UniqueRandomPool<Type> uniqueRandomPool = [];
		foreach (var item4 in EnumerateALLCardTypes())
		{
			Type item = item4.Item1;
			CardConfig item2 = item4.Item2;
			if (filter != null && !filter(item2))
			{
				continue;
			}
			float num = weightTable.WeightFor(item2, gr.Player.Id, charExSet);
			if (num > 0f)
			{
				float num2 = gr.BaseCardWeight(item2, false);
				uniqueRandomPool.Add(item, num * num2);
			}
			// uniqueRandomPool.Add(item, 1);
		}

		return uniqueRandomPool;
	}
	public static IEnumerable<(Type, CardConfig)> EnumerateALLCardTypes()
	{
		foreach (CardConfig item in CardConfig.AllConfig())
		{
			// CardType type = item.Type;
			// if (type != CardType.Misfortune && type != CardType.Status && type != 0)
			// {
			Type type2 = TypeFactory<Card>.TryGetType(item.Id);
			if (type2 is not null)
			{
				yield return (type2, item);
			}
			// }
		}
	}
	public static Card createcardwithid(string id)
	{
		var type = TypeFactory<Card>.TryGetType(id);
		if (type == null)
			return null;
		return TypeFactory<Card>.CreateInstance(type);
	}

	public static Exhibit CreateExhibitById(string id)
	{
		if (string.IsNullOrEmpty(id))
			return null;
		var type = TypeFactory<Exhibit>.TryGetType(id);
		if (type == null)
			return null;
		return TypeFactory<Exhibit>.CreateInstance(type);
	}

}
