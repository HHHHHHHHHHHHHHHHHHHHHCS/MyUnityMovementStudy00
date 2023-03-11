using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts
{
	[DisallowMultipleComponent]
	public class InputManager : MonoBehaviour
	{
		private static InputManager instance;

		public static InputManager Instance => OnInit();

		public InputAction moveAction { get; private set; }
		public InputAction lookAction { get; private set; }
		public InputAction jumpAction { get; private set; }

		private InputActionMap inputMap;

		private static InputManager OnInit()
		{
			if (instance != null)
			{
				return instance;
			}

			GameObject go = new GameObject("InputsManager")
			{
				hideFlags = HideFlags.DontSave
			};
			DontDestroyOnLoad(go);
			instance = go.AddComponent<InputManager>();
			instance.RegisterInputs();
			return instance;
		}

		private void Awake()
		{
			RegisterInputs();
		}

		private void OnDestroy()
		{
			instance = null;
		}

		private void RegisterInputs()
		{
			if (inputMap != null)
			{
				return;
			}
			
			inputMap = new InputActionMap("InputManager");

			//Renamed "Axis" and "Dpad" or "2DVector" composites to "1D Axis" and "2D Vector" composite.
			moveAction = inputMap.AddAction("move", binding: "<Gamepad>/leftStick");
			moveAction.AddCompositeBinding("Dpad")
				.With("Up", "<Keyboard>/w")
				.With("Up", "<Keyboard>/upArrow")
				.With("Down", "<Keyboard>/s")
				.With("Down", "<Keyboard>/downArrow")
				.With("Left", "<Keyboard>/a")
				.With("Left", "<Keyboard>/leftArrow")
				.With("Right", "<Keyboard>/d")
				.With("Right", "<Keyboard>/rightArrow");

			lookAction = inputMap.AddAction("look", binding: "<Gamepad>/rightStick");
			lookAction.AddBinding("<Mouse>/delta");

			jumpAction = inputMap.AddAction("jump", binding: "<Gamepad>/b");
			jumpAction.AddBinding("<Keyboard>/space");

			moveAction.Enable();
			lookAction.Enable();
			jumpAction.Enable();
		}
	}
}