using UnityEngine;

public class LeafSpawner : MonoBehaviour
{
    [Header("생성기 설정")]
    public GameObject leafPrefab; // 떨어뜨릴 나뭇잎 프리팹
    public float spawnDelay = 0.5f; // 나뭇잎이 생성되는 주기 (0.5초마다 1개씩)

    private float timer;
    private RectTransform parentRect;

    void Start()
    {
        // 나뭇잎이 생성될 부모 캔버스(또는 패널)의 크기 정보를 가져옵니다.
        parentRect = GetComponent<RectTransform>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 설정한 시간이 지날 때마다 나뭇잎을 하나씩 만듭니다.
        if (timer >= spawnDelay)
        {
            timer = 0f;
            SpawnLeaf();
        }
    }

    void SpawnLeaf()
    {
        if (leafPrefab == null || parentRect == null) return;

        // 1. 나뭇잎 프리팹 복사 (현재 패널을 부모로 설정)
        GameObject newLeaf = Instantiate(leafPrefab, parentRect);
        RectTransform leafRect = newLeaf.GetComponent<RectTransform>();

        // 2. 시작 위치 설정
        // X 위치: 부모 패널의 가로 길이 안에서 랜덤하게 지정 (-절반 ~ +절반)
        float randomX = Random.Range(-parentRect.rect.width / 2f, parentRect.rect.width / 2f);

        // Y 위치: 화면 맨 위쪽 (자연스럽게 보이기 위해 패널 높이보다 살짝 더 위(+50)에서 시작)
        float startY = parentRect.rect.height / 2f + 50f;

        leafRect.anchoredPosition = new Vector2(randomX, startY);
    }
}