using UnityEngine;
using TMPro;

public class CurrencyLinker : MonoBehaviour
{
    public enum CurrencyType { Gold, Soul }
    public CurrencyType type;

    void Start()
    {
        if (GameManager.Instance == null) return;

        // 타입에 맞춰 GameManager의 변수에 자신을 연결
        if (type == CurrencyType.Gold)
            GameManager.Instance.goldText = GetComponent<TextMeshProUGUI>();
        else
            GameManager.Instance.soulText = GetComponent<TextMeshProUGUI>();

        // 연결 직후 현재 값을 표시
        GameManager.Instance.UpdateCurrencyUI();
    }
}