using UnityEngine;
using TMPro;
using UnityEngine.UI; // ★ 이미지를 다루기 위해 추가!

public class BattleUIManager : MonoBehaviour
{
    [Header("플레이어 UI 연결")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI shieldText;

    public Image hpBarFill; // ★ 여기에 새로 만든 빨간색 체력바를 넣습니다!

    void Update()
    {
        // GameManager가 존재하는지 먼저 확인
        if (GameManager.Instance != null)
        {
            // 1. 기존 텍스트 업데이트
            if (hpText != null)
            {
                hpText.text = $"HP: {GameManager.Instance.playerCurrentHp} / {GameManager.Instance.playerMaxHp}";
            }

            // 2. ★ 체력바 이미지 게이지 실시간 업데이트
            if (hpBarFill != null && GameManager.Instance.playerMaxHp > 0)
            {
                hpBarFill.fillAmount = (float)GameManager.Instance.playerCurrentHp / GameManager.Instance.playerMaxHp;
            }

            // 3. 방어도 텍스트 업데이트
            if (GameManager.Instance.playerCurrentShield > 0)
            {
                if (shieldText != null) shieldText.gameObject.SetActive(true);
                if (shieldText != null) shieldText.text = $"Shield: {GameManager.Instance.playerCurrentShield}";
            }
            else
            {
                if (shieldText != null) shieldText.gameObject.SetActive(false);
            }
        }
    }
}