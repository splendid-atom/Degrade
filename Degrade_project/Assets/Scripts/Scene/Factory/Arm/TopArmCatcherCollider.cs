using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopArmCatcherCollider : MonoBehaviour
{
    public List<GameObject> objectsInside = new List<GameObject>();
    public GameObject lastObjectInside; // 存储最后一个进入的对象
    public ParticleSystem ps;
    public AudioSource audioSource;
    public AudioClip clipTrashAttract;
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    public void EnableCatachingEffect()
    {
        ps.Play();
        audioSource.PlayOneShot(clipTrashAttract);
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("collider:" + other.name);
        // 当对象进入时添加到列表
        if (!objectsInside.Contains(other.gameObject))
        {
            objectsInside.Add(other.gameObject);
            // 更新最后一个进入的对象
            lastObjectInside = other.gameObject;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!objectsInside.Contains(other.gameObject))
        {
            objectsInside.Add(other.gameObject);
            // 更新最后一个进入的对象
            lastObjectInside = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (objectsInside.Contains(other.gameObject))
        {
            objectsInside.Remove(other.gameObject);
            // 如果离开的是最后一个进入的对象，且列表还有其他对象
            if (other.gameObject == lastObjectInside && objectsInside.Count > 0)
            {
                // 将 lastObjectInside 更新为列表中的最后一个对象
                lastObjectInside = objectsInside[objectsInside.Count - 1];
            }
            // 如果列表为空，则清空 lastObjectInside
            else if (objectsInside.Count == 0)
            {
                lastObjectInside = null;
            }
        }
    }

    // 可选：添加一个公共方法来获取最后一个进入的对象
    public GameObject GetLastObjectInside()
    {
        return lastObjectInside;
    }
}
