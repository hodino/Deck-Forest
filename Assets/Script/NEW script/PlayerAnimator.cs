using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public static PlayerAnimator Instance;
    private Animator anim;

    void Awake()
    {
        if (Instance == null) Instance = this;

        // 내 몸에 붙어있는 Animator 컴포넌트를 자동으로 찾습니다.
        anim = GetComponent<Animator>();
    }

    // 카드를 쓸 때 외부에서 이 함수를 부릅니다.
    public void PlayCardAnimation(string triggerName)
    {
        if (anim != null && !string.IsNullOrEmpty(triggerName))
        {
            anim.SetTrigger(triggerName);
        }
    }
}