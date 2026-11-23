using UnityEngine;

// 이 스크립트를 'Turn End' 버튼에 직접 붙여줍니다.
public class TurnButton : MonoBehaviour
{
    // 버튼의 OnClick()에 연결할 함수
    public void ClickEndTurn()
    {
        // 씬 안에 살아있는 BattleManager를 자동으로 찾아서 턴 종료 함수를 실행!
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnEndTurnButton();
            Debug.Log("턴 종료 버튼 눌림!");
        }
        else
        {
            Debug.LogWarning("BattleManager를 찾을 수 없습니다.");
        }
    }
}