using UnityEngine;

// 카드의 종류를 정의합니다. (나중에 Status, Buff 등을 여기에 추가하면 됩니다)
public enum CardType
{
    Attack,     // 공격형 (데미지)
    Defense,    // 방어형 (방어도 증가)
    Heal        // 회복형 (체력 회복)
}

[CreateAssetMenu(fileName = "New Card", menuName = "Card/New Card")]
public class CardData : ScriptableObject
{
    [Header("System")]
    public int id;

    [Header("Card Details")]
    public string cardName;
    [TextArea(3, 5)]
    public string cardDescription;
    public Sprite cardImage;
    public int cardCost;

    [Header("Card Function")]
    // ★ 이 카드가 어떤 타입인지 유니티 인스펙터에서 고를 수 있게 됩니다.
    public CardType cardType;

    [Header("Values")]
    public int attackPower;   // Attack 타입일 때 쓸 데미지
    public int defenseAmount; // Defense 타입일 때 얻을 방어도
    public int healAmount;    // Heal 타입일 때 회복할 체력

    [Header("상태이상 효과")]
    public int giveEnemyVulnerable = 0; // 적에게 걸 취약(턴 수)
    public int givePlayerStrength = 0;  // 나에게 걸 근력(수치)

    // ▼▼▼ 새로 추가된 애니메이션 설정란 ▼▼▼
    [Header("애니메이션 설정")]
    [Tooltip("애니메이터에 설정한 Trigger 이름 (예: Slash, Guard, Heal)")]
    public string animTriggerName;

    [Header("상점 가격 설정")]
    public int minShopPrice = 100; // 이 카드가 상점에 나올 때 가질 수 있는 최소 가격
    public int maxShopPrice = 500; // 이 카드가 상점에 나올 때 가질 수 있는 최대 가격

    [Header("연출 설정")]
    public GameObject effectPrefab; // 이 카드를 썼을 때 날아갈 효과 애니메이션 프리팹
    public SoundSetting attackSfx;  // [추가] 카드 공격 시 재생할 전용 타격음 (비워두면 적 기본 피격음 재생)
}
