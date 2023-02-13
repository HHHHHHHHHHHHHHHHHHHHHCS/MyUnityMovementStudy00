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
			var map = new InputActionMap("Moving Sphere");
			moveAction = map.AddAction("move", binding: "<Gamepad>/leftStick");
			jumpAction = map.AddAction("jump", binding: "<Gamepad>/b");
			//Renamed "Axis" and "Dpad" composites to "1D Axis" and "2D Vector" composite.
			moveAction.AddCompositeBinding("Dpad")
				.With("Up", "<Keyboard>/w")
				.With("Up", "<Keyboard>/upArrow")
				.With("Down", "<Keyboard>/s")
				.With("Down", "<Keyboard>/downArrow")
				.With("Left", "<Keyboard>/a")
				.With("Left", "<Keyboard>/leftArrow")
				.With("Right", "<Keyboard>/d")
				.With("Right", "<Keyboard>/rightArrow");
			moveAction.Enable();
			jumpAction.AddBinding("<Keyboard>/space");
			jumpAction.Enable();
		}
	}
}