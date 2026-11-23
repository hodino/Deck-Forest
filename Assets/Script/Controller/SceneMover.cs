using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMover : MonoBehaviour
{
    [Header("이동할 씬 이름을 정확히 적으세요")]
    public string targetSceneName;

    public void MoveToTargetScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log($"{targetSceneName} 씬으로 이동합니다!");

            if (GlobalSceneFader.Instance != null)
            {
                GlobalSceneFader.Instance.LoadScene(targetSceneName);
            }
            else
            {
                SceneManager.LoadScene(targetSceneName);
            }
        }
        else
        {
            Debug.LogError("이동할 씬 이름이 지정되지 않았습니다!");
        }
    }
}