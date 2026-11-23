using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("연결 필요")]
    public Transform handAreaTransform;
    public Transform dropZoneTransform;
    public Transform historyAreaTransform;
    public GameObject cardPrefab;
    public EnemyController enemy;
    public PlayerController player;
    public PlayerMotionController playerMotion;

    [Header("효과음 연결 (Player/Enemy에 붙이면 자동 연결)")]
    public ObjectSFXPlayer playerSfx;
    public ObjectSFXPlayer enemySfx;

    [Header("★ UI 카드 날리기 연출 설정 (CardThrow_A/B/C)")]
    public GameObject uiFlyingCardPrefab;
    public RectTransform playerUIFirePoint;

    [Header("전투 연출(이펙트) 설정")]
    public Transform playerFirePoint;
    public Transform enemyHitPoint;
    public GameObject defaultCardEffectPrefab;
    public float effectTravelDuration = 0.45f;

    public GameObject gameOverPanel;
    public GameObject stageUICanvas;
    public GameObject playerHand;

    [Header("설정")]
    public int startingHandSize = 5;
    public int maxHandSize = 5;
    public int maxHistoryCount = 3;
    public float historyCardScale = 0.8f;
    public int baseDamage = 5;

    private List<CardData> drawDeck = new List<CardData>();
    private List<CardData> playedCardsThisTurn = new List<CardData>();
    private HandLayoutManager handLayoutManager;
    private int currentTurnDamage = 0;
    public bool isTurnProcessing = false;

    [Header("코스트 관리")]
    public int currentCost;
    public int currentTurnMaxCost = 1;
    public int absoluteMaxCost = 10;

    void Awake()
    {
        Instance = this;

        if (StageUIManager.Instance != null)
        {
            handAreaTransform = StageUIManager.Instance.handAreaTransform;
            dropZoneTransform = StageUIManager.Instance.dropZoneTransform;
            historyAreaTransform = StageUIManager.Instance.historyAreaTransform;
        }

        if (handAreaTransform != null)
        {
            handLayoutManager = handAreaTransform.GetComponent<HandLayoutManager>();
        }

        if (playerMotion == null && player != null)
        {
            playerMotion = player.GetComponent<PlayerMotionController>();
        }

        if (playerSfx == null && player != null)
        {
            playerSfx = player.GetComponent<ObjectSFXPlayer>();
        }
        if (enemySfx == null && enemy != null)
        {
            enemySfx = enemy.GetComponent<ObjectSFXPlayer>();
        }
    }

    void Start()
    {
        currentTurnMaxCost = 1;

        if (StageUIManager.Instance != null)
        {
            handAreaTransform = StageUIManager.Instance.handAreaTransform;
            dropZoneTransform = StageUIManager.Instance.dropZoneTransform;
            historyAreaTransform = StageUIManager.Instance.historyAreaTransform;
        }

        if (handAreaTransform != null)
        {
            handLayoutManager = handAreaTransform.GetComponent<HandLayoutManager>();
        }
        else
        {
            Debug.LogError("손패 영역을 찾지 못했습니다! 첫 씬부터 실행했는지 확인해주세요.");
            return;
        }

        if (StageUIManager.Instance != null)
        {
            StageUIManager.Instance.SwitchToCombatMode();
        }

        drawDeck.Clear();
        if (GameManager.Instance != null && GameManager.Instance.myDeck.Count > 0)
        {
            foreach (int cardId in GameManager.Instance.myDeck)
            {
                CardData data = GameManager.Instance.allCardDatabase.Find(x => x.id == cardId);
                if (data != null)
                {
                    drawDeck.Add(data);
                }
            }
        }
        else
        {
            Debug.LogWarning("덱이 비어있습니다! 임시 카드를 채웁니다.");
            for (int i = 0; i < 10; i++) drawDeck.Add(new CardData());
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerCurrentShield = 0;
            GameManager.Instance.strength = 0;
            GameManager.Instance.vulnerable = 0;
        }

        if (handAreaTransform != null)
        {
            foreach (Transform child in handAreaTransform)
            {
                Destroy(child.gameObject);
            }
        }

        if (historyAreaTransform != null)
        {
            foreach (Transform child in historyAreaTransform)
            {
                Destroy(child.gameObject);
            }
        }

        ShuffleDeck();
        StartCoroutine(DrawStartingHand());
        StartTurn();
    }

    void StartTurn()
    {
        isTurnProcessing = false;
        currentTurnDamage = 0;
        playedCardsThisTurn.Clear();

        if (historyAreaTransform != null)
        {
            foreach (Transform child in historyAreaTransform)
            {
                Destroy(child.gameObject);
            }
        }

        if (GameManager.Instance != null && GameManager.Instance.vulnerable > 0)
        {
            GameManager.Instance.vulnerable--;
        }

        if (currentTurnMaxCost < absoluteMaxCost)
        {
            currentTurnMaxCost++;
        }

        currentCost = currentTurnMaxCost;
        UpdateCostUI();
        UpdateAllCardsUsability();

        if (enemy != null)
        {
            enemy.GenerateNextIntent();
        }

        if (StageUIManager.Instance != null)
        {
            StageUIManager.Instance.UpdatePlayerStatusUI();
        }

        Debug.Log($"=== ⚔️ 나의 턴 시작 (코스트: {currentCost}/{currentTurnMaxCost}) ===");
    }

    public void OnCardPlayed(CardController card)
    {
        if (isTurnProcessing) return;

        if (card.myData != null)
        {
            if (currentCost < card.myData.cardCost)
            {
                Debug.LogWarning("코스트가 부족하여 카드를 낼 수 없습니다!");
                return;
            }

            currentCost -= card.myData.cardCost;
            UpdateCostUI();
            UpdateAllCardsUsability();

            playedCardsThisTurn.Add(card.myData);

            switch (card.myData.cardType)
            {
                case CardType.Attack:
                    currentTurnDamage += card.myData.attackPower;
                    Debug.Log($"[공격] 누적 데미지: {currentTurnDamage}");
                    break;

                case CardType.Defense:
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.AddShield(card.myData.defenseAmount);
                    }
                    break;

                case CardType.Heal:
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.HealPlayer(card.myData.healAmount);
                    }
                    break;
            }

            if (card.myData.giveEnemyVulnerable > 0 && enemy != null)
            {
                enemy.vulnerable += card.myData.giveEnemyVulnerable;
                enemy.UpdateStatusUI();
                Debug.Log($"적에게 취약 {card.myData.giveEnemyVulnerable} 부여!");
            }

            if (card.myData.givePlayerStrength > 0 && GameManager.Instance != null)
            {
                GameManager.Instance.strength += card.myData.givePlayerStrength;
                if (StageUIManager.Instance != null)
                {
                    StageUIManager.Instance.UpdatePlayerStatusUI();
                }
                Debug.Log($"플레이어 근력 {card.myData.givePlayerStrength} 획득!");
            }

            if (historyAreaTransform != null && cardPrefab != null)
            {
                while (historyAreaTransform.childCount >= maxHistoryCount)
                {
                    Transform oldestCard = historyAreaTransform.GetChild(0);
                    oldestCard.SetParent(null);
                    Destroy(oldestCard.gameObject);
                }

                GameObject historyCardObj = Instantiate(cardPrefab, historyAreaTransform);
                historyCardObj.transform.localPosition = Vector3.zero;
                historyCardObj.transform.localRotation = Quaternion.identity;

                CardController historyCard = historyCardObj.GetComponent<CardController>();
                if (historyCard != null)
                {
                    historyCard.Setup(card.myData);
                    historyCard.SetHistoryMode();
                }
            }

            Debug.Log($"코스트 {card.myData.cardCost} 사용. 남은 코스트: {currentCost}");
        }

        StartCoroutine(CardPlaySequence(card));
    }

    IEnumerator CardPlaySequence(CardController card)
    {
        if (handLayoutManager != null) handLayoutManager.RemoveCard(card);

        card.transform.SetParent(dropZoneTransform);
        card.SetInteractable(false);
        card.MoveToCenterEffect(Vector2.zero);

        yield return new WaitForSeconds(1.0f);
        yield return StartCoroutine(card.FadeOut(0.5f));

        Destroy(card.gameObject);
        DrawCard();

        UpdateAllCardsUsability();
    }

    public void OnEndTurnButton()
    {
        if (isTurnProcessing) return;
        StartCoroutine(ProcessTurnSequence());
    }

    IEnumerator ProcessTurnSequence()
    {
        isTurnProcessing = true;

        Vector3 startPos = (playerFirePoint != null) ? playerFirePoint.position :
                           (player != null ? player.transform.position : Vector3.zero);
        Vector3 targetPos = (enemyHitPoint != null) ? enemyHitPoint.position :
                            (enemy != null ? enemy.transform.position : Vector3.zero);

        // 이번 턴에 사용한 카드가 있을 때만 카드 투척 애니메이션 및 타격 연출 실행 (빈 하얀 사각형 방지)
        if (playedCardsThisTurn.Count > 0)
        {
            List<GameObject> effectsToLaunch = new List<GameObject>();
            List<Sprite> cardSpritesToLaunch = new List<Sprite>();
            List<SoundSetting> cardHitSounds = new List<SoundSetting>();

            foreach (CardData cardData in playedCardsThisTurn)
            {
                GameObject prefab = (cardData.effectPrefab != null) ? cardData.effectPrefab : defaultCardEffectPrefab;
                effectsToLaunch.Add(prefab);
                cardSpritesToLaunch.Add(cardData.cardImage);
                cardHitSounds.Add(cardData.attackSfx);
            }

            for (int i = 0; i < effectsToLaunch.Count; i++)
            {
                if (playerMotion != null)
                {
                    playerMotion.PlayAttackMotion();
                }
                if (playerSfx != null)
                {
                    playerSfx.PlayAttackSound();
                }

                float waitDuration = effectTravelDuration;

                if (uiFlyingCardPrefab != null && playerUIFirePoint != null && enemy != null)
                {
                    GameObject uiCardObj = Instantiate(uiFlyingCardPrefab, playerUIFirePoint);
                    RectTransform rect = uiCardObj.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.anchoredPosition = Vector2.zero;
                        rect.localScale = Vector3.one;
                        rect.localRotation = Quaternion.identity;
                    }

                    // CardDisplay가 붙어있으면 카드 틀+무기+이름+코스트 전체를 세팅
                    CardDisplay display = uiCardObj.GetComponent<CardDisplay>();
                    if (display != null && i < playedCardsThisTurn.Count)
                    {
                        display.Setup(playedCardsThisTurn[i]);
                    }
                    else
                    {
                        Sprite targetSprite = cardSpritesToLaunch[i];
                        if (targetSprite != null)
                        {
                            Image cardImg = uiCardObj.GetComponent<Image>();
                            if (cardImg == null) cardImg = uiCardObj.GetComponentInChildren<Image>();
                            if (cardImg != null)
                            {
                                cardImg.sprite = targetSprite;
                            }
                        }
                    }

                    Animator anim = uiCardObj.GetComponent<Animator>();
                    if (anim != null)
                    {
                        int randomType = Random.Range(0, 3);
                        anim.SetInteger("ThrowType", randomType);
                    }

                    Destroy(uiCardObj, 0.52f);
                    waitDuration = 0.4f;
                }
                else
                {
                    GameObject effectPrefab = effectsToLaunch[i];
                    if (effectPrefab != null && enemy != null)
                    {
                        GameObject effectObj = Instantiate(effectPrefab, startPos, Quaternion.identity);
                        CombatEffectProjectile projectile = effectObj.GetComponent<CombatEffectProjectile>();
                        if (projectile != null)
                        {
                            projectile.Launch(startPos, targetPos, effectTravelDuration);
                        }
                        else
                        {
                            StartCoroutine(FallbackMoveEffect(effectObj, startPos, targetPos, effectTravelDuration));
                        }
                    }
                }

                yield return new WaitForSeconds(waitDuration);

                if (enemy != null)
                {
                    enemy.PlayHitMotion();
                }

                SoundSetting customSound = cardHitSounds[i];
                if (customSound.clip != null && BGMManager.Instance != null)
                {
                    BGMManager.Instance.PlaySFX(customSound.clip, customSound.startOffset);
                }
                else if (enemySfx != null)
                {
                    enemySfx.PlayHitSound();
                }

                if (i < effectsToLaunch.Count - 1)
                {
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }

        // 실제 데미지 적용 (조장님 기준 로직 유지)
        int totalDamage = baseDamage + currentTurnDamage;
        if (enemy != null && totalDamage > 0)
        {
            int finalDamage = totalDamage + (GameManager.Instance != null ? GameManager.Instance.strength : 0);

            if (enemy.vulnerable > 0)
            {
                finalDamage = Mathf.FloorToInt(finalDamage * 1.5f);
                Debug.Log($"적 취약 적용! 1.5배 데미지: {finalDamage}");
            }

            if (enemy.currentHp <= finalDamage)
            {
                yield return new WaitForSeconds(0.35f);
            }

            enemy.TakeDamage(finalDamage);
        }

        yield return new WaitForSeconds(1.0f);

        // 적이 살아있다면 적의 턴 행동
        if (enemy != null && enemy.currentHp > 0)
        {
            if (enemy.IsNextActionAttack())
            {
                enemy.PlayAttackMotion();
                if (enemySfx != null) enemySfx.PlayAttackSound();

                yield return new WaitForSeconds(0.35f);

                if (playerMotion != null)
                {
                    playerMotion.PlayHitMotion();
                }
                if (playerSfx != null) playerSfx.PlayHitSound();

                enemy.ExecuteIntent();
            }
            else
            {
                enemy.ExecuteIntent();
            }

            if (StageUIManager.Instance != null)
            {
                StageUIManager.Instance.UpdatePlayerStatusUI();
            }
        }

        yield return new WaitForSeconds(0.8f);
        StartTurn();
    }

    private IEnumerator FallbackMoveEffect(GameObject obj, Vector3 startPos, Vector3 targetPos, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration && obj != null)
        {
            elapsed += Time.deltaTime;
            obj.transform.position = Vector3.Lerp(startPos, targetPos, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        if (obj != null) Destroy(obj);
    }

    void UpdateCostUI()
    {
        if (player != null)
        {
            player.UpdateCostUI(currentCost, currentTurnMaxCost);
        }
        else if (PlayerController.Instance != null)
        {
            PlayerController.Instance.UpdateCostUI(currentCost, currentTurnMaxCost);
        }
        else
        {
            Debug.LogWarning("PlayerController를 찾을 수 없어 코스트 UI를 업데이트할 수 없습니다.");
        }
    }

    void UpdateAllCardsUsability()
    {
        if (handAreaTransform == null) return;

        CardController[] cards = handAreaTransform.GetComponentsInChildren<CardController>();
        foreach (CardController card in cards)
        {
            card.UpdateUsability(currentCost);
        }
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < drawDeck.Count; i++)
        {
            CardData temp = drawDeck[i];
            int randomIndex = Random.Range(i, drawDeck.Count);
            drawDeck[i] = drawDeck[randomIndex];
            drawDeck[randomIndex] = temp;
        }
    }

    IEnumerator DrawStartingHand()
    {
        for (int i = 0; i < startingHandSize; i++)
        {
            DrawCard();
            yield return new WaitForSeconds(0.2f);
        }
        UpdateAllCardsUsability();
    }

    public void DrawCard()
    {
        if (handAreaTransform != null && handAreaTransform.childCount >= maxHandSize)
        {
            Debug.Log($"손패가 가득 차서(최대 {maxHandSize}장) 카드를 더 뽑지 않습니다.");
            return;
        }

        if (drawDeck.Count <= 0) return;
        CardData data = drawDeck[0];
        drawDeck.RemoveAt(0);

        GameObject newCardObj = Instantiate(cardPrefab, handAreaTransform);
        newCardObj.transform.localPosition = Vector3.zero;
        newCardObj.transform.localScale = Vector3.one;

        CardController newCard = newCardObj.GetComponent<CardController>();
        newCard.Setup(data);

        if (handLayoutManager != null) handLayoutManager.AddCard(newCard);

        newCard.UpdateUsability(currentCost);
    }
}