using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityVector2;
using LOONACIA.Unity;
using LOONACIA.Unity.Managers;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

// MP3 Player -> AudioSource
// Mp3 음원   -> AudioClip
// 관객(귀)   -> AudioListener

public class SoundManager
{
	[Header("#BGM")]
	public AudioClip bgmClip;
	private AudioSource _bgmPlayer;
	private AudioHighPassFilter _bgmEffect;

	[Header("#SFX")]
	public AudioClip[] sfxClips;
	private int _channels = 16;
	private AudioSource[] _sfxPlayer;
	private int channelIndex;

	private float _masterVolume;
	private float _bgmVolume;
	private float _sfxVolume;


	private GameObject _root;
	private AudioMixer _audioMixer;
	private AudioMixerGroup[] _mixerGroup;

	Dictionary<string, AudioClip> _clipDic = new Dictionary<string, AudioClip>(); // 캐싱

	// 바꿀 필요가 있음. Slider 가 여기 이렇게 있을필요가 없음.
	// Slider _master;
	// Slider _sfx;
	// Slider _bgm;

	
	public float MasterVolume
	{
		get => _masterVolume;
		set
		{
			_masterVolume = value;
			SetVolume();
		}
	}
	public float BgmVolume
	{
		get => _bgmVolume;
		set
		{
			_bgmVolume = value;
			SetVolume();
		}
	}
	
	public float SfxVolume
	{
		get => _sfxVolume;
		set
		{
			_sfxVolume = value;
			SetVolume();
		}
	}

	public void Init()
	{
		_root = GameObject.Find("@Audio");
		if (_root == null)
		{
			_root = new GameObject { name = "@Audio" };
			Object.DontDestroyOnLoad(_root);
		}

		_audioMixer = Resources.Load<AudioMixer>("MainMixer");
		_mixerGroup = _audioMixer.FindMatchingGroups("Master");

		InitValues();
		Reset();
		SetVolume();

		LoadClips();
	}

	void InitValues()
	{
		_masterVolume = ManagerRoot.Settings.MasterVolume;
		_bgmVolume    = ManagerRoot.Settings.BgmVolume;
		_sfxVolume    = ManagerRoot.Settings.SfxVolume;
	}

	void SetVolume()
	{
		Debug.Log($"Sound Manager | Set Volume | {_masterVolume} {_bgmVolume} {_sfxVolume}");

		if(_bgmPlayer != null){
			_bgmPlayer.volume = _masterVolume * _bgmVolume;
		}else{
			Debug.LogWarning("Sound Manager | _bgmPlayer is null");
		}
		if(_sfxPlayer != null){

			for (int i = 0; i < _sfxPlayer.Length; i++)
			{
				_sfxPlayer[i].volume = _masterVolume * _sfxVolume;
			}
		}else{
			Debug.LogWarning("Sound Manager | _sfxPlayer is null");
		}
		
	}

	private void Reset()
	{
		// bgm player
		GameObject bgmObject = new GameObject("BgmPlayer");
		bgmObject.transform.parent = _root.transform;
		_bgmPlayer = bgmObject.AddComponent<AudioSource>();
		_bgmPlayer.playOnAwake = false;
		_bgmPlayer.loop = true;
		_bgmPlayer.clip = bgmClip;
		_bgmPlayer.outputAudioMixerGroup = _mixerGroup[1];

		_bgmEffect = GameObjectExtension.GetOrAddComponent<AudioHighPassFilter>(Camera.main.gameObject);
		_bgmEffect.enabled = false;

		// sfx player
		GameObject sfxObject = new GameObject("SfxPlayer");
		sfxObject.transform.parent = _root.transform;
		_sfxPlayer = new AudioSource[_channels];
		for (int i = 0; i < _sfxPlayer.Length; i++)
		{
			_sfxPlayer[i] = sfxObject.AddComponent<AudioSource>();
			_sfxPlayer[i].playOnAwake = false;
			_sfxPlayer[i].bypassListenerEffects = true;
			_sfxPlayer[i].outputAudioMixerGroup = _mixerGroup[2];
		}

		// Volume Settings TODO : 설정 창 UI랑 연동하기
		// var sliders = GameObject.Find("Test").GetComponentsInChildren<Slider>();
		// _master = sliders[0];
		// _sfx = sliders[1];
		// _bgm = sliders[2];

		// _master.onValueChanged.AddListener(delegate { OnMasterChanged(); });
		// _bgm.onValueChanged.AddListener(delegate { OnBgmChanged(); });
		// _sfx.onValueChanged.AddListener(delegate { OnSfxChanged(); });

	}

	// void OnMasterChanged()
	// {
	// 	_audioMixer.SetFloat("Master", Mathf.Log10(_master.value) * 20 );
	// }
	
	// void OnBgmChanged()
	// {
	// 	_audioMixer.SetFloat("BGM", Mathf.Log10(_bgm.value) * 20);
	// }

	// void OnSfxChanged()
	// {
	// 	_audioMixer.SetFloat("SFX", Mathf.Log10(_sfx.value) * 20);
	// }


	private void LoadClips()
	{
		var clips = Resources.LoadAll<AudioClip>("Sounds/");
		foreach (var clip in clips)
		{
			_clipDic.TryAdd(clip.name, clip);
		}
	}

	public void Clear() // 씬 이동시 사용
	{
		foreach(var clip in _sfxPlayer)
		{
			clip.Stop();
		}
		_bgmPlayer.Stop();
	}

	public void PlaySfx(string source_, float volume_ = 0.5f)
	{
		if ( _sfxPlayer == null) { Debug.LogWarning("SoundManager | _sfxPlayer is null"); return; }

		for (int i = 0; i < _sfxPlayer.Length; i++)
		{
			int loopIndex = (i + channelIndex) % _sfxPlayer.Length;

			if (_sfxPlayer[loopIndex].isPlaying) continue;

			channelIndex = loopIndex;
			
			if( _clipDic.TryGetValue(source_, out AudioClip clip) )
			{
				_sfxPlayer[loopIndex].clip = clip;
				_sfxPlayer[loopIndex].Play();
				break;
			}
		}
		
	}

	public void PlayBgm(bool isPlay_, string source_ = null)
	{
		if (isPlay_)
		{
			if( _clipDic.TryGetValue(source_, out AudioClip clip) )
			{
				_bgmPlayer.clip = clip;
				_bgmPlayer.Play();
			}
		}
		else
		{
			_bgmPlayer.Stop();
		}
	}

	public void EffectBgm(bool isPlay_)
	{
		_bgmEffect.enabled = isPlay_;
	}

	// public void Play(string path)
	// {
	// 	AudioClip audioClip = GetOrAddAudioClip(path, type);
	// 	if (audioClip == null)
	// 	{
	// 		Debug.Log($"AudioClip is null. path : {path}");
	// 		return;
	// 	}
	// 	// Play(audioClip, volume, type, pitch);
	// }

	// public void Play(AudioClip audioClip, float volume = 1.0f, SoundType type = SoundType.Effect, float pitch = 1.0f)
	// {
	// 	if (audioClip == null)
	// 	{
	// 		Debug.Log($"AudioClip is null");
	// 		return;
	// 	}
	// 	if (type == SoundType.Bgm)
	// 	{
	// 		AudioSource audioSource = _audioSource[(int)SoundType.Bgm];
	// 		if (audioSource.isPlaying)
	// 			audioSource.Stop();

	// 		audioSource.volume = volume;
	// 		audioSource.pitch = pitch;
	// 		audioSource.clip = audioClip;
	// 		audioSource.Play();
	// 	}
	// 	else
	// 	{
	// 		AudioSource audioSource = _audioSource[(int)SoundType.Effect];
	// 		if (audioSource == null)
	// 		{
	// 			Debug.Log($"_audioSource[{SoundType.Effect}] is null: _audioSource.Length:{_audioSource.Length}");
	// 			return;
	// 		}

	// 		//if (IsPlaying(audioClip, type))
	// 		if (IsPlaying(audioSource))
	// 		{
	// 			pitch *= 0.95f; // 예: 원래 피치의 95%로 설정
	// 			pitch = Mathf.Clamp(pitch, 0.4f, 1.0f);
	// 			//audioSource.Stop();
	// 		}

	// 		audioSource.volume = volume;
	// 		audioSource.pitch = pitch;
	// 		audioSource.PlayOneShot(audioClip);
	// 	}
	// }

	// SoundType.Effect 타입의 AudioSource가 재생중인지 확인
	private bool IsPlaying(AudioSource audioSource)
	{
		return audioSource != null && audioSource.isPlaying;
	}

	private bool IsPlaying(AudioClip audioClip)
	{
		if (audioClip == null)
		{
			Debug.Log($"AudioClip is null");
			return false;
		}
		return IsPlaying(audioClip.name);
	}
	
	//특정 AudioClip이 현재 재생 중인지 확인
	public bool IsPlaying(string soundName)
	{
		AudioClip audioClip = GetOrAddAudioClip(soundName);
		if (audioClip == null)
		{
			Debug.Log($"AudioClip is null. soundName : {soundName}");
			return false;
		}
		return IsPlaying(audioClip);
	}

	AudioClip GetOrAddAudioClip(string soundName)
	{
		if (_clipDic.TryGetValue(soundName, out AudioClip audioClip) == false)
		{
			audioClip = ManagerRoot.Resource.Load<AudioClip>(soundName);
			_clipDic.Add(soundName, audioClip);
		}
		return audioClip;
	}

	// 	AudioClip audioClip = null;

	// 	if (type == SoundType.Bgm)
	// 	{
	// 		audioClip = ManagerRoot.Resource.Load<AudioClip>(path);
	// 	}
	// 	else
	// 	{
	// 		if (_clipDic.TryGetValue(path, out audioClip) == false)
	// 		{
	// 			audioClip = ManagerRoot.Resource.Load<AudioClip>(path);
	// 			_clipDic.Add(path, audioClip);
	// 		}
	// 	}

	// 	if (audioClip == null)
	// 	{
	// 		Debug.Log($"AudioClip is null. path : {path}");
	// 	}
	// 	return audioClip;
	// }
	

}
