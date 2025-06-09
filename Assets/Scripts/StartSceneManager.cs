using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StartSceneManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button startButton;
    public Button settingsButton;
    public Button highscoreButton;
    public Button exitButton; // 추가

    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject rankingPanel;

    [Header("Settings UI")]
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Animation Objects")]
    public GameObject logoObject;
    public GameObject characterObject;

    // 관리자 참조
    private AudioManager audioManager;
    private RankingManager rankingManager;

    void Start()
    {
        // 관리자 참조 가져오기
        audioManager = AudioManager.Instance;
        rankingManager = FindFirstObjectByType<RankingManager>();

        // 패널 초기 상태 설정 - 반드시 비활성화로 시작
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (rankingPanel != null) rankingPanel.SetActive(false);

        // 버튼 이벤트 리스너 설정
        if (startButton != null) startButton.onClick.AddListener(StartGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(ToggleSettingsPanel);
        if (highscoreButton != null) highscoreButton.onClick.AddListener(ToggleRankingPanel);
        if (exitButton != null) exitButton.onClick.AddListener(ExitGame);

        // 설정 초기화
        InitializeSettings();

        // 로고와 캐릭터 애니메이션 시작
        StartLogoAnimation();
        StartCharacterAnimation();
        
        // 오디오 시작 - AudioManager가 있으면 그것을 사용
        if (AudioManager.Instance != null && AudioManager.Instance.mainBGM != null)
        {
            AudioManager.Instance.PlayBGM(AudioManager.Instance.mainBGM);
        }
    }

    private void InitializeSettings()
    {
        // 오디오 관리자가 존재하면 슬라이더 초기화
        if (audioManager != null)
        {
            // BGM 볼륨 슬라이더 설정
            if (bgmVolumeSlider != null)
            {
                bgmVolumeSlider.value = audioManager.bgmVolume;
                bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }

            // SFX 볼륨 슬라이더 설정
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = audioManager.sfxVolume;
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
        }
        else
        {
            Debug.LogWarning("AudioManager not found! Volume sliders will not function.");
            
            // 레거시 방식으로 설정 - 추후 삭제 예정
            if (bgmVolumeSlider != null && sfxVolumeSlider != null)
            {
                float savedVolume = PlayerPrefs.GetFloat("SoundVolume", 0.75f);
                bgmVolumeSlider.value = savedVolume;
                sfxVolumeSlider.value = savedVolume;
                AudioListener.volume = savedVolume;

                bgmVolumeSlider.onValueChanged.AddListener(LegacyUpdateSoundVolume);
                sfxVolumeSlider.onValueChanged.AddListener(LegacyUpdateSoundVolume);
            }
        }
    }

    // BGM 볼륨 변경 이벤트
    private void OnBGMVolumeChanged(float volume)
    {
        if (audioManager != null)
        {
            audioManager.SetBGMVolume(volume);
        }
    }

    // SFX 볼륨 변경 이벤트
    private void OnSFXVolumeChanged(float volume)
    {
        if (audioManager != null)
        {
            audioManager.SetSFXVolume(volume);
            
            // 볼륨 변경 시 테스트 사운드 재생
            audioManager.PlayButtonClick();
        }
    }

    // 레거시 사운드 볼륨 업데이트 - 추후 삭제 예정
    private void LegacyUpdateSoundVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("SoundVolume", volume);
        PlayerPrefs.Save();
    }

    public void StartGame()
    {
        // 효과음 재생
        PlayButtonSound();

        // 게임 씬으로 전환
        SceneManager.LoadScene("GameScene");
    }

    public void ToggleSettingsPanel()
    {
        PlayButtonSound();
        
        if (settingsPanel != null)
        {
            bool isActive = !settingsPanel.activeSelf;
            settingsPanel.SetActive(isActive);
            
            // 설정 패널이 활성화되면 랜킹 패널은 닫기
            if (isActive && rankingPanel != null && rankingPanel.activeSelf)
            {
                rankingPanel.SetActive(false);
            }
        }
    }

    public void ToggleRankingPanel()
    {
        PlayButtonSound();

        // 랭킹 관리자가 있는 경우
        if (rankingManager != null)
        {
            rankingManager.ToggleRankingPanel();
            
            // 랭킹 패널이 활성화되면 설정 패널은 닫기
            if (rankingManager.rankingPanel != null && rankingManager.rankingPanel.activeSelf)
            {
                if (settingsPanel != null) settingsPanel.SetActive(false);
            }
        }
        // 랜킹 관리자가 없는 경우 (레거시 코드)
        else if (rankingPanel != null)
        {
            bool isActive = !rankingPanel.activeSelf;
            rankingPanel.SetActive(isActive);
            
            if (isActive)
            {
                // 설정 패널 닫기
                if (settingsPanel != null) settingsPanel.SetActive(false);
                
                // 고전 방식으로 하이스코어 표시
                int highscore = PlayerPrefs.GetInt("HighScore", 0);
                if (GameObject.Find("HighscoreText") != null)
                {
                    GameObject.Find("HighscoreText").GetComponent<TextMeshProUGUI>().text = "최고 점수: " + highscore;
                }
            }
        }
    }

    public void ExitGame()
    {
        PlayButtonSound();
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void PlayButtonSound()
    {
        // 오디오 관리자를 사용하여 버튼 클릭 사운드 재생
        if (audioManager != null)
        {
            audioManager.PlayButtonClick();
        }
        // 레거시 코드 - 추후 삭제 예정
        else
        {
            AudioSource source = GetComponent<AudioSource>();
            if (source != null && source.clip != null)
            {
                source.Play();
            }
        }
    }

    private void StartLogoAnimation()
    {
        if (logoObject != null)
        {
            // 로고 애니메이션
            LeanTween.moveY(logoObject, logoObject.transform.position.y + 0.5f, 1.5f)
                .setEase(LeanTweenType.easeInOutSine)
                .setLoopPingPong();
        }
    }

    private void StartCharacterAnimation()
    {
        if (characterObject != null)
        {
            MoveCharacterRandomly(); // 초기 호출
        }
    }

    private void MoveCharacterRandomly()
    {
        if (characterObject == null) return;
        
        Vector3 currentPos = characterObject.transform.position;

        // 카메라 뷰포트 범위 확인
        Camera cam = Camera.main;
        if (cam == null) return;
        
        float zDistance = Mathf.Abs(cam.transform.position.z - characterObject.transform.position.z);

        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, zDistance));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, zDistance));

        // 캐릭터가 이동 가능한 범위 설정 (가장자리에 매우 가깝게 주기 피함)
        float margin = 0.5f; // 캐릭터가 너무 가장자리에 가지 않도록
        float minX = bottomLeft.x + margin;
        float maxX = topRight.x - margin;
        float minY = bottomLeft.y + margin;
        float maxY = topRight.y - margin;

        // 다음 목표 위치 계산 (카메라 뷰포트 내부로 제한)
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        Vector3 targetPos = new Vector3(randomX, randomY, currentPos.z);

        // LeanTween으로 이동
        LeanTween.move(characterObject, targetPos, 1.5f)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnComplete(() => MoveCharacterRandomly());
    }
}