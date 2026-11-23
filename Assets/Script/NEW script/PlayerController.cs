using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // ★ 1. 유니티 UI(이미지)를 제어하기 위해 꼭 추가해야 합니다!

public class PlayerController : MonoBehaviour
{
    // 외부 스크립트(InteractiveEventUI 등) 참조용 싱글톤
    public static PlayerController Instance;

    [Header("UI 연결")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI costText;

    public Image hpBarFill; // ★ 2. 에디터에서 만든 빨간색 체력바 이미지를 넣을 빈칸!

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // ▼▼▼ [수정] 숨겨져 있거나 프리팹 깊숙이 있는 CostText도 무조건 찾아내는 코드 ▼▼▼
        TextMeshProUGUI[] allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        foreach (TextMeshProUGUI txt in allTexts)
        {
            // 화면에 나타나는 씬에 존재하고 이름이 정확히 "CostText"인 것을 찾음
            if (txt.name == "CostText" && txt.gameObject.scene.isLoaded)
            {
                costText = txt;
                Debug.Log("CostText를 성공적으로 찾아서 연결했습니다!");
                break;
            }
        }

        if (costText == null)
        {
            Debug.LogWarning("여전히 CostText 오브젝트를 찾을 수 없습니다! 이름을 다시 확인해주세요.");
        }
        // ▲▲▲ 수정 끝 ▲▲▲

        UpdateHPUI();
    }

    // 데미지 입기
    public void TakeDamage(int damageAmount)
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.playerCurrentHp -= damageAmount;
        if (GameManager.Instance.playerCurrentHp < 0)
        {
            GameManager.Instance.playerCurrentHp = 0;
        }

        Debug.Log($"플레이어 피격! 남은 HP: {GameManager.Instance.playerCurrentHp}");
        UpdateHPUI();

        // 체력이 0 이하가 되면 게임 오버 씬으로 이동
        if (GameManager.Instance.playerCurrentHp <= 0)
        {
            Debug.Log("게임 오버! 씬 전환...");

            // GlobalSceneFader가 등록되어 있다면 페이드 효과와 함께 전환
            if (GlobalSceneFader.Instance != null)
            {
                GlobalSceneFader.Instance.LoadScene("GameOverScene");
            }
            else
            {
                SceneManager.LoadScene("GameOverScene");
            }
        }
    }

    // 체력 회복
    public void Heal(int healAmount)
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.playerCurrentHp += healAmount;

        // 최대 체력(playerMaxHp)을 초과하지 않도록 제한
        if (GameManager.Instance.playerCurrentHp > GameManager.Instance.playerMaxHp)
        {
            GameManager.Instance.playerCurrentHp = GameManager.Instance.playerMaxHp;
        }

        Debug.Log($"플레이어 체력 회복! 남은 HP: {GameManager.Instance.playerCurrentHp}");
        UpdateHPUI();
    }

    // ★ 3. HP UI 갱신 로직 업그레이드
    public void UpdateHPUI()
    {
        if (GameManager.Instance != null)
        {
            // 기존: 텍스트가 있으면 텍스트 갱신 (텍스트를 끄셨더라도 코드는 두는 게 안전합니다)
            if (hpText != null)
            {
                hpText.text = $"HP: {GameManager.Instance.playerCurrentHp} / {GameManager.Instance.playerMaxHp}";
            }

            // 신규: 체력바 이미지가 연결되어 있다면 게이지를 채우거나 깎음
            if (hpBarFill != null && GameManager.Instance.playerMaxHp > 0)
            {
                // (현재체력 / 최대체력)을 소수점으로 계산하여 0.0 ~ 1.0 비율로 적용
                hpBarFill.fillAmount = (float)GameManager.Instance.playerCurrentHp / GameManager.Instance.playerMaxHp;
            }
        }
    }

    // Cost UI 갱신
    public void UpdateCostUI(int current, int max)
    {
        if (costText != null) costText.text = $"Cost: {current} / {max}";
    }
}