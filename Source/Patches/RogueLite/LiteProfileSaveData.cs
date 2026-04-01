using System;
using System.Collections.Generic;
using System.Reflection;
using LBoL.Core;
using LBoL.Presentation;
using LBoLEntitySideloader.PersistentValues;
using LBoLEntitySideloader.Utils;
using YamlDotNet.Serialization;
using static LBoL.Presentation.GameMaster;

namespace lvalonmima.Source.Patches
{
	public class LiteProfileSaveData
	{
		public const string filePrefix = "gr";
		public static void RegisterCustomSaveData(LiteProfileSaveData customSaveData, string GUID)
		{
			var ass = Assembly.GetCallingAssembly();
			InternalRegisterCustomData(ass, customSaveData);
		}
		public void RegisterSelf(string GUID)
		{
			var ass = Assembly.GetCallingAssembly();
			InternalRegisterCustomData(ass, this);
		}

		public SaveDataID GetID(string GUID) => new SaveDataID()
		{
			GUID = GUID,
			Name = Name,
			midfix = filePrefix
		};

		private static void InternalRegisterCustomData(Assembly assembly, LiteProfileSaveData customData)
		{
			// If the registering assembly was loaded from disk, remember it persistently so it survives reloads
			if (assembly.IsLoadedFromDisk())
				MiniTracker.AddLoadedFromDisk(customData);

			// Set the active singleton instance (SetActive will log on duplicates)
			MiniTracker.Instance.SetActive(customData);

		}
		[YamlIgnore]
		public virtual string Name => "";
		[YamlIgnore]
		public bool DeleteFileOnGamerunEnd => false;
		internal bool EncodeToBinary => true;
		public virtual IEnumerable<IYamlTypeConverter> TypeConverters()
		{
			yield break;
		}
		public Dictionary<string, LiteShop> Saves = new Dictionary<string, LiteShop>();
		private string _activeRunProfileKey;

		private string GetProfileKey()
		{
			var profile = GameMaster.Instance?.CurrentProfile;
			if (profile != null)
				return $"profile:{profile.CreationTimestamp}_{profile.Name}";
			return null;
		}

		public LiteShop GetShopForCurrentProfile()
		{
			var key = GetProfileKey();
			if (key == null)
				return null;

			if (!Saves.TryGetValue(key, out var shop))
			{
				shop = new LiteShop();
				Saves[key] = shop;
			}

			return shop;
		}

		private LiteShop GetShopForProfileKey(string key)
		{
			if (string.IsNullOrEmpty(key))
				return null;

			if (!Saves.TryGetValue(key, out var shop))
			{
				shop = new LiteShop();
				Saves[key] = shop;
			}

			return shop;
		}

		private string GetActiveRunProfileKey()
		{
			if (_activeRunProfileKey != null)
				return _activeRunProfileKey;

			if (GameMaster.Instance?.CurrentGameRun == null)
				return GetProfileKey();

			_activeRunProfileKey = GetProfileKey();
			return _activeRunProfileKey;
		}

		private void ClearActiveRunProfileKey()
		{
			_activeRunProfileKey = null;
		}

		/// <summary>
		/// applies blue point ONLY when the run is ending during gamerun.
		/// </summary>
		// public void Save()
		// {
		// 	var gameRun = GameMaster.Instance?.CurrentGameRun;
		// 	var shop = GetShopForProfileKey(GetActiveRunProfileKey());
		// 	if (shop == null || !shop.ChallengerModeEnabled)
		// 		return;

		// 	if (GameMaster.Instance?.CurrentGameRun?.Status == GameRunStatus.Running)
		// 	{
		// 		return; // skip sl
		// 	}

		// 	int bluePoint = EndGameStatistics(gameRun, gameRun.GameRunRecord.ResultType).BluePoint;
		// 	bluePoint = ApplyNormalizeSaveRules(shop, gameRun, bluePoint);
		// 	if (bluePoint > 0)
		// 	{
		// 		shop.AddMoney(bluePoint);
		// 	}
		// 	ClearActiveRunProfileKey();
		// }

		public void Save(int amount, bool ending = true)
		{
			var shop = GetShopForProfileKey(GetActiveRunProfileKey());
			if (shop == null || !shop.ChallengerModeEnabled)
				return;

			int adjusted = ApplyBluePointMults(shop, GameMaster.Instance?.CurrentGameRun, amount);
			if (adjusted > 0)
			{
				shop.AddMoney(adjusted);
			}
			if (ending)
				ClearActiveRunProfileKey();
		}

		private static int ApplyBluePointMults(LiteShop shop, GameRunController gameRun, int bluePoint)
		{
			double mult = 1.0;
			if (shop?.GetItem("difficulty.reverse")?.CurrentTier > 0)
			{
				if (!IsWinningResult(gameRun?.GameRunRecord?.ResultType ?? GameResultType.Failure))
					return 0;
				mult *= 0.5;
			}

			if (shop?.GetItem("difficulty.ascension")?.CurrentTier > 0)
				mult *= 2;

			return toolbox.Round(mult * bluePoint);
		}

		private static bool IsWinningResult(GameResultType resultType)
		{
			return resultType != GameResultType.Failure;
		}

		/// <summary>
		/// Restore merges deserialized data into the active LiteProfileSaveData instance (if present), or registers itself as active.
		/// </summary>
		public void Restore()
		{
			try
			{
				var active = MiniTracker.Instance.CustomGrSaveData;
				if (active == null)
				{
					ReconcileSaves(Saves);
					// No active instance; set this deserialized instance as active
					MiniTracker.Instance.SetActive(this);
					return;
				}

				// Merge saved shops into active instance
				foreach (var kv in Saves)
				{
					active.Saves[kv.Key] = LiteShop.ReconcileWithDefaults(kv.Value);
				}
			}
			catch (Exception)
			{
			}
		}

		private static void ReconcileSaves(Dictionary<string, LiteShop> saves)
		{
			if (saves == null || saves.Count == 0)
				return;

			var keys = new List<string>(saves.Keys);
			foreach (var key in keys)
			{
				saves[key] = LiteShop.ReconcileWithDefaults(saves[key]);
			}
		}

		/// <summary>
		/// Execute code after gamerun has ended. GameMaster.Instance.CurrentGameRun might still be available at that point.
		/// </summary>
		public virtual void OnGamerunEnded()
		{
		}
	}
}