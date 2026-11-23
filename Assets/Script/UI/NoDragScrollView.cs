using UnityEngine;
using UnityEngine.UI; // UI 기능을 가져옵니다
using UnityEngine.EventSystems; // 이벤트 처리를 가져옵니다

// 기존 ScrollRect를 상속받아서 드래그 기능만 무력화시키는 스크립트입니다.
public class NoDragScrollView : ScrollRect
{
    // 드래그 시작 이벤트를 받지만, 아무것도 하지 않음 (기능 무시)
    public override void OnBeginDrag(PointerEventData eventData) { }

    // 드래그 중 이벤트를 받지만, 아무것도 하지 않음
    public override void OnDrag(PointerEventData eventData) { }

    // 드래그 종료 이벤트를 받지만, 아무것도 하지 않음
    public override void OnEndDrag(PointerEventData eventData) { }
}