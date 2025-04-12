using UnityEngine;

namespace UnityFactorySceneHDRP
{
    public class ArmIKTester : MonoBehaviour
    {
        [SerializeField] private Camera _camera;             // 拖入你的主摄像机
        [SerializeField] private ArmIKTest _armIK;           // 拖入你的 ArmIK 脚本引用
        [SerializeField] private LayerMask _raycastMask;     // 设置一个 LayerMask，用来检测点击位置（比如地面）

        // 用于控制鼠标点击位置与目标位置之间的缩放比例
        [SerializeField] private float distanceFactor = 1.0f; // 控制目标位置的距离缩放比例

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // 从摄像机的鼠标位置发射射线
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

                // 使用射线检测点击位置
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, _raycastMask))
                {
                    if (_armIK != null)
                    {
                        // 获取原始点击位置
                        Vector3 clickedPosition = hit.point;

                        // 计算机械臂基准点与点击位置的方向
                        Vector3 directionToTarget = (clickedPosition - _armIK.transform.position).normalized;

                        // 计算机械臂基准点与点击位置的距离
                        float distanceToTarget = Vector3.Distance(_armIK.transform.position, clickedPosition);

                        // 根据distanceFactor调整目标位置
                        float adjustedDistance = distanceToTarget * distanceFactor;

                        // 计算调整后的目标位置
                        Vector3 adjustedTargetPosition = _armIK.transform.position + directionToTarget * adjustedDistance;

                        // 更新目标位置
                        _armIK.SetTargetPosition(adjustedTargetPosition);

                        Debug.Log("Adjusted Target Position: " + adjustedTargetPosition);
                    }
                }
            }
        }
    }
}
