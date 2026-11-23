using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MainButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("설정")]
    public float hoverScale = 1.3f;
    public float animationSpeed = 10f;

    [Header("UI 연결")]
    public CanvasGroup labelCanvasGroup;

    private Vector3 originalScale;
    private bool isHovering = false;

    void Awake()
    {
        // 시작할 때의 스케일을 기억합니다.
        originalScale = transform.localScale;

        if (labelCanvasGroup != null)
        {
            labelCanvasGroup.alpha = 0;
        }
    }

    void Update()
    {
        // 1. 크기 변경 로직 (X, Y만 확대하도록 수정)
        float currentFactor = isHovering ? hoverScale : 1f;

        // ▼▼▼ [수정] Z값은 originalScale.z를 그대로 유지합니다. ▼▼▼
        Vector3 targetScale = new Vector3(
            originalScale.x * currentFactor,
            originalScale.y * currentFactor,
            originalScale.z
        );

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);

        // 2. 글자 투명도 로직
        if (labelCanvasGroup != null)
        {
            float targetAlpha = isHovering ? 1f : 0f;
            labelCanvasGroup.alpha = Mathf.Lerp(labelCanvasGroup.alpha, targetAlpha, Time.deltaTime * animationSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;

        // ▼▼▼ [삭제] transform.SetAsLastSibling(); 을 지웠습니다. ▼▼▼
        // 이제 마우스를 올려도 버튼이 뒤에서 앞으로 튀어나오지 않고 
        // 원래 배치된 순서(Z-depth)를 유지하며 크기만 커집니다.
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }
}