using System.Collections;
using UnityEngine;

public class PlayerMotionController : MonoBehaviour
{
    [Header("컴포넌트 연결")]
    public SpriteRenderer playerSprite;
    public Animator playerAnimator;

    [Header("모션 이미지 연결")]
    public Sprite idleSprite;
    public Sprite throwCardSprite;
    public Sprite hitSprite;

    [Header("공격(카드 투척) 모션 설정")]
    public float attackLungeDistance = 1.1f; // 앞으로 돌진하는 거리
    public float attackTiltAngle = -14f;     // 앞으로 기울어지는 각도
    public float attackDuration = 0.45f;     // 전체 공격 모션 시간

    [Header("피격 모션 설정")]
    public float hitKnockbackDistance = 0.7f; // 뒤로 밀려나는 거리
    public float hitTiltAngle = 16f;          // 뒤로 젖혀지는 각도
    public float hitDuration = 0.45f;         // 전체 피격 모션 시간

    private Vector3 originalLocalPos;
    private Vector3 originalLocalScale;
    private Quaternion originalLocalRot;
    private Coroutine activeMotionCoroutine;

    void Awake()
    {
        if (playerSprite == null) playerSprite = GetComponentInChildren<SpriteRenderer>();
        if (playerAnimator == null) playerAnimator = GetComponentInChildren<Animator>();

        if (playerSprite != null)
        {
            originalLocalPos = playerSprite.transform.localPosition;
            originalLocalScale = playerSprite.transform.localScale;
            originalLocalRot = playerSprite.transform.localRotation;

            if (idleSprite == null) idleSprite = playerSprite.sprite;
        }
    }

    // 1. 카드를 날리는 공격 모션
    public void PlayAttackMotion()
    {
        if (!gameObject.activeInHierarchy) return;
        if (activeMotionCoroutine != null) StopCoroutine(activeMotionCoroutine);
        activeMotionCoroutine = StartCoroutine(AttackRoutine());
    }

    // 2. 적에게 공격당했을 때 피격 모션
    public void PlayHitMotion()
    {
        if (!gameObject.activeInHierarchy) return;
        if (activeMotionCoroutine != null) StopCoroutine(activeMotionCoroutine);
        activeMotionCoroutine = StartCoroutine(HitRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        // 대기 애니메이션이 스프라이트를 덮어쓰지 않도록 잠시 비활성화
        if (playerAnimator != null) playerAnimator.enabled = false;
        if (playerSprite == null) yield break;

        Transform tr = playerSprite.transform;

        // [1단계: 예비 동작] 뒤로 살짝 젖히며 움츠림 (0.1초)
        float windupTime = 0.1f;
        float elapsed = 0f;
        while (elapsed < windupTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / windupTime;
            tr.localPosition = Vector3.Lerp(originalLocalPos, originalLocalPos + new Vector3(-0.25f, 0f, 0f), t);
            tr.localRotation = Quaternion.Lerp(originalLocalRot, Quaternion.Euler(0f, 0f, 8f), t);
            tr.localScale = Vector3.Lerp(originalLocalScale, new Vector3(originalLocalScale.x * 0.9f, originalLocalScale.y * 1.08f, originalLocalScale.z), t);
            yield return null;
        }

        // 투척 이미지(char_att_01)로 교체
        if (throwCardSprite != null) playerSprite.sprite = throwCardSprite;

        // [2단계: 투척 돌진] 앞으로 빠르게 나아가며 기울어짐 (0.12초)
        float lungeTime = 0.12f;
        elapsed = 0f;
        Vector3 startLungePos = tr.localPosition;
        Quaternion startLungeRot = tr.localRotation;
        Vector3 startLungeScale = tr.localScale;

        Vector3 targetLungePos = originalLocalPos + new Vector3(attackLungeDistance, 0.08f, 0f);
        Quaternion targetLungeRot = Quaternion.Euler(0f, 0f, attackTiltAngle);
        Vector3 targetLungeScale = new Vector3(originalLocalScale.x * 1.12f, originalLocalScale.y * 0.92f, originalLocalScale.z);

        while (elapsed < lungeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lungeTime;
            tr.localPosition = Vector3.Lerp(startLungePos, targetLungePos, t);
            tr.localRotation = Quaternion.Lerp(startLungeRot, targetLungeRot, t);
            tr.localScale = Vector3.Lerp(startLungeScale, targetLungeScale, t);
            yield return null;
        }

        // [3단계: 복귀] 부드럽게 원래 자세로 돌아옴
        float returnTime = Mathf.Max(0.15f, attackDuration - windupTime - lungeTime);
        elapsed = 0f;
        while (elapsed < returnTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnTime;
            tr.localPosition = Vector3.Lerp(targetLungePos, originalLocalPos, t);
            tr.localRotation = Quaternion.Lerp(targetLungeRot, originalLocalRot, t);
            tr.localScale = Vector3.Lerp(targetLungeScale, originalLocalScale, t);
            yield return null;
        }

        ResetVisuals();
    }

    private IEnumerator HitRoutine()
    {
        // 대기 애니메이션이 피격 스프라이트를 덮어쓰지 않도록 잠시 비활성화
        if (playerAnimator != null) playerAnimator.enabled = false;
        if (playerSprite == null) yield break;

        Transform tr = playerSprite.transform;

        // 피격 이미지(char_hit_01)로 교체 및 붉은 점멸
        if (hitSprite != null) playerSprite.sprite = hitSprite;
        playerSprite.color = new Color(1f, 0.35f, 0.35f, 1f);

        // [1단계: 넉백 + 흔들림] 뒤로 밀려나며 흔들림 (0.22초)
        float knockbackTime = 0.22f;
        float elapsed = 0f;
        Vector3 knockbackPos = originalLocalPos + new Vector3(-hitKnockbackDistance, 0.05f, 0f);
        Quaternion knockbackRot = Quaternion.Euler(0f, 0f, hitTiltAngle);
        Vector3 squashScale = new Vector3(originalLocalScale.x * 0.88f, originalLocalScale.y * 1.1f, originalLocalScale.z);

        while (elapsed < knockbackTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / knockbackTime;
            float shakeY = Random.Range(-0.08f, 0.08f);

            tr.localPosition = Vector3.Lerp(originalLocalPos, knockbackPos, t) + new Vector3(0f, shakeY, 0f);
            tr.localRotation = Quaternion.Lerp(originalLocalRot, knockbackRot, t);
            tr.localScale = Vector3.Lerp(originalLocalScale, squashScale, t);
            yield return null;
        }

        // [2단계: 자세 회복] 원래 위치와 색상으로 복귀
        float recoverTime = Mathf.Max(0.15f, hitDuration - knockbackTime);
        elapsed = 0f;
        Color hitColor = playerSprite.color;

        while (elapsed < recoverTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / recoverTime;

            tr.localPosition = Vector3.Lerp(knockbackPos, originalLocalPos, t);
            tr.localRotation = Quaternion.Lerp(knockbackRot, originalLocalRot, t);
            tr.localScale = Vector3.Lerp(squashScale, originalLocalScale, t);
            playerSprite.color = Color.Lerp(hitColor, Color.white, t);
            yield return null;
        }

        ResetVisuals();
    }

    private void ResetVisuals()
    {
        if (playerSprite != null)
        {
            if (idleSprite != null) playerSprite.sprite = idleSprite;
            playerSprite.color = Color.white;
            playerSprite.transform.localPosition = originalLocalPos;
            playerSprite.transform.localScale = originalLocalScale;
            playerSprite.transform.localRotation = originalLocalRot;
        }

        // 대기 애니메이션 다시 활성화
        if (playerAnimator != null) playerAnimator.enabled = true;
        activeMotionCoroutine = null;
    }
}