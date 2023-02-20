using UnityEngine;

namespace Scripts
{
	//CopyBy: https://catlikecoding.com/unity/tutorials/movement/
	public class MovingSphere : MonoBehaviour
	{
		private static readonly int baseColor_ID = Shader.PropertyToID("_BaseColor");

		[SerializeField, Range(0f, 100f)] private float maxSpeed = 10f;
		[SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f;
		[SerializeField, Range(0f, 100f)] private float maxAirAcceleration = 1f;
		[SerializeField, Range(0f, 10f)] private float jumpHeight = 2f;
		[SerializeField, Range(0, 5)] private int maxAirJumps = 0;
		[SerializeField, Range(0f, 90f)] private float maxGroundAngle = 25f;

		private InputsManager inputManager;
		private Rigidbody body;
		private Vector3 velocity, desiredVelocity;
		private bool desiredJump;
		private int groundContactCount;
		private int jumpPhase;
		private float minGroundDotProduct;
		private Vector3 contactNormal;
		private Material mat;
		private int stepsSinceLastGrounded;

		bool OnGround => groundContactCount > 0;

		private void Awake()
		{
			inputManager = InputsManager.Instance;
			body = GetComponent<Rigidbody>();
			mat = GetComponent<MeshRenderer>().material;
		}

		private void OnDestroy()
		{
			inputManager?.OnDestroy();
		}


		private void OnValidate()
		{
			minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
		}

		private void Update()
		{
			Vector2 playerInput = inputManager.moveAction.ReadValue<Vector2>();
			//不用normalized 是 为了轻微输入
			playerInput = Vector2.ClampMagnitude(playerInput, 1.0f);
			desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
			desiredJump |= inputManager.jumpAction.WasPressedThisFrame();
			mat.SetColor(baseColor_ID, OnGround ? Color.black : Color.white);
		}

		private void FixedUpdate()
		{
			UpdateState();
			AdjustVelocity();

			if (desiredJump)
			{
				desiredJump = false;
				Jump();
			}

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
			stepsSinceLastGrounded += 1;
			velocity = body.velocity;
			//未接触地面时, 才调用SnapToGround
			if (OnGround || SnapToGround())
			{
				stepsSinceLastGrounded = 0;
				jumpPhase = 0;
				if (groundContactCount > 1)
				{
					contactNormal.Normalize();
				}
			}
			else
			{
				contactNormal = Vector3.up;
			}
		}

		private void ClearState()
		{
			groundContactCount = 0;
			contactNormal = Vector3.zero;
		}

		private void Jump()
		{
			if (OnGround || jumpPhase < maxAirJumps)
			{
				jumpPhase += 1;
				// v0^2 - v1^2 = 2at 因为 最高点的 v1 = 0  g为-9.81
				float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
				// 后续跳的高度会比第一次要矮
				float alignedSpeed = Vector3.Dot(velocity, contactNormal);
				if (alignedSpeed > 0f)
				{
					jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
				}

				velocity += contactNormal * jumpSpeed;
			}
		}


		private void EvaluateCollision(Collision collision)
		{
			for (int i = 0; i < collision.contactCount; i++)
			{
				Vector3 normal = collision.GetContact(i).normal;
				if (normal.y >= minGroundDotProduct)
				{
					groundContactCount += 1;
					//沿着法线做跳起, 同时+号是为了多接触面法线
					contactNormal += normal;
				}
			}
		}

		private Vector3 ProjectOnContactPlane(Vector3 vector)
		{
			// Vector3.ProjectOnPlane() 
			// 因为我们传入的始终是单位向量, 上面这个还会除以法线的平方长度
			return vector - contactNormal * Vector3.Dot(vector, contactNormal);
		}

		// 物体沿着平面移动
		private void AdjustVelocity()
		{
			Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
			Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

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
			if (stepsSinceLastGrounded > 1)
			{
				return false;
			}

			if (!Physics.Raycast(body.position, Vector3.down, out RaycastHit hit))
			{
				return false;
			}

			if (hit.normal.y < minGroundDotProduct)
			{
				return false;
			}

			groundContactCount = 1;
			contactNormal = hit.normal;
			float speed = velocity.magnitude;
			float dot = Vector3.Dot(velocity, hit.normal);
			//如果此时速度已经向下就算了
			if (dot > 0f)
			{
				//也能防止球体在从台阶上弹起时被发射
				velocity = (velocity - hit.normal * dot).normalized * speed;
			}

			return true;
		}
	}
}