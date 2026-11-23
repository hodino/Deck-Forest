using UnityEngine;
using System.Collections.Generic;

public class LibraryManager : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject libraryPanel;
    public Transform contentArea;
    public GameObject cardPrefab;

    [Header("미획득 카드 설정")]
    public Sprite unknownSprite;

    // ▼▼▼ 새로 추가된 페이지 애니메이션 대상 ▼▼▼
    [Header("페이지 애니메이션 대상")]
    public Transform pageContainer; // ContentArea의 상위 부모(ScrollView 전체 등)를 연결

    public void OpenLibrary()
    {
        if (GlobalSceneFader.Instance != null)
        {
            GlobalSceneFader.Instance.OpenPanelWithFade(libraryPanel);
        }
        else
        {
            libraryPanel.SetActive(true);
        }

        RefreshLibrary();
    }

    public void CloseLibrary()
    {
        if (GlobalSceneFader.Instance != null)
        {
            GlobalSceneFader.Instance.ClosePanelWithFade(libraryPanel);
        }
        else
        {
            libraryPanel.SetActive(false);
        }
    }

    // ▼▼▼ 새로 추가된 책 넘기기 (새로고침) 함수 ▼▼▼
    // UI 버튼(다음 페이지 등)의 OnClick 이벤트에 연결하세요.
    public void TurnPageAndRefresh()
    {
        StartCoroutine(PageFlipAnim());
    }

    // ▼▼▼ 새로 추가된 책 넘기기 애니메이션 코루틴 ▼▼▼
    private System.Collections.IEnumerator PageFlipAnim()
    {
        // pageContainer가 비어있다면 임시로 contentArea를 움직입니다.
        if (pageContainer == null) pageContainer = contentArea;

        float duration = 0.15f; // 반쪽 넘기는 속도 (총 0.3초)
        float t = 0;

        // 1. 가로축을 0으로 찌그러뜨림 (책이 닫히는 느낌)
        while (t < duration)
        {
            t += Time.deltaTime;
            float scaleX = Mathf.Lerp(1f, 0f, t / duration);
            pageContainer.localScale = new Vector3(scaleX, 1f, 1f);
            yield return null;
        }

        // 2. 화면이 완전히 얇아졌을 때(안 보일 때) 내부 카드들 갱신
        RefreshLibrary();

        // 3. 가로축을 다시 1로 늘림 (다음 장이 펼쳐지는 느낌)
        t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float scaleX = Mathf.Lerp(0f, 1f, t / duration);
            pageContainer.localScale = new Vector3(scaleX, 1f, 1f);
            yield return null;
        }

        pageContainer.localScale = Vector3.one;
    }

    void RefreshLibrary()
    {
        foreach (Transform child in contentArea) Destroy(child.gameObject);

        if (GameManager.Instance == null) return;

        List<CardData> sortedList = new List<CardData>(GameManager.Instance.allCardDatabase);
        sortedList.Sort((a, b) => a.id.CompareTo(b.id));

        foreach (CardData data in sortedList)
        {
            GameObject newCard = Instantiate(cardPrefab, contentArea);
            newCard.transform.localScale = Vector3.one;

            CardController cardCtrl = newCard.GetComponent<CardController>();
            cardCtrl.Setup(data);

            cardCtrl.SetInventoryMode();

            if (!GameManager.Instance.collectionHistory.Contains(data.id))
            {
                cardCtrl.SetUnknown(unknownSprite);
            }
            else
            {
                cardCtrl.SetInventoryMode();
            }
        }
    }
}