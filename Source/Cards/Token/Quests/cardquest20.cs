using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;
using lvalonmima.Exhibits;
using LBoL.Core;
using LBoL.Core.Cards;

namespace lvalonmima.Cards
{
	public sealed class cardquest20Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.Colorless };
			config.Rarity = Rarity.Uncommon;

			config.Value1 = 1;
			config.Value2 = 2;

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.RelativeCards = new List<string>() { nameof(cardstone1), nameof(cardstone2), nameof(cardstone3), nameof(cardstone4) };

			config.Illustrator = "小一郎";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 20);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest20Def))]
	public sealed class cardquest20 : questCard
	{
		private const string DefaultCard = "RandomCard";
		public string chosenCard
		{
			get
			{
				exquesting exhibit = GameRun?.Player?.GetExhibit<exquesting>();
				if (exhibit == null)
				{
					return DefaultCard;
				}

				if (exhibit.TryGetQuestRequirement(Id, out string cardId))
				{
					return Library.TryCreateCard(cardId, false)?.Id ?? DefaultCard;
				}

				return DefaultCard;
			}
		}
		public string cardName
		{
			get
			{
				if (chosenCard == DefaultCard)
				{
					return LocalizeProperty(key: chosenCard, decorated: true, required: true).RuntimeFormat(FormatWrapper);
				}
				string tmp = "|" + TypeFactory<Card>.LocalizeProperty(chosenCard, "Name", true, true) + "|";
				return StringDecorator.Decorate(tmp);
			}
		}
	}
}


