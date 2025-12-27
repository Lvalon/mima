using Cysharp.Threading.Tasks;
using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using LBoLEntitySideloader.Utils;
using UnityEngine;
using lvalonmima.Localization;
using LBoL.Presentation;

namespace lvalonmima.model

{
	public sealed class lvalonmimamodel : UnitModelTemplate
	{
		//If an ingame model is load, load the chararacter model, otherwise use DirResources/lvalonmimamodel.png 
		public static bool useInGameModel = BepinexPlugin.useInGameModel;
		public static string model_name = useInGameModel ? BepinexPlugin.modelName : "lvalonmimamodel.png";
		//If a custom model is used, use a custom sprite for the Ultimate animation.
		public static string spellsprite_name = "lvalonmimaStand.png";

		public override IdContainer GetId()
		{
			//return new lvalonmimaPlayerDef().UniqueId;
			return BepinexPlugin.modUniqueID;
		}

		public override LocalizationOption LoadLocalization()
		{
			return lvalonmimaLocalization.UnitModelBatchLoc.AddEntity(this);
		}

		public override ModelOption LoadModelOptions()
		{
			if (useInGameModel)
			{
				//Load the character's spine.
				return new ModelOption(ResourcesHelper.LoadSpineUnitAsync(model_name));
			}

			else
			{
				//Load the custom character's sprite.
				return new ModelOption(ResourceLoader.LoadSpriteAsync(model_name, BepinexPlugin.directorySource, ppu: 1344));
			}
		}

		public override UniTask<Sprite> LoadSpellSprite()
		{
			if (useInGameModel)
			{
				//Load the ingame character's portrait for the Ultimate.
				return ResourcesHelper.LoadSpellPortraitAsync(model_name);
			}
			else
			{
				//Load the custom character's portrait.
				return ResourceLoader.LoadSpriteAsync(spellsprite_name, BepinexPlugin.directorySource, ppu: 336);
			}
		}

		public override UnitModelConfig MakeConfig()
		{
			if (useInGameModel)
			{
				UnitModelConfig config = UnitModelConfig.FromName(model_name).Copy();
				//Flipping the model is only necessary for enemy portraits. 
				config.Flip = BepinexPlugin.modelIsFlipped;
				return config;
			}
			else
			{
				// UnitModelConfig config = DefaultConfig().Copy();
				// config.SpellColor = new List<Color32>
				// {
				// 	new Color32(230, 72, 230, byte.MaxValue),
				// 	new Color32(213, 118, 223, byte.MaxValue),
				// 	new Color32(213, 118, 223, 150),
				// 	new Color32(208, 127, 220, byte.MaxValue)
				// };
				// config.SpellScale = 2f;
				// config.Flip = BepinexPlugin.modelIsFlipped;
				// config.Type = 0;
				// config.Offset = new Vector2(-0.10f, -0.10f);
				// config.HasSpellPortrait = true;
				// return config;
				UnitModelConfig unitModelConfig = new UnitModelConfig(Name: "Mima", Type: 0, EffectName: null, Offset: new Vector2(0f, 0f), Flip: true, Dielevel: 2,
					Box: new Vector2(0.8f, 1.8f), Shield: 1.2f, Block: 1.3f, Hp: new Vector2(0.0f, -1.3f), HpLength: 66, Info: new Vector2(0.0f, 1.2f),
					Select: new Vector2(1.6f, 2.0f), ShootStartTime: new float[] { 0.1f }, new Vector2[] { new Vector2(0.6f, 0.3f) }, ShooterPoint: new Vector2[] { new Vector2(0.6f, 0.3f) },
					Hit: new Vector2(0.3f, 0.0f), HitRep: 0.1f, GuardRep: 0.1f, Chat: new Vector2(0.4f, 0.8f), ChatPortraitXY: new Vector2(-0.8f, -0.58f),
					ChatPortraitWH: new Vector2(0.6f, 0.5f), HasSpellPortrait: true, SpellPosition: new Vector2(400.00f, 100.00f), SpellScale: 1.0f,
					SpellColor: new Color32[] { new Color32(186, 66, 255, 255), new Color32(155, 5, 193, 255), new Color32(186, 66, 255, 150), new Color32(155, 5, 193, 255) });
				return unitModelConfig;
			}
		}
	}
}