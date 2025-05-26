using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StartSceneManager : MonoBehaviour
{
    public Button startButton;
    public Button settingsButton;
    public Button highscoreButton;

    public GameObject settingsPanel;
    public GameObject highscorePanel;

    public TextMeshProUGUI highscoreText;
    public Slider soundSlider;

    public GameObject logoObject;
    public GameObject characterObject;

    void Start()
    {
        // 패널 초기 상태 설정
        settingsPanel.SetActive(false);
        highscorePanel.SetActive(false);

        // 버튼 리스너 설정
        startButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(ToggleSettings);
        highscoreButton.onClick.AddListener(ToggleHighscore);

        // 로고와 캐릭터 애니메이션 시작
        StartLogoAnimation();
        StartCharacterAnimation();

        // 최고 점수 로드
        UpdateHighscoreDisplay();

        // 사운드 볼륨 설정 로드
        float savedVolume = PlayerPrefs.GetFloat("SoundVolume", 0.75f);
        soundSlider.value = savedVolume;
        AudioListener.volume = savedVolume;

        // 사운드 슬라이더 리스너 설정
        soundSlider.onValueChanged.AddListener(UpdateSoundVolume);
    }

    public void StartGame()
    {
        // 효과음 재생
        PlayButtonSound();

        // 게임 씬으로 전환
        SceneManager.LoadScene("GameScene");
    }

    void ToggleSettings()
    {
        PlayButtonSound();
        settingsPanel.SetActive(!settingsPanel.activeSelf);
        highscorePanel.SetActive(false);
    }

    void ToggleHighscore()
    {
        PlayButtonSound();
        highscorePanel.SetActive(!highscorePanel.activeSelf);
        settingsPanel.SetActive(false);
        UpdateHighscoreDisplay();
    }

    void UpdateHighscoreDisplay()
    {
        int highscore = PlayerPrefs.GetInt("HighScore", 0);
        highscoreText.text = "최고 점수: " + highscore;
    }

    void UpdateSoundVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("SoundVolume", volume);
        PlayerPrefs.Save();
    }

    void PlayButtonSound()
    {
        // 버튼 클릭 사운드 재생
        GetComponent<AudioSource>().Play();
    }

    void StartLogoAnimation()
    {
        // 로고 애니메이션
        LeanTween.moveY(logoObject, logoObject.transform.position.y + 0.5f, 1.5f)
            .setEase(LeanTweenType.easeInOutSine)
            .setLoopPingPong();
    }

    void StartCharacterAnimation()
    {
        MoveCharacterRandomly(); // 최초 호출
    }

    void MoveCharacterRandomly()
    {
        Vector3 currentPos = characterObject.transform.position;

        // 카메라 월드 영역 계산
        Camera cam = Camera.main;
        float zDistance = Mathf.Abs(cam.transform.position.z - characterObject.transform.position.z);

        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, zDistance));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, zDistance));

        // 캐릭터가 이동 가능한 범위 (안쪽에 여유 공간도 주기 위해)
        float margin = 0.5f; // 캐릭터가 너무 경계에 가지 않도록
        float minX = bottomLeft.x + margin;
        float maxX = topRight.x - margin;
        float minY = bottomLeft.y + margin;
        float maxY = topRight.y - margin;

        // 랜덤 목표 위치 계산 (카메라 뷰포트 안쪽으로 제한)
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        Vector3 targetPos = new Vector3(randomX, randomY, currentPos.z);

        // LeanTween으로 이동
        LeanTween.move(characterObject, targetPos, 1.5f)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnComplete(() => MoveCharacterRandomly());
    }
}