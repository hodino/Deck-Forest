using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
// ▼▼▼ [1. 에러 CS0246 해결] List<>를 사용하기 위해 이 줄이 반드시 필요합니다! ▼▼▼
using System.Collections.Generic;

public class StageUIManager : MonoBehaviour
{
    public static StageUIManager Instance;

    [Header("UI 모드 연결")]
    public GameObject combatUI;
    public GameObject exploreUI;

    [Header("전투 구역 세부 연결")]
    public Transform handAreaTransform;
    public Transform dropZoneTransform;

    [Header("카드 영역 연결")]
    public Transform historyAreaTransform; // 카드 기록 패널을 담을 변수

    [Header("플레이어 상태이상 UI 연결")]
    public GameObject playerStrengthUIObj;
    public TextMeshProUGUI playerStrengthText;
    public GameObject playerVulnerableUIObj;
    public TextMeshProUGUI playerVulnerableText;

    [Header("랜덤 이동 씬 설정")]
    [Tooltip("이동 가능한 전투 씬들의 이름을 이곳에 모두 적어주세요.")]
    public string[] targetScenes;
    private string lastSceneName = "";

    [Header("적 UI 연결")]
    public TextMeshProUGUI enemyHpText;
    public Image enemyHpBarFill; // ★ 이 줄을 새로 추가합니다!

    [Header("맵 기록 패널 UI")]
    public Transform mapHistoryGroup;
    public GameObject mapIconPrefab;

    [Header("맵 아이콘 스프라이트")]
    public Sprite battleIcon;
    public Sprite restIcon;
    public Sprite treasureIcon;

    [Header("미니맵 트래커 연결")]
    public MinimapTracker minimapTracker; // 인스펙터에서 Mappanel을 드래그해서 넣으세요.

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); //

            // 씬 로드 이벤트 구독
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        lastSceneName = "";

        // 1. 파괴 조건: 메인이나 스테이지 선택으로 가면 UI 매니저 삭제
        if (scene.name == "MainScene" || scene.name == "StageSelectScene")
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetSoul(); // 휘발성 재화 삭제
            }
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Destroy(gameObject);
            return;
        }

        // 2. 모드 전환 조건: 전투 씬일 때
        if (scene.name == "BattleScene")
        {
            if (combatUI != null) combatUI.SetActive(true);
            if (exploreUI != null) exploreUI.SetActive(false);
            Debug.Log("UI 전환: 전투 모드");
        }
        // 3. 모드 전환 조건: 탐험(Game) 씬일 때
        else if (scene.name == "GameScene")
        {
            if (combatUI != null) combatUI.SetActive(false);
            if (exploreUI != null) exploreUI.SetActive(true);

            // ▼▼▼ [추가] 스테이지 진입 시 미니맵 초기화 호출 ▼▼▼
            InitMinimapAtStart();
            Debug.Log("UI 전환: 탐험 모드 및 미니맵 초기화");
        }
    }

    // ★ 스테이지 첫 진입 시 미니맵 상태를 설정하는 함수
    void InitMinimapAtStart()
    {
        if (minimapTracker == null) return;

        if (GameManager.Instance != null)
        {
            // 처음 시작이므로 기록 초기화
            GameManager.Instance.passedRooms.Clear();
            GameManager.Instance.currentRoomType = RoomType.Start;

            // 다음에 마주할 방을 미리 예고 (UI 표시용)
            RoomType firstNextRoom = GetRandomRoomType();

            // 트래커 UI 갱신 (왼쪽 숨김, 오른쪽 아이콘 표시)
            minimapTracker.UpdateRoomIcons(firstNextRoom);
        }
    }

    // 랜덤 방 타입을 결정하는 공통 함수
    private RoomType GetRandomRoomType()
    {
        int rand = Random.Range(0, 100);
        if (rand < 70) return RoomType.Battle;
        if (rand < 90) return RoomType.Rest;
        return RoomType.Treasure;
    }

    // ▼▼▼ 매개변수 sceneName을 다시 제거하고 괄호를 비웁니다! ▼▼▼
    public void GenerateAndEnterNextRoom()
    {
        if (GameManager.Instance == null) return;

        // 1. 방 진행 카운터 증가 및 기록
        GameManager.Instance.passedRooms.Add(GameManager.Instance.currentRoomType);
        GameManager.Instance.roomsClearedCount++; // 방 클리어 횟수를 1 올립니다.

        // 2. 미니맵 갱신을 위한 다음 방 결정
        RoomType nextRoom = GetRandomRoomType();
        GameManager.Instance.currentRoomType = nextRoom;

        if (minimapTracker != null)
        {
            minimapTracker.UpdateRoomIcons(nextRoom);
        }

        string sceneToLoad = "";

        // ★★★ 3. 보스 방 출현 분기점! ★★★
        if (GameManager.Instance.roomsClearedCount >= GameManager.Instance.bossEncounterCount)
        {
            // 목표 횟수에 도달했다면 무조건 보스 씬 지정
            sceneToLoad = "BossScene"; // (실제 만드신 보스 씬 이름으로 변경하세요!)
            Debug.Log("보스 스테이지에 진입합니다!");

            // 보스를 깬 후 다음 맵을 위해 카운터 초기화
            GameManager.Instance.roomsClearedCount = 0;
        }
        else
        {
            // 목표 횟수가 아니라면 일반 전투 씬 중 하나를 랜덤으로 뽑음
            if (targetScenes != null && targetScenes.Length > 0)
            {
                List<string> availableScenes = new List<string>();

                foreach (string sceneName in targetScenes)
                {
                    if (sceneName != lastSceneName)
                    {
                        availableScenes.Add(sceneName);
                    }
                }

                if (availableScenes.Count == 0)
                {
                    availableScenes.AddRange(targetScenes);
                }

                int randomIndex = Random.Range(0, availableScenes.Count);
                sceneToLoad = availableScenes[randomIndex];
                lastSceneName = sceneToLoad;
            }
            else
            {
                sceneToLoad = "BattleScene"; // 배열이 비어있을 때의 안전장치
                Debug.LogWarning("StageUIManager에 랜덤 이동할 targetScenes가 등록되지 않았습니다! 기본 BattleScene으로 이동합니다.");
            }

            Debug.Log($"일반 전투 [{sceneToLoad}] 씬으로 이동합니다. (현재 진행: {GameManager.Instance.roomsClearedCount}/{GameManager.Instance.bossEncounterCount})");
        }

        // 4. 결정된 씬으로 이동 실행
        if (SceneTransition.instance != null)
        {
            SceneTransition.instance.LoadSceneWithFade(sceneToLoad);
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    public void UpdatePlayerStatusUI()
    {
        if (GameManager.Instance == null) return;

        int str = GameManager.Instance.strength;
        int vul = GameManager.Instance.vulnerable;

        if (str > 0)
        {
            playerStrengthUIObj.SetActive(true);
            playerStrengthText.text = str.ToString();
        }
        else playerStrengthUIObj.SetActive(false);

        if (vul > 0)
        {
            playerVulnerableUIObj.SetActive(true);
            playerVulnerableText.text = vul.ToString();
        }
        else playerVulnerableUIObj.SetActive(false);
    }

    // 하단 맵 기록 리스트 기능 (필요할 때 사용)
    public void AddRoomToMap(RoomType newRoom)
    {
        if (mapIconPrefab == null || mapHistoryGroup == null) return;

        GameObject iconObj = Instantiate(mapIconPrefab, mapHistoryGroup);
        RectTransform rt = iconObj.GetComponent<RectTransform>();
        if (rt != null) rt.localScale = Vector3.one;

        Image iconImage = iconObj.GetComponent<Image>();
        if (iconImage != null)
        {
            iconImage.color = Color.white;
            switch (newRoom)
            {
                case RoomType.Battle: iconImage.sprite = battleIcon; break;
                case RoomType.Rest: iconImage.sprite = restIcon; break;
                case RoomType.Treasure: iconImage.sprite = treasureIcon; break;
            }
        }
    }

    public void SwitchToCombatMode()
    {
        if (exploreUI != null) exploreUI.SetActive(false);
        if (combatUI != null) combatUI.SetActive(true);
    }

    public void SwitchToExploreMode()
    {
        if (combatUI != null) combatUI.SetActive(false);
        if (exploreUI != null) exploreUI.SetActive(true);
    }
}