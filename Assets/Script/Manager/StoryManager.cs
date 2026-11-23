using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class StoryManager : MonoBehaviour
{
    [Header("스토리 컷 설정")]
    [Tooltip("가려둔 검은색 패널들을 순서대로 넣어주세요 (1번 컷을 가린 패널이 0번 인덱스)")]
    public GameObject[] blackPanels; 

    [Header("다음 씬 설정")]
    public string nextSceneName = "MainScene"; // 이동할 마을 씬 이름

    private int clickCount = 0; // 클릭 횟수를 저장하는 변수

    // 화면 클릭(투명 버튼) 시 호출할 함수
    public void OnScreenClicked()
    {
        // 클릭 횟수가 배열의 길이(가려진 패널 개수)보다 작으면 패널을 하나씩 끕니다.
        if (clickCount < blackPanels.Length)
        {
            // 부드럽게 사라지는 효과를 주고 싶다면 코루틴을 사용합니다.
            StartCoroutine(FadeOutPanel(blackPanels[clickCount]));
            clickCount++;
        }
        else
        {
            // 모든 컷을 다 보고 한 번 더 클릭하면 마을 씬으로 이동합니다.
            GoToNextScene();
        }
    }

    // 패널이 서서히 투명해지며 사라지는 코루틴
    private IEnumerator FadeOutPanel(GameObject panel)
    {
        Image panelImage = panel.GetComponent<Image>();
        if (panelImage != null)
        {
            Color color = panelImage.color;
            float fadeDuration = 0.5f; // 사라지는 시간 (0.5초)
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                color.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                panelImage.color = color;
                yield return null;
            }
            
            panel.SetActive(false); // 완전히 투명해지면 오브젝트 끄기
        }
        else
        {
            // Image 컴포넌트가 없으면 그냥 바로 끕니다.
            panel.SetActive(false);
        }
    }

    // 다음 씬으로 이동하는 함수 (Skip 버튼에도 연결 가능)
    public void GoToNextScene()
    {
        // 씬 전환 매니저가 있다면 페이드 효과와 함께 사용합니다.
        if (GlobalSceneFader.Instance != null)
        {
            GlobalSceneFader.Instance.LoadScene(nextSceneName);
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}