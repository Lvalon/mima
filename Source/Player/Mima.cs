using Cysharp.Threading.Tasks;
//using DG.Tweening;
using LBoL.ConfigData;
using LBoL.Core.Units;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using UnityEngine;
using lvalonmima.ImageLoader;
using lvalonmima.Localization;
using LBoL.Core.Battle;
using LBoL.Core;
using LBoL.Core.Battle.BattleActions;
using lvalonmima.StatusEffects;
//using lvalonmima.BattleActions;

namespace lvalonmima
{
	public sealed class lvalonmimaDef : PlayerUnitTemplate
	{
		public UniTask<Sprite>? LoadSpellPortraitAsync { get; private set; }

		public override IdContainer GetId()
		{
			return BepinexPlugin.modUniqueID;
		}

		public override LocalizationOption LoadLocalization()
		{
			return lvalonmimaLocalization.PlayerUnitBatchLoc.AddEntity(this);
		}

		public override PlayerImages LoadPlayerImages()
		{
			return lvalonmimaImageLoader.LoadPlayerImages(BepinexPlugin.playerName);
		}
		// public override EikiSummonInfo AssociateEikiSummon()
		// {
		// 	return new EikiSummonInfo(typeof(Enemies.lvalonmima));
		// }

		public override PlayerUnitConfig MakeConfig()
		{
			return lvalonmimaLoadouts.playerUnitConfig;
		}

		[EntityLogic(typeof(lvalonmimaDef))]
		public sealed class lvalonmima : PlayerUnit
		{
			protected override void OnEnterBattle(BattleController battle)
			{
				HandleBattleEvent(Battle.Player.StatusEffectAdding, OnSEAdding, GameEventPriority.Highest);
				HandleBattleEvent(Battle.Player.StatusEffectRemoving, OnSERemoving, GameEventPriority.Highest);
				HandleBattleEvent(Battle.BattleStarting, OnBattleStarting, GameEventPriority.Highest);
			}

			private void OnBattleStarting(GameEventArgs args)
			{
				React(new ApplyStatusEffectAction<seevilspirit>(Battle.Player, 1, 0, 0, 0));
				React(new ApplyStatusEffectAction<secreative>(Battle.Player, 1, 0, 0, 0));
			}
			private void OnSEAdding(StatusEffectApplyEventArgs args)
			{
				if (args.Effect is seevilspirit || args.Effect is secreative)
				{
					args.CanCancel = false;
				}
			}
			private void OnSERemoving(StatusEffectEventArgs args)
			{
				if (args.Effect is seevilspirit || args.Effect is secreative)
				{
					args.CancelBy(this);
				}
			}
		}
	}
}