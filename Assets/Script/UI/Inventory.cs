using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    public GameObject inventoryPanel;

    // ★ 기존의 단일 cardContentArea 대신 가방과 덱 영역으로 나눴습니다.
    public Transform bagContentArea;  // 왼쪽: 가방 영역
    public Transform deckContentArea; // 오른쪽: 덱 영역

    public GameObject cardPrefab;

    private bool isInventoryOpen = false;

    void Start()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    void Update()
    {
        // 키보드 'I' 키를 누르면 인벤토리가 열리거나 닫힙니다.
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (isInventoryOpen) CloseInventory();
            else OpenInventory();
        }
    }

    public void OpenInventory()
    {
        if (inventoryPanel == null) return;
        isInventoryOpen = true;
        inventoryPanel.SetActive(true);
        PopulateInventory();
    }

    public void CloseInventory()
    {
        if (inventoryPanel == null) return;
        isInventoryOpen = false;
        inventoryPanel.SetActive(false);
    }

    void PopulateInventory()
    {
        if (cardPrefab == null) return;

        // 1. 기존 카드들 청소 (가방, 덱 양쪽 모두 깔끔하게!)
        if (bagContentArea != null)
        {
            foreach (Transform child in bagContentArea) Destroy(child.gameObject);
        }
        if (deckContentArea != null)
        {
            foreach (Transform child in deckContentArea) Destroy(child.gameObject);
        }

        if (GameManager.Instance != null)
        {
            // 2. 가방(myBag) 카드 생성
            if (bagContentArea != null)
            {
                foreach (int cardId in GameManager.Instance.myBag)
                {
                    // ID를 가지고 데이터베이스에서 실제 카드 정보를 찾아옵니다.
                    CardData cardData = GameManager.Instance.allCardDatabase.Find(x => x.id == cardId);
                    if (cardData != null) CreateCard(cardData, bagContentArea);
                }
            }

            // 3. 덱(myDeck) 카드 생성
            if (deckContentArea != null)
            {
                foreach (int cardId in GameManager.Instance.myDeck)
                {
                    CardData cardData = GameManager.Instance.allCardDatabase.Find(x => x.id == cardId);
                    if (cardData != null) CreateCard(cardData, deckContentArea);
                }
            }
        }
    }

    // ★ 중복 코드를 줄이기 위해 카드를 생성하는 전용 함수를 만들었습니다.
    private void CreateCard(CardData card, Transform parentArea)
    {
        GameObject cardGO = Instantiate(cardPrefab, parentArea);
        cardGO.transform.localScale = Vector3.one;

        CardController controller = cardGO.GetComponent<CardController>();

        if (controller != null)
        {
            controller.SetInventoryMode();
            controller.Setup(card);
        }
    }
}