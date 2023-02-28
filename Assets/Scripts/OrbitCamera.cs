using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts
{
	[RequireComponent(typeof(Camera))]
	public class OrbitCamera : MonoBehaviour
	{
		[SerializeField] private Transform focus = default;
		[SerializeField, Range(1f, 20f)] private float distance = 5f;
		[SerializeField, Min(0f)] private float focusRadius = 1f;
		[SerializeField, Range(0f, 1f)] private float focusCentering = 0.5f;
		[SerializeField, Range(1f, 360f)] private float rotationSpeed = 90f;

		private InputsManager inputManager;


		private Vector3 focusPoint;
		private Vector2 orbitAngles = new Vector2(45f, 0f);

		private void Awake()
		{
			inputManager = InputsManager.Instance;
			focusPoint = focus.position;
		}

		private void OnDestroy()
		{
			inputManager?.OnDestroy();
		}

		void LateUpdate()
		{
			UpdateFocusPoint();
			ManualRotation();
			Quaternion lookRotation = Quaternion.Euler(orbitAngles);
			Vector3 lookDirection = lookRotation * transform.forward;
			Vector3 lookPosition = focusPoint - lookDirection * distance;
			transform.SetPositionAndRotation(lookPosition, lookRotation);
		}

		private void UpdateFocusPoint()
		{
			Vector3 targetPoint = focus.position;
			if (focusRadius > 0.0f)
			{
				float distance = Vector3.Distance(targetPoint, focusPoint);
				float t = 1;
				if (distance > 0.01f && focusCentering > 0.0f)
				{
					t = Mathf.Pow(1 - focusCentering, Time.unscaledDeltaTime);
				}

				if (distance > focusRadius)
				{
					t = Mathf.Min(t, focusRadius / distance);
				}

				focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
			}
			else
			{
				focusPoint = targetPoint;
			}
		}

		private void ManualRotation()
		{
			Vector2 input = Vector2.zero;
			float intensity = 0.0f;

			Mouse mouse = Mouse.current;
			Gamepad gamepad = Gamepad.current;

			input = inputManager.lookAction.ReadValue<Vector2>();
			if (mouse != null && mouse.rightButton.isPressed
			                  && mouse.delta.magnitude > 0)
			{
				intensity = 0.05f;
			}
			else if (gamepad != null && gamepad.rightStick.magnitude > 0)
			{
				input.x *= -1f;
				intensity = 0.5f;
			}

			(input.x, input.y) = (-input.y * intensity, -input.x * intensity);

			const float e = 0.001f;
			if (input.x is < -e or > e || input.y is < -e or > e)
			{
				orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
			}
		}
	}
}