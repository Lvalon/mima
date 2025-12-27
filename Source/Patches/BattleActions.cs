using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Units;

namespace lvalonmima.Patches
{
	public class DeadEventArgs : GameEventArgs
	{
		public int Amount { get; set; }
		public Unit argsunit { get; set; }

		// not working

		public override string GetBaseDebugString()
		{
			return "Life Changed: " + this.Amount + " for " + (this.argsunit != null ? this.argsunit.Name : "Player");
		}
	}
	[HarmonyPatch]
	public sealed class DeadAction : SimpleEventBattleAction<DeadEventArgs>
	{
		internal DeadAction()
		{
			this.Args = new DeadEventArgs { };
		}

		public override void PreEventPhase()
		{
			this.Trigger(CustomGameEventManager.PreDeadEvent);
		}
		public override void MainPhase()
		{
		}
		public override void PostEventPhase()
		{
			this.Trigger(CustomGameEventManager.PostDeadEvent);
		}
	}
}
