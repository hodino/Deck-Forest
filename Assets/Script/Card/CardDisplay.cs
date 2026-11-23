using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용하려면 이 네임스페이스가 필요합니다.

public class CardDisplay : MonoBehaviour
{
    // 프리팹의 UI 요소들을 여기에 연결합니다.
    // Unity 에디터에서 프리팹을 선택하고 Inspector 창에서 드래그 앤 드롭하세요.

    [Header("UI Elements")]
    public TextMeshProUGUI nameText;        // 카드 이름 텍스트 (TMP)
    public Image artworkImage;              // 카드 이미지
    public TextMeshProUGUI descriptionText; // 카드 설명 텍스트 (TMP)
    public TextMeshProUGUI costText;        // 카드 비용 텍스트 (TMP)

    // (참고: 만약 TextMeshPro(TMP) 대신 기본 Text를 사용한다면)
    // public Text nameText;
    // public Text descriptionText;
    // public Text costText;

    /// <summary>
    /// 카드 데이터를 받아와서 UI를 설정합니다.
    /// </summary>
    /// <param name="card">표시할 카드 데이터</param>
    public void Setup(CardData card)
    {
        if (card == null) return;

        // 위에서부터 순서대로 데이터를 UI에 할당
        nameText.text = card.cardName;
        artworkImage.sprite = card.cardImage;
        descriptionText.text = card.cardDescription;
        costText.text = card.cardCost.ToString(); // int를 string으로 변환
    }
}