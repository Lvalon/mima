using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;

namespace lvalonmima.Localization;

public sealed class lvalonmimaLocalization
{
	public static string Cards = "Cards";
	public static string Exhibits = "Exhibits";
	public static string PlayerUnit = "PlayerUnit";
	//public static string EnemiesUnit = "EnemyUnit";
	public static string UnitModel = "UnitModel";
	public static string UltimateSkills = "UltimateSkills";
	public static string StatusEffects = "StatusEffects";
	public static string JadeBoxes = "JadeBoxes";
	public static string Packs = "Packs";
	public static string RogueliteShop = "RogueliteShop";

	public static BatchLocalization CardsBatchLoc = new(BepinexPlugin.directorySource, typeof(CardTemplate), Cards);
	public static BatchLocalization ExhibitsBatchLoc = new(BepinexPlugin.directorySource, typeof(ExhibitTemplate), Exhibits);
	public static BatchLocalization PlayerUnitBatchLoc = new(BepinexPlugin.directorySource, typeof(PlayerUnitTemplate), PlayerUnit);
	//public static BatchLocalization EnemiesUnitBatchLoc = new BatchLocalization(BepinexPlugin.directorySource, typeof(EnemyUnitTemplate), EnemiesUnit);
	public static BatchLocalization UnitModelBatchLoc = new(BepinexPlugin.directorySource, typeof(UnitModelTemplate), UnitModel);
	public static BatchLocalization UltimateSkillsBatchLoc = new(BepinexPlugin.directorySource, typeof(UltimateSkillTemplate), UltimateSkills);
	public static BatchLocalization StatusEffectsBatchLoc = new(BepinexPlugin.directorySource, typeof(StatusEffectTemplate), StatusEffects);
	public static BatchLocalization JadeBoxBatchLoc = new(BepinexPlugin.directorySource, typeof(JadeBoxTemplate), JadeBoxes);
	public static BatchLocalization PacksBatchLoc = new(BepinexPlugin.directorySource, typeof(PackTemplate), Packs);

	// Not tied to an entity template, so it's a bare LocalizationFiles rather
	// than a BatchLocalization: discovers RogueliteShop{Locale}.yaml the same
	// way BatchLocalization does, but is queried directly by string key
	// (see LocalisationKeys in Patches/RogueLite/LiteShopButton.cs).
	public static readonly LocalizationFiles RogueliteShopLocFiles = CreateRogueliteShopLocFiles();

	private static LocalizationFiles CreateRogueliteShopLocFiles()
	{
		var files = new LocalizationFiles(BepinexPlugin.directorySource);
		files.DiscoverAndLoadLocFiles(RogueliteShop);
		return files;
	}

	// maybe it's better to have controlled file discovery tah
	public static void Init()
	{
		CardsBatchLoc.DiscoverAndLoadLocFiles(Cards);
		ExhibitsBatchLoc.DiscoverAndLoadLocFiles(Exhibits);
		PlayerUnitBatchLoc.DiscoverAndLoadLocFiles(PlayerUnit);
		//EnemiesUnitBatchLoc.DiscoverAndLoadLocFiles(EnemiesUnit);
		UnitModelBatchLoc.DiscoverAndLoadLocFiles(UnitModel);
		UltimateSkillsBatchLoc.DiscoverAndLoadLocFiles(UltimateSkills);
		StatusEffectsBatchLoc.DiscoverAndLoadLocFiles(StatusEffects);
		JadeBoxBatchLoc.DiscoverAndLoadLocFiles(JadeBoxes);
		PacksBatchLoc.DiscoverAndLoadLocFiles(Packs);
	}
}
