using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

[System.Serializable]
public class SaveData
{
    public int playerCurrentHp;
    public int playerGold;
    public List<int> collectionHistoryIds = new List<int>();

    public List<int> ownedCardIds = new List<int>();

    public List<int> savedDeck = new List<int>();
    public List<int> savedBag = new List<int>();
    public List<int> savedStorage = new List<int>();
}

public enum RoomType
{
    Start, Battle, Rest, Treasure
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("전투 진입 데이터")]
    public EnemyData currentEncounteredEnemy;

    [Header("Game Database")]
    public List<CardData> allCardDatabase = new List<CardData>();

    [Header("Player Data")]
    public List<CardData> myCards = new List<CardData>();

    public HashSet<int> collectionHistory = new HashSet<int>();

    public int playerMaxHp = 100;
    public int playerCurrentHp;
    public int playerMaxCost = 3;

    public int playerCurrentShield = 0;

    [Header("플레이어 상태이상")]
    public int strength = 0;
    public int vulnerable = 0;

    public RoomType currentRoomType = RoomType.Start;
    public List<RoomType> passedRooms = new List<RoomType>();

    // ▼▼▼ [추가] 보스 방 등장 기준 변수 (인스펙터에서 수정 가능) ▼▼▼
    [Header("스테이지 진행 설정")]
    public int roomsClearedCount = 0;      // 현재까지 클리어한 방의 수
    public int bossEncounterCount = 5;     // 보스 방이 등장하는 목표 횟수 (예: 5번)

    [Header("재화 시스템")]
    public int gold = 1000; // ★ 기본 골드를 1000으로 설정
    public int soul;
    [Header("재화 UI 연결")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI soulText;

    [Header("카드 보관 시스템")]
    public List<int> myDeck = new List<int>();
    public List<int> myBag = new List<int>();
    public List<int> myStorage = new List<int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGameData();
        }
        else
        {
            Destroy(gameObject);
        }
        UpdateCurrencyUI();
    }

    public void AddShield(int amount)
    {
        playerCurrentShield += amount;
        Debug.Log($"방어도 {amount} 획득! 현재 방어도: {playerCurrentShield}");
    }

    public void HealPlayer(int amount)
    {
        playerCurrentHp += amount;
        if (playerCurrentHp > playerMaxHp) playerCurrentHp = playerMaxHp;
        Debug.Log($"체력 {amount} 회복! 현재 체력: {playerCurrentHp}");
    }

    public void TakeDamage(int damage)
    {
        if (playerCurrentShield >= damage)
        {
            playerCurrentShield -= damage;
            Debug.Log($"방어도로 공격을 막아냈습니다! 남은 방어도: {playerCurrentShield}");
        }
        else
        {
            int remainingDamage = damage - playerCurrentShield;
            playerCurrentShield = 0;
            playerCurrentHp -= remainingDamage;

            SaveGameData();

            if (playerCurrentHp <= 0)
            {
                Debug.Log("플레이어 사망!");
                GameObject stageUI = GameObject.Find("StageUICanvas");
                if (stageUI != null) stageUI.SetActive(false);
                SceneManager.LoadScene("GameOverScene");
            }
        }
    }

    public void AddCard(CardData newCard)
    {
        myCards.Add(newCard);
        myBag.Add(newCard.id);

        if (!collectionHistory.Contains(newCard.id))
        {
            collectionHistory.Add(newCard.id);
            Debug.Log($"도감에 등록됨: {newCard.cardName}");
        }

        Debug.Log($"카드 획득 (가방에 추가됨): {newCard.cardName}");
        SaveGameData();
    }

    public void SaveGameData()
    {
        SaveData data = new SaveData();

        data.playerCurrentHp = playerCurrentHp;
        data.playerGold = gold;

        foreach (CardData card in myCards) data.ownedCardIds.Add(card.id);
        foreach (int id in collectionHistory) data.collectionHistoryIds.Add(id);

        data.savedDeck = new List<int>(myDeck);
        data.savedBag = new List<int>(myBag);
        data.savedStorage = new List<int>(myStorage);

        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.persistentDataPath, "savefile.json");
        System.IO.File.WriteAllText(path, json);

        Debug.Log($"게임 저장 완료! 위치: {path}");
    }

    public void LoadGameData()
    {
        string path = Path.Combine(Application.persistentDataPath, "savefile.json");

        if (System.IO.File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            playerCurrentHp = data.playerCurrentHp;
            gold = data.playerGold;
            if (playerCurrentHp <= 0) playerCurrentHp = playerMaxHp;

            myCards.Clear();
            foreach (int id in data.ownedCardIds)
            {
                CardData foundCard = allCardDatabase.Find(x => x.id == id);
                if (foundCard != null) myCards.Add(foundCard);
            }

            collectionHistory.Clear();
            foreach (int id in data.collectionHistoryIds) collectionHistory.Add(id);

            myDeck = new List<int>(data.savedDeck);
            myBag = new List<int>(data.savedBag);
            myStorage = new List<int>(data.savedStorage);

            Debug.Log("게임 불러오기 성공!");
        }
        else
        {
            // ▼▼▼ 파일이 없을 때 자동으로 기본템을 세팅하도록 대체합니다 ▼▼▼
            Debug.Log("저장된 파일이 없습니다. 기본템을 지급하고 새로 시작합니다.");
            GiveStarterItems();
        }
    }

    public void UpdateCurrencyUI()
    {
        if (goldText != null) goldText.text = gold.ToString();
        if (soulText != null) soulText.text = soul.ToString();
    }

    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"골드 획득! 현재 골드: {gold}");
        UpdateCurrencyUI();
        SaveGameData();
    }

    public bool UseGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            UpdateCurrencyUI();
            SaveGameData();
            return true;
        }
        Debug.Log("골드가 부족합니다.");
        return false;
    }

    public void AddSoul(int amount)
    {
        soul += amount;
        Debug.Log($"소울 획득! 현재 소울: {soul}");
        UpdateCurrencyUI();
    }

    public void ResetSoul()
    {
        soul = 0;
        Debug.Log("스테이지 종료: 소울이 소멸했습니다.");
        UpdateCurrencyUI();
    }

    public void MoveBagToStorage()
    {
        if (myBag.Count > 0)
        {
            myStorage.AddRange(myBag);
            myBag.Clear();
            Debug.Log("가방의 아이템이 창고로 모두 이동되었습니다.");
        }
    }

    [ContextMenu("Delete Save File")]
    public void DeleteSaveFile()
    {
        string path = Path.Combine(Application.persistentDataPath, "savefile.json");
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("세이브 파일 삭제됨.");
        }
        // ▼▼▼ [수정된 부분] 메모리에 있는 기존 데이터를 완전히 비웁니다 ▼▼▼
        myCards.Clear();
        myDeck.Clear();
        myBag.Clear();
        myStorage.Clear();
        collectionHistory.Clear();

        playerCurrentHp = playerMaxHp;
        gold = 1000;

        UpdateCurrencyUI();
    }

    // ▼▼▼ 새로 추가된 기본템 지급(스타터 팩) 함수 ▼▼▼
    public void GiveStarterItems()
    {
        playerMaxHp = 100;
        playerCurrentHp = playerMaxHp;
        gold = 500;
        playerMaxCost = 5;

        myCards.Clear();
        myDeck.Clear();
        myBag.Clear();
        myStorage.Clear();
        collectionHistory.Clear();

        // ▼▼▼ 수정: 데이터베이스에서 실제 카드를 찾아 myCards에도 넣어줍니다 ▼▼▼
        CardData card1 = allCardDatabase.Find(x => x.id == 1);
        CardData card2 = allCardDatabase.Find(x => x.id == 2);

        for (int i = 0; i < 4; i++)
        {
            myDeck.Add(1);
            if (card1!= null) myCards.Add(card1);
        }
        for (int i = 0; i < 4; i++)
        {
            myDeck.Add(2);
            if (card2 != null) myCards.Add(card2);
        }
        // ▲▲▲ 수정 끝 ▲▲▲

        UpdateCurrencyUI();
        Debug.Log("기본 아이템 및 스타터 덱 지급 완료!");

        SaveGameData();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (goldText != null || soulText != null)
            UpdateCurrencyUI();
    }
#endif
}