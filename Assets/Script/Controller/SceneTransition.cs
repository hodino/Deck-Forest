using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
   public static SceneTransition instance; // 싱글톤 인스턴스

    public Image fadeImage; // 1단계에서 만든 검은색 Image UI
    public float fadeDuration = 1.0f; // 페이드되는 속도

    private void Awake()
    {
        // 싱글톤 패턴: 씬에 인스턴스가 하나만 존재하도록
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않음
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 다른 스크립트에서 이 함수를 호출
    public void LoadSceneWithFade(string sceneName)
    {
        // 이미 씬 전환이 진행 중이라면 중복 실행 방지
        if (Time.timeScale == 0f)
        {
            return;
        }

        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    // 페이드 아웃 -> 씬 로드 -> 페이드 인
    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        //필드 일시정지
        Time.timeScale = 0.0f;
        // 1. 페이드 아웃 (점점 검은색으로)
        yield return StartCoroutine(Fade(1.0f)); // 알파 1 (불투명)

        // 2. 씬 로드
        SceneManager.LoadScene(sceneName);

        //필드 일시정지 풀림
        Time.timeScale = 1.0f;

        // 3. 페이드 인 (점점 투명하게)
        // "천천히" 전환된다고 하셨으니, 페이드 인 시간을 조금 더 길게 (예: 1.5초)
        yield return StartCoroutine(Fade(0.0f, 1.5f)); // 알파 0 (투명)
    }

    // 실제 페이드 효과를 주는 코루틴
    private IEnumerator Fade(float targetAlpha, float duration = -1)
    {
        if (duration == -1)
        {
            duration = fadeDuration;
        }

        Color currentColor = fadeImage.color;
        float startAlpha = currentColor.a;
        float timer = 0.0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            fadeImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null;
        }

        fadeImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
    }
}
