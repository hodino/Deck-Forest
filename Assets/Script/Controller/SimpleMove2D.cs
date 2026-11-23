using UnityEngine;
// 새로운 입력 시스템을 사용하기 위해 꼭 추가해야 합니다!
using UnityEngine.InputSystem;

public class SimpleMove2D : MonoBehaviour
{
    
    public float moveSpeed = 5f; // 이동속도 5F정도로 이동

    void Update()
    {
        // 현재 연결된 키보드 상태 가져오기
        var keyboard = Keyboard.current;
        if (keyboard == null) // 만약에 키보드가 연결되어 있지 않으면 중단
        {
            return;
        }

        // 방향키 입력 감지하기
        // WasPressedThisFrame()은 키가 눌리는 순간 true, IsPressed()는 누르고 있는 동안 true 입니다.
        // 여기서는 누르고 있는 동안 계속 움직여야 해야되서 IsPressed()를 사용합니다.
        Vector2 moveInput = Vector2.zero;
        if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed)
        {
            moveInput.x = -1;
        }
        if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed)
        {
            moveInput.x = 1;
        }
        if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed)
        {
            moveInput.y = 1;
        }
        if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed)
        {
            moveInput.y = -1;
        }

        // 이동 처리
        // Time.deltaTime을 곱해 프레임 속도와 무관하게 이동
        // .normalized로 대각선 이동 속도 보정
        transform.Translate(moveInput.normalized * moveSpeed * Time.deltaTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        // 부딪힌 오브젝트의 태그가 "Enemy"라면
        if (other.CompareTag("Enemy"))
        {
            // 적 오브젝트를 잠시 비활성화 (중복 충돌 방지)
            other.gameObject.SetActive(false);

            // 플레이어 이동을 멈춤 (선택 사항)
            // this.enabled = false; // 스크립트 자체를 비활성화

            // 화면 전환(Fade)을 시작하고 "BattleScene"으로 이동
            // ※ "SceneTransition" 스크립트는 2단계에서 만듭니다.
            SceneTransition.instance.LoadSceneWithFade("BattleScene");
        }
    }
}