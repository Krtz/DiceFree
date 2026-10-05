using DiceFree.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.Core
{
    [RequireComponent(typeof(Camera))]
    public sealed class ExplorationCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float distance = 34f;
        [SerializeField] private float pitch = 42f;
        [SerializeField] private float yaw = 45f;

        private Vector3 velocity;
        private InputBindings bindings;
        private InputAction zoom, vista, pan, drag, pointerDelta, recenter, followToggle, rotate;
        private Vector3 anchor;
        private bool following = true;
        private bool lookingOut;
        private bool initialized;
        private Camera viewCamera;

        public bool LookingOut => lookingOut;
        public bool Following => following;

        private void Awake()
        {
            viewCamera = GetComponent<Camera>();
            bindings = InputBindings.Current;
            zoom = bindings.Action("Camera/Zoom");
            vista = bindings.Action("Camera/Look toward World 1");
            pan = bindings.Action("Camera/Camera pan");
            drag = bindings.Action("Camera/Camera drag");
            pointerDelta = bindings.Action("Camera/Camera pointer delta");
            recenter = bindings.Action("Camera/Recenter");
            followToggle = bindings.Action("Camera/Toggle follow");
            rotate = bindings.Action("Camera/Rotate camera");
        }

        private void OnEnable() => initialized = false;

        private void LateUpdate()
        {
            if (target == null) return;
            if (!initialized) anchor = target.position;

            if (followToggle.WasPressedThisFrame()) following = !following;
            if (recenter.WasPressedThisFrame()) anchor = target.position;

            yaw += rotate.ReadValue<float>() * 70f * Time.deltaTime;
            var panInput = pan.ReadValue<Vector2>() * (22f * Time.deltaTime);
            if (drag.IsPressed()) panInput -= pointerDelta.ReadValue<Vector2>() * (distance * 0.0012f);

            if (panInput.sqrMagnitude > 0.00001f)
            {
                following = false;
                var basis = Quaternion.Euler(0, yaw, 0);
                anchor += basis * new Vector3(panInput.x, 0, panInput.y);
                anchor.x = Mathf.Clamp(anchor.x, -56, 125);
                anchor.z = Mathf.Clamp(anchor.z, -44, 72);
            }

            if (following) anchor = target.position;
            if (vista.WasPressedThisFrame()) lookingOut = !lookingOut;

            distance = Mathf.Clamp(distance - zoom.ReadValue<float>() * 0.025f, 22f, 58f);
            viewCamera.fieldOfView = Mathf.Lerp(viewCamera.fieldOfView, lookingOut ? 65f : 55f, 8f * Time.deltaTime);

            var rotation = Quaternion.Euler(lookingOut ? 10f : pitch, yaw, 0f);
            var focus = anchor + Vector3.up * (lookingOut ? 6f : 1.5f);
            var desired = focus - rotation * Vector3.forward * distance;
            var delta = desired - focus;
            if (Physics.SphereCast(focus, 0.3f, delta.normalized, out var obstruction, distance, 1 << 9,
                    QueryTriggerInteraction.Ignore))
                desired = focus + delta.normalized * Mathf.Max(2f, obstruction.distance - 0.3f);

            transform.position = initialized
                ? Vector3.SmoothDamp(transform.position, desired, ref velocity, 0.13f)
                : desired;
            transform.rotation = initialized
                ? Quaternion.Slerp(transform.rotation, rotation, 12f * Time.deltaTime)
                : rotation;
            initialized = true;
        }

        public void Configure(Transform follow) => target = follow;
    }
}
