using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // ★ TextMeshPro 사용을 위해 추가

// 선택지 결과 종류
public enum EventOutcomeType
{
    None,         // 아무 일도 없음
    TakeDamage,   // 데미지 입음
    Heal,         // 체력 회복
    GainGold,     // 골드 획득
    LoseGold      // 골드 차감
}

[System.Serializable]
public class EventChoice
{
    public string choiceText;            // 선택지 버튼에 표시될 텍스트
    public EventOutcomeType outcomeType; // 선택 결과 타입
    public int valueAmount;              // 효과 수치 (데미지량, 회복량, 골드 수치 등)
    [TextArea(2, 4)]
    public string resultDialogue;        // 선택 후 출력될 결과 대화
}

public class InteractiveEventUI : MonoBehaviour
{
    public static InteractiveEventUI Instance;

    [Header("UI 요소 연결")]
    public GameObject eventPanel;         // 이벤트 전체 팝업 패널

    // ★ 기존 Text를 TextMeshProUGUI로 변경
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI dialogueText;

    public Transform choiceButtonParent;  // 선택지 버튼들이 생성될 부모 Transform
    public GameObject choiceButtonPrefab; // 선택지 버튼 프리팹
    public Button closeButton;            // 대화 종료 버튼 (결과창에서 사용)

    private InteractiveEventTrigger currentTrigger;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (eventPanel != null) eventPanel.SetActive(false);
        if (closeButton != null) closeButton.gameObject.SetActive(false);
    }

    // 이벤트 대화 시작
    public void OpenEvent(string title, string dialogue, List<EventChoice> choices, InteractiveEventTrigger trigger)
    {
        currentTrigger = trigger;
        eventPanel.SetActive(true);
        if (closeButton != null) closeButton.gameObject.SetActive(false);

        if (titleText != null) titleText.text = title;
        if (dialogueText != null) dialogueText.text = dialogue;

        // 기존 선택지 버튼 제거
        foreach (Transform child in choiceButtonParent)
        {
            Destroy(child.gameObject);
        }

        // 선택지 버튼 동적 생성
        foreach (var choice in choices)
        {
            GameObject btnObj = Instantiate(choiceButtonPrefab, choiceButtonParent);

            // ★ 버튼 안의 텍스트도 TextMeshProUGUI로 찾도록 변경
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = choice.choiceText;

            Button btn = btnObj.GetComponent<Button>();
            EventChoice targetChoice = choice; // 람다 캡처용
            btn.onClick.AddListener(() => OnSelectChoice(targetChoice));
        }

        // 플레이어 이동 일시 정지
        SetPlayerMovement(false);
    }

    // 선택지 클릭 시 실행
    private void OnSelectChoice(EventChoice choice)
    {
        // 1. 선택지 버튼 제거
        foreach (Transform child in choiceButtonParent)
        {
            Destroy(child.gameObject);
        }

        // 2. 효과 적용
        ApplyOutcome(choice.outcomeType, choice.valueAmount);

        // 3. 결과 대화 출력
        if (dialogueText != null)
        {
            dialogueText.text = choice.resultDialogue;
        }

        // 4. 닫기 버튼 활성화
        if (closeButton != null)
        {
            closeButton.gameObject.SetActive(true);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseEvent);
        }
    }

    // 선택지에 따른 데이터 변화 처리
    private void ApplyOutcome(EventOutcomeType outcome, int amount)
    {
        if (GameManager.Instance == null) return;

        switch (outcome)
        {
            case EventOutcomeType.TakeDamage:
                // 방어도 상관없이 직접 HP 감소 (0 미만 방지)
                GameManager.Instance.playerCurrentHp = Mathf.Max(0, GameManager.Instance.playerCurrentHp - amount);
                Debug.Log($"[이벤트] 데미지 입음: {amount} (현재 HP: {GameManager.Instance.playerCurrentHp})");
                break;

            case EventOutcomeType.Heal:
                // 최대 체력을 넘지 않도록 체력 회복
                GameManager.Instance.playerCurrentHp = Mathf.Min(GameManager.Instance.playerMaxHp, GameManager.Instance.playerCurrentHp + amount);
                Debug.Log($"[이벤트] 체력 회복: {amount} (현재 HP: {GameManager.Instance.playerCurrentHp})");
                break;

            case EventOutcomeType.GainGold:
                GameManager.Instance.gold += amount;
                Debug.Log($"[이벤트] 골드 획득: {amount} (현재 골드: {GameManager.Instance.gold})");
                break;

            case EventOutcomeType.LoseGold:
                GameManager.Instance.gold = Mathf.Max(0, GameManager.Instance.gold - amount);
                Debug.Log($"[이벤트] 골드 차감: {amount} (현재 골드: {GameManager.Instance.gold})");
                break;

            case EventOutcomeType.None:
            default:
                Debug.Log("[이벤트] 아무 일도 일어나지 않았습니다.");
                break;
        }

        // 1. 골드 UI 즉시 갱신
        GameManager.Instance.UpdateCurrencyUI();

        // 2. 체력(HP) UI 즉시 갱신
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.UpdateHPUI();
        }

        // 3. 상태이상 (근력, 취약) UI 갱신 (기존 코드 유지)
        if (StageUIManager.Instance != null)
        {
            StageUIManager.Instance.UpdatePlayerStatusUI();
        }

        // 변경된 데이터를 세이브 파일에 반영
        GameManager.Instance.SaveGameData();
    }

    // 이벤트 종료
    public void CloseEvent()
    {
        eventPanel.SetActive(false);
        SetPlayerMovement(true);

        // 일회성 이벤트일 경우 오브젝트 비활성화
        if (currentTrigger != null && currentTrigger.isOneTimeEvent)
        {
            currentTrigger.DisableTrigger();
        }
    }

    // 이벤트 진행 중 플레이어 이동 제어
    private void SetPlayerMovement(bool enable)
    {
        MonoBehaviour moveScript = FindObjectOfType<SideScrollMove>();
        if (moveScript != null)
        {
            moveScript.enabled = enable;
        }
    }
}