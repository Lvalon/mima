using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class sefuckyou700Def : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.Order = 1;
			config.HasCount = true;
			return config;
		}
	}

	[EntityLogic(typeof(sefuckyou700Def))]
	public sealed class sefuckyou700 : StatusEffect
	{
		int truecount = 1000;
		public int truecounter => truecount;
		float buffer = 0;
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			truecount = 1000;
			buffer = 0;
			Count = truecount;
			HandleOwnerEvent(Battle.Player.DamageReceived, OnDmgReceived);
		}

		private void OnDmgReceived(DamageEventArgs args)
		{
			if (toolbox.Round(args.DamageInfo.Amount + buffer) < toolbox.hpfrompercent(Battle.Player, 1))
			{
				buffer += args.DamageInfo.Amount;
				return;
			}
			int count2 = Count - toolbox.Round((args.DamageInfo.Amount + buffer) * 100.0 / Battle.Player.MaxHp);
			buffer = 0;
			if (count2 <= 0)
			{
				NotifyActivating();
				Battle.RequestDebugAction(new InstantWinAction(), "lvalonmima: Transmigrating to Perlereino+ instawin effect");
			}
			else
			{
				truecount = count2;
				Count = truecount;
			}
		}
	}
}