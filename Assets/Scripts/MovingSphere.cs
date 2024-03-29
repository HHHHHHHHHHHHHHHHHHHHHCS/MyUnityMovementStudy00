using UnityEngine;

namespace Scripts
{
	//CopyBy: https://catlikecoding.com/unity/tutorials/movement/
	public class MovingSphere : MonoBehaviour
	{
		private static readonly int baseColor_ID = Shader.PropertyToID("_BaseColor");

		[SerializeField] private Transform playerInputSpace = null;

		[SerializeField, Range(0f, 100f)] private float maxSpeed = 10f;
		[SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f;
		[SerializeField, Range(0f, 100f)] private float maxAirAcceleration = 1f;
		[SerializeField, Range(0f, 10f)] private float jumpHeight = 2f;
		[SerializeField, Range(0, 5)] private int maxAirJumps = 0;
		[SerializeField, Range(0f, 90f)] private float maxGroundAngle = 25f;
		[SerializeField, Range(0f, 90f)] private float maxStairsAngle = 50f;
		[SerializeField, Range(0f, 100f)] private float maxSnapSpeed = 100f;
		[SerializeField, Min(0f)] private float probeDistance = 1f;
		[SerializeField] private LayerMask probeMask = -1;
		[SerializeField] private LayerMask stairsMask = -1;

		private InputManager inputManager;
		private Rigidbody body;
		private Material mat;
		private Vector3 velocity, desiredVelocity;
		private bool desiredJump;
		private int groundContactCount, steepContactCount;
		private int jumpPhase;
		private float minGroundDotProduct, minStairsDotProduct;
		private Vector3 contactNormal, steepNormal;
		private int steepsSinceLastGrounded, steepsSinceLastJump;
		private Vector3 upAxis, rightAxis, forwardAxis;

		bool OnGround => groundContactCount > 0;
		bool OnSteep => steepContactCount > 0;

		private void Awake()
		{
			inputManager = InputManager.Instance;
			body = GetComponent<Rigidbody>();
			body.useGravity = false;
			mat = GetComponent<MeshRenderer>().material;
			OnValidate();
		}

		private void OnDestroy()
		{
		}

		private void OnValidate()
		{
			minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
			minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
		}

		private void Update()
		{
			Vector2 playerInput = inputManager.moveAction.ReadValue<Vector2>();
			//不用normalized 是 为了轻微输入
			playerInput = Vector2.ClampMagnitude(playerInput, 1.0f);
			if (playerInputSpace)
			{
				rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
				forwardAxis = ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
			}
			else
			{
				rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
				forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
			}

			desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
			desiredJump |= inputManager.jumpAction.WasPressedThisFrame();
			// mat.SetColor(baseColor_ID, OnGround ? Color.black : Color.white);
		}

		private void FixedUpdate()
		{
			Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);
			UpdateState();
			AdjustVelocity();

			if (desiredJump)
			{
				desiredJump = false;
				Jump(gravity);
			}

			velocity += gravity * Time.fixedDeltaTime;

			body.velocity = velocity;
			ClearState();
		}

		private void OnCollisionEnter(Collision collision)
		{
			EvaluateCollision(collision);
		}

		private void OnCollisionStay(Collision collision)
		{
			EvaluateCollision(collision);
		}

		private void UpdateState()
		{
			steepsSinceLastGrounded += 1;
			steepsSinceLastJump += 1;
			velocity = body.velocity;
			//未接触地面时, 才调用SnapToGround
			if (OnGround || SnapToGround() || CheckSteepContacts())
			{
				steepsSinceLastGrounded = 0;
				if (steepsSinceLastJump > 1)
				{
					jumpPhase = 0;
				}

				if (groundContactCount > 1)
				{
					contactNormal.Normalize();
				}
			}
			else
			{
				contactNormal = upAxis;
			}
		}

		private void ClearState()
		{
			groundContactCount = steepContactCount = 0;
			contactNormal = steepNormal = Vector3.zero;
		}

		private void Jump(Vector3 gravity)
		{
			Vector3 jumpDirection;
			if (OnGround)
			{
				jumpDirection = contactNormal;
			}
			else if (OnSteep)
			{
				jumpDirection = steepNormal;
				jumpPhase = 0;
			}
			else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps)
			{
				if (jumpPhase == 0)
				{
					jumpPhase = 1;
				}

				jumpDirection = contactNormal;
			}
			else
			{
				return;
			}

			steepsSinceLastJump = 0;
			jumpPhase += 1;
			// v0^2 - v1^2 = 2at 因为 最高点的 v1 = 0  g为-9.81
			float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
			// 做 法线+垂直 的跳跃
			jumpDirection = (jumpDirection + upAxis).normalized;
			// 后续跳的高度会比第一次要矮
			float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
			if (alignedSpeed > 0f)
			{
				jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
			}

			velocity += jumpDirection * jumpSpeed;
		}

		private float GetMinDot(int layer)
		{
			return (stairsMask & (1 << layer)) == 0 ? minGroundDotProduct : minStairsDotProduct;
		}

		private void EvaluateCollision(Collision collision)
		{
			float minDot = GetMinDot(collision.gameObject.layer);
			for (int i = 0; i < collision.contactCount; i++)
			{
				Vector3 normal = collision.GetContact(i).normal;
				float upDot = Vector3.Dot(upAxis, normal);
				if (upDot >= minDot)
				{
					groundContactCount += 1;
					//沿着法线做跳起, 同时+号是为了多接触面法线
					contactNormal += normal;
				}
				else if (upDot > -0.01f)
				{
					//如果没有接触地面, 看是否接触到了垂直于墙体等物体, -0.01为宽容条件
					steepContactCount += 1;
					steepNormal += normal;
				}
			}
		}

		private Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
		{
			// Vector3.ProjectOnPlane() 
			// 因为我们传入的始终是单位向量, 上面这个还会除以法线的平方长度
			return (direction - normal * Vector3.Dot(direction, normal)).normalized;
		}

		// 物体沿着平面移动
		private void AdjustVelocity()
		{
			Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);
			Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal);

			//平面速度
			float currentX = Vector3.Dot(velocity, xAxis);
			float currentZ = Vector3.Dot(velocity, zAxis);

			float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
			float maxSpeedChange = acceleration * Time.fixedDeltaTime;

			float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
			float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

			velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
		}


		private bool SnapToGround()
		{
			//steepsSinceLastJump给2 是因为存在一定的碰撞延迟
			if (steepsSinceLastGrounded > 1 || steepsSinceLastJump <= 2)
			{
				return false;
			}

			float speed = velocity.magnitude;
			if (speed > maxSnapSpeed)
			{
				return false;
			}

			if (!Physics.Raycast(body.position, -upAxis, out RaycastHit hit
				    , probeDistance, probeMask))
			{
				return false;
			}

			float upDot = Vector3.Dot(upAxis, hit.normal);
			if (upDot < GetMinDot(hit.collider.gameObject.layer))
			{
				return false;
			}

			groundContactCount = 1;
			contactNormal = hit.normal;
			float dot = Vector3.Dot(velocity, hit.normal);
			//如果此时速度已经向下就算了
			if (dot > 0f)
			{
				//也能防止球体在从台阶上弹起时被发射
				velocity = (velocity - hit.normal * dot).normalized * speed;
			}

			return true;
		}

		private bool CheckSteepContacts()
		{
			if (steepContactCount > 1)
			{
				steepNormal.Normalize();
				float upDot = Vector3.Dot(upAxis, steepNormal);
				if (upDot >= minGroundDotProduct)
				{
					groundContactCount = 1;
					contactNormal = steepNormal;
					return true;
				}
			}

			return false;
		}
	}
}