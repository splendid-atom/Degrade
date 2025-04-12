using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRunController : MonoBehaviour
{
    public static LineRunController Instance;
    [SerializeField] private List<BeltTriggerMovement> activeBelts = new List<BeltTriggerMovement>(); // 存储当前影响玩家的传送带
    [SerializeField] private List<SingleBeltTriggerMovement> activeSingleBelts = new List<SingleBeltTriggerMovement>(); // 存储当前影响玩家的传送带
    public Transform LineRunCameraTransform; // 👈 新增的 public 变量，指定相机的位置
    public Transform InitialCameraTransform; // 👈 新增的 public 变量，指定相机的位置
    public Camera MainCamera;
    public bool isLineRunPassed = false;
    private void Awake()
    {
        Instance = this;
    }
    public bool isLineRunPassedCheck()
    {
        if(isLineRunPassed){
            return true;
        }
        else{
            return false;
        }
    }
    public void AddBelt(BeltTriggerMovement belt)
    {
        if (!activeBelts.Contains(belt))
        {
            activeBelts.Add(belt);
        }
    }
    public void AddSingleBelt(SingleBeltTriggerMovement belt)
    {
        if (!activeSingleBelts.Contains(belt))
        {
            activeSingleBelts.Add(belt);
        }
    }

    public void RemoveBelt(BeltTriggerMovement belt)
    {
        if (activeBelts.Contains(belt))
        {
            activeBelts.Remove(belt);
        }
    }

    public void RemoveSingleBelt(SingleBeltTriggerMovement belt,bool pop = false)
    {
        if (activeSingleBelts.Count > 0){
            if(!pop){
                if (activeSingleBelts.Contains(belt)){
                    activeSingleBelts.Remove(belt);
                }            
            }
            else{
                activeSingleBelts.RemoveAt(0);
            }
        }
    }

    public BeltTriggerMovement GetActiveBelt()
    {
        return activeBelts.Count > 0 ? activeBelts[0] : null; // 返回最先加入的传送带
    }
    public SingleBeltTriggerMovement GetActiveSingleBelt()
    {
        return activeSingleBelts.Count > 0 ? activeSingleBelts[0] : null; // 返回最先加入的传送带
    }
    public void CheckAndSetCamera()
    {
        if (activeBelts.Count > 0)
        {
            if (MainCamera.transform.localPosition != LineRunCameraTransform.localPosition)
            {
                MainCamera.transform.localPosition = LineRunCameraTransform.localPosition;
            }
            if (MainCamera.transform.localRotation != LineRunCameraTransform.localRotation)
            {
                MainCamera.transform.localRotation = LineRunCameraTransform.localRotation;
            }
            RotatingCamera.Instance.SetRotationTo90Degrees();
        }
    }

    public void ResetCamera()
    {
        if (MainCamera != null && InitialCameraTransform != null)
        {
            MainCamera.transform.localPosition = InitialCameraTransform.localPosition;
            MainCamera.transform.localRotation = InitialCameraTransform.localRotation;
        }
        else
        {
            Debug.LogError("MainCamera 或 InitialCameraTransform 未设置！");
        }
    }


}
