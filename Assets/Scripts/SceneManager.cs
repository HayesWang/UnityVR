using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }

    [Header("场景过渡设置")]
    public float defaultTransitionDelay = 0.5f;

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// 加载指定场景
    /// </summary>
    /// <param name="sceneName">场景名称</param>
    /// <param name="delay">延迟时间(秒)</param>
    public void LoadScene(string sceneName, float delay = -1)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("场景名称不能为空!");
            return;
        }

        // 使用默认延迟或指定延迟
        float actualDelay = delay >= 0 ? delay : defaultTransitionDelay;
        StartCoroutine(LoadSceneRoutine(sceneName, actualDelay));
    }

    /// <summary>
    /// 加载场景协程
    /// </summary>
    private IEnumerator LoadSceneRoutine(string sceneName, float delay)
    {
        // 等待指定延迟
        yield return new WaitForSeconds(delay);

        // 加载目标场景
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}