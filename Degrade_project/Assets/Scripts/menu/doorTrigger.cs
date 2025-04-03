using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doorTrigger : MonoBehaviour
{
    public static doorTrigger doortrigger;
    public bool doorcontrol = false;
    public bool trickControl = false;
    private BoxCollider2D triggerCollider;
    private GameObject player;
    private GameObject trick;
    public bool isTrickRaised = false;
    public bool isFallen = false;
    private bool cameraSwitch;
    public GameObject door;
    public Animation doorOpen;

    private bool isSwitching = false; // 标记是否正在切换
    private float switchDelay = 0.2f; // 切换延迟时间

    void Awake()
    {
        if (doortrigger == null)
        {
            doortrigger = this;
        }
    }

    void Start()
    {
        cameraSwitch = false;
        player = GameObject.Find("PlayerCharacter");
        trick = GameObject.Find("trick");
        triggerCollider = gameObject.GetComponent<BoxCollider2D>();
        doorOpen = door.GetComponent<Animation>();
    }

    void Update()
    {
        if (doorcontrol == true && !isTrickRaised)
        {
            float newZ = Mathf.Lerp(trick.transform.position.z, 0, Time.deltaTime);
            Vector3 newPosition = trick.transform.position;
            newPosition.z = newZ;
            trick.transform.position = newPosition;
            if (!isTrickRaised && Mathf.Abs(trick.transform.position.z) < 0.5f)
            {
                isTrickRaised = true;
            }
        }
        if (trickControl == true && !isFallen)
        {
            float newZ = Mathf.Lerp(trick.transform.position.z, 3f, Time.deltaTime);
            Vector3 newPosition = trick.transform.position;
            newPosition.z = newZ;
            trick.transform.position = newPosition;
            if (!isFallen && Mathf.Abs(trick.transform.position.z - 3f) < 0.5f)
            {
                isFallen = true;
                GameObject.Find("trick").SetActive(false);
                doorOpen.Play("door");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == player && !isFallen)
        {
            doorcontrol = true;
            Debug.Log("Player entered the trigger zone.");
        }
        else if (other.gameObject == player && isFallen && !cameraSwitch && !isSwitching)
        {
            StartCoroutine(SwitchCamera(true));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == player && isFallen && cameraSwitch && !isSwitching)
        {
            StartCoroutine(SwitchCamera(false));
        }
    }

    // 延迟切换摄像机
    private IEnumerator SwitchCamera(bool toHallway)
    {
        isSwitching = true;
        yield return new WaitForSeconds(switchDelay); // 等待缓冲时间

        if (toHallway)
        {
            GameObject.Find("MainCamera").transform.position = GameObject.Find("hallway").transform.position;
            GameObject.Find("MainCamera").transform.rotation = GameObject.Find("hallway").transform.rotation;
            cameraSwitch = true;
        }
        else
        {
            GameObject.Find("MainCamera").transform.position = GameObject.Find("firstroomC").transform.position;
            GameObject.Find("MainCamera").transform.rotation = GameObject.Find("firstroomC").transform.rotation;
            cameraSwitch = false;
        }

        isSwitching = false;
    }
}
