using Cysharp.Threading.Tasks;
using LBoL.ConfigData;
using LBoL.Core.Units;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using LBoLEntitySideloader.Utils;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static lvalonmima.BepinexPlugin;
using static lvalonmima.playermimadef;

namespace lvalonmima
{
    public sealed class playermimadef : PlayerUnitTemplate
    {
        //public static DirectorySource dir = new DirectorySource(PluginInfo.GUID, "Mima");

        public static string name = nameof(Mima);

        public override IdContainer GetId() => nameof(Mima);

        public override LocalizationOption LoadLocalization()
        {
            var loc = new GlobalLocalization(BepinexPlugin.embeddedSource);
            loc.LocalizationFiles.AddLocaleFile(LBoL.Core.Locale.En, "playerEn.yaml");
            return loc;
        }

        public override PlayerImages LoadPlayerImages()
        {
            var sprites = new PlayerImages();
            var asyncLoading = ResourceLoader.LoadSpriteAsync("playermimaportrait.png", (DirectorySource)embeddedSource);
            //var asyncLoading = ResourceLoader.LoadSpriteAsync("utsuho.png", directorySource);

            sprites.SetStartPanelStand(asyncLoading);
            sprites.SetWinStand(asyncLoading);
            sprites.SetDeckStand(asyncLoading);

            return sprites;
        }

        public override PlayerUnitConfig MakeConfig()
        {
            var reimuConfig = PlayerUnitConfig.FromId("Reimu").Copy();

            var config = new PlayerUnitConfig(
            Id: "",
            ShowOrder: 2147483647,
            Order: 0,
            UnlockLevel: 0,
            ModleName: "",
            NarrativeColor: "#007e56",
            IsSelectable: true,
            MaxHp: 40,
            InitialMana: new LBoL.Base.ManaGroup() { Blue = 2, Black = 2 },
            InitialMoney: 120,
            InitialPower: 0,
            UltimateSkillA: "ReimuUltR",
            UltimateSkillB: "ReimuUltW",
            ExhibitA: "ReimuR",
            ExhibitB: "ReimuW",
            DeckA: new List<string> { "Shoot", "Shoot", "Shoot", "Shoot", "Shoot", "Shoot", "Shoot", "Shoot", "Shoot", "Shoot" },
            DeckB: new List<string> { "Shoot", "Shoot", "Shoot", "Shoot", "Shoot", "Shoot", "Shoot", "Shoot", "Shoot", "Shoot" },
            //UltimateSkillA: "bombmimaA",
            //UltimateSkillB: "bombmimaB",
            //ExhibitA: "mimaA",
            //ExhibitB: "mimaB",
            //DeckA: new List<string> { "Shoot", "Shoot", "Shoot", "Shoot", "mimaattack", "mimaattack", "mimaattack", "mimaattack", "mimadiscover", "mimadiscover" },
            //DeckB: new List<string> { "Shoot", "Shoot", "Shoot", "Shoot", "mimaattack", "mimaattack", "mimaattack", "mimaattack", "mimastarteraoe", "mimastarteraoe" },
            DifficultyA: 3,
            DifficultyB: 2
            );
            return config;
        }

        [EntityLogic(typeof(playermimadef))]
        public sealed class Mima : PlayerUnit { }
    }

    public sealed class mimamodeldef : UnitModelTemplate
    {
        public override IdContainer GetId() => nameof(mimamodeldef);
        //public override IdContainer GetId() => new mimamodeldef().UniqueId;

        public override LocalizationOption LoadLocalization() => new DirectLocalization(new Dictionary<string, object>() { { "Default", "Mima" }, { "Short", "Mima" } });

        public override ModelOption LoadModelOptions()
        {
            return new ModelOption(ResourceLoader.LoadSpriteAsync("playermimasprite.png", (DirectorySource)embeddedSource, ppu: 84));
        }

        public override UniTask<Sprite> LoadSpellSprite() => ResourceLoader.LoadSpriteAsync("playermimabomb.png", (DirectorySource)embeddedSource, ppu: 336);

        public override UnitModelConfig MakeConfig()
        {
            var config = UnitModelConfig.FromName("Reimu").Copy();
            config.Flip = false;
            config.Type = 0;
            config.Offset = new Vector2(0, 0);
            //config.Offset = new Vector2(0, 0.04f);
            return config;
        }
    }
}
