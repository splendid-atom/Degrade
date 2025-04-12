using UnityEngine;

namespace UnityFactorySceneHDRP
{
    public class ArmIKTest : MonoBehaviour
    {
        [SerializeField] private Transform _stand;        // The stand object (used for reference, not the parent)
        [SerializeField] private Transform _arm1;
        [SerializeField] private Transform _arm2;
        [SerializeField] private Transform _arm3;

        [Space(10)]
        [SerializeField] private Transform _arm1Base;
        [SerializeField] private Transform _target;

        public Transform parent;                         // The new parent object to rotate

        private float _upperArmLength;
        private float _foreArmLength;

        // 插值速度控制
        [SerializeField] private float rotationSpeed = 5f; // 旋转速度
        [SerializeField] private float movementSpeed = 5f; // 位置移动速度

        private void Update()
        {
            // 动态计算臂长，考虑缩放
            _upperArmLength = Vector3.Distance(_arm1.position, _arm2.position);
            _foreArmLength = Vector3.Distance(_arm2.position, _arm3.position);

            // 根据父对象的旋转调整
            if (parent != null)
            {
                Vector3 standPosition = _stand.position;
                Quaternion parentRotation = parent.rotation;  // 使用父对象的旋转，而不是 _stand 的旋转

                // 计算目标的方向，调整为父对象的旋转
                Vector3 targetDirection = (_target.position - standPosition).normalized;
                float arm1Angle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;

                // 逐渐过渡到目标旋转
                _stand.rotation = Quaternion.Slerp(_stand.rotation, parentRotation * Quaternion.Euler(0, arm1Angle - 90, 0), Time.deltaTime * rotationSpeed);
            }

            // 重新计算目标的位置和角度
            Vector2 targetLocalPos = _arm1Base.InverseTransformPoint(_target.position);
            float targetDistance = targetLocalPos.magnitude;

            // 检查目标是否在可达范围内
            if (targetDistance < _upperArmLength + _foreArmLength)
            {
                // 基于机械臂的几何计算角度
                float angleA = Mathf.Asin(targetLocalPos.y / targetDistance) * Mathf.Rad2Deg;
                float angleB = Mathf.Acos((_upperArmLength * _upperArmLength + targetDistance * targetDistance - _foreArmLength * _foreArmLength) / (2 * _upperArmLength * targetDistance)) * Mathf.Rad2Deg;
                float angleC = Mathf.Acos((_upperArmLength * _upperArmLength + _foreArmLength * _foreArmLength - targetDistance * targetDistance) / (2 * _upperArmLength * _foreArmLength)) * Mathf.Rad2Deg;

                // 使用插值平滑过渡旋转
                _arm1.localRotation = Quaternion.Slerp(_arm1.localRotation, Quaternion.Euler(0, 0, -(90 - (angleA + angleB))), Time.deltaTime * rotationSpeed);
                _arm2.localRotation = Quaternion.Slerp(_arm2.localRotation, Quaternion.Euler(0, 0, -(180 - angleC)), Time.deltaTime * rotationSpeed);
            }
            else
            {
                // 当目标超出可达范围时，只调整第一个手臂的角度，第二个手臂保持中性
                float angleA = Mathf.Asin(targetLocalPos.y / targetDistance) * Mathf.Rad2Deg;

                _arm1.localRotation = Quaternion.Slerp(_arm1.localRotation, Quaternion.Euler(0, 0, -(90 - angleA)), Time.deltaTime * rotationSpeed);
                _arm2.localRotation = Quaternion.identity; // 保持第二个手臂不动
            }
        }

        // 更新目标位置
        public void SetTargetPosition(Vector3 newTargetPosition)
        {
            _target.position = newTargetPosition;
        }
    }
}
