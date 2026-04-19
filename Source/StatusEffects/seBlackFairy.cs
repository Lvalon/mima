using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seBlackFairyDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(seBlackFairyDef))]
	public sealed class seBlackFairy : StatusEffect
	{
		public ManaGroup Mana => new ManaGroup { Black = 1 };
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.CardUsed, OnCardUsed);
			HandleOwnerEvent(unit.StatusEffectAdded, OnSEAdded);
		}

		private void OnSEAdded(StatusEffectApplyEventArgs args)
		{
			Highlight = args.Effect is Lunatic;
		}

		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			NotifyActivating();
			yield return ToBlack(Battle.BattleMana, 1);
			yield return DamageAction.Reaction(Battle.Player, 1);
		}
		public static ConvertManaAction ToBlack(ManaGroup mana, int count)
		{
			ManaGroup empty = ManaGroup.Empty;
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				ManaColor maxTrivialColor = ManaColors.TrivialColors
						.Where(color => color != ManaColor.Black)
						.MaxBy(mana.GetValue);
				if (mana[maxTrivialColor] <= 0)
				{
					break;
				}

				num++;
				ManaColor color = maxTrivialColor;
				int value = empty[color] + 1;
				empty[color] = value;
				color = maxTrivialColor;
				value = mana[color] - 1;
				mana[color] = value;
			}

			if (num <= 0)
			{
				return null;
			}

			return new ConvertManaAction(empty, ManaGroup.Blacks(num), allowPartial: true);
		}
	}
}