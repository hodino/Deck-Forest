using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필요합니다.

public class RewardManager : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject victoryPanel;
    public Transform rewardCardPosition;
    public GameObject cardPrefab;
    public GameObject nextRoomButton;

    [Header("애니메이션 설정")]
    public float fadeDuration = 0.8f;
    public float moveDistance = 50f;

    [Header("엔딩 설정 (보스용)")]
    public bool isBossRoom = false; // 보스 씬인지 체크
    public int bossRewardCardID = 10; // 보스 격파 시 지급할 특정 카드의 ID 입력
    public GameObject toBeContinuePanel; // 튜비컨티뉴 패널

    public void ShowVictoryScreen()
    {
        victoryPanel.SetActive(true);
        if (nextRoomButton != null) nextRoomButton.SetActive(false);
        if (toBeContinuePanel != null) toBeContinuePanel.SetActive(false); // 처음엔 꺼둠

        foreach (Transform child in rewardCardPosition) Destroy(child.gameObject);
        StartCoroutine(AnimateRewardSequence());
    }

    IEnumerator AnimateRewardSequence()
    {
        if (GameManager.Instance == null || GameManager.Instance.allCardDatabase.Count == 0) yield break;

        CardData rewardCard = null;

        // ★ 보스 방이면 지정된 ID의 카드 지급, 아니면 랜덤 지급
        if (isBossRoom)
        {
            rewardCard = GameManager.Instance.allCardDatabase.Find(x => x.id == bossRewardCardID);
            if (rewardCard == null)
            {
                Debug.LogWarning($"ID가 {bossRewardCardID}인 보스 보상 카드를 찾을 수 없습니다! 랜덤으로 대체합니다.");
                rewardCard = GameManager.Instance.allCardDatabase[Random.Range(0, GameManager.Instance.allCardDatabase.Count)];
            }
        }
        else
        {
            int randomIndex = Random.Range(0, GameManager.Instance.allCardDatabase.Count);
            rewardCard = GameManager.Instance.allCardDatabase[randomIndex];
        }

        GameObject newCard = Instantiate(cardPrefab, rewardCardPosition);

        CardController cardCtrl = newCard.GetComponent<CardController>();
        if (cardCtrl != null)
        {
            cardCtrl.Setup(rewardCard);
            cardCtrl.SetInventoryMode();
            cardCtrl.ResetOriginalScale(Vector3.one);

            // ▼ 수정: 카드의 Canvas 컴포넌트가 확실히 있을 때만 Sorting Order를 변경하도록 안전장치 추가
            Canvas cardCanvas = newCard.GetComponent<Canvas>();
            if (cardCanvas != null)
            {
                cardCtrl.SetSortingOrder(200);
            }
        }

        RectTransform rt = newCard.GetComponent<RectTransform>();
        CanvasGroup cg = newCard.GetComponent<CanvasGroup>();
        if (cg == null) cg = newCard.AddComponent<CanvasGroup>();

        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.sizeDelta = new Vector2(400f, 550f);
        }

        cg.alpha = 0;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        Button btn = newCard.GetComponent<Button>();
        if (btn == null) btn = newCard.AddComponent<Button>();

        // 카드 클릭 시 실행될 동작 연결
        btn.onClick.AddListener(() => OnCardSelected(rewardCard, newCard));

        Vector3 endPos = Vector3.zero;
        Vector3 startPos = endPos + new Vector3(0, -moveDistance, 0);

        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);

            cg.alpha = smoothProgress;
            if (rt != null) rt.anchoredPosition = Vector2.Lerp(startPos, endPos, smoothProgress);
            yield return null;
        }
    }

    void OnCardSelected(CardData pickedCard, GameObject cardObj)
    {
        if (GameManager.Instance != null) GameManager.Instance.AddCard(pickedCard);

        StartCoroutine(PulseEffect(cardObj.transform));
        cardObj.GetComponent<Button>().interactable = false;

        if (nextRoomButton != null)
        {
            nextRoomButton.SetActive(true);
            StartCoroutine(FadeInButton(nextRoomButton));
        }
    }

    IEnumerator FadeInButton(GameObject btnObj)
    {
        CanvasGroup cg = btnObj.GetComponent<CanvasGroup>();
        if (cg == null) cg = btnObj.AddComponent<CanvasGroup>();
        cg.alpha = 0;
        float timer = 0;
        while (timer < 0.5f) { timer += Time.deltaTime; cg.alpha = timer / 0.5f; yield return null; }
    }

    IEnumerator PulseEffect(Transform target)
    {
        Vector3 originScale = Vector3.one;
        Vector3 upScale = Vector3.one * 1.15f;
        float t = 0;
        while (t < 0.2f) { t += Time.deltaTime; target.localScale = Vector3.Lerp(originScale, upScale, t / 0.2f); yield return null; }
        target.localScale = upScale;
    }

    // ★ 다음 버튼을 눌렀을 때 실행되는 함수
    public void GoToNextRoom()
    {
        if (isBossRoom)
        {
            // 보스 방이면 To Be Continue 패널을 켬
            if (toBeContinuePanel != null)
            {
                toBeContinuePanel.SetActive(true);
                victoryPanel.SetActive(false); // 기존 보상 패널 숨김
            }
        }
        else
        {
            // 일반 방이면 기존처럼 다음 랜덤 씬으로 이동
            if (StageUIManager.Instance != null)
                StageUIManager.Instance.GenerateAndEnterNextRoom();
            else
                SceneManager.LoadScene("GameScene");
        }
    }

    // ★ To Be Continue 화면에서 NEXT를 눌렀을 때 메인으로 돌아가는 함수
    public void ReturnToMainMenu()
    {
        // 씬 페이드 아웃이 있다면 연동하고, 아니면 바로 메인으로 이동
        if (SceneTransition.instance != null)
        {
            SceneTransition.instance.LoadSceneWithFade("MainScene");
        }
        else
        {
            SceneManager.LoadScene("MainScene");
        }
    }
}