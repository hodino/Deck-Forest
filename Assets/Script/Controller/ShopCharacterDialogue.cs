using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI; // ★ NPC 이미지를 다루기 위해 필수로 추가해야 합니다!

public class ShopCharacterDialogue : MonoBehaviour, IPointerClickHandler
{
    [Header("NPC 비주얼 설정 (새로 추가됨)")]
    [Tooltip("상점 캐릭터의 이미지를 띄워줄 UI Image를 연결하세요.")]
    public Image npcImageUI;
    [Tooltip("대화가 바뀔 때 함께 랜덤으로 보여줄 NPC 이미지(Sprite)들을 넣어주세요.")]
    public Sprite[] npcSprites;

    [Header("말풍선 텍스트 연결")]
    [Tooltip("상점 캐릭터의 대사를 띄울 TextMeshPro UI를 연결하세요.")]
    public TextMeshProUGUI dialogueTextUI;

    [Header("대화 설정")]
    [Tooltip("캐릭터가 중얼거릴 대사들을 여러 개 적어주세요.")]
    [TextArea(2, 3)]
    public string[] randomDialogues;

    [Tooltip("대화가 자동 변경되는 주기 (분 단위). 예: 1.5 = 1분 30초")]
    public float changeIntervalMinutes = 1.0f;

    // 내부 타이머 및 중복 방지 변수
    private float timer = 0f;
    private int lastDialogueIndex = -1;
    private int lastImageIndex = -1; // ★ 이전 이미지 인덱스 저장 (연속 같은 이미지 방지용)

    void OnEnable()
    {
        timer = 0f;
        ChangeDialogue(); // 창이 열리자마자 대사 및 이미지 하나를 띄움
    }

    void Update()
    {
        if (randomDialogues == null || randomDialogues.Length == 0) return;

        // 상점이 열려있을 때 Time.timeScale = 0 이므로 unscaledDeltaTime 사용
        timer += Time.unscaledDeltaTime;

        if (timer >= changeIntervalMinutes * 60f)
        {
            timer = 0f;
            ChangeDialogue();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (randomDialogues == null || randomDialogues.Length == 0) return;

        Debug.Log("캐릭터가 클릭되었습니다! 대사와 이미지를 변경합니다.");

        timer = 0f;
        ChangeDialogue();
    }

    // 실제 대사와 이미지를 바꿔주는 함수
    public void ChangeDialogue()
    {
        // 1. 대사 랜덤 변경 로직
        if (randomDialogues != null && randomDialogues.Length > 0 && dialogueTextUI != null)
        {
            int randomIndex = Random.Range(0, randomDialogues.Length);

            // 대사가 여러 개일 때, 직전 대사와 똑같은 대사가 또 나오는 것 방지
            if (randomDialogues.Length > 1)
            {
                while (randomIndex == lastDialogueIndex)
                {
                    randomIndex = Random.Range(0, randomDialogues.Length);
                }
            }

            lastDialogueIndex = randomIndex;
            dialogueTextUI.text = randomDialogues[randomIndex];
        }

        // 2. ★ 새로 추가된 이미지 랜덤 변경 로직
        if (npcSprites != null && npcSprites.Length > 0 && npcImageUI != null)
        {
            int randomImgIndex = Random.Range(0, npcSprites.Length);

            // 이미지가 여러 개일 때, 직전 이미지와 똑같은 이미지가 또 나오는 것 방지 (대사와 동일한 방식)
            if (npcSprites.Length > 1)
            {
                while (randomImgIndex == lastImageIndex)
                {
                    randomImgIndex = Random.Range(0, npcSprites.Length);
                }
            }

            lastImageIndex = randomImgIndex;
            npcImageUI.sprite = npcSprites[randomImgIndex];
        }
    }
}