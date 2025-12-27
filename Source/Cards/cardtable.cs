using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Battle.BattleActions;
using LBoL.Base.Extensions;
using LBoL.Core;
using System.Linq;
using lvalonmima.StatusEffects;
using LBoL.Core.Randoms;

namespace lvalonmima.Cards
{
	public sealed class cardtableDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Blue, ManaColor.Black, ManaColor.Green };
			config.Cost = new ManaGroup() { Any = 1, Blue = 1, Black = 1, Green = 1 };
			config.Rarity = Rarity.Rare;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Nobody;
			config.Keywords = Keyword.Exile;
			config.UpgradedKeywords = Keyword.Exile;
			config.RelativeKeyword = Keyword.TempMorph;
			config.UpgradedRelativeKeyword = Keyword.TempMorph;
			config.UpgradedRelativeEffects = new List<string>() { nameof(setranscendence) };

			config.Value1 = 1;

			config.Illustrator = "camellia";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardtableDef))]
	public sealed class cardtable : lvalonmimaCard
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			int num = Battle.MaxHand - Battle.HandZone.Count;
			List<Card> list = Battle.RollCards(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.Valid, CardTypeWeightTable.OnlySkill), num, (CardConfig config) => !config.Keywords.HasFlag(Keyword.Forbidden)).ToList();
			if (list.NotEmpty())
			{
				foreach (Card card in list)
				{
					if (!card.IsXCost)
					{
						ManaColor[] components = card.Cost.EnumerateComponents().SampleManyOrAll(1, GameRun.BattleRng);
						card.DecreaseTurnCost(ManaGroup.FromComponents(components));
					}
					card.IsExile = true;
					card.IsEthereal = true;
				}
				yield return new AddCardsToHandAction(list);
			}
			if (IsUpgraded)
			{
				if (Battle.BattleShouldEnd) { yield break; }
				yield return new ApplyStatusEffectAction<setranscendence>(Battle.Player, Value1, 0, 0, 0);
			}
			yield break;
		}
	}
}


