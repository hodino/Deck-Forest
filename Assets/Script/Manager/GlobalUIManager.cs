using UnityEngine;

public class GlobalUIManager : MonoBehaviour
{
    public static GlobalUIManager Instance;

    void Awake()
    {
        // 싱글톤 패턴: 게임 전체에 단 하나만 존재하게 만듭니다.
        if (Instance == null)
        {
            Instance = this;
            // 씬이 넘어가도 이 캔버스와 자식들(인벤, 옵션)은 파괴되지 않습니다.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 중복 생성 방지
            Destroy(gameObject);
        }
    }
}