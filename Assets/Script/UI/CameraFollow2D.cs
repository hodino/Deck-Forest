using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("따라갈 대상")]
    [Tooltip("카메라가 따라갈 대상(플레이어)을 여기에 연결하세요.")]
    public Transform target;

    [Header("카메라 움직임")]
    [Tooltip("카메라가 대상을 따라가는 속도입니다. 높을수록 빠르게 따라갑니다.")]
    public float smoothSpeed = 0.125f;

    [Tooltip("카메라와 대상 사이의 Z축 거리입니다. 2D에서는 보통 -10을 사용합니다.")]
    public float zOffset = -10f;

    void LateUpdate()
    {
        // 1. 대상(target)이 설정되어 있는지 확인합니다.
        if (target == null)
        {
            Debug.LogWarning("카메라가 따라갈 대상을 설정해주세요!");
            return;
        }

        // 2. 목표 위치를 계산합니다.
        // 대상의 x, y 위치는 그대로 가져오고, z 위치는 설정된 zOffset 값을 사용합니다.
        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, zOffset);

        // 3. 부드러운 이동을 위해 Lerp(선형 보간) 함수를 사용합니다.
        // 현재 카메라 위치에서 목표 위치까지 smoothSpeed에 따라 점진적으로 이동합니다.
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 4. 계산된 위치로 카메라를 이동시킵니다.
        transform.position = smoothedPosition;
    }
}