using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance;

    public GameObject cutscenePanel;
    public Image cutsceneImage;
    public AudioSource audioSource;

    private void Awake()
    {
        Instance = this;
        if (cutscenePanel != null) cutscenePanel.SetActive(false);
    }

    public IEnumerator PlayCutscene(Sprite image, AudioClip clip, float duration = 1.0f)
    {
        if (image == null) yield break;
        if (cutscenePanel == null || cutsceneImage == null) yield break;

        cutscenePanel.SetActive(true);
        cutscenePanel.transform.SetAsLastSibling();

        cutsceneImage.sprite = image;

        // ★ [핵심 1] RectTransform을 사용해 크기와 위치를 강제로 올바르게 세팅합니다.
        RectTransform rectTransform = cutsceneImage.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 위치를 캔버스 정중앙으로 초기화
            rectTransform.anchoredPosition = Vector2.zero;

            // 크기를 너무 작게 줄이지 않고, 원본 크기로 튀어나오게 합니다. 
            // 0.5f로 줄였다가 커지면 너무 안 보일 수 있습니다. (1.0f -> 1.5f 로 변경)
            rectTransform.localScale = Vector3.one * 1.0f;
        }

        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }

        float timer = 0f;

        // ★ [핵심 2] 줌인 애니메이션을 좀 더 눈에 띄게 (1.0배 -> 1.5배) 변경
        Vector3 startScale = Vector3.one * 1.0f;
        Vector3 endScale = Vector3.one * 1.5f;

        Vector3 startPos = Vector3.zero;
        Vector3 endPos = new Vector3(30f, 0, 0); // 이동 거리도 늘림

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.Lerp(startScale, endScale, progress);
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);
            }

            yield return null;
        }

        cutscenePanel.SetActive(false);
    }
}