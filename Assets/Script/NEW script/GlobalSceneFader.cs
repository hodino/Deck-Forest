using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GlobalSceneFader : MonoBehaviour
{
    public static GlobalSceneFader Instance;

    [Header("UI 연결")]
    public Image fadeImage; // 화면을 덮을 검은색 이미지

    [Header("페이드 속도 설정")]
    public float fadeDuration = 0.8f; // 페이드 아웃/인에 걸리는 시간(초)

    private bool isFading = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeIn());
    }

    // 씬 전환용
    public void LoadScene(string sceneName)
    {
        if (isFading) return;
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        isFading = true;

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.raycastTarget = true;
        }

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, 1f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        isFading = false;
    }

    private IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        fadeImage.raycastTarget = true;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = 1f - Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 0f);
        fadeImage.raycastTarget = false;
    }

    // ==========================================
    // ★ [추가됨] 패널(UI) 페이드 인/아웃 시스템
    // ==========================================

    // 1. 특정 패널 열기
    public void OpenPanelWithFade(GameObject panelToShow)
    {
        if (isFading) return;
        StartCoroutine(PanelFadeCoroutine(null, panelToShow));
    }

    // 2. 특정 패널 닫기
    public void ClosePanelWithFade(GameObject panelToHide)
    {
        if (isFading) return;
        StartCoroutine(PanelFadeCoroutine(panelToHide, null));
    }

    // 3. 패널 교체하기 (A패널 닫고 B패널 열기)
    public void SwitchPanelWithFade(GameObject panelToHide, GameObject panelToShow)
    {
        if (isFading) return;
        StartCoroutine(PanelFadeCoroutine(panelToHide, panelToShow));
    }

    // 패널 전환 코루틴 로직
    private IEnumerator PanelFadeCoroutine(GameObject panelToHide, GameObject panelToShow)
    {
        isFading = true;
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.raycastTarget = true; // 전환 도중 화면 터치 방지
        }

        // 1. 화면이 점점 어두워짐 (페이드 아웃)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime; // Time.timeScale = 0 이어도 작동
            if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(timer / fadeDuration));
            yield return null;
        }
        if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, 1f);

        // 2. ★ 화면이 완전히 까맣게 변했을 때 패널 상태 교체
        if (panelToHide != null) panelToHide.SetActive(false);
        if (panelToShow != null) panelToShow.SetActive(true);

        // 3. 화면이 다시 밝아짐 (페이드 인)
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, 1f - Mathf.Clamp01(timer / fadeDuration));
            yield return null;
        }

        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0f);
            fadeImage.raycastTarget = false;
        }
        isFading = false;
    }
}