using UnityEngine;

namespace Scripts
{
	//CopyBy: https://catlikecoding.com/unity/tutorials/movement/
	public class MovingSphere : MonoBehaviour
	{
		[SerializeField, Range(0f, 100f)] private float maxSpeed = 10f;
		[SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f;

		private InputsManager inputManager;

		private Rigidbody body;
		private Vector3 velocity, desiredVelocity;

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
			bool isDown = inputManager.jumpAction.WasPressedThisFrame();
			Debug.Log(isDown);
		}

		private void FixedUpdate()
		{
			velocity = body.velocity;
			float maxSpeedChange = maxAcceleration * Time.fixedDeltaTime;
			velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
			velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
			body.velocity = velocity;
		}
	}
}