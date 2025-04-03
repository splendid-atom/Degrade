using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public enum laserStatus
{
	upDown, leftRight, openClose, clockwiseRotate, antiClockwiseRotate
}

namespace VolumetricLines
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class VolumetricLineBehavior : MonoBehaviour
	{
		// Used to compute the average value of all the Vector3's components:

		static readonly Vector3 Average = new Vector3(1f / 3f, 1f / 3f, 1f / 3f);

		#region private variables
		/// <summary>
		/// Template material to be used
		/// </summary>
		[SerializeField]
		public Material m_templateMaterial;

		/// <summary>
		/// Set to false in order to change the material's properties as specified in this script.
		/// Set to true in order to *initially* leave the material's properties as they are in the template material.
		/// </summary>
		[SerializeField]
		private bool m_doNotOverwriteTemplateMaterialProperties;

		/// <summary>
		/// The start position relative to the GameObject's origin
		/// </summary>
		[SerializeField]
		private Vector3 m_startPos;

		/// <summary>
		/// The end position relative to the GameObject's origin
		/// </summary>
		[SerializeField]
		private Vector3 m_endPos = new Vector3(0f, 0f, 100f);

		/// <summary>
		/// Line Color
		/// </summary>
		[SerializeField]
		private Color m_lineColor;

		/// <summary>
		/// The width of the line
		/// </summary>
		[SerializeField]
		private float m_lineWidth;

		/// <summary>
		/// Light saber factor
		/// </summary>
		[SerializeField]
		[Range(0.0f, 1.0f)]
		private float m_lightSaberFactor;

		/// <summary>
		/// This GameObject's specific material
		/// </summary>
		private Material m_material;

		/// <summary>
		/// This GameObject's mesh filter
		/// </summary>
		private MeshFilter m_meshFilter;
		#endregion

		#region properties
		/// <summary>
		/// Gets or sets the tmplate material.
		/// Setting this will only have an impact once. 
		/// Subsequent changes will be ignored.
		/// </summary>
		public Material TemplateMaterial
		{
			get { return m_templateMaterial; }
			set { m_templateMaterial = value; }
		}

		public bool DoNotOverwriteTemplateMaterialProperties
		{
			get { return m_doNotOverwriteTemplateMaterialProperties; }
			set { m_doNotOverwriteTemplateMaterialProperties = value; }
		}
		public Color LineColor
		{
			get { return m_lineColor; }
			set
			{
				CreateMaterial();
				if (null != m_material)
				{
					m_lineColor = value;
					m_material.color = m_lineColor;
				}
			}
		}

		public float LineWidth
		{
			get { return m_lineWidth; }
			set
			{
				CreateMaterial();
				if (null != m_material)
				{
					m_lineWidth = value;
					m_material.SetFloat("_LineWidth", m_lineWidth);
				}
				UpdateBounds();
			}
		}
		public float LightSaberFactor
		{
			get { return m_lightSaberFactor; }
			set
			{
				CreateMaterial();
				if (null != m_material)
				{
					m_lightSaberFactor = value;
					m_material.SetFloat("_LightSaberFactor", m_lightSaberFactor);
				}
			}
		}

		/// <summary>
		/// Get or set the start position of this volumetric line's mesh
		/// </summary>
		public Vector3 StartPos
		{
			get { return m_startPos; }
			set
			{
				m_startPos = value;
				SetStartAndEndPoints(m_startPos, m_endPos);
			}
		}

		/// <summary>
		/// Get or set the end position of this volumetric line's mesh
		/// </summary>
		public Vector3 EndPos
		{
			get { return m_endPos; }
			set
			{
				m_endPos = value;
				SetStartAndEndPoints(m_startPos, m_endPos);
			}
		}
		//new variable
		public BoxCollider2D laserCollider;//激光碰撞体
		private SortingGroup sortingGroup;
		public GameObject player;
		public bool LaserOn;//激活激光
		public bool firstOpen;
		public laserStatus status;
		public float moveSpeedY = 2f;           // 移动速度
		public float moveSpeedZ = 1f;           // 移动速度
		public float moveDistanceY = 2.2f;        // 移动距离
		public float stretchSpeed = 200f;   //伸缩速度
		public float rotateSpeed = 90f; // 旋转速度（度/秒）
		public float posY2D;//2d位置，用于比较渲染
		private float startY2D;
		private float targetY2D;
		public int laserNumber;
		private static VolumetricLineBehavior[] allLasers; // 存储所有激光实例
		private Coroutine currentCoroutine; // 追踪当前运行的协程
		#endregion

		#region methods
		/// <summary>
		/// Creates a copy of the template material for this instance
		/// </summary>
		private void CreateMaterial()
		{
			if (null == m_material || null == GetComponent<MeshRenderer>().sharedMaterial)
			{
				if (null != m_templateMaterial)
				{
					m_material = Material.Instantiate(m_templateMaterial);
					GetComponent<MeshRenderer>().sharedMaterial = m_material;
					SetAllMaterialProperties();
				}
				else
				{
					m_material = GetComponent<MeshRenderer>().sharedMaterial;
				}
			}
		}

		/// <summary>
		/// Destroys the copy of the template material which was used for this instance
		/// </summary>
		private void DestroyMaterial()
		{
			if (null != m_material)
			{
				DestroyImmediate(m_material);
				m_material = null;
			}
		}

		/// <summary>
		/// Calculates the (approximated) _LineScale factor based on the object's scale.
		/// </summary>
		private float CalculateLineScale()
		{
			return Vector3.Dot(transform.lossyScale, Average);
		}

		/// <summary>
		/// Updates the line scaling of this volumetric line based on the current object scaling.
		/// </summary>
		public void UpdateLineScale()
		{
			if (null != m_material)
			{
				m_material.SetFloat("_LineScale", CalculateLineScale());
			}
		}

		/// <summary>
		/// Sets all material properties (color, width, light saber factor, start-, endpos)
		/// </summary>
		private void SetAllMaterialProperties()
		{
			SetStartAndEndPoints(m_startPos, m_endPos);

			if (null != m_material)
			{
				if (!m_doNotOverwriteTemplateMaterialProperties)
				{
					m_material.color = m_lineColor;
					m_material.SetFloat("_LineWidth", m_lineWidth);
					m_material.SetFloat("_LightSaberFactor", m_lightSaberFactor);
				}
				UpdateLineScale();
			}
		}

		/// <summary>
		/// Calculate the bounds of this line based on start and end points,
		/// the line width, and the scaling of the object.
		/// </summary>
		private Bounds CalculateBounds()
		{
			var maxWidth = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
			var scaledLineWidth = maxWidth * LineWidth * 0.5f;

			var min = new Vector3(
				Mathf.Min(m_startPos.x, m_endPos.x) - scaledLineWidth,
				Mathf.Min(m_startPos.y, m_endPos.y) - scaledLineWidth,
				Mathf.Min(m_startPos.z, m_endPos.z) - scaledLineWidth
			);
			var max = new Vector3(
				Mathf.Max(m_startPos.x, m_endPos.x) + scaledLineWidth,
				Mathf.Max(m_startPos.y, m_endPos.y) + scaledLineWidth,
				Mathf.Max(m_startPos.z, m_endPos.z) + scaledLineWidth
			);

			return new Bounds
			{
				min = min,
				max = max
			};
		}

		/// <summary>
		/// Updates the bounds of this line according to the current properties, 
		/// which there are: start point, end point, line width, scaling of the object.
		/// </summary>
		public void UpdateBounds()
		{
			if (null != m_meshFilter)
			{
				var mesh = m_meshFilter.sharedMesh;
				Debug.Assert(null != mesh);
				if (null != mesh)
				{
					mesh.bounds = CalculateBounds();
				}
			}
		}

		/// <summary>
		/// Sets the start and end points - updates the data of the Mesh.
		/// </summary>
		public void SetStartAndEndPoints(Vector3 startPoint, Vector3 endPoint)
		{
			m_startPos = startPoint;
			m_endPos = endPoint;

			Vector3[] vertexPositions = {
				m_startPos,
				m_startPos,
				m_startPos,
				m_startPos,
				m_endPos,
				m_endPos,
				m_endPos,
				m_endPos,
			};

			Vector3[] other = {
				m_endPos,
				m_endPos,
				m_endPos,
				m_endPos,
				m_startPos,
				m_startPos,
				m_startPos,
				m_startPos,
			};

			if (null != m_meshFilter)
			{
				var mesh = m_meshFilter.sharedMesh;
				Debug.Assert(null != mesh);
				if (null != mesh)
				{
					mesh.vertices = vertexPositions;
					mesh.normals = other;
					UpdateBounds();
				}
			}
		}
		#endregion

		#region event functions
		void Start()
		{
			Mesh mesh = new Mesh();
			m_meshFilter = GetComponent<MeshFilter>();
			m_meshFilter.mesh = mesh;
			SetStartAndEndPoints(m_startPos, m_endPos);
			StartPos = new Vector3(0f, 0f, 0f);
			EndPos = new Vector3(0f, 0f, 0f);
			mesh.uv = VolumetricLineVertexData.TexCoords;
			mesh.uv2 = VolumetricLineVertexData.VertexOffsets;
			mesh.SetIndices(VolumetricLineVertexData.Indices, MeshTopology.Triangles, 0);
			CreateMaterial();
			laserCollider = gameObject.GetComponent<BoxCollider2D>();
			gameObject.GetComponent<MeshRenderer>().enabled = false;
			sortingGroup = GetComponent<SortingGroup>();
			if (sortingGroup == null) sortingGroup = gameObject.AddComponent<SortingGroup>();
			LaserOn = false;
			firstOpen = false;
			startY2D = posY2D;
			targetY2D = posY2D + moveDistanceY;

			if (allLasers == null)
			{
				allLasers = FindObjectsOfType<VolumetricLineBehavior>();
				System.Array.Sort(allLasers, (a, b) => a.laserNumber.CompareTo(b.laserNumber));
			}
			// TODO: Need to set vertices before assigning new Mesh to the MeshFilter's mesh property => Why?
		}

		void OnDestroy()
		{
			if (null != m_meshFilter)
			{
				if (Application.isPlaying)
				{
					Mesh.Destroy(m_meshFilter.sharedMesh);
				}
				else // avoid "may not be called from edit mode" error
				{
					Mesh.DestroyImmediate(m_meshFilter.sharedMesh);
				}
				m_meshFilter.sharedMesh = null;
			}
			DestroyMaterial();
		}

		void Update()
		{
			if (transform.hasChanged)
			{
				UpdateLineScale();
				UpdateBounds();
			}
			if (doorTrigger.doortrigger.isFallen && !firstOpen && !LaserOn)
			{
				gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -0.1f);
				float newy = Mathf.Lerp(StartPos.y, 100f, Time.deltaTime * 2f);
				Vector3 newPosition = StartPos;
				gameObject.GetComponent<MeshRenderer>().enabled = true;
				newPosition.y = newy;
				StartPos = newPosition;
				if (!LaserOn && Mathf.Abs(StartPos.y - 100f) < 0.5f)
				{
					LaserOn = true;
					firstOpen = true;
					StartPos = new Vector3(m_startPos.x, 100f, m_startPos.z);
					if (gameObject.name == "SingleLine-LightSaber-Arc")
					{
						StartCoroutine(continueClockwiseRotateLaser());
					}
					else StartCoroutine(RandomSwitchBehavior()); // 启动切换
				}
			}
			UpdateRenderOrder();//控制渲染关系
			updateCollider();//控制碰撞体跟随激光
		}

		void OnValidate()
		{
			// This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only).
			//  => make sure, everything stays up-to-date
			if (string.IsNullOrEmpty(gameObject.scene.name) || string.IsNullOrEmpty(gameObject.scene.path))
			{
				return; // ...but not if a Prefab is selected! (Only if we're using it within a scene.)
			}
			CreateMaterial();
			SetAllMaterialProperties();
			UpdateBounds();
		}

		void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawLine(gameObject.transform.TransformPoint(m_startPos), gameObject.transform.TransformPoint(m_endPos));
		}
		void updateCollider()
		{
			if (laserCollider != null)
			{
				laserCollider.size = new Vector2(laserCollider.size.x, StartPos.y);
				laserCollider.offset = new Vector2(laserCollider.offset.x, StartPos.y / 2);
			}
		}
		//更新碰撞体形状
		IEnumerator RandomSwitchBehavior()
		{
			if (!Application.isPlaying) yield break;

			while (true)
			{
				// 获取相邻激光状态
				VolumetricLineBehavior prevLaser = laserNumber > 1 ? allLasers[laserNumber - 2] : null;
				VolumetricLineBehavior nextLaser = laserNumber < 7 ? allLasers[laserNumber] : null;

				float randomValue = Random.value;
				if (randomValue < 0.35f) // 35%
					status = laserStatus.upDown;
				else if (randomValue < 0.7f) // 35%
					status = laserStatus.leftRight;
				else if (randomValue < 0.8f) // 10%
					status = laserStatus.openClose;
				else if (randomValue < 0.9f && // 10%, 检查上一个条件
						(prevLaser == null || (prevLaser.status != laserStatus.leftRight && prevLaser.status != laserStatus.antiClockwiseRotate)))
					status = laserStatus.clockwiseRotate;
				else if (randomValue < 1.0f && // 10%, 检查下一个条件
						(nextLaser == null || nextLaser.status != laserStatus.clockwiseRotate))
					status = laserStatus.antiClockwiseRotate;
				else
					status = laserStatus.upDown; // 默认回退

				if (currentCoroutine != null)
				{
					StopCoroutine(currentCoroutine);
				}

				switch (status)
				{
					case laserStatus.upDown:
						currentCoroutine = StartCoroutine(upAndDown());
						break;
					case laserStatus.leftRight:
						currentCoroutine = StartCoroutine(forwardAndBack());
						break;
					case laserStatus.openClose:
						currentCoroutine = StartCoroutine(StretchLaser());
						break;
					case laserStatus.clockwiseRotate:
						currentCoroutine = StartCoroutine(clockwiseRotateLaser());
						break;
					case laserStatus.antiClockwiseRotate:
						currentCoroutine = StartCoroutine(antiClockwiseRotateLaser());
						break;
				}

				yield return new WaitForSeconds(5f);
			}
		}
		//逆时针旋转
		IEnumerator antiClockwiseRotateLaser()
		{
			if (!Application.isPlaying) yield break;

			Quaternion startRotation = Quaternion.Euler(0f, 0f, -90f); // 起点 -90 度
			Quaternion targetRotation = Quaternion.Euler(0f, 0f, 0f); // 终点 -180 度
			float duration = 90f / rotateSpeed; // 90 度的旋转时间
																					// 从 -90 度旋转到 0 度
			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float t = elapsed / duration;
				t = Mathf.SmoothStep(0f, 1f, t);
				transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
				yield return null;
			}
			transform.localRotation = targetRotation; // 确保精确到达 0 度
			yield return new WaitForSeconds(0.5f); // 暂停 0.5 秒

			// 从 -180 度返回到 -90 度
			elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float t = elapsed / duration;
				t = Mathf.SmoothStep(0f, 1f, t);
				transform.localRotation = Quaternion.Slerp(targetRotation, startRotation, t);
				yield return null;
			}
			transform.localRotation = startRotation; // 确保精确回到 -90 度
			yield return new WaitForSeconds(0.5f); // 暂停 0.5 秒
		}
		//顺时针旋转
		IEnumerator clockwiseRotateLaser()
		{
			if (!Application.isPlaying) yield break;

			Quaternion startRotation = Quaternion.Euler(0f, 0f, -90f); // 起点 -90 度
			Quaternion targetRotation = Quaternion.Euler(0f, 0f, -180f); // 终点 -180 度
			float duration = 90f / rotateSpeed; // 90 度的旋转时间
																					// 从 -90 度旋转到 -180 度

			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float t = elapsed / duration;
				t = Mathf.SmoothStep(0f, 1f, t);
				transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
				yield return null;
			}
			transform.localRotation = targetRotation; // 确保精确到达 -180 度
			yield return new WaitForSeconds(0.5f); // 暂停 0.5 秒

			// 从 -180 度返回到 -90 度
			elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float t = elapsed / duration;
				t = Mathf.SmoothStep(0f, 1f, t);
				transform.localRotation = Quaternion.Slerp(targetRotation, startRotation, t);
				yield return null;
			}
			transform.localRotation = startRotation; // 确保精确回到 -90 度
			yield return new WaitForSeconds(0.5f); // 暂停 0.5 秒
		}

		//顺时针旋转
		IEnumerator continueClockwiseRotateLaser()
		{
			if (!Application.isPlaying) yield break;

			Quaternion startRotation = Quaternion.Euler(0f, 0f, -90f); // 起点 -90 度
			Quaternion targetRotation = Quaternion.Euler(0f, 0f, -180f); // 终点 -180 度
			float duration = 90f / rotateSpeed; // 90 度的旋转时间
																					// 从 -90 度旋转到 -180 度
			while (true)
			{
				float elapsed = 0f;
				while (elapsed < duration)
				{
					elapsed += Time.deltaTime;
					float t = elapsed / duration;
					t = Mathf.SmoothStep(0f, 1f, t);
					transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
					yield return null;
				}
				transform.localRotation = targetRotation; // 确保精确到达 -180 度
				yield return new WaitForSeconds(0.5f); // 暂停 0.5 秒

				// 从 -180 度返回到 -90 度
				elapsed = 0f;
				while (elapsed < duration)
				{
					elapsed += Time.deltaTime;
					float t = elapsed / duration;
					t = Mathf.SmoothStep(0f, 1f, t);
					transform.localRotation = Quaternion.Slerp(targetRotation, startRotation, t);
					yield return null;
				}
				transform.localRotation = startRotation; // 确保精确回到 -90 度
				yield return new WaitForSeconds(0.5f); // 暂停 0.5 秒
			}
		}

		IEnumerator forwardAndBack()
		{
			Vector3 originalPosition = transform.localPosition;
			float targetY = originalPosition.y + moveDistanceY;
			float duration = moveDistanceY / moveSpeedY;
			// 向前移动
			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float t = elapsed / duration;
				t = Mathf.SmoothStep(0f, 1f, t); // 使用 SmoothStep 平滑插值
				transform.localPosition = Vector3.Lerp(originalPosition, new Vector3(originalPosition.x, targetY, originalPosition.z), t);
				posY2D = Mathf.Lerp(startY2D, targetY2D, t);
				yield return null;
			}
			transform.localPosition = new Vector3(originalPosition.x, targetY, originalPosition.z); // 确保精确到达
			posY2D = targetY2D;

			// 向后移动
			elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float t = elapsed / duration;
				t = Mathf.SmoothStep(0f, 1f, t); // 平滑返回
				transform.localPosition = Vector3.Lerp(new Vector3(originalPosition.x, targetY, originalPosition.z), originalPosition, t);
				posY2D = Mathf.Lerp(targetY2D, startY2D, t);
				yield return null;
			}
			transform.localPosition = originalPosition; // 确保精确回到原位
			posY2D = startY2D;
			yield return new WaitForSeconds(0.1f);
		}
		//上下移动
		IEnumerator upAndDown()
		{
			Vector3 originalPosition = transform.localPosition;
			float startZ = -0.1f;  // 移动范围起点
			float targetZ = -3.0f; // 移动范围终点
			float duration = Mathf.Abs(targetZ - startZ) / moveSpeedZ; // 计算单程时间
																																 // 向下移动到 -3.0
			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float t = elapsed / duration;
				t = Mathf.SmoothStep(0f, 1f, t); // 平滑插值
				float newZ = Mathf.Lerp(startZ, targetZ, t);
				transform.localPosition = new Vector3(originalPosition.x, originalPosition.y, newZ);

				// 检查是否进入安全区域 (-2.6 到 -3.3)
				float currentZ = transform.localPosition.z;
				if (currentZ <= -2.6f && currentZ >= -3.0f)
				{
					laserCollider.enabled = false; // 禁用触发器
				}
				else
				{
					laserCollider.enabled = true; // 启用触发器
				}

				updateCollider(); // 更新碰撞器
				yield return null;
			}
			transform.localPosition = new Vector3(originalPosition.x, originalPosition.y, targetZ); // 确保精确到达 -3.3

			// 在 -3.3 停留 1 秒
			yield return new WaitForSeconds(1f);

			// 向上移动回到 -0.1
			elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float t = elapsed / duration;
				t = Mathf.SmoothStep(0f, 1f, t); // 平滑返回
				float newZ = Mathf.Lerp(targetZ, startZ, t);
				transform.localPosition = new Vector3(originalPosition.x, originalPosition.y, newZ);

				// 检查是否在安全区域
				float currentZ = transform.localPosition.z;
				if (currentZ <= -2.6f && currentZ >= -3.0f)
				{
					laserCollider.enabled = false; // 禁用触发器
				}
				else
				{
					laserCollider.enabled = true; // 启用触发器
				}

				updateCollider(); // 更新碰撞器
				yield return null;
			}
			transform.localPosition = new Vector3(originalPosition.x, originalPosition.y, startZ); // 确保精确回到 -0.1
			laserCollider.enabled = true; // 确保最终触发器启用
		}
		//伸缩
		IEnumerator StretchLaser()
		{
			float startY = 100f; // StartPos 的起点
			float targetY = 0f;  // StartPos 的收缩目标
			float duration = Mathf.Abs(startY - targetY) / stretchSpeed; // 单程时间

			// 收缩：从 100 到 0
			float elapsed = 0f;
			gameObject.GetComponent<MeshRenderer>().enabled = true; // 确保收缩时可见
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float t = elapsed / duration;
				t = Mathf.SmoothStep(0f, 1f, t); // 平滑插值
				float newY = Mathf.Lerp(startY, targetY, t);
				StartPos = new Vector3(m_startPos.x, newY, m_startPos.z); // 更新 StartPos
				yield return null;
			}
			StartPos = new Vector3(m_startPos.x, targetY, m_startPos.z); // 确保精确到达 0

			// 关闭 MeshRenderer 避免单点光效
			gameObject.GetComponent<MeshRenderer>().enabled = false;

			// 在收缩处（0）停留 0.5 秒
			yield return new WaitForSeconds(0.5f);

			// 伸展：从 0 到 100
			elapsed = 0f;
			gameObject.GetComponent<MeshRenderer>().enabled = true; // 重新启用渲染
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float t = elapsed / duration;
				t = Mathf.SmoothStep(0f, 1f, t); // 平滑插值
				float newY = Mathf.Lerp(targetY, startY, t);
				StartPos = new Vector3(m_startPos.x, newY, m_startPos.z); // 更新 StartPos
				yield return null;
			}
			StartPos = new Vector3(m_startPos.x, startY, m_startPos.z); // 确保精确回到 100

			// 在最大伸展处（100）停留 0.5 秒
			yield return new WaitForSeconds(0.5f);
		}

		//修改激光与玩家渲染层级
		void UpdateRenderOrder()
		{
			if (player != null && sortingGroup != null)
			{
				float playerY = player.transform.position.y;
				sortingGroup.sortingOrder = posY2D < playerY ? 1 : -1;
			}
		}
		//受伤
		private void OnTriggerEnter2D(Collider2D other)
		{
			if (other.gameObject == player)
			{
				player.GetComponent<PlayerController>().PlayerHealth -= 15;
			}
		}
		#endregion
	}
}