using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Presentation;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using lvalonmima.Config;
using UnityEngine;
using static LBoL.Presentation.AudioManager;

namespace lvalonmima.SFX.Template
{
	public abstract class lvalonmimaSFXTemplate : SfxTemplate
	{
		public override IdContainer GetId()
		{
			return lvalonmimaDefaultConfig.DefaultID(this);
		}

		public override List<UniTask<AudioClip>> LoadSfxListAsync()
		{
			return lvalonmimaAudioClipLoader.LoadOggSFXs(this);
		}

		public SfxConfig GetCardDefaultConfig()
		{
			return DefaultConfig();
		}

		public static string GetSfxId<T>() where T : lvalonmimaSFXTemplate
		{
			var IDdef = typeof(T).Name;
			return IDdef.Remove(IDdef.Length - 3);
		}
	}

	public static class lvalonmimaAudioClipLoader
	{
		public static string file_extension = ".ogg";

		public static DirectorySource sfx_dir = new DirectorySource(PInfo.GUID, "SFX");

		public static List<UniTask<AudioClip>> LoadOggSFXs(string name)
		{
			var list = new List<UniTask<AudioClip>>
			{
				ResourceLoader.LoadAudioClip(name + file_extension, AudioType.OGGVORBIS, sfx_dir)
			};
			return list;
		}

		public static List<UniTask<AudioClip>> LoadOggSFXs(lvalonmimaSFXTemplate template)
		{
			return LoadOggSFXs(template.GetId());
		}

		public static void FixedPlaySfx(this AudioManager manager, string sfxName, float volume = -1f)
		{
			if (GameMaster.IsTurboMode)
			{
				manager.TurboPlaySfxHandler(sfxName, volume);
			}
			else
			{
				manager.PlaySfxHandler(sfxName, volume);
			}
		}

		/// <summary>
		/// 适用于加速模式的SFX播放
		/// 
		/// 请使用FixedPlaySfx
		/// </summary>
		/// <param name="manager"></param>
		/// <param name="sfxName"></param>
		/// <param name="volume"></param>
		public static void TurboPlaySfxHandler(this AudioManager manager, string sfxName, float volume = -1f)
		{
			if (!sfxName.IsNullOrEmpty() && !(sfxName == "Empty"))
			{
				if (!manager._sfxTable.TryGetValue(sfxName, out var value))
				{
					Debug.LogWarning("Sfx with name " + sfxName + " not found");
				}
				else if (AudioSettings.dspTime > value.PreviousPlayTime + value.ReplayLimit)
				{
					manager.TurboInternalPlaySfx(value, volume);
				}
				else if (AudioSettings.dspTime > value.PreviousPlayTime + value.ReplayLimit * 0.6)
				{
					double scheduledTime = value.PreviousPlayTime + value.ReplayLimit;
					manager.TurboInternalPlaySfx(value, volume, scheduledTime);
				}
			}
		}


		/// <summary>
		/// 别用，请使用FixedPlaySfx
		/// </summary>
		/// <param name="manager"></param>
		/// <param name="entry"></param>
		/// <param name="volume"></param>
		public static void TurboInternalPlaySfx(this AudioManager manager, SfxEntry entry, float volume)
		{
			AudioClip sfxClip = manager.GetSfxClip(entry);
			manager.TurboPlaySfxImmediately(sfxClip, (volume > 0f) ? (volume * entry.Volume) : entry.Volume);
			entry.PreviousPlayTime = AudioSettings.dspTime;
		}
		/// <summary>
		/// 别用，请使用FixedPlaySfx
		/// </summary>
		/// <param name="manager"></param>
		/// <param name="entry"></param>
		/// <param name="volume"></param>
		/// <param name="scheduledTime"></param>
		public static void TurboInternalPlaySfx(this AudioManager manager, SfxEntry entry, float volume, double scheduledTime)
		{
			AudioClip sfxClip = manager.GetSfxClip(entry);
			manager.TurboInternalPlaySfxScheduled(sfxClip, (volume > 0f) ? volume : entry.Volume, scheduledTime);
			entry.PreviousPlayTime = scheduledTime;
		}
		/// <summary>
		/// 别用，请使用FixedPlaySfx
		/// </summary>
		/// <param name="manager"></param>
		/// <param name="clip"></param>
		/// <param name="volume"></param>
		public static void TurboPlaySfxImmediately(this AudioManager manager, AudioClip clip, float volume)
		{
			AudioSource audioSource = manager._sourceHolder.AddComponent<AudioSource>();
			audioSource.clip = clip;
			audioSource.outputAudioMixerGroup = manager._sfxGroup;
			audioSource.volume = volume;
			audioSource.Play();
			UnityEngine.Object.Destroy(audioSource, audioSource.clip.length * 2 + 1f);
		}
		/// <summary>
		/// 别用，请使用FixedPlaySfx
		/// </summary>
		/// <param name="manager"></param>
		/// <param name="clip"></param>
		/// <param name="volume"></param>
		/// <param name="scheduledTime"></param>
		public static void TurboInternalPlaySfxScheduled(this AudioManager manager, AudioClip clip, float volume, double scheduledTime)
		{
			double num = scheduledTime - AudioSettings.dspTime;
			if (num >= 0.0)
			{
				AudioSource audioSource = manager._sourceHolder.AddComponent<AudioSource>();
				audioSource.clip = clip;
				audioSource.outputAudioMixerGroup = manager._sfxGroup;
				audioSource.volume = volume;
				audioSource.PlayScheduled(scheduledTime);
				UnityEngine.Object.Destroy(audioSource, ((float)num + audioSource.clip.length) * 2 + 1f);
			}
			else
			{
				Debug.LogWarning("一个音效的预定下次播放时间，早于当前时间，舍弃这次预定。");
			}
		}

	}

}
