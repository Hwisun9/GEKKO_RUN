using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Clips")]
    public AudioClip mainBGM;
    public AudioClip buttonClickSound;
    public AudioClip itemCollectSound;
    public AudioClip hitSound;
    public AudioClip skillActivateSound;
    public AudioClip magnetSound;
    public AudioClip mushroomSound;
    public AudioClip hidePotionSound;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float bgmVolume = 1.0f;
    [Range(0f, 1f)]
    public float sfxVolume = 1.0f;

    // 내부적으로 관리되는 오디오 소스들 (Inspector에 노출되지 않음)
    private AudioSource bgmSource;
    private List<AudioSource> sfxSources = new List<AudioSource>();

    // 볼륨 값 저장을 위한 PlayerPrefs 키
    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    private void Awake()
    {
        // 싱글톤 패턴 적용
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 초기 설정
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudio()
    {
        // PlayerPrefs에서 저장된 볼륨 설정 로드
        bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1.0f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1.0f);

        // BGM용 AudioSource 생성
        CreateBGMSource();

        // 기본 SFX 소스 몇 개 미리 생성 (성능 최적화)
        CreateInitialSFXSources(3);

        // 볼륨 적용
        ApplyVolume();

        // BGM 재생
        if (mainBGM != null)
        {
            PlayBGM(mainBGM);
        }
    }

    private void CreateBGMSource()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolume;
    }

    private void CreateInitialSFXSources(int count)
    {
        for (int i = 0; i < count; i++)
        {
            CreateNewSFXSource();
        }
    }

    private AudioSource CreateNewSFXSource()
    {
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.loop = false;
        newSource.playOnAwake = false;
        newSource.volume = sfxVolume;
        sfxSources.Add(newSource);
        return newSource;
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null) return;

        if (bgmSource.clip != clip)
        {
            bgmSource.Stop();
            bgmSource.clip = clip;
            bgmSource.Play();
        }
        else if (!bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1.0f)
    {
        if (clip == null) return;

        // 사용 가능한 오디오 소스 찾기
        AudioSource source = GetAvailableSFXSource();
        source.clip = clip;
        source.volume = sfxVolume * volumeScale;
        source.Play();
    }
    
    // 사용 가능한 SFX 오디오 소스 가져오기
    private AudioSource GetAvailableSFXSource()
    {
        // 현재 재생 중이 아닌 오디오 소스 찾기
        foreach (var source in sfxSources)
        {
            if (source != null && !source.isPlaying)
                return source;
        }
        
        // 사용 가능한 소스가 없으면 새로 생성
        return CreateNewSFXSource();
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, bgmVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        
        // 모든 SFX 소스의 볼륨 조정
        foreach (var source in sfxSources)
        {
            if (source != null)
                source.volume = sfxVolume;
        }
        
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.Save();
    }

    private void ApplyVolume()
    {
        // BGM 볼륨 적용
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }
        
        // SFX 볼륨 적용
        foreach (var source in sfxSources)
        {
            if (source != null)
                source.volume = sfxVolume;
        }
    }

    // 다른 오디오 관련 기능
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    public void PauseBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.UnPause();
        }
    }

    public void MuteBGM(bool mute)
    {
        if (bgmSource != null)
        {
            bgmSource.mute = mute;
        }
    }

    public void MuteSFX(bool mute)
    {
        foreach (var source in sfxSources)
        {
            if (source != null)
                source.mute = mute;
        }
    }
    
    // 일반적으로 사용되는 효과음 플레이 함수들
    public void PlayButtonClick()
    {
        if (buttonClickSound != null)
            PlaySFX(buttonClickSound);
    }
    
    public void PlayItemCollect()
    {
        if (itemCollectSound != null)
            PlaySFX(itemCollectSound);
    }
    
    public void PlayHitSound()
    {
        if (hitSound != null)
            PlaySFX(hitSound);
    }
    
    public void PlaySkillActivate()
    {
        if (skillActivateSound != null)
            PlaySFX(skillActivateSound);
    }

    public void PlayMagnetSound()
    {
        if (magnetSound != null)
            PlaySFX(magnetSound);
    }

    public void PlayMushroomSound()
    {
        if (mushroomSound != null)
            PlaySFX(mushroomSound);
    }

    public void PlayHidePotionSound()
    {
        if (hidePotionSound != null)
            PlaySFX(hidePotionSound);
    }

    // 메모리 정리 (필요시 호출)
    public void CleanupUnusedSources()
    {
        for (int i = sfxSources.Count - 1; i >= 0; i--)
        {
            if (sfxSources[i] == null)
            {
                sfxSources.RemoveAt(i);
            }
            else if (!sfxSources[i].isPlaying && sfxSources.Count > 3)
            {
                // 기본 3개 이상의 소스가 있고 재생 중이 아닌 경우 제거
                DestroyImmediate(sfxSources[i]);
                sfxSources.RemoveAt(i);
            }
        }
    }

    // GameManager에서 필요한 BGM 관련 메소드들
    public void SetBGMPitch(float pitch)
    {
        if (bgmSource != null)
        {
            bgmSource.pitch = pitch;
        }
    }

    public float GetBGMPitch()
    {
        return bgmSource != null ? bgmSource.pitch : 1f;
    }

    public float GetBGMVolume()
    {
        return bgmSource != null ? bgmSource.volume : bgmVolume;
    }

    public void SetBGMVolumeDirectly(float volume)
    {
        if (bgmSource != null)
        {
            bgmSource.volume = volume;
        }
    }

    public bool IsBGMPlaying()
    {
        return bgmSource != null && bgmSource.isPlaying;
    }
}
