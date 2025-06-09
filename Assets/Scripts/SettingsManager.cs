using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Settings Panel")]
    public GameObject settingsPanel;

    [Header("Volume Sliders")]
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;

    private void Start()
    {
        InitializeSettings();
    }

    private void InitializeSettings()
    {
        // 초기 패널 상태 설정
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // AudioManager가 존재하는지 확인
        if (AudioManager.Instance != null)
        {
            // BGM 볼륨 슬라이더 설정
            if (bgmVolumeSlider != null)
            {
                bgmVolumeSlider.value = AudioManager.Instance.bgmVolume;
                bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }

            // SFX 볼륨 슬라이더 설정
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = AudioManager.Instance.sfxVolume;
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
        }
        else
        {
            Debug.LogWarning("AudioManager not found! Volume sliders will not function.");
        }
    }

    // BGM 볼륨 변경 이벤트
    private void OnBGMVolumeChanged(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(volume);
        }
    }

    // SFX 볼륨 변경 이벤트
    private void OnSFXVolumeChanged(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(volume);
        }
    }

    // 설정 패널 토글
    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }

    // 설정 패널 열기
    public void OpenSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    // 설정 패널 닫기
    public void CloseSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
}
