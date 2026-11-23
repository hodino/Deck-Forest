using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.SceneManagement; // 씬 관리를 위해 추가

public class OptionsManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject optionsPanel;           // 옵션창 전체 패널
    public GameObject mainOptionsContent;     // 기본 옵션 버튼 3개 (설정, 저장, 종료)
    public GameObject settingsContent;        // 상세 설정 UI (해상도, 볼륨 등)
    public GameObject exitConfirmationPanel;  // 종료 확인 팝업
    public GameObject savedNotificationPanel; // '저장되었습니다' 알림

    // ▼▼▼ 새로 추가된 데이터 삭제 팝업 변수 ▼▼▼
    public GameObject deleteConfirmationPanel; // 데이터 삭제 확인 팝업

    [Header("저장 및 데이터 제어")]
    // ▼▼▼ 새로 추가된 저장 제어 변수 ▼▼▼
    public Button saveButton; // 옵션 창의 저장 버튼
    public List<string> noSaveScenes = new List<string> { "StartScene", "BattleScene" }; // 저장을 막을 씬 이름

    [Header("화면 설정 UI")]
    public TMP_Dropdown resolutionDropdown;   // 해상도 드롭다운

    [Header("볼륨 설정 UI")]
    public Slider mainVolumeSlider;           // 마스터 볼륨 슬라이더
    public Slider musicVolumeSlider;          // 배경음 볼륨 슬라이더
    public Slider sfxVolumeSlider;            // 효과음 볼륨 슬라이더

    [Header("오디오 믹서 연결")]
    public AudioMixer audioMixer;             // 유니티 오디오 믹서 파일

    // 내부 상태 기억용 변수들
    private bool isOptionsOpen = false;
    private List<Resolution> availableResolutions = new List<Resolution>();
    private int currentResolutionIndex = 0;

    void Start()
    {
        // 시작 시 모든 관련 UI를 닫힌 상태로 초기화
        optionsPanel.SetActive(false);
        exitConfirmationPanel.SetActive(false);
        savedNotificationPanel.SetActive(false);

        if (deleteConfirmationPanel != null) deleteConfirmationPanel.SetActive(false);

        // 옵션 패널 내부도 초기화 (메인 버튼은 보이고, 설정창은 숨김)
        mainOptionsContent.SetActive(true);
        settingsContent.SetActive(false);

        // 아래에서 만들 해상도/사운드 초기 세팅 함수 호출
        InitResolutionSettings();
        InitVolumeSettings();
    }

    void Update()
    {
        // 'P' 키로 옵션 창 열기/닫기
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isOptionsOpen)
            {
                CloseOptions();
            }
            else
            {
                OpenOptions();
            }
        }
    }

    // --- 1. 메인 옵션 창 (열기/닫기) ---

    public void OpenOptions()
    {
        isOptionsOpen = true;
        Time.timeScale = 0f; // 게임 일시 정지
        optionsPanel.SetActive(true);

        // 항상 메인 버튼 3개가 있는 화면부터 보이도록 설정
        mainOptionsContent.SetActive(true);
        settingsContent.SetActive(false);
        exitConfirmationPanel.SetActive(false);
        if (deleteConfirmationPanel != null) deleteConfirmationPanel.SetActive(false);

        // ★ 현재 씬 이름을 확인하여 저장 버튼 클릭 활성화/비활성화
        if (saveButton != null)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            saveButton.interactable = !noSaveScenes.Contains(currentScene);
        }
    }

    public void CloseOptions()
    {
        isOptionsOpen = false;
        Time.timeScale = 1f; // 게임 재개
        optionsPanel.SetActive(false);

        // 닫을 때 혹시 열려있을 수 있는 팝업들도 모두 비활성화
        exitConfirmationPanel.SetActive(false);
        if (deleteConfirmationPanel != null) deleteConfirmationPanel.SetActive(false);
    }

    // --- 2. 메인 옵션 버튼 기능들 ---

    public void OnSettingsButton()
    {
        mainOptionsContent.SetActive(false);
        settingsContent.SetActive(true);
    }

    public void OnSaveButton()
    {
        // (나중에 여기에 실제 저장 로직 추가)
        StartCoroutine(ShowSavedNotification());
    }

    public void OnExitButton()
    {
        exitConfirmationPanel.SetActive(true);
    }

    // ▼▼▼ 새로 추가된 데이터 삭제 관련 버튼 기능 ▼▼▼
    public void OnDeleteDataButton()
    {
        if (deleteConfirmationPanel != null) deleteConfirmationPanel.SetActive(true);
    }

    public void OnConfirmDeleteYes()
    {
        // 1. 실제 데이터 삭제
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DeleteSaveFile();
        }

        CloseOptions();

        // 2. ★ 수정됨: 왼쪽 위 UI가 들어있는 'GlobalUICanvas'를 찾아서 파괴합니다.
        GameObject globalUI = GameObject.Find("GlobalUICanvas");
        if (globalUI != null)
        {
            Destroy(globalUI); // 시작 화면에 남지 않도록 완전히 삭제
        }

        // 3. 기존 전투 UI도 찾아서 파괴합니다.
        GameObject stageUI = GameObject.Find("StageUICanvas");
        if (stageUI != null)
        {
            Destroy(stageUI);
        }

        // 4. 시간 정지 해제
        Time.timeScale = 1f;

        // 5. 시작 씬(StartScene)으로 강제 이동
        if (GlobalSceneFader.Instance != null)
        {
            GlobalSceneFader.Instance.LoadScene("StartScene");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
        }
    }

    public void OnConfirmDeleteNo()
    {
        if (deleteConfirmationPanel != null) deleteConfirmationPanel.SetActive(false);
    }

    // --- 3. 설정 창 (SettingsContent) ---

    public void OnSettingsBackButton()
    {
        settingsContent.SetActive(false);
        mainOptionsContent.SetActive(true);
    }

    // --- 4. 종료 확인 팝업 (ExitConfirmationPanel) ---

    public void OnConfirmExitYes()
    {
        Debug.Log("게임을 종료합니다..."); // 에디터 테스트용
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnConfirmExitNo()
    {
        exitConfirmationPanel.SetActive(false);
    }

    // --- 5. 저장 알림 (Coroutine) ---

    private IEnumerator ShowSavedNotification()
    {
        savedNotificationPanel.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);
        savedNotificationPanel.SetActive(false);
    }

    // ========================================================
    // [해상도 및 화면 모드 기능]
    // ========================================================
    void InitResolutionSettings()
    {
        if (resolutionDropdown == null) return;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        availableResolutions.Clear();

        int[,] standardRes = new int[,] { { 1280, 720 }, { 1600, 900 }, { 1920, 1080 }, { 2560, 1440 }, { 3840, 2160 } };

        for (int i = 0; i < standardRes.GetLength(0); i++)
        {
            int w = standardRes[i, 0]; int h = standardRes[i, 1];
            if (w <= Screen.currentResolution.width && h <= Screen.currentResolution.height)
            {
                Resolution res = new Resolution { width = w, height = h };
                availableResolutions.Add(res);
                string optionText = $"{w} x {h}";
                if (w == 1920) optionText += " (FHD)";
                else if (w == 2560) optionText += " (QHD)";
                else if (w == 1280) optionText += " (HD)";
                options.Add(optionText);

                if (Screen.width == w && Screen.height == h) currentResolutionIndex = options.Count - 1;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetFullScreenMode() { Screen.fullScreenMode = FullScreenMode.FullScreenWindow; }
    public void SetWindowedMode() { Screen.fullScreenMode = FullScreenMode.Windowed; }
    public void OnResolutionDropdownChanged(int index)
    {
        currentResolutionIndex = index;
        Resolution selectedRes = availableResolutions[currentResolutionIndex];
        Screen.SetResolution(selectedRes.width, selectedRes.height, Screen.fullScreenMode);
    }

    // ========================================================
    // [사운드 볼륨 조절 기능 - 오디오 믹서 연동]
    // ========================================================
    void InitVolumeSettings()
    {
        if (audioMixer == null) return;

        float masterVol, musicVol, sfxVol;

        if (mainVolumeSlider != null && audioMixer.GetFloat("MasterVol", out masterVol))
            mainVolumeSlider.value = Mathf.Pow(10, masterVol / 20f);

        if (musicVolumeSlider != null && audioMixer.GetFloat("MusicVol", out musicVol))
            musicVolumeSlider.value = Mathf.Pow(10, musicVol / 20f);

        if (sfxVolumeSlider != null && audioMixer.GetFloat("SFXVol", out sfxVol))
            sfxVolumeSlider.value = Mathf.Pow(10, sfxVol / 20f);
    }

    public void OnMainVolumeChanged(float value)
    {
        if (audioMixer == null) return;
        float volume = value <= 0 ? -80f : Mathf.Log10(value) * 20f;
        audioMixer.SetFloat("MasterVol", volume);
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (audioMixer == null) return;
        float volume = value <= 0 ? -80f : Mathf.Log10(value) * 20f;
        audioMixer.SetFloat("MusicVol", volume);
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (audioMixer == null) return;
        float volume = value <= 0 ? -80f : Mathf.Log10(value) * 20f;
        audioMixer.SetFloat("SFXVol", volume);
    }
}