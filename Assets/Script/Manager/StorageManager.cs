using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class StorageManager : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject storagePanel;
    public Transform deckContentArea;    // 위쪽: 현재 전투에 가져갈 덱
    public Transform storageContentArea; // 아래쪽: 가방(여분 카드)
    public GameObject cardPrefab;

    public Action<CardController> onClickAction;

    [Header("선택 효과 연출")]
    public Color selectedColor = new Color(0.7f, 1f, 0.7f);

    private CardController firstSelectedCard = null;
    private bool isFirstDeckArea = false;

    public void OpenStorage()
    {
        // ★ 기존의 즉시 켜기(SetActive) 삭제 및 페이드 연동
        if (GlobalSceneFader.Instance != null)
        {
            GlobalSceneFader.Instance.OpenPanelWithFade(storagePanel);
        }
        else
        {
            storagePanel.SetActive(true); // 페이드 매니저가 없을 때를 대비한 안전장치
        }

        RefreshStorage();
    }

    public void CloseStorage()
    {
        // ★ 기존의 즉시 끄기(SetActive) 삭제 및 페이드 연동
        if (GlobalSceneFader.Instance != null)
        {
            GlobalSceneFader.Instance.ClosePanelWithFade(storagePanel);
        }
        else
        {
            storagePanel.SetActive(false);
        }

        ClearSelection();
    }

    public void RefreshStorage()
    {
        ClearSelection();

        // 1. 화면에 있는 기존 카드들을 전부 청소합니다.
        foreach (Transform child in deckContentArea) Destroy(child.gameObject);
        foreach (Transform child in storageContentArea) Destroy(child.gameObject);

        // 2. 현재 덱(myDeck)에 있는 카드들을 위쪽에 생성합니다.
        foreach (int cardId in GameManager.Instance.myDeck)
        {
            CardData data = GameManager.Instance.allCardDatabase.Find(x => x.id == cardId);
            if (data != null) CreateCard(data, deckContentArea, true);
        }

        // 3. ★ 가방(myBag)에 있는 카드들을 아래쪽에 생성합니다.
        foreach (int cardId in GameManager.Instance.myBag)
        {
            CardData data = GameManager.Instance.allCardDatabase.Find(x => x.id == cardId);
            if (data != null) CreateCard(data, storageContentArea, false);
        }
    }

    private void CreateCard(CardData data, Transform parentArea, bool isDeckArea)
    {
        GameObject newCard = Instantiate(cardPrefab, parentArea);
        newCard.transform.localScale = Vector3.one;

        CardController cardCtrl = newCard.GetComponent<CardController>();
        cardCtrl.Setup(data);
        cardCtrl.SetInventoryMode();

        cardCtrl.onClickAction = (clickedCard) => OnCardClicked(clickedCard, isDeckArea);
    }

    private void OnCardClicked(CardController clickedCard, bool isDeckArea)
    {
        // 첫 번째 카드를 선택했을 때
        if (firstSelectedCard == null)
        {
            firstSelectedCard = clickedCard;
            isFirstDeckArea = isDeckArea;
            HighlightCard(clickedCard, true);
            return;
        }

        // 같은 카드를 다시 누르면 선택 취소
        if (firstSelectedCard == clickedCard)
        {
            ClearSelection();
            return;
        }

        // ★ 덱과 가방 사이의 교체(Swap) 로직
        if (isFirstDeckArea != isDeckArea)
        {
            int deckCardId = isFirstDeckArea ? firstSelectedCard.myData.id : clickedCard.myData.id;
            int bagCardId = isFirstDeckArea ? clickedCard.myData.id : firstSelectedCard.myData.id;

            int deckIndex = GameManager.Instance.myDeck.IndexOf(deckCardId);
            int bagIndex = GameManager.Instance.myBag.IndexOf(bagCardId);

            if (deckIndex != -1 && bagIndex != -1)
            {
                // 실제 데이터를 맞바꿉니다.
                GameManager.Instance.myDeck[deckIndex] = bagCardId;
                GameManager.Instance.myBag[bagIndex] = deckCardId;
                Debug.Log($"카드 교체 성공: 덱({deckCardId}) <-> 가방({bagCardId})");

                // 데이터가 바뀌었으니 저장!
                GameManager.Instance.SaveGameData();

                // ★ 수정됨: 데이터만 먼저 바꾸고, 즉시 새로고침하는 대신 부드러운 애니메이션 코루틴을 실행합니다.
                StartCoroutine(SwapAnimation(firstSelectedCard, clickedCard));
            }
        }
        else
        {
            // 같은 영역(덱->덱 또는 가방->가방)의 카드를 누르면 그냥 선택을 바꿉니다.
            HighlightCard(firstSelectedCard, false);
            firstSelectedCard = clickedCard;
            isFirstDeckArea = isDeckArea;
            HighlightCard(clickedCard, true);
        }
    }

    // ▼▼▼ 새로 추가된 부드러운 위치 교환 애니메이션 코루틴 ▼▼▼
    private System.Collections.IEnumerator SwapAnimation(CardController card1, CardController card2)
    {
        // 두 카드가 이동 중에 다른 카드들에 가려지지 않도록 하이어라키 맨 아래(화면 맨 앞)로 순서를 당겨줍니다.
        card1.transform.SetAsLastSibling();
        card2.transform.SetAsLastSibling();

        Vector3 startPos1 = card1.transform.position;
        Vector3 startPos2 = card2.transform.position;

        float duration = 0.3f; // 카드가 날아가는 시간 (0.3초)
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 부드럽게 출발하고 감속하며 도착하도록 만드는 SmoothStep 공식 적용
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);

            // 서서히 서로의 위치로 이동
            card1.transform.position = Vector3.Lerp(startPos1, startPos2, t);
            card2.transform.position = Vector3.Lerp(startPos2, startPos1, t);

            yield return null;
        }

        // 애니메이션이 완전히 끝나면 화면을 깔끔하게 다시 그려줍니다.
        RefreshStorage();
    }

    private void HighlightCard(CardController card, bool isSelected)
    {
        if (card != null && card.backgroundImage != null)
        {
            card.backgroundImage.color = isSelected ? selectedColor : Color.white;
        }
    }

    private void ClearSelection()
    {
        HighlightCard(firstSelectedCard, false);
        firstSelectedCard = null;
    }
}