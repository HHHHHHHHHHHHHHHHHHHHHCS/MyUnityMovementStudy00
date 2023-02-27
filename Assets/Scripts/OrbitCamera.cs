using System;
using UnityEngine;

namespace Scripts
{
	[RequireComponent(typeof(Camera))]
	public class OrbitCamera : MonoBehaviour
	{
		[SerializeField] private Transform focus = default;
		[SerializeField, Range(1f, 20f)] private float distance = 5f;
		[SerializeField, Min(0f)] private float focusRadius = 1f;
		[SerializeField, Range(0f, 1f)] private float focusCentering = 0.5f;

		private Vector3 focusPoint;

		private void Awake()
		{
			focusPoint = focus.position;
		}

		void LateUpdate()
		{
			UpdateFocusPoint();
			Vector3 lookDirection = transform.forward;
			transform.localPosition = focusPoint - lookDirection * distance;
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
	}
}