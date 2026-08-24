using UnityEngine;

namespace Game.Core
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 12f, -8f);
        [SerializeField] [Min(0f)] private float smoothSpeed = 5f;
        [SerializeField] private bool lookAtTarget = true;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);

            if (lookAtTarget)
                transform.LookAt(target);
        }
    }
}
