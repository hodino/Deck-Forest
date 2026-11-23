using UnityEngine;

public class RandomPortal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. 필드 몬스터 조우 데이터 전달 (기존 유지)
            FieldEnemy fieldEnemy = GetComponent<FieldEnemy>();
            if (fieldEnemy != null && GameManager.Instance != null)
            {
                GameManager.Instance.currentEncounteredEnemy = fieldEnemy.enemyData;
            }

            gameObject.SetActive(false);

            Debug.Log("포탈 작동! 사령탑(StageUIManager)에 다음 방 이동을 요청합니다.");

            // 2. 씬 이동은 StageUIManager에게 전적으로 맡김
            if (StageUIManager.Instance != null)
            {
                StageUIManager.Instance.GenerateAndEnterNextRoom();
            }
            else
            {
                Debug.LogWarning("씬에 StageUIManager가 존재하지 않습니다!");
            }
        }
    }
}