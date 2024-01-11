using HarmonyLib;

namespace lvalonmima
{
    public static class PInfo
    {
        // each loaded plugin needs to have a unique GUID. usually author+generalCategory+Name is good enough
        public const string GUID = "llbol.ea.mima";
        public const string Name = "llvalonmima";
        public const string version = "0.0.34";
        public static readonly Harmony harmony = new Harmony(GUID);

    }
}
