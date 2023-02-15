using UnityEngine;

namespace Scripts
{
	//CopyBy: https://catlikecoding.com/unity/tutorials/movement/
	public class MovingSphere : MonoBehaviour
	{
		[SerializeField, Range(0f, 100f)] private float maxSpeed = 10f;
		[SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f;
		[SerializeField, Range(0f, 100f)] private float maxAirAcceleration = 1f;
		[SerializeField, Range(0f, 10f)] private float jumpHeight = 2f;
		[SerializeField, Range(0, 5)] private int maxAirJumps = 0;


		private InputsManager inputManager;
		private Rigidbody body;
		private Vector3 velocity, desiredVelocity;
		private bool desiredJump;
		private bool onGround;
		private int jumpPhase;

		private void Awake()
		{
			inputManager = InputsManager.Instance;
			body = GetComponent<Rigidbody>();
		}

		private void OnDestroy()
		{
			inputManager?.OnDestroy();
		}

		private void Update()
		{
			Vector2 playerInput = inputManager.moveAction.ReadValue<Vector2>();
			//不用normalized 是 为了轻微输入
			playerInput = Vector2.ClampMagnitude(playerInput, 1.0f);
			desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
			desiredJump |= inputManager.jumpAction.WasPressedThisFrame();
		}

		private void FixedUpdate()
		{
			UpdateState();
			float acceleration = onGround ? maxAcceleration : maxAirAcceleration;
			float maxSpeedChange = acceleration * Time.fixedDeltaTime;
			velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
			velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

			if (desiredJump)
			{
				desiredJump = false;
				Jump();
			}

			body.velocity = velocity;
			onGround = false;
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
			velocity = body.velocity;
			if (onGround)
			{
				jumpPhase = 0;
			}
		}

		private void Jump()
		{
			if (onGround || jumpPhase < maxAirJumps)
			{
				jumpPhase += 1;
				// v0^2 - v1^2 = 2at 因为 最高点的 v1 = 0  g为-9.81
				float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
				// 后续跳的高度会比第一次要矮
				if (velocity.y > 0f)
				{
					jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
				}

				velocity.y += jumpSpeed;
			}
		}


		private void EvaluateCollision(Collision collision)
		{
			for (int i = 0; i < collision.contactCount; i++)
			{
				Vector3 normal = collision.GetContact(i).normal;
				onGround |= normal.y >= 0.9f;
			}
		}
	}
}