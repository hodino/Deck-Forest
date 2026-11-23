using UnityEngine;

// 유니티 우클릭 메뉴에서 쉽게 적 데이터를 생성할 수 있게 해줍니다.
[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy/New Enemy")]
public class EnemyData : ScriptableObject
{
    [Header("기본 정보")]
    public string enemyName;      // 적 이름
    public Sprite enemyImage;     // ★ 적의 외형 이미지

    [Header("크기 설정")]
    public float enemyScale = 1.0f; // 1.0이 기본 크기, 작게 하려면 0.6 ~ 0.7 입력

    [Header("전투 스탯")]
    public int maxHp;             // 최대 체력

    [Header("행동 패턴 수치")]
    public int minAttackDamage;   // 최소 공격력
    public int maxAttackDamage;   // 최대 공격력
    public int minDefendAmount;   // 최소 방어량
    public int maxDefendAmount;   // 최대 방어량

    [Header("전투 모션 이미지")]
    public Sprite attackSprite; // 적이 공격할 때 바뀔 이미지
    public Sprite hitSprite;    // 적이 공격당했을 때(피격) 바뀔 이미지

    [Header("보스 설정")]
    public bool isBoss = false; // ★ 이 몬스터가 보스인지 체크하는 용도
}