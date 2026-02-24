using System;
using LBoLEntitySideloader.PersistentValues;
using LBoLEntitySideloader;

namespace lvalonmima.Source.Patches
{
	/// <summary>
	/// Minimal tracker that supports a single disk-registered LiteProfileSaveData instance across reloads.
	/// </summary>
	public class MiniTracker
	{
		private static MiniTracker _instance;

		// persistent store for disk-registered custom save data (survives reloads)
		private static LiteProfileSaveData s_loadedFromDiskCustomGrSaveData;

		public static MiniTracker Instance => _instance ??= new MiniTracker();

		/// <summary>
		/// Reset the active instance (used during reload).
		/// </summary>
		public static void DestroySelf() => _instance = null;

		/// <summary>
		/// Active runtime singleton save data instance.
		/// </summary>
		public LiteProfileSaveData CustomGrSaveData { get; private set; }

		public static LiteProfileSaveData LoadedFromDiskCustomGrSaveData => s_loadedFromDiskCustomGrSaveData;

		private MiniTracker()
		{
			// pick up the persisted instance if present
			if (s_loadedFromDiskCustomGrSaveData != null)
				CustomGrSaveData = s_loadedFromDiskCustomGrSaveData;
		}

		internal static void AddLoadedFromDisk(LiteProfileSaveData data)
		{
			if (s_loadedFromDiskCustomGrSaveData != null && !ReferenceEquals(s_loadedFromDiskCustomGrSaveData, data))
			{
				return;
			}
			s_loadedFromDiskCustomGrSaveData = data;
		}

		public void SetActive(LiteProfileSaveData data)
		{
			if (CustomGrSaveData != null && !ReferenceEquals(CustomGrSaveData, data))
			{
				return;
			}
			CustomGrSaveData = data;
		}

		public void MergeLoadedFromDisk()
		{
			if (CustomGrSaveData == null && s_loadedFromDiskCustomGrSaveData != null)
			{
				CustomGrSaveData = s_loadedFromDiskCustomGrSaveData;
			}
		}
	}
}

