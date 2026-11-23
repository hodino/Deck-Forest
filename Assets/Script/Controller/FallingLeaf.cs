using UnityEngine;

public class FallingLeaf : MonoBehaviour
{
    [Header("나뭇잎 움직임 설정")]
    public float minFallSpeed = 100f; // 최소 낙하 속도
    public float maxFallSpeed = 300f; // 최대 낙하 속도
    public float minRotationSpeed = -150f; // 최소 회전 속도 (마이너스는 반시계 방향)
    public float maxRotationSpeed = 150f;  // 최대 회전 속도 (플러스는 시계 방향)
    public float lifeTime = 8f; // 화면 밖으로 나가면 삭제될 때까지의 시간 (메모리 최적화)

    private float fallSpeed;
    private float rotationSpeed;
    private RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();

        // 생성될 때마다 속도와 회전 방향을 랜덤하게 결정합니다.
        fallSpeed = Random.Range(minFallSpeed, maxFallSpeed);
        rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);

        // 8초 뒤에 자기 자신을 파괴합니다. (화면에 나뭇잎이 무한히 쌓여 렉 걸리는 것 방지)
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (rect == null) return;

        // 1. 아래로 떨어지기 (Y값을 마이너스로 이동)
        rect.anchoredPosition += new Vector2(0, -fallSpeed * Time.deltaTime);

        // 2. 빙글빙글 회전하기 (Z축을 기준으로 회전)
        rect.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}