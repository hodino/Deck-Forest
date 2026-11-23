using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using UnityEngine.EventSystems;

public class CardController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("UI 연결")]
    public Image artworkImage;
    public Image backgroundImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI descriptionText;
    public CanvasGroup canvasGroup;


    [Header("설정")]
    public bool isInventoryMode = false;
    public float defaultMoveSpeed = 10f;
    public float hoverScale = 1.2f;

    // 데이터
    public CardData myData;

    // 상태 변수
    private RectTransform rectTransform;
    private Vector3 originalScale = Vector3.one; // [수정] 기본값 (1,1,1) 고정
    private Transform originalParent;
    private int originalSiblingIndex;

    private Vector3 currentTargetPos;
    private float currentSpeed = 10f;

    private bool isDragging = false;
    private bool isHovering = false;
    private bool isInHand = true;

    // 코스트 시스템 변수
    public bool isUsable = true;

    public bool isHistoryMode = false; // 기록 모드인지 확인하는 변수

    // 인벤토리용 변수
    private Canvas sortCanvas;

    // 레이아웃 및 드래그 변수
    private Vector2 layoutTargetPos;
    public static bool isAnyCardDragging = false;

    private int baseSortingOrder = 10;

    public Action<CardController> onClickAction;

    void Awake()
    {
        // RectTransform과 CanvasGroup 기본 컴포넌트 연결
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // CanvasGroup이 없을 경우를 대비한 안전장치
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // [핵심 삭제] -------------------------------------------------------
        // 마스크(Mask) 기능과 충돌하는 Canvas 및 GraphicRaycaster 추가 로직,
        // 그리고 sortingOrder를 직접 수정하던 코드를 모두 제거했습니다.
        // ------------------------------------------------------------------

        // 카드가 (0,0,0) 크기로 초기화되어 사라지는 현상을 방지합니다.
        originalScale = transform.localScale;
        if (originalScale == Vector3.zero) originalScale = Vector3.one;
    }

    // --- [에러 해결을 위한 필수 함수들 복구] ---

    public void SetSortingOrder(int order)
    {
        if (sortCanvas == null) sortCanvas = GetComponent<Canvas>();
        sortCanvas.overrideSorting = true;
        baseSortingOrder = order;
        sortCanvas.sortingOrder = order;
    }

    public void ResetOriginalScale(Vector3 newScale)
    {
        originalScale = newScale;
        transform.localScale = newScale;
    }

    public void SetInventoryMode()
    {
        isInventoryMode = true;
        isInHand = false;
        this.enabled = true;
    }

    public void SetHistoryMode()
    {
        isHistoryMode = true;
        isInHand = false;

        // 크기를 기존보다 크게 설정합니다 (예: 1배 또는 0.8배)
        originalScale = new Vector3(2f, 2f, 2f);
        transform.localScale = originalScale;

        // 레이캐스트를 켜서 상호작용 가능하게 합니다.
        if (canvasGroup != null)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void Setup(CardData data)
    {
        myData = data;
        if (costText != null) costText.text = data.cardCost.ToString();
        if (descriptionText != null) descriptionText.text = data.cardDescription;
        if (nameText != null) nameText.text = data.cardName;
        if (artworkImage != null) artworkImage.sprite = data.cardImage;
    }

    // LibraryManager에서 호출하는 함수 복구
    // CardController.cs 내부의 함수입니다. 
    // 괄호 안에 Sprite unknownSprite 가 들어있는지 꼭 확인해 주세요!
    public void SetUnknown(Sprite unknownSprite)
    {
        // 1. 카드 전체(배경 + 아이콘)를 어둡게 만듭니다.
        Color darkColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        if (backgroundImage != null) backgroundImage.color = darkColor;

        if (artworkImage != null)
        {
            artworkImage.color = darkColor;

            // 2. 전달받은 '?' 이미지가 있다면 그걸로 아이콘을 교체합니다.
            if (unknownSprite != null)
            {
                artworkImage.sprite = unknownSprite;
            }
        }

        // 3. 텍스트들을 모두 "???"로 변경합니다.
        if (nameText != null) nameText.text = "???";
        if (descriptionText != null) descriptionText.text = "???";

        // 코스트(비용) 텍스트가 있다면 이것도 가려줍니다.
        if (costText != null) costText.text = "?";
    }

    public void UpdateUsability(int currentMana)
    {
        if (myData == null || isInventoryMode) return;

        if (myData.cardCost <= currentMana)
        {
            isUsable = true;
            if (artworkImage != null) artworkImage.color = Color.white;
        }
        else
        {
            isUsable = false;
            if (artworkImage != null) artworkImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }

    // HandLayoutManager에서 호출하는 함수 복구
    public void SetLayoutTarget(Vector2 targetPos, float speed)
    {
        if (isInventoryMode) return;
        layoutTargetPos = targetPos;
        if (!isDragging && !isHovering && isInHand) currentSpeed = speed;
    }

    // BattleManager에서 호출하는 함수 복구
    public void SetInteractable(bool state)
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = state;
            canvasGroup.blocksRaycasts = state;
        }
    }

    // BattleManager에서 호출하는 함수 복구
    public void MoveToCenterEffect(Vector2 centerPos)
    {
        if (isInventoryMode) return;
        isInHand = false;
        isHovering = false;
        isDragging = false;
        layoutTargetPos = centerPos;
        currentSpeed = 5f;
        transform.localScale = originalScale * 1.2f;
    }

    // BattleManager에서 호출하는 함수 복구
    public IEnumerator FadeOut(float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, time / duration);
            yield return null;
        }
        canvasGroup.alpha = 0;
    }

    // --- [로직 업데이트] ---

    void Update()
    {
        if (originalScale == Vector3.zero) originalScale = Vector3.one;

        // isHistoryMode일 때 강제 위치 이동을 멈추고 크기만 조절합니다.
        if (isInventoryMode || isHistoryMode)
        {
            Vector3 targetScaleVec = isHovering ? originalScale * hoverScale : originalScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScaleVec, Time.unscaledDeltaTime * 10f);
            return; // 레이아웃 그룹의 통제를 따르도록 여기서 return 합니다.
        }

        if (isInventoryMode)
        {
            Vector3 targetScaleVec = isHovering ? originalScale * hoverScale : originalScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScaleVec, Time.unscaledDeltaTime * 10f);
            return;
        }

        CalculateTargetPosition();

        if (isDragging)
        {
            transform.position = Vector3.Lerp(transform.position, currentTargetPos, Time.deltaTime * 20f);
        }
        else
        {
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, currentTargetPos, Time.deltaTime * currentSpeed);
            Vector3 targetScale = (isHovering && isInHand) ? originalScale * hoverScale : originalScale;
            if (!isInHand && !isDragging) targetScale = transform.localScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 10f);
        }
    }

    void CalculateTargetPosition()
    {
        if (isDragging) currentTargetPos = Input.mousePosition;
        else if (isHovering && isInHand) currentTargetPos = layoutTargetPos + new Vector2(0, 80f);
        else currentTargetPos = layoutTargetPos;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isInventoryMode)
        {
            // 1. 기존 기능: 클릭한 카드 이름 출력 (확인용)
            Debug.Log($"[클릭] {myData?.cardName}");

            // 2. 새로운 기능: 창고 매니저 등 외부 스크립트로 클릭 신호 보내기 (핵심!)
            onClickAction?.Invoke(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging || isAnyCardDragging) return;
        isHovering = true;



        if (!isInventoryMode && isInHand)
        {
            originalSiblingIndex = transform.GetSiblingIndex();
            transform.SetAsLastSibling();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isInventoryMode || !isInHand) return;

        isAnyCardDragging = true;
        isDragging = true;
        isInHand = false;

        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isInventoryMode) return;
        if (isDragging) currentTargetPos = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isInventoryMode) return;

        isAnyCardDragging = false;
        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        bool isDroppedOnZone = (eventData.pointerEnter != null && eventData.pointerEnter.CompareTag("DropZone"));

        if (isDroppedOnZone && isUsable)
        {
            if (BattleManager.Instance != null) BattleManager.Instance.OnCardPlayed(this);
        }
        else
        {
            isInHand = true;
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
        }
    }
    
}