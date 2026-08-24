using UnityEngine;
using Game.Core;
using Game.UI;

namespace Game.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private JoystickPackAdapter movementInputSource;

        [Min(0f)] [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private bool rotateTowardsMovement = true;
        [Min(0f)] [SerializeField] private float rotationSpeed = 720f;

        private IMovementInput _movementInput;

        private void Awake()
        {
            _movementInput = movementInputSource;
        }

        private void Update()
        {
            if (_movementInput == null) return;

            Vector2 input = _movementInput.GetInput();
            if (input.sqrMagnitude < 0.0001f) return;

            Vector3 direction = new Vector3(input.x, 0f, input.y);
            transform.position += direction * (moveSpeed * Time.deltaTime);

            if (rotateTowardsMovement)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}
