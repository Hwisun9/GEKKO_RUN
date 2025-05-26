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
        // �г� �ʱ� ���� ����
        settingsPanel.SetActive(false);
        highscorePanel.SetActive(false);

        // ��ư ������ ����
        startButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(ToggleSettings);
        highscoreButton.onClick.AddListener(ToggleHighscore);

        // �ΰ�� ĳ���� �ִϸ��̼� ����
        StartLogoAnimation();
        StartCharacterAnimation();

        // �ְ� ���� �ε�
        UpdateHighscoreDisplay();

        // ���� ���� ���� �ε�
        float savedVolume = PlayerPrefs.GetFloat("SoundVolume", 0.75f);
        soundSlider.value = savedVolume;
        AudioListener.volume = savedVolume;

        // ���� �����̴� ������ ����
        soundSlider.onValueChanged.AddListener(UpdateSoundVolume);
    }

    public void StartGame()
    {
        // ȿ���� ���
        PlayButtonSound();

        // ���� ������ ��ȯ
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
        highscoreText.text = "�ְ� ����: " + highscore;
    }

    void UpdateSoundVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("SoundVolume", volume);
        PlayerPrefs.Save();
    }

    void PlayButtonSound()
    {
        // ��ư Ŭ�� ���� ���
        GetComponent<AudioSource>().Play();
    }

    void StartLogoAnimation()
    {
        // �ΰ� �ִϸ��̼�
        LeanTween.moveY(logoObject, logoObject.transform.position.y + 0.5f, 1.5f)
            .setEase(LeanTweenType.easeInOutSine)
            .setLoopPingPong();
    }

    void StartCharacterAnimation()
    {
        MoveCharacterRandomly(); // ���� ȣ��
    }

    void MoveCharacterRandomly()
    {
        Vector3 currentPos = characterObject.transform.position;

        // ī�޶� ���� ���� ���
        Camera cam = Camera.main;
        float zDistance = Mathf.Abs(cam.transform.position.z - characterObject.transform.position.z);

        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, zDistance));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, zDistance));

        // ĳ���Ͱ� �̵� ������ ���� (���ʿ� ���� ������ �ֱ� ����)
        float margin = 0.5f; // ĳ���Ͱ� �ʹ� ��迡 ���� �ʵ���
        float minX = bottomLeft.x + margin;
        float maxX = topRight.x - margin;
        float minY = bottomLeft.y + margin;
        float maxY = topRight.y - margin;

        // ���� ��ǥ ��ġ ��� (ī�޶� ����Ʈ �������� ����)
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        Vector3 targetPos = new Vector3(randomX, randomY, currentPos.z);

        // LeanTween���� �̵�
        LeanTween.move(characterObject, targetPos, 1.5f)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnComplete(() => MoveCharacterRandomly());
    }
}