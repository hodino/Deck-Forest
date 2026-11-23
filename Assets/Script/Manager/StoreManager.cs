using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreManager : MonoBehaviour
{
    [Header("패널 연결")]
    public GameObject storePanel;
    public GameObject gachaPanel;

    [Header("상점 설정 (SellList 오브젝트를 연결하세요)")]
    public Transform shopContent;
    public GameObject shopItemPrefab;

    [Header("뽑기 설정 (카드 프리팹 방식)")]
    public Transform resultPoint;
    public GameObject cardPrefab;

    [Header("★ 뽑기 연출 설정 (상자 오픈)")]
    public GameObject gachaAnimPanel;      // 뽑기 시 화면을 덮는 연출 패널
    public Image gachaBoxImage;            // boxopen01~03이 순서대로 표시될 Image
    public Sprite[] boxOpenSprites;        // 3장 순서대로 연결 (boxopen01, 02, 03)
    public AudioSource gachaAudioSource;   // 비워두면 자동 생성/연결됨
    public AudioClip openCrateClip;        // Item_open_crate 음원
    public AudioClip unlockResultClip;     // tf2-achievement-unlocked-sound 음원
    public float fallbackDuration = 3.0f;  // 음원이 비어있을 때 사용할 기본 연출 시간

    [Header("구매 완료 연출")]
    public GameObject purchaseCompletePanel;
    public TextMeshProUGUI purchaseCompleteText;

    private CardData selectedCard;
    private ShopItemSlot selectedSlot;
    private bool isGachaPlaying = false;

    void Awake()
    {
        if (gachaAnimPanel != null) gachaAnimPanel.SetActive(false);

        if (gachaAudioSource == null)
        {
            gachaAudioSource = GetComponent<AudioSource>();
            if (gachaAudioSource == null)
            {
                gachaAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    // 1. 상점 열기
    public void OpenStore()
    {
        if (isGachaPlaying) return;

        if (GlobalSceneFader.Instance != null)
        {
            GlobalSceneFader.Instance.OpenPanelWithFade(storePanel);
        }
        else
        {
            storePanel.SetActive(true);
        }

        gachaPanel.SetActive(false);
        if (gachaAnimPanel != null) gachaAnimPanel.SetActive(false);
        RefreshShopItems();
    }

    // 2. 상점에서 뽑기 창으로 넘어가기
    public void OpenGachaPanel()
    {
        if (isGachaPlaying) return;

        if (GlobalSceneFader.Instance != null)
        {
            GlobalSceneFader.Instance.SwitchPanelWithFade(storePanel, gachaPanel);
        }
        else
        {
            storePanel.SetActive(false);
            gachaPanel.SetActive(true);
        }

        if (gachaAnimPanel != null) gachaAnimPanel.SetActive(false);
        ClearResultArea();
    }

    // 3. 뽑기 창에서 상점으로 돌아가기
    public void BackToStore()
    {
        if (isGachaPlaying) return;

        if (GlobalSceneFader.Instance != null)
        {
            GlobalSceneFader.Instance.SwitchPanelWithFade(gachaPanel, storePanel);
        }
        else
        {
            gachaPanel.SetActive(false);
            storePanel.SetActive(true);
        }

        RefreshShopItems();
    }

    // 4. 모든 창 닫기
    public void CloseAllPanels()
    {
        if (isGachaPlaying) return;

        if (GlobalSceneFader.Instance != null)
        {
            if (storePanel.activeSelf) GlobalSceneFader.Instance.ClosePanelWithFade(storePanel);
            else if (gachaPanel.activeSelf) GlobalSceneFader.Instance.ClosePanelWithFade(gachaPanel);
        }
        else
        {
            storePanel.SetActive(false);
            gachaPanel.SetActive(false);
        }
    }

    public void RefreshShopItems()
    {
        selectedSlot = null;
        selectedCard = null;

        if (shopContent == null || shopItemPrefab == null) return;

        foreach (Transform child in shopContent) Destroy(child.gameObject);

        List<int> availableIds = new List<int>();
        if (GameManager.Instance.collectionHistory != null)
        {
            availableIds.AddRange(GameManager.Instance.collectionHistory);
        }

        if (availableIds.Count == 0)
        {
            if (GameManager.Instance.allCardDatabase != null)
            {
                foreach (var card in GameManager.Instance.allCardDatabase)
                {
                    if (card != null) availableIds.Add(card.id);
                }
            }
        }

        if (availableIds.Count == 0) return;

        int shopSlotCount = Mathf.Min(3, availableIds.Count);
        List<int> selectedHorizontalIds = new List<int>();

        while (selectedHorizontalIds.Count < shopSlotCount)
        {
            int randId = availableIds[Random.Range(0, availableIds.Count)];
            if (!selectedHorizontalIds.Contains(randId))
            {
                selectedHorizontalIds.Add(randId);
            }
        }

        foreach (int cardId in selectedHorizontalIds)
        {
            CardData cardData = GameManager.Instance.allCardDatabase.Find(x => x.id == cardId);
            if (cardData != null)
            {
                GameObject slot = Instantiate(shopItemPrefab, shopContent);
                slot.GetComponent<ShopItemSlot>().Setup(cardData, this);
            }
        }
    }

    public void SelectCard(CardData data, ShopItemSlot clickedSlot)
    {
        if (selectedSlot != null && selectedSlot != clickedSlot) selectedSlot.SetSelectState(false);
        selectedCard = data;
        selectedSlot = clickedSlot;
        if (selectedSlot != null) selectedSlot.SetSelectState(true);
    }

    public void BuyCard()
    {
        if (selectedCard == null || selectedSlot == null) return;
        if (selectedSlot.IsSoldOut) return;

        int price = 100;
        if (GameManager.Instance.UseGold(price))
        {
            GameManager.Instance.AddCard(selectedCard);
            Debug.Log($"{selectedCard.cardName} 상점 구매 완료!");
            selectedSlot.SetSoldOut();

            StartCoroutine(ShowPurchasePopup(selectedCard.cardName));
        }
    }

    private IEnumerator ShowPurchasePopup(string cardName)
    {
        if (purchaseCompletePanel == null) yield break;

        if (purchaseCompleteText != null)
            purchaseCompleteText.text = $"[{cardName}]\n구매가 완료되었습니다!";

        purchaseCompletePanel.SetActive(true);
        Transform popupTr = purchaseCompletePanel.transform;
        popupTr.localScale = Vector3.zero;

        float t = 0;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            popupTr.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t / 0.2f);
            yield return null;
        }
        popupTr.localScale = Vector3.one;

        yield return new WaitForSeconds(1.0f);

        t = 0;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            popupTr.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t / 0.2f);
            yield return null;
        }
        purchaseCompletePanel.SetActive(false);
    }

    public void DoGacha()
    {
        if (isGachaPlaying) return;

        int gachaPrice = 100;
        if (GameManager.Instance.allCardDatabase.Count == 0) return;

        if (GameManager.Instance.UseGold(gachaPrice))
        {
            int randomIndex = Random.Range(0, GameManager.Instance.allCardDatabase.Count);
            CardData pulledCard = GameManager.Instance.allCardDatabase[randomIndex];

            GameManager.Instance.AddCard(pulledCard);
            StartCoroutine(PlayGachaSequence(pulledCard));
        }
    }

    private IEnumerator PlayGachaSequence(CardData pulledCard)
    {
        isGachaPlaying = true;
        ClearResultArea();

        // 1. 연출 패널 활성화 및 초기 투명도 0 세팅
        if (gachaBoxImage != null)
        {
            Color c = gachaBoxImage.color;
            c.a = 0f;
            gachaBoxImage.color = c;
        }

        if (gachaAnimPanel != null)
        {
            gachaAnimPanel.SetActive(true);
            gachaAnimPanel.transform.SetAsLastSibling(); // GachaPanel 맨 앞에 표시
        }

        // 2. Item_open_crate 사운드 재생 및 길이 측정
        float totalDuration = fallbackDuration;
        if (openCrateClip != null)
        {
            totalDuration = openCrateClip.length;
            gachaAudioSource.PlayOneShot(openCrateClip);
        }

        // 3. 음원 총 길이를 이미지 수(3장)로 나눠 페이드인 -> 유지 -> 페이드아웃 반복
        if (gachaBoxImage != null && boxOpenSprites != null && boxOpenSprites.Length > 0)
        {
            int count = boxOpenSprites.Length;
            float stepDuration = totalDuration / count;
            float fadeTime = stepDuration * 0.25f;
            float holdTime = stepDuration * 0.5f;

            for (int i = 0; i < count; i++)
            {
                if (boxOpenSprites[i] != null)
                {
                    gachaBoxImage.sprite = boxOpenSprites[i];
                }

                yield return StartCoroutine(FadeImageAlpha(gachaBoxImage, 0f, 1f, fadeTime));
                yield return new WaitForSeconds(holdTime);
                yield return StartCoroutine(FadeImageAlpha(gachaBoxImage, 1f, 0f, fadeTime));
            }
        }
        else
        {
            yield return new WaitForSeconds(totalDuration);
        }

        // 4. 연출 패널 종료 -> 달성 사운드 재생 & ResultPoint에 카드 표시
        if (gachaAnimPanel != null)
        {
            gachaAnimPanel.SetActive(false);
        }

        if (unlockResultClip != null)
        {
            gachaAudioSource.PlayOneShot(unlockResultClip);
        }

        ShowResultCard(pulledCard);
        isGachaPlaying = false;
    }

    private IEnumerator FadeImageAlpha(Image targetImage, float startAlpha, float endAlpha, float duration)
    {
        if (targetImage == null) yield break;

        Color c = targetImage.color;
        if (duration <= 0f)
        {
            c.a = endAlpha;
            targetImage.color = c;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, Mathf.Clamp01(elapsed / duration));
            targetImage.color = c;
            yield return null;
        }

        c.a = endAlpha;
        targetImage.color = c;
    }

    void ShowResultCard(CardData cardData)
    {
        ClearResultArea();
        if (cardPrefab != null && resultPoint != null)
        {
            GameObject newCard = Instantiate(cardPrefab, resultPoint);
            newCard.transform.localPosition = Vector3.zero;
            newCard.transform.localScale = Vector3.one;

            CardController cardCtrl = newCard.GetComponent<CardController>();
            if (cardCtrl != null)
            {
                cardCtrl.Setup(cardData);
                cardCtrl.SetInventoryMode();
            }
        }
    }

    void ClearResultArea()
    {
        if (resultPoint == null) return;
        foreach (Transform child in resultPoint) Destroy(child.gameObject);
    }
}