using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum EnemyActionType { Attack, Defend }

public class EnemyController : MonoBehaviour
{
    [Header("적 데이터 연결")]
    public EnemyData[] possibleEnemies;
    public EnemyData myData;
    public SpriteRenderer enemySprite;
    public Animator enemyAnimator; // 애니메이터가 있는 경우 선택 연결

    [Header("★ UI 캔버스 위치 추적")]
    [Tooltip("StageUICanvas에 있는 적 체력바 그룹(EnemyHP_Group)을 여기에 연결하세요.")]
    public RectTransform canvasUIGroup;
    public float uiOffset = 0.5f;

    [Header("적 스탯")]
    public int currentHp;
    public int currentShield = 0;

    [Header("UI 연결")]
    public TextMeshProUGUI hpText;
    public Image hpBarFill;

    [Header("의도(Intent) UI")]
    public GameObject intentGroup;
    public Image intentIcon;
    public TextMeshProUGUI intentText;
    public Sprite attackSprite;
    public Sprite defendSprite;

    [Header("적 상태이상 UI 연결")]
    public GameObject strengthUIObj;
    public TextMeshProUGUI strengthText;
    public GameObject vulnerableUIObj;
    public TextMeshProUGUI vulnerableText;

    private EnemyActionType nextAction;
    private int nextActionValue;

    [Header("적 상태이상")]
    public int strength = 0;
    public int vulnerable = 0;

    [Header("모션 연출 설정")]
    public float motionDuration = 0.45f;
    public float attackLungeX = -0.6f; // 적은 왼쪽(플레이어 방향)으로 돌진
    // [추가] 배경이 투명하지 않은 이미지를 위한 피격 붉은 점멸 스위치
    [Tooltip("피격 시 스프라이트를 붉게 점멸할지 여부 (배경이 투명하지 않은 이미지는 체크 해제)")]
    public bool useRedFlashOnHit = false;

    private Vector3 originalSpriteLocalPos;
    private Coroutine motionCoroutine;
    // [추가] 적 효과음 재생기 연결
    private ObjectSFXPlayer sfxPlayer;

    void Awake()
    {
        // [추가] 적 효과음 컴포넌트 자동 연결
        sfxPlayer = GetComponent<ObjectSFXPlayer>();

        if (GameManager.Instance != null && GameManager.Instance.currentEncounteredEnemy != null)
        {
            myData = GameManager.Instance.currentEncounteredEnemy;
            GameManager.Instance.currentEncounteredEnemy = null;
        }
        else if (possibleEnemies != null && possibleEnemies.Length > 0)
        {
            myData = possibleEnemies[Random.Range(0, possibleEnemies.Length)];
        }

        if (myData == null) return;
        currentHp = myData.maxHp;

        if (enemySprite != null)
        {
            originalSpriteLocalPos = enemySprite.transform.localPosition;

            if (myData.enemyImage != null)
            {
                enemySprite.sprite = myData.enemyImage;
            }

            if (myData.enemyScale > 0f)
            {
                enemySprite.transform.localScale = new Vector3(myData.enemyScale, myData.enemyScale, 1f);
            }
        }

        if (StageUIManager.Instance != null)
        {
            if (StageUIManager.Instance.enemyHpText != null) hpText = StageUIManager.Instance.enemyHpText;
            if (StageUIManager.Instance.enemyHpBarFill != null) hpBarFill = StageUIManager.Instance.enemyHpBarFill;
        }

        UpdateHpUI();
        UpdateStatusUI();
    }

    void Update()
    {
        if (canvasUIGroup != null && enemySprite != null && enemySprite.sprite != null)
        {
            float headTopY = enemySprite.bounds.max.y;
            float centerX = enemySprite.bounds.center.x;

            Vector3 worldPos = new Vector3(centerX, headTopY + uiOffset, transform.position.z);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            canvasUIGroup.position = screenPos;
        }
    }

    public bool IsNextActionAttack()
    {
        return nextAction == EnemyActionType.Attack;
    }

    public void PlayAttackMotion()
    {
        if (!gameObject.activeInHierarchy) return;
        if (motionCoroutine != null) StopCoroutine(motionCoroutine);
        motionCoroutine = StartCoroutine(AttackMotionRoutine());
    }

    public void PlayHitMotion()
    {
        if (!gameObject.activeInHierarchy) return;
        if (motionCoroutine != null) StopCoroutine(motionCoroutine);
        motionCoroutine = StartCoroutine(HitMotionRoutine());
    }

    private IEnumerator AttackMotionRoutine()
    {
        if (enemyAnimator != null) enemyAnimator.SetTrigger("Attack");

        if (enemySprite != null && myData != null)
        {
            if (myData.attackSprite != null) enemySprite.sprite = myData.attackSprite;
            enemySprite.transform.localPosition = originalSpriteLocalPos + new Vector3(attackLungeX, 0f, 0f);
        }

        yield return new WaitForSeconds(motionDuration);

        ResetEnemyVisual();
    }

    private IEnumerator HitMotionRoutine()
    {
        if (enemyAnimator != null) enemyAnimator.SetTrigger("Hit");

        if (enemySprite != null && myData != null)
        {
            if (myData.hitSprite != null) enemySprite.sprite = myData.hitSprite;
            // [수정] useRedFlashOnHit가 켜져 있을 때만 붉은 점멸 적용
            if (useRedFlashOnHit) enemySprite.color = new Color(1f, 0.45f, 0.45f, 1f);
        }

        float elapsed = 0f;
        float shakeDuration = 0.25f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            if (enemySprite != null)
            {
                float shakeX = Random.Range(-0.18f, 0.18f);
                enemySprite.transform.localPosition = originalSpriteLocalPos + new Vector3(shakeX, 0f, 0f);
            }
            yield return null;
        }

        float remain = Mathf.Max(0f, motionDuration - shakeDuration);
        if (remain > 0f) yield return new WaitForSeconds(remain);

        ResetEnemyVisual();
    }

    private void ResetEnemyVisual()
    {
        if (enemySprite != null && myData != null)
        {
            if (myData.enemyImage != null) enemySprite.sprite = myData.enemyImage;
            enemySprite.color = Color.white;
            enemySprite.transform.localPosition = originalSpriteLocalPos;
        }
        motionCoroutine = null;
    }

    public void UpdateStatusUI()
    {
        if (strength > 0)
        {
            if (strengthUIObj != null) strengthUIObj.SetActive(true);
            if (strengthText != null) strengthText.text = strength.ToString();
        }
        else if (strengthUIObj != null) strengthUIObj.SetActive(false);

        if (vulnerable > 0)
        {
            if (vulnerableUIObj != null) vulnerableUIObj.SetActive(true);
            if (vulnerableText != null) vulnerableText.text = vulnerable.ToString();
        }
        else if (vulnerableUIObj != null) vulnerableUIObj.SetActive(false);
    }

    public void GenerateNextIntent()
    {
        currentShield = 0;
        if (vulnerable > 0) vulnerable--;

        int randomChoice = Random.Range(0, 2);
        if (randomChoice == 0)
        {
            nextAction = EnemyActionType.Attack;
            nextActionValue = Random.Range(myData.minAttackDamage, myData.maxAttackDamage + 1);
            if (intentIcon != null) intentIcon.sprite = attackSprite;
        }
        else
        {
            nextAction = EnemyActionType.Defend;
            nextActionValue = Random.Range(myData.minDefendAmount, myData.maxDefendAmount + 1);
            if (intentIcon != null) intentIcon.sprite = defendSprite;
            currentShield += nextActionValue;
        }

        if (intentText != null) intentText.text = nextActionValue.ToString();
        if (intentGroup != null) intentGroup.SetActive(true);

        UpdateHpUI();
        UpdateStatusUI();
    }

    public void ExecuteIntent()
    {
        if (intentGroup != null) intentGroup.SetActive(false);
        switch (nextAction)
        {
            case EnemyActionType.Attack:
                int finalDamage = nextActionValue + strength;
                if (GameManager.Instance != null && GameManager.Instance.vulnerable > 0)
                    finalDamage = Mathf.FloorToInt(finalDamage * 1.5f);
                if (GameManager.Instance != null) GameManager.Instance.TakeDamage(finalDamage);
                break;
            case EnemyActionType.Defend:
                break;
        }
    }

    public void TakeDamage(int damage)
    {
        int actualDamage = damage;
        if (currentShield > 0)
        {
            if (currentShield >= damage) { currentShield -= damage; actualDamage = 0; }
            else { actualDamage = damage - currentShield; currentShield = 0; }
        }

        currentHp -= actualDamage;
        if (currentHp <= 0)
        {
            currentHp = 0;
            // [추가] 적 사망 효과음 재생
            if (sfxPlayer != null) sfxPlayer.PlayDieSound();

            RewardManager rewardManager = FindObjectOfType<RewardManager>();
            if (rewardManager != null) rewardManager.ShowVictoryScreen();
            gameObject.SetActive(false);
        }
        UpdateHpUI();
        UpdateStatusUI();
    }

    private void UpdateHpUI()
    {
        if (myData == null) return;
        if (hpText != null)
        {
            string shieldStr = currentShield > 0 ? $" (DEF: {currentShield})" : "";
            hpText.text = $"HP: {currentHp} / {myData.maxHp}{shieldStr}";
        }
        if (hpBarFill != null && myData.maxHp > 0)
        {
            hpBarFill.fillAmount = (float)currentHp / myData.maxHp;
        }
    }
}