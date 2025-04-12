using System.Collections;
using UnityEngine;
using UnityEngine.Splines; // Make sure this using statement is present
using Unity.Mathematics;  // Make sure this using statement is present

namespace UnityFactorySceneHDRP
{
	[ExecuteAlways]
	public class CustomSplineAnimate : MonoBehaviour
	{
		[System.Serializable]
		private struct StopPoint
		{
			public float time;              // (停留点在路径上的标准化时间 0-1)
			public float duration;          // (在该停留点停留的时长)
			public Animation robotArmAnimation; // (在停留点播放的动画 (可选))
		}

		[SerializeField] public SplineContainer _spline; // (需要跟随的Spline路径容器)
		[SerializeField] public float _duration = 5f;   // (完成整个路径所需的时间(秒))
		[SerializeField] private float _startOffset = 0f;// (开始时的时间偏移量 0-1)
		[SerializeField] private StopPoint[] _stopPoints;// (路径上的停留点数组)

		[Header("Preview")]
		[SerializeField, Range(0, 1)] private float _previewTime; // (在编辑器模式下预览位置的时间点 0-1)

		[Header("Control")]
		public bool IsMoving = true;  // (控制物体是否沿路径前进)
		public bool isOnBelt = true; // ✅ 控制物体是否需要跟随Spline路径

		private Transform _transform;
		private float _time = 0; // (当前在路径上的标准化时间 0-1)
		private Coroutine _animationCoroutine;
		private void Awake()
		{
			if (Application.isPlaying)
			{
				_transform = transform; // (获取当前对象的Transform组件)
				_time += _startOffset;  // (应用起始偏移量)
			}
		}

		private void Start()
		{
			if (Application.isPlaying)
			{
				// 检查是否有有效的Spline和正的持续时间
				if (_spline != null && _duration > 0)
				{
					_animationCoroutine = StartCoroutine(Animate()); // (启动动画协程)
				}
				else if (_spline == null)
				{
					Debug.LogWarning("Spline Container not assigned.", this);
				}
				else if (_duration <= 0)
				{
					Debug.LogWarning("Duration must be greater than 0.", this);
				}
			}
		}

		private IEnumerator Animate()
		{
			bool[] isPassed = new bool[_stopPoints.Length]; // (标记每个停留点是否已经经过)
			float stopOverrun = 0; // (记录因为停留而超出目标停留时间的量)

			// 初始化：检查在起始偏移后是否已经越过了某些停留点
			for (int i = 0; i < _stopPoints.Length; i++)
			{
				if (_time > _stopPoints[i].time && !isPassed[i])
				{
					isPassed[i] = true;
				}
			}

			// 主动画循环
			while (true)
			{
				// ✅ 检查是否应该在皮带上 (是否启用循迹)
				if (!isOnBelt)
				{
					// 如果不在皮带上，则不执行任何移动或旋转更新
					// 允许外部脚本或其他逻辑控制Transform
					yield return null; // (等待下一帧)
					continue; // (跳过本帧剩余的循迹逻辑)
				}

				// --- 以下代码仅在 isOnBelt 为 true 时执行 ---

				// 如果 IsMoving 为 false，暂停前进，但保持在当前位置
				if (!IsMoving)
				{
					SetPositionAndRotation(_time); // (保持当前姿态)
					yield return null; // (等待下一帧)
					continue; // (跳过本帧剩余的前进逻辑)
				}

				// --- 以下代码仅在 isOnBelt 和 IsMoving 都为 true 时执行 ---

				// 处理 StopPoint 停留点
				for (int i = 0; i < _stopPoints.Length; i++)
				{
					// 检查是否到达或超过了一个尚未处理的停留点
					if (_time >= _stopPoints[i].time && !isPassed[i])
					{
						isPassed[i] = true; // (标记此停留点已处理)
						stopOverrun = _time - _stopPoints[i].time; // (计算超出的时间)

						// 如果有指定动画，则播放
						if (_stopPoints[i].robotArmAnimation != null)
						{
							_stopPoints[i].robotArmAnimation.Play();
						}

						_time = _stopPoints[i].time; // (将时间精确设置到停留点)
						SetPositionAndRotation(_time); // (更新位置和旋转到停留点)
						yield return new WaitForSeconds(_stopPoints[i].duration); // (等待指定的停留时间)
						// 停留结束后，将在下一轮循环开始时加上 stopOverrun 恢复正常前进
					}
				}

				// 正常前进逻辑
				// 累加时间，考虑上一帧可能因为停留而暂停的时间以及stopOverrun
				_time = _time + stopOverrun + Time.deltaTime / _duration;
				stopOverrun = 0; // (重置stopOverrun)

				// 检查是否到达路径终点，进行循环
				if (_time > 1)
				{
					_time %= 1; // (时间取模，实现循环)
					// 重置所有停留点的通过标记，以便下次循环可以再次停留
					for (int i = 0; i < isPassed.Length; i++)
					{
						isPassed[i] = false;
					}
				}

				// 根据当前时间设置物体的位置和旋转
				SetPositionAndRotation(_time);

				yield return null; // (等待下一帧)
			}
		}

		// 根据标准化时间 time 设置物体的位置和旋转
		private void SetPositionAndRotation(float time)
		{
            if (_spline == null || _transform == null) return; // 添加空检查

			// 防止 time 超出 [0, 1] 范围，虽然循环逻辑会处理，但直接传入时可能需要
			time = Mathf.Clamp01(time);

			// 从 Spline 获取指定时间点的位置
			// 使用 EvaluatePosition(splineIndex, t) 如果你有多个spline在一个Container里
			Vector3 position = _spline.EvaluatePosition(time);

			// 从 Spline 获取指定时间点的切线方向 (前进方向)
			float3 tangent = _spline.EvaluateTangent(time);

            // 检查切线是否为零向量，避免LookRotation错误
            if (math.lengthsq(tangent) < 0.0001f)
            {
                // 如果切线太小，保持当前旋转或使用默认旋转
                // _transform.position = position; // 只更新位置
                // 或者使用前一帧的朝向，或者默认朝向，例如:
                tangent = (float3)transform.forward; // 使用物体当前的前方作为切线
                if (math.lengthsq(tangent) < 0.0001f) tangent = new float3(0,0,1); // 如果当前前方也无效，用世界Z轴
            }


			_transform.position = position; // (更新位置)
            // 使物体的 Z 轴朝向切线方向，Y 轴朝向世界空间的上方
			_transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
		}
		public void SetSpline(SplineContainer spline)
		{
			_spline = spline;
		}
		public void SetDuration(float duration)
		{
			_duration = duration;
		}
        // --- 修改 ResetTime ---
        public void ResetTime()
        {
            _time = 0f; // 重置时间变量

            // 停止当前正在运行的协程 (如果存在)
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine); // <--- 使用 StopCoroutine
            }
            // 重新启动 Animate 协程并存储新的引用
            _animationCoroutine = StartCoroutine(Animate()); // <--- 重启并存储
        }

#if UNITY_EDITOR
		// 在编辑器模式下运行时更新预览位置
		private void Update()
		{
			// 仅在编辑器中且非播放模式下执行
			if (!Application.isPlaying && _spline != null)
			{
				// 如果 _transform 未初始化 (例如刚添加脚本时)
				if (_transform == null)
				{
					_transform = transform;
				}
				// 设置预览位置
				SetPositionAndRotation(_previewTime);
			}
		}
#endif
	}
}