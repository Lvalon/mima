using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.EntityLib.Cards.Neutral.NoColor;
using lvalonmima.Cards;
using lvalonmima.Exhibits;
using lvalonmima.lvalonmimaUlt;
namespace lvalonmima
{
	public class lvalonmimaLoadouts
	{
		public static string UltimateSkillA = nameof(ultmimaa);
		public static string UltimateSkillB = nameof(ultmimab);

		public static string ExhibitA = nameof(exmimaa);
		public static string ExhibitB = nameof(exmimab);
		public static int maxhp = 33;
		public static List<string> DeckA = new List<string>{
			nameof(Shoot),
			nameof(Shoot),
			nameof(Shoot),
			nameof(Boundary),
			nameof(Boundary),
			nameof(cardmimaa),
			nameof(cardmimaa),
			nameof(cardmimaa),
			nameof(carderosion),
			nameof(cardchannelling),
		};

		public static List<string> DeckB = new List<string>{
			nameof(Shoot),
			nameof(Shoot),
			nameof(Shoot),
			nameof(Boundary),
			nameof(Boundary),
			nameof(cardmimab),
			nameof(cardmimab),
			nameof(cardmimab),
			nameof(cardwheresleep),
			nameof(cardoncetime),
		};

		public static PlayerUnitConfig playerUnitConfig = new PlayerUnitConfig(
			Id: BepinexPlugin.modUniqueID,
			HasHomeName: false,
			ShowOrder: int.MaxValue,
			Order: 0,
			UnlockLevel: 10,
			ModleName: "",
			NarrativeColor: "#ffffff",
			IsSelectable: true,
			MaxHp: maxhp,
			InitialMana: new ManaGroup() { Black = 2, Green = 2 },
			InitialMoney: 66,
			InitialPower: 0,
			BasicRingOrder: null,
			LeftColor: ManaColor.Philosophy,
			RightColor: ManaColor.Colorless,
			UltimateSkillA: UltimateSkillA,
			UltimateSkillB: UltimateSkillB,
			ExhibitA: ExhibitA,
			ExhibitB: ExhibitB,
			DeckA: DeckA,
			DeckB: DeckB,
			DifficultyA: 6,
			DifficultyB: 6
		);
	}
}
