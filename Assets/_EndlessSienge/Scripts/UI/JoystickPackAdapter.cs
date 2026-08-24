using UnityEngine;
using Game.Core;

namespace Game.UI
{
    public class JoystickPackAdapter : MonoBehaviour, IMovementInput
    {
        [SerializeField] private Joystick joystick;

        public Vector2 GetInput()
        {
            if (joystick == null) return Vector2.zero;
            return new Vector2(joystick.Horizontal, joystick.Vertical);
        }
    }
}
