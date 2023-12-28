using LBoLEntitySideloader.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using LBoLEntitySideloader;
using static lvalonmima.BepinexPlugin;
using LBoLEntitySideloader.Resource;
using LBoL.ConfigData;
using LBoLEntitySideloader.Utils;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;
using Cysharp.Threading.Tasks;
using UnityEngine;
using LBoL.Core;

namespace lvalonmima
{
    public sealed class playermima : PlayerUnitTemplate
    {
        public static DirectorySource dir = new DirectorySource(PluginInfo.GUID, "lvalonmima");

        public static string name = nameof(Mima);

        public override IdContainer GetId() => nameof(Mima);

        public override LocalizationOption LoadLocalization()
        {
            return toolbox.locplayer();
        }

        public override PlayerImages LoadPlayerImages()
        {
            var sprites = new PlayerImages();

            sprites.AutoLoad("", (s) => ResourceLoader.LoadSprite(s, dir, ppu: 100, 1, FilterMode.Bilinear, generateMipMaps: true), (s) => ResourceLoader.LoadSpriteAsync(s, dir));

            return sprites;
        }

        public override PlayerUnitConfig MakeConfig()
        {
            var reimuConfig = PlayerUnitConfig.FromId("Reimu").Copy();

            var config = new PlayerUnitConfig(
            Id: "",
            ShowOrder: 2147483647,
            Order: 0,
            UnlockLevel: 10,
            ModleName: "",
            NarrativeColor: "#ffffff",
            IsSelectable: true,
            MaxHp: 66,
            InitialMana: new LBoL.Base.ManaGroup() { Philosophy = 2, Colorless = 2 },
            InitialMoney: 66,
            InitialPower: 0,
            //temp
            UltimateSkillA: "ulta",
            UltimateSkillB: "ultb",
            ExhibitA: "mimaa",
            ExhibitB: "mimab",
            DeckA: new List<string> { "Shoot", "Shoot", "Shoot", "Shoot" },
            DeckB: new List<string> { "Shoot", "Shoot", "Shoot", "Shoot" },
            DifficultyA: 6,
            DifficultyB: 6
            );
            return config;
        }

        [EntityLogic(typeof(playermima))]
        public sealed class Mima : PlayerUnit { }
    }

    public sealed class mimamodel : UnitModelTemplate
    {


        public override IdContainer GetId() => new playermima().UniqueId;

        public override LocalizationOption LoadLocalization()
        {
            return toolbox.locmodel();
        }

        public override ModelOption LoadModelOptions()
        {
            return new ModelOption(ResourceLoader.LoadSpriteAsync("playermimasprite.png", directorySource, ppu: 336));
        }


        public override UniTask<Sprite> LoadSpellSprite() => ResourceLoader.LoadSpriteAsync("playermimabomb.png", playermima.dir, ppu: 336);


        public override UnitModelConfig MakeConfig()
        {

            var config = UnitModelConfig.FromName("Reimu").Copy();
            config.Flip = true;
            config.Type = 0;
            config.Offset = new Vector2(0, -0.1f);
            return config;

        }
    }
}
