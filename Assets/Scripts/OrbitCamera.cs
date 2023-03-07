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
		[SerializeField, Range(-89f, 89f)] private float minVerticalAngle = -30f, maxVerticalAngle = 60f;
		[SerializeField, Min(0f)] private float alignDelay = 5f;
		[SerializeField, Range(0, 90f)] private float alignSmoothRange = 45f;

		private InputsManager inputManager;

		private Vector3 focusPoint, previousFocusPoint;
		private Vector2 orbitAngles = new Vector2(22.5f, 0f);
		private float lastManualRotationTime;

		private void Awake()
		{
			inputManager = InputsManager.Instance;
			focusPoint = focus.position;
			transform.localRotation = Quaternion.Euler(orbitAngles);
		}

		private void OnDestroy()
		{
			inputManager?.OnDestroy();
		}

		private void OnValidate()
		{
			if (maxVerticalAngle < minVerticalAngle)
			{
				maxVerticalAngle = minVerticalAngle;
			}
		}

		private void LateUpdate()
		{
			previousFocusPoint = focusPoint;
			UpdateFocusPoint();
			Quaternion lookRotation;
			if (ManualRotation() || AutomaticRotation())
			{
				ConstrainAngles();
				lookRotation = Quaternion.Euler(orbitAngles);
			}
			else
			{
				lookRotation = transform.localRotation;
			}

			Vector3 lookDirection = lookRotation * Vector3.forward;
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

		private bool ManualRotation()
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

			(input.x, input.y) = (-input.y * intensity, input.x * intensity);

			const float e = float.Epsilon;
			if (input.x is < -e or > e || input.y is < -e or > e)
			{
				orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
				lastManualRotationTime = Time.unscaledTime;
				return true;
			}

			return false;
		}

		private void ConstrainAngles()
		{
			orbitAngles.x = Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

			if (orbitAngles.y < 0.0f)
			{
				orbitAngles.y += 360f;
			}
			else if (orbitAngles.y >= 360f)
			{
				orbitAngles.y -= 360.0f;
			}
		}

		private static float GetAngle(Vector2 direction)
		{
			float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
			return direction.x < 0f ? 360f - angle : angle;
		}

		private bool AutomaticRotation()
		{
			if (Time.unscaledTime - lastManualRotationTime < alignDelay)
			{
				return false;
			}

			Vector2 movement = new Vector2(
				focusPoint.x - previousFocusPoint.x,
				focusPoint.z - previousFocusPoint.z
			);
			float movementDeltaSqr = movement.sqrMagnitude;
			if (movementDeltaSqr < 0.0001f)
			{
				return false;
			}

			float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
			float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
			float rotationChange = rotationSpeed *  Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
			if (deltaAbs < alignSmoothRange)
			{
				rotationChange *= deltaAbs / alignSmoothRange;
			}
			else if (180f - deltaAbs < alignSmoothRange)
			{
				rotationChange *= (180f - deltaAbs) / alignSmoothRange;
			}

			orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);

			return true;
		}
	}
}