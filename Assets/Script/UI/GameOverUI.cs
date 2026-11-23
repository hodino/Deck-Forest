using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    // 버튼에 연결할 함수
    public void OnRetryButton()
    {
        if (GameManager.Instance != null)
        {
            // 1. 플레이어 체력을 다시 꽉 채워줍니다.
            GameManager.Instance.playerCurrentHp = GameManager.Instance.playerMaxHp;

            // 2. 가방(Inventory) 비우기
            // myCards(실제 보유 데이터)와 myBag(ID 목록)을 모두 비워줍니다.
            // 단, myDeck(덱)과 myStorage(창고), collectionHistory(도감)는 유지합니다.
            GameManager.Instance.myCards.Clear();
            GameManager.Instance.myBag.Clear();

            // 3. 골드 소실 페널티 (예: 현재 골드의 50%를 잃음)
            // 잃고 싶은 양이나 고정 수치가 있다면 아래 공식을 수정하면 됩니다.
            int goldLoss = Mathf.FloorToInt(GameManager.Instance.gold * 0.5f);
            GameManager.Instance.gold -= goldLoss;

            // 골드가 0 미만으로 떨어지지 않게 안전장치
            if (GameManager.Instance.gold < 0)
            {
                GameManager.Instance.gold = 0;
            }

            Debug.Log($"게임 오버 페널티 적용! 가방이 비워지고 {goldLoss} 골드를 잃었습니다.");

            // 4. UI 및 세이브 데이터 즉시 갱신
            GameManager.Instance.UpdateCurrencyUI();
            GameManager.Instance.SaveGameData();
        }

        // 5. 마을(MainScene)로 돌아갑니다.
        // 페이드 효과 매니저가 있다면 그걸 쓰고, 없다면 기본 씬 이동을 씁니다.
        if (GlobalSceneFader.Instance != null)
        {
            GlobalSceneFader.Instance.LoadScene("MainScene");
        }
        else
        {
            SceneManager.LoadScene("MainScene");
        }
    }
}