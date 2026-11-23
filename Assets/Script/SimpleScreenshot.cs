using UnityEngine;
using System.IO;

public class SimpleScreenshot : MonoBehaviour
{
    [Header("캡처 단축키 (기본값: P)")]
    public KeyCode captureKey = KeyCode.P;

    private void Start()
    {
        // 게임 시작 시 콘솔창에 이 메시지가 떠야 스크립트가 정상 작동 중인 것입니다.
        Debug.Log("📸 스크린샷 준비 완료! 누를 키: " + captureKey);
    }

    private void Update()
    {
        if (Input.GetKeyDown(captureKey))
        {
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            // Assets 폴더 최상단에 바로 저장합니다.
            string fileName = $"Screenshot_{timestamp}.png";
            string savePath = Path.Combine(Application.dataPath, fileName);

            ScreenCapture.CaptureScreenshot(savePath);

            // 캡처 성공 시 콘솔에 저장 경로를 띄웁니다.
            Debug.Log($"✅ 캡처 성공! 파일 위치: {savePath}");
        }
    }
}