using LBoLEntitySideloader.Entities;
using System.Collections.Generic;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Resource;
using LBoL.ConfigData;
using LBoLEntitySideloader.Utils;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;
using Cysharp.Threading.Tasks;
using UnityEngine;
using LBoL.Base;

namespace lvalonmima
{
    public sealed class playermima : PlayerUnitTemplate
    {
        public static DirectorySource dir = new DirectorySource(PInfo.GUID, "");

        public static string name = nameof(Mima);

        public override IdContainer GetId()
        {
            return nameof(Mima);
        }

        public override LocalizationOption LoadLocalization()
        {
            return BepinexPlugin.playerbatchloc.AddEntity(this);
        }

        public override PlayerImages LoadPlayerImages()
        {
            PlayerImages sprites = new PlayerImages();

            sprites.AutoLoad("", (s) => ResourceLoader.LoadSprite(s, dir, ppu: 100, 1, FilterMode.Bilinear, generateMipMaps: true), (s) => ResourceLoader.LoadSpriteAsync(s, BepinexPlugin.directorySource));

            return sprites;
        }

        public override PlayerUnitConfig MakeConfig()
        {
            PlayerUnitConfig reimuConfig = PlayerUnitConfig.FromId("Reimu").Copy();

            PlayerUnitConfig config = new PlayerUnitConfig(
            Id: "",
            HasHomeName: false,
            ShowOrder: 2147483647,
            Order: 0,
            UnlockLevel: 10,
            ModleName: "",
            NarrativeColor: "#ffffff",
            IsSelectable: true,
            BasicRingOrder: null,
            MaxHp: 66,
            InitialMana: new LBoL.Base.ManaGroup() { Philosophy = 2, Colorless = 2 },
            InitialMoney: 66,
            InitialPower: 0,
            LeftColor: ManaColor.Philosophy,
            RightColor: ManaColor.Colorless,
            UltimateSkillA: "ulta",
            UltimateSkillB: "ultb",
            ExhibitA: "mimaa",
            ExhibitB: "mimab",
            DeckA: new List<string> { "cardchannelling", "cardmountain", "cardmimaalib", "cardremini", "carderosion" },
            DeckB: new List<string> { "cardmimablib" },
            DifficultyA: 6,
            DifficultyB: 6
            );
            return config;
        }

        [EntityLogic(typeof(playermima))]
        public sealed class Mima : PlayerUnit
        {
        }
    }

    public sealed class mimamodel : UnitModelTemplate
    {


        public override IdContainer GetId()
        {
            return new playermima().UniqueId;
        }

        public override LocalizationOption LoadLocalization()
        {
            return BepinexPlugin.modelbatchloc.AddEntity(this);
        }

        public override ModelOption LoadModelOptions()
        {
            return new ModelOption(ResourceLoader.LoadSpriteAsync("playermimasprite.png", BepinexPlugin.directorySource, ppu: 336));
        }


        public override UniTask<Sprite> LoadSpellSprite()
        {
            return ResourceLoader.LoadSpriteAsync("playermimabomb.png", playermima.dir, ppu: 336);
        }

        public override UnitModelConfig MakeConfig()
        {

            UnitModelConfig unitModelConfig = new UnitModelConfig(Name: "Mima", Type: 0, EffectName: null, Offset: new Vector2(0, -0.1f), Flip: true, Dielevel: 2,
            Box: new Vector2(0.8f, 1.8f), Shield: 1.2f, Block: 1.3f, Hp: new Vector2(0.0f, -1.3f), HpLength: 666, Info: new Vector2(0.0f, 1.2f),
            Select: new Vector2(1.6f, 2.0f), ShootStartTime: new float[] { 0.1f }, new Vector2[] { new Vector2(0.6f, 0.3f) }, ShooterPoint: new Vector2[] { new Vector2(0.6f, 0.3f) },
            Hit: new Vector2(0.3f, 0.0f), HitRep: 0.1f, GuardRep: 0.1f, Chat: new Vector2(0.4f, 0.8f), ChatPortraitXY: new Vector2(-0.8f, -0.58f),
            ChatPortraitWH: new Vector2(0.6f, 0.5f), HasSpellPortrait: true, SpellPosition: new Vector2(400.00f, 0.00f), SpellScale: 0.9f,
            SpellColor: new Color32[] { new Color32(186, 66, 255, 255), new Color32(155, 5, 193, 255), new Color32(186, 66, 255, 150), new Color32(155, 5, 193, 255) });
            return unitModelConfig;
        }
    }
}


