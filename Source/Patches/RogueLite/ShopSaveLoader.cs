using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.SaveData;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Serialization;
using static LBoL.Presentation.GameMaster;
using lvalonmima.Exhibits;

namespace lvalonmima.Source.Patches;

public static class ShopSaveLoader
{
	private const string SaveFileName = "lvalonmimaShopSave.txt";
	private static bool _loadFailed;
	private static bool _pendingRestoreQuestHydration;
	public static bool IsGameRunRestoreInProgress { get; private set; }

	private static readonly byte[] EncryptionKey =
	[
		0x4C, 0x76, 0x61, 0x6C, 0x6F, 0x6E, 0x6D, 0x69,
		0x6D, 0x61, 0x53, 0x68, 0x6F, 0x70, 0x4B, 0x65,
		0x79, 0x32, 0x30, 0x32, 0x34, 0x53, 0x65, 0x63,
		0x72, 0x65, 0x74, 0x44, 0x61, 0x74, 0x61, 0x21
	];

	private static readonly byte[] EncryptionIV =
	[
		0x21, 0x61, 0x74, 0x61, 0x44, 0x74, 0x65, 0x72,
		0x63, 0x65, 0x53, 0x34, 0x32, 0x30, 0x32, 0x79
	];

	private static byte[] Encrypt(byte[] data)
	{
		using Aes aes = Aes.Create();
		aes.Key = EncryptionKey;
		aes.IV = EncryptionIV;
		aes.Mode = CipherMode.CBC;
		aes.Padding = PaddingMode.PKCS7;

		using ICryptoTransform encryptor = aes.CreateEncryptor();
		using MemoryStream ms = new();
		using CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write);
		cs.Write(data, 0, data.Length);
		cs.FlushFinalBlock();
		return ms.ToArray();
	}

	private static byte[] Decrypt(byte[] encryptedData)
	{
		using Aes aes = Aes.Create();
		aes.Key = EncryptionKey;
		aes.IV = EncryptionIV;
		aes.Mode = CipherMode.CBC;
		aes.Padding = PaddingMode.PKCS7;

		using ICryptoTransform decryptor = aes.CreateDecryptor();
		using MemoryStream ms = new(encryptedData);
		using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
		using MemoryStream resultStream = new();
		cs.CopyTo(resultStream);
		return resultStream.ToArray();
	}

	private static string GetSaveFilePath()
	{
		return Path.Combine(GameMaster.PlatformHandler.GetSaveDataFolder(), SaveFileName);
	}

	public static bool ConsumePendingRestoreQuestHydration()
	{
		if (!_pendingRestoreQuestHydration)
			return false;

		_pendingRestoreQuestHydration = false;
		return true;
	}

	public static void SetGameRunRestoreInProgress(bool inProgress)
	{
		IsGameRunRestoreInProgress = inProgress;
	}

	public static bool GetGameRunRestoreInProgress()
	{
		return IsGameRunRestoreInProgress;
	}

	public static void Save()
	{
		var filePath = GetSaveFilePath();
		if (_loadFailed && File.Exists(filePath))
			return;

		var customData = MiniTracker.Instance.CustomGrSaveData;
		var csd = customData;
		var shop = customData?.GetShopForCurrentProfile();
		Dictionary<string, int> originalQuestProgress = null;
		Dictionary<string, string> originalQuestRequirements = null;
		HashSet<string> originalCompletedQuestCards = null;
		Dictionary<string, int> originalQuestModifiers = null;
		try
		{
			if (shop != null)
			{
				originalQuestProgress = shop.QuestProgress != null
					? new Dictionary<string, int>(shop.QuestProgress, StringComparer.Ordinal)
					: new Dictionary<string, int>(StringComparer.Ordinal);

				originalQuestRequirements = shop.QuestRequirements != null
					? new Dictionary<string, string>(shop.QuestRequirements, StringComparer.Ordinal)
					: new Dictionary<string, string>(StringComparer.Ordinal);

				originalCompletedQuestCards = shop.QuestCompletedCards != null
					? new HashSet<string>(shop.QuestCompletedCards.Where(id => !string.IsNullOrEmpty(id)), StringComparer.Ordinal)
					: new HashSet<string>(StringComparer.Ordinal);

				originalQuestModifiers = shop.QuestModifiers != null
					? new Dictionary<string, int>(shop.QuestModifiers, StringComparer.Ordinal)
					: new Dictionary<string, int>(StringComparer.Ordinal);

				Dictionary<string, int> persistedProgress = originalQuestProgress
					.Where(kvp => !string.IsNullOrEmpty(kvp.Key))
					.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);

				Dictionary<string, string> persistedRequirements = originalQuestRequirements
					.Where(kvp => !string.IsNullOrEmpty(kvp.Key)
						&& !string.IsNullOrEmpty(kvp.Value)
						&& persistedProgress.ContainsKey(kvp.Key))
					.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);

				HashSet<string> persistedCompleted = new(
					originalCompletedQuestCards.Where(id => !string.IsNullOrEmpty(id)),
					StringComparer.Ordinal);

				Dictionary<string, int> persistedModifiers = originalQuestModifiers
					.Where(kvp => !string.IsNullOrEmpty(kvp.Key))
					.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);

				shop.QuestProgress = persistedProgress;
				shop.QuestRequirements = persistedRequirements;
				shop.QuestCompletedCards = persistedCompleted;
				shop.QuestModifiers = persistedModifiers;

			}

			// csd.Save(); // Note: csd.Save() adds BluePoints from current run, usually not desired if just saving shop state from menu.
			// However, if we are just persisting the current state (which includes purchased items), we just serialize csd.

			using StringWriter stringWriter = new() { NewLine = "\n" };
			var seBuilder = new SerializerBuilder().DisableAliases();
			csd.TypeConverters().Do((tc) => { seBuilder = seBuilder.WithTypeConverter(tc); });

			seBuilder.Build().Serialize(stringWriter, csd);
			var data = SaveDataHelper.EncodeYaml(stringWriter.ToString(), false);
			var encryptedData = Encrypt(data);

			WriteSaveData(SaveFileName, encryptedData);
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogDebug(e);
		}
		finally
		{
			if (shop != null)
			{
				shop.QuestProgress = originalQuestProgress ?? new Dictionary<string, int>(StringComparer.Ordinal);
				shop.QuestRequirements = originalQuestRequirements ?? new Dictionary<string, string>(StringComparer.Ordinal);
				shop.QuestCompletedCards = originalCompletedQuestCards ?? new HashSet<string>(StringComparer.Ordinal);
				shop.QuestModifiers = originalQuestModifiers ?? new Dictionary<string, int>(StringComparer.Ordinal);
			}
		}
	}

	public static void Load(string location)
	{
		_pendingRestoreQuestHydration = string.Equals(location, "GameRunController.Restore", StringComparison.Ordinal);
		var customData = MiniTracker.Instance.CustomGrSaveData ?? MiniTracker.LoadedFromDiskCustomGrSaveData;
		if (customData == null)
		{
			_pendingRestoreQuestHydration = false;
			return;
		}
		if (MiniTracker.Instance.CustomGrSaveData == null)
			MiniTracker.Instance.SetActive(customData);
		var csd = customData;

		var filePath = GetSaveFilePath();

		if (!File.Exists(filePath))
		{
			_pendingRestoreQuestHydration = false;
			return;
		}

		try
		{
			var deBuilder = new DeserializerBuilder().IgnoreUnmatchedProperties();
			csd.TypeConverters().Do((tc) => { deBuilder = deBuilder.WithTypeConverter(tc); });

			var encryptedData = File.ReadAllBytes(filePath);
			var decryptedData = Decrypt(encryptedData);
			var decodedYaml = SaveDataHelper.DecodeYaml(decryptedData);
			object csdObject = deBuilder.Build().Deserialize(decodedYaml, csd.GetType());

			var loadedData = (LiteProfileSaveData)csdObject;
			if (loadedData?.Saves != null)
			{
				foreach (var key in loadedData.Saves.Keys.ToList())
				{
					loadedData.Saves[key] = LiteShop.ReconcileWithDefaults(loadedData.Saves[key]);
				}
			}

			loadedData.Restore();
			_loadFailed = false;
		}
		catch (Exception)
		{
			_loadFailed = true;
			_pendingRestoreQuestHydration = false;
		}
	}
}

[HarmonyPatch(typeof(MainMenuPanel), nameof(MainMenuPanel.Awake))]
class MainMenuPanel_Awake_Patch
{
	static void Postfix()
	{
		ShopSaveLoader.Load("MainMenuPanel.Awake");
		LiteShopButton.RefreshMainMenuButtonLabel();
	}
}

[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.Save))]
[HarmonyPriority(Priority.VeryLow)]
class GameRunController_Save_Patch
{

	static void Postfix()
	{
		// 2do maybe. Optimize memory usage by not storing container values statically.
		var customData = MiniTracker.Instance?.CustomGrSaveData;
		if (customData == null)
			return;

		customData.Save(0, false);

		ShopSaveLoader.Save();
	}
}

[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.Restore))]
[HarmonyPriority(Priority.VeryHigh)]
class GameRunController_Restore_Patch
{
	static void Prefix()
	{
		ShopSaveLoader.SetGameRunRestoreInProgress(true);
	}

	static void Postfix()
	{
		try
		{
			ShopSaveLoader.Load("GameRunController.Restore");
			exquesting.ProcessDeferredRestoreHydration();

			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop != null && shop.ChallengerModeEnabled && shop.GetItem("alter.wings")?.CurrentTier > 0)
				GameMaster.Instance?.StartCoroutine(DeferredAddMapModeOverrider());
		}
		finally
		{
			ShopSaveLoader.SetGameRunRestoreInProgress(false);
		}
	}

	private static IEnumerator DeferredAddMapModeOverrider()
	{
		yield return null;

		var gameRun = Singleton<GameMaster>.Instance?.CurrentGameRun;
		if (gameRun?._mapModeOverriders != null && !gameRun._mapModeOverriders.Contains(RogueliteCrosser.Instance))
		{
			gameRun._mapModeOverriders.Add(RogueliteCrosser.Instance);
			gameRun.CheckMapMode();
		}
	}
}

[HarmonyPatch(typeof(GameMaster))]
[HarmonyPatch(nameof(CoAbandonGameRun))]
public static class CoAbandonGameRun_Postfix
{
	static void Postfix(GameStatisticData data)
	{
		var customData = MiniTracker.Instance.CustomGrSaveData;
		try
		{
			// Save logic from LiteProfileSaveData (add BluePoints)
			customData.Save(0, false);

			// Serialize to disk
			ShopSaveLoader.Save();
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogDebug(e);
		}
	}
}
