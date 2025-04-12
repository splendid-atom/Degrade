using System.Collections;
using UnityEngine;
using UnityEngine.Splines; // Make sure this using statement is present
using Unity.Mathematics;  // Make sure this using statement is present

namespace UnityFactorySceneHDRP
{
	[ExecuteAlways]
	public class CustomSplineAnimateTrashEnemies : MonoBehaviour
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
			// Get transform reference
			_transform = transform;

			if (Application.isPlaying)
			{
				// Apply start offset only when playing
				_time = _startOffset;
			}
		}

		private void Start()
		{
			if (Application.isPlaying)
			{
				// Start animation coroutine if conditions met
				if (_spline != null && _duration > 0)
				{
					_animationCoroutine = StartCoroutine(Animate());
				}
				else
				{
					if (_spline == null) Debug.LogWarning("Spline Container not assigned.", this);
					if (_duration <= 0) Debug.LogWarning("Duration must be greater than 0.", this);
				}
			}
		}

        private void OnDisable()
        {
            // Stop coroutine if component is disabled
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }
        }

		private IEnumerator Animate()
		{
            // Initialize or Reinitialize state for the animation run
			bool[] isPassed = null;
            if(_stopPoints != null) {
                isPassed = new bool[_stopPoints.Length];
            } else {
                isPassed = new bool[0];
            }

			float stopOverrun = 0;

			// Check initial stop points based on current _time (set in Awake or ResetTime)
            for (int i = 0; i < isPassed.Length; i++)
            {
                if (_time > _stopPoints[i].time) {
                    isPassed[i] = true;
                } else {
                    isPassed[i] = false;
                }
            }


			// Main animation loop
			while (true)
			{
                yield return null; // Wait for next frame

				// --- Condition Checks ---
                if (!this.enabled || !isOnBelt || _spline == null)
                {
                    continue; // Skip frame if disabled, not on belt, or no spline
                }

				if (!IsMoving)
				{
					SetPositionAndRotation(_time); // Keep current pose if paused
					continue;
				}

				// --- Stop Point Handling ---
                bool stoppedAndWaited = false;
                if (isPassed != null) // Check if array is valid
                {
                    for (int i = 0; i < _stopPoints.Length; i++)
                    {
                        if (_time >= _stopPoints[i].time && !isPassed[i])
                        {
                            isPassed[i] = true;
                            stopOverrun = _time - _stopPoints[i].time;

                            if (_stopPoints[i].robotArmAnimation != null) _stopPoints[i].robotArmAnimation.Play();

                            _time = _stopPoints[i].time;
                            SetPositionAndRotation(_time); // Update to stop point

                            if (_stopPoints[i].duration > 0.001f)
                            {
                                yield return new WaitForSeconds(_stopPoints[i].duration);
                                stoppedAndWaited = true; // Mark that we waited
                            }
                            break; // Process only one stop point per frame
                        }
                    }
                }

				// --- Movement Logic (only if we didn't wait at a stop point) ---
                if (!stoppedAndWaited)
                {
                    // Advance time
                    if (_duration > 0)
                    {
                        _time = _time + stopOverrun + Time.deltaTime / _duration;
                        stopOverrun = 0; // Reset overrun after applying
                    }

                    // Handle Looping
                    if (_time >= 1f)
                    {
                        _time %= 1f;
                        // Reset passed flags for the new loop
                        if (isPassed != null)
                        {
                           for (int i = 0; i < isPassed.Length; i++) isPassed[i] = false;
                        }
                    }

                    // Update transform based on new time
                    SetPositionAndRotation(_time);
                }
			} // End while loop
		}

		/// <summary>
		/// Sets the object's position based on spline time,
		/// AND forces its rotation to zero (Quaternion.identity).
		/// </summary>
		/// <param name="time">Normalized time (0-1) along the spline.</param>
		private void SetPositionAndRotation(float time)
		{
            // Basic checks
            if (_spline == null || _transform == null || _spline.Spline == null) return;

			// Clamp time
			time = Mathf.Clamp01(time);

			// Get POSITION only from the spline
			Vector3 position = _spline.EvaluatePosition(time);

            // --- Apply Changes ---
			_transform.position = position;         // Set the calculated position
			_transform.rotation = Quaternion.identity; // FORCE rotation to zero (0, 0, 0)
		}


		// --- Public Control Methods ---

		public void SetSpline(SplineContainer spline)
		{
			_spline = spline;
            // Restart animation if running to apply the new spline
            if (Application.isPlaying && this.enabled)
            {
                ResetTime(); // Resetting time effectively restarts the animation
            }
		}

		public void SetDuration(float duration)
		{
            if (duration > 0)
            {
			    _duration = duration;
            } else {
                Debug.LogWarning("Duration must be greater than 0. Not set.", this);
            }
		}

        public void ResetTime()
        {
            _time = 0f; // Reset time variable to start

            // Stop the current animation coroutine if it's running
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }

            // Restart the animation coroutine only if conditions are met
            if (Application.isPlaying && this.enabled && _spline != null && _duration > 0)
            {
                 _animationCoroutine = StartCoroutine(Animate());
            } else {
                _animationCoroutine = null; // Ensure reference is null if not restarted
            }
        }

#if UNITY_EDITOR
		// --- Editor Preview ---
		private void Update()
		{
			// Only run in editor, not playing, and if object is selected
			if (!Application.isPlaying && UnityEditor.Selection.Contains(gameObject) && _spline != null)
			{
				if (_transform == null) _transform = transform; // Ensure transform is set

				if (_transform != null)
                {
                    // Call the modified method to preview position AND forced zero rotation
				    SetPositionAndRotation(_previewTime);
                }
			}
		}
#endif
	} // End of class
} // End of namespace