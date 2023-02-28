using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts
{
	public class InputsManager
	{
		private static InputsManager instance;

		public static InputsManager Instance
		{
			get { return instance ??= new InputsManager().OnInit(); }
		}

		public InputAction moveAction { get; private set; }
		public InputAction lookAction { get; private set; }
		public InputAction jumpAction { get; private set; }

		public InputsManager OnInit()
		{
			instance = this;
			RegisterInputs();
			return this;
		}

		public void OnDestroy()
		{
			instance = null;
		}

		private void RegisterInputs()
		{
			var inputMap = new InputActionMap("InputManager");
			
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