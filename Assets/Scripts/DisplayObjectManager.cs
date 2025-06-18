using System.Collections.Generic;
using UnityEngine;

public class DisplayObjectManager : MonoBehaviour
{
    [System.Serializable]
    public class DisplayableObject
    {
        public string objectId;
        public GameObject sceneObject;
    }

    public List<DisplayableObject> displayObjects = new List<DisplayableObject>();
    private GameObject currentObject;
    
    void Start()
    {
        // 初始隐藏所有物体
        HideAllObjects();
    }
    
    // 显示指定ID的物体
    public GameObject ShowObject(string objectId)
    {
        HideAllObjects();
        
        DisplayableObject targetObj = displayObjects.Find(obj => obj.objectId == objectId);
        if (targetObj != null && targetObj.sceneObject != null)
        {
            currentObject = targetObj.sceneObject;
            currentObject.SetActive(true);
            return currentObject;
        }
        
        return null;
    }
    
    // 隐藏所有物体
    public void HideAllObjects()
    {
        foreach (var obj in displayObjects)
        {
            if (obj.sceneObject != null)
            {
                obj.sceneObject.SetActive(false);
            }
        }
        currentObject = null;
    }
    
    // 获取当前显示的物体
    public GameObject GetCurrentObject()
    {
        return currentObject;
    }
}