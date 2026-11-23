using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    [Header("UI 배경 파동 설정")]
    [Tooltip("마우스가 움직일 때 배경이 이동하는 최대 거리(픽셀 단위)입니다. (추천: 20 ~ 50)")]
    public float mouseInfluence = 30f;

    [Tooltip("배경이 마우스를 따라가는 부드러움 정도입니다. (추천: 3 ~ 8)")]
    public float smoothSpeed = 5f;

    // UI 전용 트랜스폼(RectTransform)을 사용해야 화면이 정상적으로 움직입니다!
    private RectTransform rectTransform;
    private Vector2 basePosition;
    private Vector2 currentOffset;

    void Start()
    {
        // 이 스크립트가 붙은 UI(배경)의 정보를 가져와서 원래 자리를 기억합니다.
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            basePosition = rectTransform.anchoredPosition;
        }
        else
        {
            Debug.LogError("DynamicCamera: 이 코드는 UI(이미지 등)에 붙여야 작동합니다!");
        }
    }

    void Update()
    {
        if (rectTransform == null) return;

        // 1. 화면 내 마우스의 위치를 -1 ~ 1 사이의 비율로 계산합니다.
        float mouseX = Mathf.Clamp((Input.mousePosition.x / Screen.width) * 2f - 1f, -1f, 1f);
        float mouseY = Mathf.Clamp((Input.mousePosition.y / Screen.height) * 2f - 1f, -1f, 1f);

        // 2. 입체감을 위해 마우스가 가는 방향과 '반대'로 배경을 살짝 밀어냅니다 (-1f 곱하기)
        // (마우스를 따라가게 하고 싶다면 -1f를 지우면 됩니다!)
        Vector2 targetOffset = new Vector2(mouseX, mouseY) * mouseInfluence * -1f;

        // 3. 부드럽게 목표 위치로 이동시킵니다.
        currentOffset = Vector2.Lerp(currentOffset, targetOffset, smoothSpeed * Time.deltaTime);

        // 4. UI 전용 좌표(anchoredPosition)에 최종 위치를 적용합니다.
        rectTransform.anchoredPosition = basePosition + currentOffset;
    }
}