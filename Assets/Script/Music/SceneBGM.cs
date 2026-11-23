using UnityEngine;

public class SceneBGM : MonoBehaviour
{
    [Header("이 씬에서 재생할 배경음악")]
    public AudioClip thisSceneMusic;

    void Start()
    {
        // 1. 인스펙터 빈칸에 음악을 안 넣은 경우
        if (thisSceneMusic == null)
        {
            Debug.LogWarning("⚠️ [원인 1] SceneBGM: 이 씬에 틀어줄 음악(AudioClip) 빈칸이 비어있습니다! 인스펙터를 확인하세요.");
            return;
        }

        // 2. BGM 매니저가 같이 넘어오지 않고 파괴된 경우
        if (BGMManager.Instance == null)
        {
            Debug.LogWarning("⚠️ [원인 2] SceneBGM: BGMManager가 파괴되었습니다! BGM_Player가 다른 오브젝트 안에 들어가 있진 않나요?");
            return;
        }

        // 3. 정상적으로 작동할 때
        Debug.Log($"🎵 배경음악 교체 성공! 새 음악: {thisSceneMusic.name}");
        BGMManager.Instance.PlayBGM(thisSceneMusic);
    }
}