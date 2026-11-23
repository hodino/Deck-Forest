using System.Collections.Generic;
using UnityEngine;

public class InteractiveEventTrigger : MonoBehaviour
{
    [Header("이벤트 설정")]
    public string eventTitle = "수상한 상인";
    [TextArea(3, 5)]
    public string eventDialogue = "길가에 수상한 약병이 떨어져 있다. 마셔볼까?";

    [Tooltip("1회성 이벤트인 경우 체크 (대화 완료 후 다시 작동하지 않음)")]
    public bool isOneTimeEvent = true;

    [Header("선택지 목록")]
    public List<EventChoice> choices = new List<EventChoice>();

    private bool isUsed = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isUsed) return;

        // 플레이어 태그 확인 (collision.collider로 접근)
        if (collision.collider.CompareTag("Player"))
        {
            if (InteractiveEventUI.Instance != null && InteractiveEventUI.Instance.gameObject != null)
            {
                InteractiveEventUI.Instance.OpenEvent(eventTitle, eventDialogue, choices, this);
            }
            else
            {
                Debug.LogWarning("씬에 InteractiveEventUI가 존재하지 않습니다.");
            }
        }
    }

    public void DisableTrigger()
    {
        isUsed = true;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }
}