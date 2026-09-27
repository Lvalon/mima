using LBoLEntitySideloader.CustomKeywords;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards.Template;

public static class lvalonmimakeyword
{
	public static CardKeyword Used = new(nameof(seused)) { descPos = KwDescPos.First };
	public static CardKeyword Linked = new(nameof(selinked)) { descPos = KwDescPos.First };
	public static CardKeyword Quest = new(nameof(sequest)) { descPos = KwDescPos.First };
}
