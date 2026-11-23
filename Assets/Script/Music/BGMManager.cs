using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    // 인스펙터에 보이지 않고 코드가 알아서 찾아서 기억할 스피커들
    private AudioSource bgmSource;
    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // ★ 핵심: 내 몸(BGM_Player)에 붙어있는 '모든' AudioSource를 찾습니다.
            AudioSource[] sources = GetComponents<AudioSource>();

            // 첫 번째 스피커는 BGM용으로 사용
            if (sources.Length > 0)
            {
                bgmSource = sources[0];
            }

            // 두 번째 스피커는 SFX용으로 사용
            if (sources.Length > 1)
            {
                sfxSource = sources[1];
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBGM(AudioClip newClip)
    {
        if (bgmSource == null || newClip == null) return;
        if (bgmSource.clip == newClip) return;

        bgmSource.clip = newClip;
        bgmSource.Play();
    }

    // ★ startTime 매개변수가 추가되었습니다.
    public void PlaySFX(AudioClip clip, float startTime = 0f)
    {
        if (sfxSource == null || clip == null) return;

        if (startTime <= 0f)
        {
            // 건너뛸 시간이 없으면 기존처럼 가볍게 재생
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            // ★ 건너뛸 시간이 있다면 1회용 임시 스피커를 만들어 해당 위치부터 재생
            GameObject tempGO = new GameObject("TempSFX_" + clip.name);
            AudioSource tempSource = tempGO.AddComponent<AudioSource>();

            tempSource.clip = clip;
            tempSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup; // 믹서 볼륨 동기화
            tempSource.time = startTime; // 설정한 시간(0.1초 등)부터 시작

            tempSource.Play();

            // 소리가 다 끝나면 1회용 스피커 파괴
            Destroy(tempGO, clip.length - startTime);
        }
    }
}