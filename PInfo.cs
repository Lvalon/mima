using HarmonyLib;

namespace lvalonmima
{
	public static class PInfo
	{
		public const string GUID = "llbol.char.mima";
		public const string Name = "Mima";
		public const string version = "0.1.7";
		public static readonly Harmony harmony = new Harmony(GUID);

	}
}
