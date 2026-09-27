#nullable enable
using LBoL.Core.Units;
using LBoL.Presentation;

namespace lvalonmima.Common;

// Facade over BepinexPlugin's per-frame HP-threshold snapshot (u50/u25/u10).
// Checklist G10: confirmed intentional, not a bug. The per-frame snapshot is
// meant to reflect the threshold as of the latest available moment (i.e. the
// most recent Update()), not to recompute live mid-Actions -- kept as-is.
public static class MimaHp
{
	public static bool Below50 => BepinexPlugin.u50;
	public static bool Below25 => BepinexPlugin.u25;
	public static bool Below10 => BepinexPlugin.u10;

	public static int Threshold(int percent)
	{
		var player = GameMaster.Instance?.CurrentGameRun?.Player;
		return player == null ? 0 : toolbox.hpfrompercent(player, percent);
	}

	// One text helper backing the hpNNns/hpNNbs/hpNNfs properties on lvalonmimaCard,
	// which YAML descriptions read by name (e.g. {hp50ns}) and so cannot be removed.
	public static string Text(int percent, string prefix, string suffix, string fallback)
	{
		Unit? player = GameMaster.Instance?.CurrentGameRun?.Player;
		return player == null ? fallback : prefix + toolbox.hpfrompercent(player, percent) + suffix;
	}
}
