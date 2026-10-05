using DiceFree.Foundation;
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
            zoom = ControlBindings.Create("zoom");
            vista = ControlBindings.Create("vista");
            pan = ControlBindings.Create("pan");
            drag = ControlBindings.Create("drag");
            pointerDelta = ControlBindings.Create("pointerDelta");
            recenter = ControlBindings.Create("recenter");
            followToggle = ControlBindings.Create("followToggle");
            rotate = ControlBindings.Create("rotate");
        }
        private void OnEnable()
        {
            zoom.Enable(); vista.Enable(); pan.Enable(); drag.Enable(); pointerDelta.Enable();
            recenter.Enable(); followToggle.Enable(); rotate.Enable(); initialized = false;
        }
        private void OnDisable()
        {
            zoom.Disable(); vista.Disable(); pan.Disable(); drag.Disable(); pointerDelta.Disable();
            recenter.Disable(); followToggle.Disable(); rotate.Disable();
        }
        private void OnDestroy()
        {
            ControlBindings.Release(zoom); ControlBindings.Release(vista); ControlBindings.Release(pan); ControlBindings.Release(drag); ControlBindings.Release(pointerDelta);
            ControlBindings.Release(recenter); ControlBindings.Release(followToggle); ControlBindings.Release(rotate);
        }
        private void LateUpdate()
        {
            if (ControlBindings.BlockGameplay) return;
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
            transform.position = initialized ? Vector3.SmoothDamp(transform.position, desired, ref velocity, 0.13f) : desired;
            transform.rotation = initialized ? Quaternion.Slerp(transform.rotation, rotation, 12f * Time.deltaTime) : rotation;
            initialized = true;
        }
        public void Configure(Transform follow) => target = follow;
    }
}
