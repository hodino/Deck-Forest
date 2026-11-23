using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenuManager : MonoBehaviour
{
    [Header("씬 이동 설정")]
    [Tooltip("새로 시작 시 이동할 스토리 씬의 이름을 적으세요")]
    public string storySceneName = "StoryScene";

    [Tooltip("이어하기 시 바로 이동할 탐험/마을 씬의 이름을 적으세요")]
    public string gameSceneName = "GameScene";

    [Header("UI 연결")]
    public GameObject exitPopup;
    public Button continueButton;

    void Start()
    {
        if (exitPopup != null) exitPopup.SetActive(false);

        string path = Path.Combine(Application.persistentDataPath, "savefile.json");

        if (continueButton != null)
        {
            continueButton.interactable = File.Exists(path);
        }
    }

    // [새 게임] 버튼 클릭 시
    public void OnClickNewGame()
    {
        string path = Path.Combine(Application.persistentDataPath, "savefile.json");
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("기존 세이브 파일 강제 삭제 완료!");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GiveStarterItems();
        }

        // ▼▼▼ 새 게임은 스토리 씬으로 이동 ▼▼▼
        LoadScene(storySceneName);
    }

    // [이어하기] 버튼 클릭 시
    public void OnClickContinue()
    {
        // ▼▼▼ 이어하기는 게임(마을) 씬으로 바로 이동 ▼▼▼
        LoadScene(gameSceneName);
    }

    // ▼▼▼ 씬 이름을 받아서 이동하도록 변경된 공통 함수 ▼▼▼
    private void LoadScene(string sceneToLoad)
    {
        if (GlobalSceneFader.Instance != null)
        {
            GlobalSceneFader.Instance.LoadScene(sceneToLoad);
        }
        else if (SceneTransition.instance != null)
        {
            SceneTransition.instance.LoadSceneWithFade(sceneToLoad);
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    // (종료 관련 함수들은 기존과 동일하게 유지)
    public void ShowExitPopup()
    {
        if (exitPopup != null) exitPopup.SetActive(true);
    }

    public void HideExitPopup()
    {
        if (exitPopup != null) exitPopup.SetActive(false);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}