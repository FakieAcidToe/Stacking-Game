using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
	// Used to name each AudioClip and play sounds by referencing by name
	/*
	[System.Serializable]
	class AudioData
	{
		public string clipName;
		public AudioClip audioClip;

		public AudioData(string _clipName, AudioClip _audioClip)
		{
			clipName = _clipName;
			audioClip = _audioClip;
		}
	}*/

	public AudioSource EffectsSource;
	public AudioSource MusicSource;

	//[SerializeField] List<AudioData> audioList = new List<AudioData>();

	public static SoundManager Instance = null;

	AudioClip bgmIntro = null;
	AudioClip bgmLoop = null;
	bool bgmInIntro = false;

	void Awake()
	{
		if (Instance != null) Destroy(Instance);
		Instance = this;
		//DontDestroyOnLoad(gameObject);
	}

	void Update()
	{
		if (bgmInIntro && !MusicSource.isPlaying)
		{
			PlayMusicLoop(bgmLoop);
		}
	}

	public void Play(AudioClip clip)
	{
		EffectsSource.clip = clip;
		EffectsSource.PlayOneShot(clip);
	}

	public void Play(AudioClip clip, Vector3 location)
	{
		AudioSource.PlayClipAtPoint(clip, location);
	}

	/*
	public void Play(string _clipName)
	{
		foreach (AudioData data in audioList) if (data.clipName == _clipName)
			{
				Play(data.audioClip);
				return;
			}
		Debug.LogWarning("Could not find AudioClip '" + _clipName + "'.");
	}*/

	public void PlayMusic(AudioClip _loop, AudioClip _intro = null)
	{
		// Music has Intro that plays once, then loops the Loop section
		bgmLoop = _loop;
		bgmIntro = _intro;
		if (bgmIntro != null)
		{
			PlayMusicLoop(bgmIntro);
			bgmInIntro = true;
			MusicSource.loop = false;
		}
		else
		{
			PlayMusicLoop(bgmLoop);
		}
	}

	public void PlayMusicLoop(AudioClip clip)
	{
		bgmInIntro = false;
		MusicSource.loop = true;
		MusicSource.clip = clip;
		MusicSource.Play();
	}
}