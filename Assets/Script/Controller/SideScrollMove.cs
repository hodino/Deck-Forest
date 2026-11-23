using UnityEngine;
using UnityEngine.InputSystem;

public class SideScrollMove : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;

    [Header("애니메이션 설정")]
    public Animator animator;
    private bool isFacingRight = true;

    [Header("발소리 설정 (연속 음원용)")]
    public ObjectSFXPlayer sfxPlayer;
    public AudioSource walkAudioSource;
    [Range(0f, 1f)] public float walkVolume = 1f;

    private bool isWalkAudioPaused = false;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (sfxPlayer == null)
        {
            sfxPlayer = GetComponent<ObjectSFXPlayer>();
        }

        if (walkAudioSource == null)
        {
            walkAudioSource = gameObject.AddComponent<AudioSource>();
        }

        walkAudioSource.playOnAwake = false;
        walkAudioSource.loop = true;
        walkAudioSource.volume = walkVolume;

        if (walkAudioSource.clip == null && sfxPlayer != null && sfxPlayer.walkSound.clip != null)
        {
            walkAudioSource.clip = sfxPlayer.walkSound.clip;
            if (sfxPlayer.walkSound.startOffset > 0f && sfxPlayer.walkSound.startOffset < walkAudioSource.clip.length)
            {
                walkAudioSource.time = sfxPlayer.walkSound.startOffset;
            }
        }
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        float horizontalInput = 0f;

        if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed)
        {
            horizontalInput = -1f;
        }
        else if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed)
        {
            horizontalInput = 1f;
        }

        Vector3 moveVector = new Vector3(horizontalInput, 0, 0) * moveSpeed * Time.deltaTime;
        transform.Translate(moveVector);

        bool isMoving = horizontalInput != 0;

        if (animator != null)
        {
            animator.SetBool("isWalk", isMoving);
        }

        if (walkAudioSource != null && walkAudioSource.clip != null)
        {
            walkAudioSource.volume = walkVolume;

            if (isMoving)
            {
                if (!walkAudioSource.isPlaying)
                {
                    if (isWalkAudioPaused)
                    {
                        walkAudioSource.UnPause();
                        isWalkAudioPaused = false;
                    }
                    else
                    {
                        walkAudioSource.Play();
                    }
                }
            }
            else
            {
                if (walkAudioSource.isPlaying)
                {
                    walkAudioSource.Pause();
                    isWalkAudioPaused = true;
                }
            }
        }

        if (horizontalInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0 && isFacingRight)
        {
            Flip();
        }
    }

    void OnDisable()
    {
        if (walkAudioSource != null && walkAudioSource.isPlaying)
        {
            walkAudioSource.Pause();
            isWalkAudioPaused = true;
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;
    }
}
