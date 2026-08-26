using UnityEngine;
using Game.Core;
using Game.UI;

namespace Game.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private JoystickPackAdapter movementInputSource;
        [Min(0f)] [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float animSmoothTime = 0.1f;

        [Header("Combat Facing")]
        [SerializeField] private bool rotateTowardsEnemy = true;
        [SerializeField] private float rotationSmoothTime = 0.15f;
        [Tooltip("Point the body turns around; usually the muzzle. Empty = player center")]
        [SerializeField] private Transform aimOrigin;

        private IMovementInput _movementInput;
        private Animator _animator;
        private PlayerWeapon _weapon;

        private static readonly int MoveXParam = Animator.StringToHash("MoveX");
        private static readonly int MoveZParam = Animator.StringToHash("MoveZ");

        private float _currentX;
        private float _currentZ;
        private float _xVel;
        private float _zVel;

        private void Awake()
        {
            _movementInput = movementInputSource;
            _animator = GetComponentInChildren<Animator>();
            _weapon = GetComponent<PlayerWeapon>();
        }

        private void Update()
        {
            if (_movementInput == null) return;

            Vector2 input = _movementInput.GetInput();

            if (rotateTowardsEnemy)
                RotateTowardsNearestEnemy();

            if (_animator != null)
            {
                Vector3 forward = transform.forward;
                Vector3 right = transform.right;
                Vector3 worldDir = new Vector3(input.x, 0f, input.y);

                float localX = Vector3.Dot(worldDir, right);
                float localZ = Vector3.Dot(worldDir, forward);

                _currentX = Mathf.SmoothDamp(_currentX, localX, ref _xVel, animSmoothTime);
                _currentZ = Mathf.SmoothDamp(_currentZ, localZ, ref _zVel, animSmoothTime);

                _animator.SetFloat(MoveXParam, _currentX);
                _animator.SetFloat(MoveZParam, _currentZ);
            }

            if (input.sqrMagnitude < 0.0001f) return;

            Vector3 direction = new Vector3(input.x, 0f, input.y);
            transform.position += direction * (moveSpeed * Time.deltaTime);
        }

        private void RotateTowardsNearestEnemy()
        {
            IDamageable target = _weapon != null ? _weapon.CurrentTarget : null;
            if (target == null) return;

            Vector3 origin = aimOrigin != null ? aimOrigin.position : transform.position;
            Vector3 toEnemy = ((MonoBehaviour)target).transform.position - origin;
            toEnemy.y = 0f;

            if (toEnemy.sqrMagnitude < 0.0001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(toEnemy);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRotation, Time.deltaTime / rotationSmoothTime);
        }
    }
}
