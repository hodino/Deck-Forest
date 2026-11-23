using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // 코루틴을 위해 추가

public class ShopItemSlot : MonoBehaviour
{
    [Header("UI 연결")]
    public Image cardIcon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;

    [Header("품절 연출용 (카드 배경 Image)")]
    public Image backgroundImage; // 프리팹의 가장 겉 배경 Image를 연결하세요.

    private CardData itemData;
    private StoreManager storeManager;

    private Vector3 originalScale = Vector3.one;
    private Coroutine scaleCoroutine; // 부드러운 연출용 코루틴 추적자

    // ★ 품절 상태를 기억할 변수
    public bool IsSoldOut { get; private set; } = false;

    public void Setup(CardData data, StoreManager manager)
    {
        itemData = data;
        storeManager = manager;
        IsSoldOut = false; // 상태 초기화

        originalScale = Vector3.one;
        transform.localScale = originalScale;

        // UI 데이터 매핑
        if (cardIcon != null) { cardIcon.sprite = data.cardImage; cardIcon.color = Color.white; }
        if (nameText != null) nameText.text = data.cardName;
        if (priceText != null) priceText.text = "100 G";
        if (descriptionText != null) descriptionText.text = data.cardDescription;
        if (costText != null) costText.text = data.cardCost.ToString();
        if (backgroundImage != null) backgroundImage.color = Color.white; // 색상 초기화
    }

    public void OnClickSlot()
    {
        // ★ 품절된 상태라면 클릭 시 매니저에게 알리되, 선택 연출은 패스합니다.
        if (IsSoldOut)
        {
            if (storeManager != null) storeManager.SelectCard(itemData, this);
            return;
        }

        if (storeManager != null)
        {
            storeManager.SelectCard(itemData, this);
        }
    }

    // --- [ 부드러운 크기 조절 연출 ] ---
    public void SetSelectState(bool isSelected)
    {
        // 품절된 카드는 크기가 변하지 않도록 차단
        if (IsSoldOut) return;

        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);

        Vector3 targetScale = isSelected ? originalScale * 1.3f : originalScale;
        scaleCoroutine = StartCoroutine(ScaleOverTime(targetScale, 0.15f)); // 0.15초 동안 부드럽게 변경
    }

    IEnumerator ScaleOverTime(Vector3 target, float duration)
    {
        Vector3 startScale = transform.localScale;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, target, time / duration);
            yield return null;
        }
        transform.localScale = target;
    }

    // --- [ ★ 품절(어둡게 + 기능 차단) 처리 함수 ] ---
    public void SetSoldOut()
    {
        IsSoldOut = true;

        // 카드를 어둡게 변경 (RGB 값을 0.3정도로 낮춤)
        Color darkColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        if (backgroundImage != null) backgroundImage.color = darkColor;
        if (cardIcon != null) cardIcon.color = darkColor;

        // 품절되면 크기를 다시 원래대로 안전하게 축소
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        transform.localScale = originalScale;

        if (priceText != null) priceText.text = "SOLD OUT";
    }
}