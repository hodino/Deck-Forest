using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResponsiveAutoScaler : MonoBehaviour
{
    public static ResponsiveAutoScaler Instance;

    [Header("기준 해상도 설정")]
    public float targetWidth = 1920f;
    public float targetHeight = 1080f;

    void Awake()
    {
        // 씬이 넘어가도 파괴되지 않는 싱글톤(글로벌 매니저) 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 씬이 새로 로드될 때마다 자동으로 함수를 실행하도록 연결
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyResponsiveScaleToAllCanvases();
    }

    public void ApplyResponsiveScaleToAllCanvases()
    {
        // 현재 씬에 존재하는 모든 캔버스를 싹 다 찾아냅니다.
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);

        foreach (Canvas canvas in allCanvases)
        {
            // CanvasScaler 컴포넌트가 없으면 붙여주고, 있으면 가져옵니다.
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            }

            // 가장 완벽한 반응형 비율(1920x1080, 0.5 매치)로 강제 덮어쓰기!
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(targetWidth, targetHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        Debug.Log($"[자동화 완료] {SceneManager.GetActiveScene().name} 씬의 모든 캔버스를 반응형으로 세팅했습니다!");
    }
}