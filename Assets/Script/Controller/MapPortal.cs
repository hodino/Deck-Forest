using UnityEngine;

public class MapPortal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어와 충돌했을 때만 실행
        if (collision.CompareTag("Player"))
        {
            Debug.Log("포탈 터치! 다음 방으로 이동합니다.");
            if (StageUIManager.Instance != null)
            {
                StageUIManager.Instance.GenerateAndEnterNextRoom();
            }
        }
    }
}