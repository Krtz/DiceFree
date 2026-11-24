using System;
using System.Collections;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Foundation;
using DiceFree.Input;
using DiceFree.UI;
using UnityEngine;

namespace DiceFree.ApplicationFlow
{
    [DisallowMultipleComponent]
    public sealed class WorldFaceCrossing : MonoBehaviour
    {
        private const string LockId = "travel.world-face";
        private static WorldFaceCrossing current;
        public static bool Busy => current != null;
        public bool Active { get; private set; }
        public float Angle { get; private set; }
        public int CompletedCrossings { get; private set; }
        public bool WaitingForScene { get; set; }

        private CombatActor actor;
        private ExplorationCamera worldCamera;
        private InputBindings bindings;
        private RunLoadoutLock loadout;
        private bool priorLoadout, createdLoadout, priorInvulnerable, toSecond;
        private Vector3 departureCamera;
        private Quaternion departureRotation;
        private float departureFov, opacity;
        private GameObject model;
        private Camera previewCamera;
        private RenderTexture frame;
        private GUIStyle caption;

        public void Begin(CombatActor player, ExplorationCamera camera, bool towardWorldTwo)
        {
            if (Busy) throw new InvalidOperationException("A world-face crossing is already active.");
            actor = player;
            worldCamera = camera;
            toSecond = towardWorldTwo;
            var prefab = Resources.Load<GameObject>("WorldFaceCrossing/Crossing cube");
            if (prefab == null || actor == null || camera == null)
                throw new InvalidOperationException("The world-face crossing presentation is incomplete.");
            current = this;
            Active = true;
            Angle = towardWorldTwo ? 0 : 90;
            opacity = 0;
            departureCamera = camera.transform.position;
            departureRotation = camera.transform.rotation;
            departureFov = camera.GetComponent<Camera>().fieldOfView;
            priorInvulnerable = actor.Health.Invulnerable;
            actor.Health.Invulnerable = true;
            actor.SetActionBlocked(LockId, true);
            actor.Motor.SetMotionBlocked(LockId, true);
            actor.GetComponent<InventoryPanel>()?.Close();
            actor.GetComponent<ReturnHomeAbility>()?.Cancel();
            loadout = actor.GetComponent<RunLoadoutLock>();
            createdLoadout = loadout == null;
            if (createdLoadout) loadout = actor.gameObject.AddComponent<RunLoadoutLock>();
            priorLoadout = loadout.Locked;
            loadout.Locked = true;
            bindings = InputBindings.Current;
            bindings.SetModal(true);
            HudPointerBlocker.OverlayCovers += Covers;

            model = Instantiate(prefab, new Vector3(120000, 0, 120000), Quaternion.Euler(Angle, 0, 0));
            model.name = "World-face crossing presentation";
            var cameraObject = new GameObject("World-face crossing camera");
            previewCamera = cameraObject.AddComponent<Camera>();
            previewCamera.cullingMask = 1 << 31;
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(.016f, .027f, .038f, 0);
            previewCamera.nearClipPlane = .1f;
            previewCamera.farClipPlane = 150;
            previewCamera.fieldOfView = 39;
            frame = new RenderTexture(1280, 1280, 24, RenderTextureFormat.ARGB32)
            {
                name = "Crossing the Dice", filterMode = FilterMode.Bilinear
            };
            frame.Create();
            previewCamera.targetTexture = frame;
            PosePreview(0);
        }

        private bool Covers(Vector2 _) => Active;

        private void PosePreview(float t)
        {
            var offset = Vector3.Lerp(new Vector3(18, 45, -19), new Vector3(22, 45, -19), t);
            previewCamera.transform.SetPositionAndRotation(model.transform.position + offset,
                Quaternion.LookRotation(-offset, Vector3.up));
        }

        public IEnumerator PullBack()
        {
            var focus = actor.transform.position + Vector3.up * 2;
            var far = focus - departureRotation * Vector3.forward * 130;
            for (float elapsed = 0; elapsed < .75f; elapsed += Time.unscaledDeltaTime)
            {
                float t = Mathf.SmoothStep(0, 1, elapsed / .75f);
                worldCamera.SetPresentationPose(Vector3.Lerp(departureCamera, far, t), departureRotation,
                    Mathf.Lerp(departureFov, 56, t));
                opacity = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.16f, .75f, elapsed));
                yield return null;
            }
            opacity = 1;
        }

        public IEnumerator Turn()
        {
            float start = toSecond ? 0 : 90, end = toSecond ? 90 : 0;
            for (float elapsed = 0; elapsed < 2.1f; elapsed += Time.unscaledDeltaTime)
            {
                float t = Mathf.SmoothStep(0, 1, elapsed / 2.1f);
                Angle = Mathf.Lerp(start, end, t);
                model.transform.rotation = Quaternion.Euler(Angle, 0, 0);
                PosePreview(t);
                yield return null;
            }
            Angle = end;
            model.transform.rotation = Quaternion.Euler(end, 0, 0);
        }

        public IEnumerator Settle()
        {
            worldCamera.GetFollowPose(actor.transform, out var normal, out var rotation, out var fov);
            var far = actor.transform.position + Vector3.up * 2 - rotation * Vector3.forward * 120;
            WaitingForScene = false;
            for (float elapsed = 0; elapsed < .85f; elapsed += Time.unscaledDeltaTime)
            {
                float t = Mathf.SmoothStep(0, 1, elapsed / .85f);
                worldCamera.SetPresentationPose(Vector3.Lerp(far, normal, t), rotation, Mathf.Lerp(56, fov, t));
                opacity = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.05f, .72f, elapsed));
                yield return null;
            }
            opacity = 0;
            CompletedCrossings++;
        }

        public void End()
        {
            if (!Active) return;
            Active = false;
            if (current == this) current = null;
            HudPointerBlocker.OverlayCovers -= Covers;
            if (actor != null)
            {
                actor.SetActionBlocked(LockId, false);
                actor.Motor.SetMotionBlocked(LockId, false);
                actor.Health.Invulnerable = priorInvulnerable;
            }
            if (loadout != null)
            {
                loadout.Locked = priorLoadout;
                if (createdLoadout) Destroy(loadout);
            }
            if (worldCamera != null) worldCamera.ClearPresentationPose();
            if (bindings?.Asset != null && !HudPointerBlocker.ModalOpen) bindings.SetModal(false);
            if (previewCamera != null)
            {
                previewCamera.enabled = false;
                previewCamera.targetTexture = null;
                Destroy(previewCamera.gameObject);
            }
            if (frame != null) { frame.Release(); Destroy(frame); }
            if (model != null) Destroy(model);
            frame = null;
            model = null;
        }

        private void OnDisable() => End();
        private void OnDestroy() => End();

        private void OnGUI()
        {
            if (!Active || frame == null) return;
            GUI.depth = -11000;
            if (Event.current.isMouse || Event.current.isKey || Event.current.type == EventType.ScrollWheel)
            { Event.current.Use(); return; }
            if (Event.current.type != EventType.Repaint) return;
            var previous = GUI.color;
            GUI.color = new Color(.016f, .027f, .038f, opacity);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = new Color(1, 1, 1, opacity);
            GUI.DrawTexture(new Rect(0, -Screen.height*.03f, Screen.width, Screen.height*.92f), frame, ScaleMode.ScaleToFit, true);
            caption ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 24, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(.94f, .89f, .75f) }
            };
            string message = toSecond ? "Crossing to World 2" : "Returning to World 1";
            if (WaitingForScene && LoadingScreen.Busy && Angle >= 89.9f) message = "Preparing World 2";
            GUI.Label(new Rect(0, Screen.height*.87f, Screen.width, 36), message, caption);
            GUI.color = new Color(.35f, .83f, .77f, opacity);
            float progress = toSecond ? Angle/90 : 1-Angle/90;
            GUI.DrawTexture(new Rect(Screen.width*.4f, Screen.height*.93f, Screen.width*.2f*progress, 3),
                Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
