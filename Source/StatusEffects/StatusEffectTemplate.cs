#nullable enable
using System;
using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using UnityEngine;
using lvalonmima.ImageLoader;
using lvalonmima.Localization;
using lvalonmima.Config;

namespace lvalonmima.StatusEffects;

public class lvalonmimaStatusEffectTemplate : StatusEffectTemplate
{
	public override IdContainer GetId()
	{
		return lvalonmimaDefaultConfig.DefaultID(this);
	}

	public override LocalizationOption LoadLocalization()
	{
		return lvalonmimaLocalization.StatusEffectsBatchLoc.AddEntity(this);
	}

	public override Sprite LoadSprite()
	{
		return lvalonmimaImageLoader.LoadStatusEffectLoader(status: this);
	}

	public override StatusEffectConfig MakeConfig()
	{
		return GetDefaultStatusEffectConfig();
	}

	public static StatusEffectConfig GetDefaultStatusEffectConfig()
	{
		return lvalonmimaDefaultConfig.DefaultStatusEffectConfig();
	}

	// Replaces the `var config = GetDefaultStatusEffectConfig(); config.Type = X; return config;`
	// boilerplate repeated across ~110 Def.MakeConfig overrides. tweak covers the
	// handful that also set HasCount, RelativeEffects, Order or Keywords.
	public static StatusEffectConfig Cfg(StatusEffectType type, bool hasCount = false, Action<StatusEffectConfig>? tweak = null)
	{
		StatusEffectConfig config = GetDefaultStatusEffectConfig();
		config.Type = type;
		if (hasCount) config.HasCount = true;
		tweak?.Invoke(config);
		return config;
	}
}
