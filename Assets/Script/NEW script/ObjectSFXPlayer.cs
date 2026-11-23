using UnityEngine;

// 오디오 파일과 건너뛸 시간을 하나로 묶어주는 세트
[System.Serializable]
public struct SoundSetting
{
    public AudioClip clip;
    [Tooltip("음악 앞부분의 무음 구간을 건너뛸 시간 (초 단위, 예: 0.1)")]
    public float startOffset;
}

public class ObjectSFXPlayer : MonoBehaviour
{
    [Header("상황별 효과음 설정")]
    public SoundSetting clickSound;      // 버튼 클릭 / 상호작용
    public SoundSetting attackSound;     // 공격할 때
    public SoundSetting hitSound;        // 맞았을 때 (피격)
    public SoundSetting dieSound;        // 죽었을 때 (사망)
    public SoundSetting walkSound;       // 걸어갈 때 (발소리)

    // 1. 클릭 / 상호작용 소리 재생
    public void PlayClickSound()
    {
        PlaySound(clickSound);
    }

    // 2. 공격 소리 재생
    public void PlayAttackSound()
    {
        PlaySound(attackSound);
    }

    // 3. 피격 소리 재생
    public void PlayHitSound()
    {
        PlaySound(hitSound);
    }

    // 4. 사망 소리 재생
    public void PlayDieSound()
    {
        PlaySound(dieSound);
    }

    // 5. 발소리 재생
    public void PlayWalkSound()
    {
        PlaySound(walkSound);
    }

    // 실제 소리를 BGMManager로 전달하는 내부 기능
    private void PlaySound(SoundSetting sound)
    {
        if (sound.clip != null && BGMManager.Instance != null)
        {
            // BGMManager로 클립과 건너뛸 시간을 함께 전달합니다.
            BGMManager.Instance.PlaySFX(sound.clip, sound.startOffset);
        }
    }
}