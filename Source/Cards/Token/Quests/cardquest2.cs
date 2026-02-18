using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;
using LBoL.Core;
using lvalonmima.Exhibits;

namespace lvalonmima.Cards
{
	public sealed class cardquest2Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.Blue };
			config.Rarity = Rarity.Common;

			config.Value1 = 1;
			config.Value2 = 1;

			config.Keywords = Keyword.Forbidden;

			config.RelativeKeyword = Keyword.Basic;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.Illustrator = "GCMushroom";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 2);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest2Def))]
	public sealed class cardquest2 : questCard
	{
		private const string DefaultRarityKey = "RarityRandom";
		private const string DefaultTypeKey = "TypeRandom";

		public static string EncodeRequirement(string rarityKey, string typeKey)
		{
			if (string.IsNullOrEmpty(rarityKey) || string.IsNullOrEmpty(typeKey))
			{
				return string.Empty;
			}

			return rarityKey + "!" + typeKey;
		}

		public static bool TryDecodeRequirement(string encodedRequirement, out string rarityKey, out string typeKey)
		{
			rarityKey = DefaultRarityKey;
			typeKey = DefaultTypeKey;

			if (string.IsNullOrEmpty(encodedRequirement))
			{
				return false;
			}

			string[] parts = encodedRequirement.Split('!');
			if (parts.Length < 2 || string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1]))
			{
				return false;
			}

			rarityKey = parts[0];
			typeKey = parts[1];
			return true;
		}

		public string chosenRarity
		{
			get
			{
				exquesting exhibit = GameRun?.Player?.GetExhibit<exquesting>();
				if (exhibit == null)
				{
					return DefaultRarityKey;
				}

				if (exhibit.TryGetQuestRequirement(Id, out string encodedRequirements)
					&& TryDecodeRequirement(encodedRequirements, out string rarity, out _))
				{
					return rarity;
				}

				return DefaultRarityKey;
			}
		}
		public string chosenType
		{
			get
			{
				exquesting exhibit = GameRun?.Player?.GetExhibit<exquesting>();
				if (exhibit == null)
				{
					return DefaultTypeKey;
				}

				if (exhibit.TryGetQuestRequirement(Id, out string encodedRequirements)
					&& TryDecodeRequirement(encodedRequirements, out _, out string type))
				{
					return type;
				}

				return DefaultTypeKey;
			}
		}
		public string cardRarity => LocalizeProperty(key: chosenRarity, decorated: true, required: true).RuntimeFormat(FormatWrapper);
		public string cardType => LocalizeProperty(key: chosenType, decorated: true, required: true).RuntimeFormat(FormatWrapper);
	}
}


