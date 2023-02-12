using UnityEngine;

namespace Scripts
{
	public class MovingSphere : MonoBehaviour
	{
		[SerializeField] private Rect allowedArea = new Rect(-4.5f, -4.5f, 9f, 9f);

		[SerializeField, Range(0f, 100f)] private float maxSpeed = 10f;
		[SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f;
		[SerializeField, Range(0f, 1f)] private float bounciness = 0.5f;

		private InputsManager inputManager;

		private Vector3 velocity;

		private void Awake()
		{
			inputManager = InputsManager.Instance;
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
			Vector3 desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
			float maxSpeedChange = maxAcceleration * Time.deltaTime;
			velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
			velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
			Vector3 displacement = velocity * Time.deltaTime;
			Vector3 newPosition = transform.localPosition + displacement;

			if (newPosition.x < allowedArea.xMin)
			{
				newPosition.x = allowedArea.xMin;
				velocity.x = -velocity.x * bounciness;
			}
			else if (newPosition.x > allowedArea.xMax)
			{
				newPosition.x = allowedArea.xMax;
				velocity.x = -velocity.x * bounciness;
			}

			if (newPosition.z < allowedArea.yMin)
			{
				newPosition.z = allowedArea.yMin;
				velocity.z = -velocity.z * bounciness;
			}
			else if (newPosition.z > allowedArea.yMax)
			{
				newPosition.z = allowedArea.yMax;
				velocity.z = -velocity.z * bounciness;
			}

			transform.localPosition = newPosition;
		}
	}
}