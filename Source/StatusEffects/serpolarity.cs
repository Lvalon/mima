using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class serpolarityDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.Keywords = Keyword.Exile;
			return config;
		}
	}

	[EntityLogic(typeof(serpolarityDef))]
	public sealed class serpolarity : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		public ManaGroup Mana => new ManaGroup() { Colorless = 1 };
		public ManaGroup Mana2 => new ManaGroup() { Philosophy = 1 };
		protected override void OnAdded(Unit unit)
		{
			HandleOwnerEvent(Battle.ManaGaining, OnManaGaining);
		}

		public void OnManaGaining(ManaEventArgs args)
		{
			int colorlessAmount = args.Value.Colorless;
			if (colorlessAmount > 0)
			{
				NotifyActivating();
				args.Value = args.Value.WithColorless(0) + ManaGroup.Philosophies(colorlessAmount);
				args.AddModifier(this);
			}
		}
	}
}