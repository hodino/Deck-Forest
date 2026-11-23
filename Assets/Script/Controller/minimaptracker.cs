using UnityEngine;
using UnityEngine.UI;

public class MinimapTracker : MonoBehaviour
{
    [Header("중앙 미니맵 요소")]
    public RectTransform playerMapIcon;
    public float startWorldX = 0f;
    public float endWorldX = 50f;

    [Header("양옆 방 아이콘 연결")]
    public Image lastRoomImage;
    public Image nextRoomImage;

    [Header("방 타입별 스프라이트 설정")]
    public Sprite startIcon;
    public Sprite battleIcon;
    public Sprite restIcon;
    public Sprite treasureIcon;

    private SideScrollMove playerRef; // 플레이어 참조 저장용

    void Update()
    {
        // ▼▼▼ [수정] Instance 대신 씬에서 플레이어를 직접 찾습니다. ▼▼▼
        if (playerRef == null)
        {
            // 유니티 6 권장 함수 사용 (FindObjectOfType의 최신 버전)
            playerRef = Object.FindFirstObjectByType<SideScrollMove>();
        }

        if (playerRef != null && playerMapIcon != null)
        {
            // 실제 플레이어의 X 좌표를 가져와 미니맵 바 진행도를 계산합니다.
            float progress = Mathf.InverseLerp(startWorldX, endWorldX, playerRef.transform.position.x);

            playerMapIcon.anchorMin = new Vector2(progress, 0.5f);
            playerMapIcon.anchorMax = new Vector2(progress, 0.5f);
            playerMapIcon.anchoredPosition = Vector2.zero;
        }
    }

    // 방 이동 시 호출할 함수 (지난 답변과 동일)
    public void UpdateRoomIcons(RoomType nextType)
    {
        if (GameManager.Instance == null) return;

        // 1. 왼쪽: 지나온 방 아이콘 처리
        if (lastRoomImage != null)
        {
            if (GameManager.Instance.passedRooms.Count > 0)
            {
                RoomType lastType = GameManager.Instance.passedRooms[GameManager.Instance.passedRooms.Count - 1];
                lastRoomImage.sprite = GetSpriteByRoomType(lastType);
                // ▼ [수정] 이미지를 보이게 설정 (불투명)
                lastRoomImage.color = new Color(1, 1, 1, 1);
            }
            else
            {
                // ▼ [수정] 첫 방일 때 오브젝트를 끄지 않고 투명도만 0으로 (자리 유지)
                lastRoomImage.color = new Color(1, 1, 1, 0);
            }
        }

        // 2. 오른쪽: 다음 방 아이콘 처리
        if (nextRoomImage != null)
        {
            nextRoomImage.sprite = GetSpriteByRoomType(nextType);
            // 다음 방은 항상 예고되어야 하므로 불투명하게 설정
            nextRoomImage.color = new Color(1, 1, 1, 1);
        }
    }

    private Sprite GetSpriteByRoomType(RoomType type)
    {
        switch (type)
        {
            case RoomType.Start: return startIcon;
            case RoomType.Battle: return battleIcon;
            case RoomType.Rest: return restIcon;
            case RoomType.Treasure: return treasureIcon;
            default: return null;
        }
    }
}