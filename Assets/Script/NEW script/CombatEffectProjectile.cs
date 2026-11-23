using System.Collections;
using UnityEngine;

public class CombatEffectProjectile : MonoBehaviour
{
    [Header("이펙트 설정")]
    public float travelDuration = 0.45f;   // 플레이어에서 적까지 날아가는 시간(초)
    public bool rotateTowardTarget = false; // 화살/단검처럼 적 방향으로 머리를 회전시킬지 여부
    public float destroyDelayOnHit = 0.1f;  // 적에게 닿은 뒤 이펙트가 사라지기까지 여유 시간

    public void Launch(Vector3 startPos, Vector3 targetPos, float customDuration = -1f)
    {
        transform.position = startPos;

        if (rotateTowardTarget)
        {
            Vector3 dir = (targetPos - startPos).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        float duration = (customDuration > 0f) ? customDuration : travelDuration;
        StartCoroutine(MoveToTargetRoutine(startPos, targetPos, duration));
    }

    private IEnumerator MoveToTargetRoutine(Vector3 startPos, Vector3 targetPos, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;

        if (destroyDelayOnHit > 0f)
        {
            yield return new WaitForSeconds(destroyDelayOnHit);
        }

        Destroy(gameObject);
    }
}