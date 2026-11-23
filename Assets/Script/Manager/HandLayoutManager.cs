using UnityEngine;
using System.Collections.Generic;

public class HandLayoutManager : MonoBehaviour
{
    [Header("레이아웃 설정")]
    [Tooltip("카드와 카드 사이의 간격")]
    public float cardSpacing = 160f; // 카드 크기에 맞춰 조절하세요

    [Tooltip("손패 정렬 속도")]
    public float layoutMoveSpeed = 10f;

    [Tooltip("카드의 Y축 높이 보정 (양수면 위로, 음수면 아래로)")]
    public float centerY = 0f; // 이 값을 조절해서 카드를 위아래로 움직이세요!

    private List<CardController> cards = new List<CardController>();

    public void AddCard(CardController card)
    {
        if (!cards.Contains(card))
            cards.Add(card);
    }

    public void RemoveCard(CardController card)
    {
        if (cards.Contains(card))
            cards.Remove(card);
    }

    void Update()
    {
        UpdateCardPositions();
    }

    void UpdateCardPositions()
    {
        if (cards.Count == 0) return;

        // 전체 너비 계산 (중앙 정렬을 위해)
        float totalWidth = (cards.Count - 1) * cardSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cards.Count; i++)
        {
            // 목표 X, Y 좌표 계산
            float targetX = startX + (i * cardSpacing);

            // centerY를 사용하여 Y축 위치를 고정
            Vector2 targetPos = new Vector2(targetX, centerY);

            // 카드에게 목표 위치 전달
            cards[i].SetLayoutTarget(targetPos, layoutMoveSpeed);
        }
    }
}