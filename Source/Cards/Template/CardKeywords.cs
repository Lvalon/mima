using LBoLEntitySideloader.CustomKeywords;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards.Template
{
	public static class lvalonmimakeyword
	{
		public static CardKeyword Used = new CardKeyword(nameof(seused)) { descPos = KwDescPos.First };
		public static CardKeyword Linked = new CardKeyword(nameof(selinked)) { descPos = KwDescPos.First };
		public static CardKeyword Quest = new CardKeyword(nameof(sequest)) { descPos = KwDescPos.First };
	}
}
