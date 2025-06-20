using UnityEngine;
using UnityEngine.SceneManagement; // 确保这个命名空间正确引入

public class start : MonoBehaviour
{
    [Header("场景设置")]
    public string targetSceneName = "YourTargetScene"; // 记得在Inspector中设置实际场景名
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LoadTargetScene();
        }
    }
    
    void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            try
            {
                // 使用正确的API调用
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
            }
            catch (System.Exception e)
            {
                Debug.LogError("加载场景时出错: " + e.Message);
            }
        }
        else
        {
            Debug.LogWarning("目标场景名称未设置！");
        }
    }
}