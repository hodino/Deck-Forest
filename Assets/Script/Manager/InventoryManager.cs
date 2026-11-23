using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    [Header("연결 필요")]
    public GameObject cardPrefab;   // 생성할 카드 프리팹
    public Transform contentArea;   // Scroll View의 Content 오브젝트
    public GameObject inventoryPanel; // 인벤토리 UI 전체 (열고 닫기용)

    void Start()
    {
        // 씬 시작 시 인벤토리는 꺼두고 싶다면 아래 주석 해제
        // CloseInventory(); 
    }

    // 인벤토리 열기 버튼에 연결할 함수
    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);
        RefreshCardList(); // 열 때마다 목록 갱신
    }

    // 인벤토리 닫기 버튼에 연결할 함수
    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
    }

    // ★ 핵심: GameManager의 카드 데이터를 UI로 그려주는 함수
    public void RefreshCardList()
    {
        // 1. 기존에 표시된 카드들 싹 지우기 (중복 방지)
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }

        // 2. GameManager가 없으면 중단 (안전장치)
        if (GameManager.Instance == null) return;

        // 3. 내 카드 리스트를 순회하며 프리팹 생성
        foreach (CardData data in GameManager.Instance.myCards)
        {
            GameObject newCard = Instantiate(cardPrefab, contentArea);
            newCard.transform.localScale = Vector3.one;

            CardController cardCtrl = newCard.GetComponent<CardController>();
            if (cardCtrl != null)
            {
                cardCtrl.Setup(data);
                cardCtrl.SetInventoryMode();

                // ▼▼▼ [수정] 여기서 확실하게 200을 먹입니다 ▼▼▼
                cardCtrl.SetSortingOrder(200);
            }
        }
    }
}