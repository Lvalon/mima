using LBoL.Core;
using LBoL.Core.Units;

namespace lvalonmima.Common;

public static class GameExtensions
{
	extension(GameRunController run)
	{
		// Replaces the `GameRun.SetHpAndMaxHp(Battle.Player.Hp + x, Battle.Player.MaxHp + x, true)`
		// copies scattered across cards. Player is passed explicitly (rather than read
		// from GameRun) because call sites use Battle.Player, which is the same instance
		// but keeps this a drop-in replacement without relying on that being guaranteed.
		public void GainMaxHpAndHp(PlayerUnit player, int amount) =>
			run.SetHpAndMaxHp(player.Hp + amount, player.MaxHp + amount, true);
	}
}
