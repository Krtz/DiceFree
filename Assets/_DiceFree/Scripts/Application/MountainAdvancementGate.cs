using System;
using System.Collections;
using System.Linq;
using DiceFree.Advancement;
using DiceFree.Characters;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace DiceFree.World
{
    /// <summary>
    /// Travel between the already-running Cornberg session and a separate loaded
    /// mountain trial scene. Player/HUD/manifestations remain alive across the move.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MountainTrialTraveller : MonoBehaviour
    {
        private const string DefaultTrialScene = "NoviceMountainTrial";
        private static readonly Vector3 TrialEntry = new(10000f, 0.20f, 10000f);
        private static readonly Vector3 CornbergReturn = new(-42f, 2.8f, -30f);
        private TraversalMotor motor;
        private AdvancementController advancement;
        private bool busy;
        private string sceneName = DefaultTrialScene;
        public bool InsideTrial { get; private set; }
        public bool Busy => busy;
        public string TravelDiagnostic { get; private set; } = "idle";

        private void Awake()
        {
            motor = GetComponent<TraversalMotor>();
            advancement = GetComponent<AdvancementController>();
            SceneManager.sceneLoaded += OnMapLoaded;
            SceneManager.sceneUnloaded += OnMapUnloaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnMapLoaded;
            SceneManager.sceneUnloaded -= OnMapUnloaded;
        }

        private void OnMapLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!busy || InsideTrial || scene.name != sceneName) return;
            FinishEntering();
        }

        private void OnMapUnloaded(Scene scene)
        {
            if (scene.name == sceneName && !InsideTrial)
            {
                busy = false;
                TravelDiagnostic = "returned to Cornberg";
            }
        }

        public bool Eligible =>
            advancement != null &&
            advancement.CurrentClassId == "class.novice" &&
            advancement.Definitions.Any(advancement.CanAdvance);

        public void Enter(string scene)
        {
            if (!Eligible || InsideTrial || busy) return;
            sceneName = string.IsNullOrWhiteSpace(scene) ? DefaultTrialScene : scene;
            busy = true;
            TravelDiagnostic = "starting async load for " + sceneName;

            var existing = SceneManager.GetSceneByName(sceneName);
            if (existing.isLoaded)
            {
                FinishEntering();
                return;
            }

            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (operation == null)
            {
                TravelDiagnostic = "cannot load trial scene " + sceneName;
                busy = false;
                return;
            }

            // The sceneLoaded event handles map transfers. AsyncOperation.completed
            // is a defensive second route and deliberately guards against duplicate entry.
            operation.completed += _ =>
            {
                if (busy && !InsideTrial) FinishEntering();
            };
        }

        private void FinishEntering()
        {
            if (!busy || InsideTrial) return;
            TravelDiagnostic = "scene loaded, starting NavMesh transfer";
            motor.Stop();

            if (!motor.TeleportAcrossMaps(TrialEntry))
            {
                TravelDiagnostic = "entry transfer failed (new NavMesh agent did not bind)";
                Debug.LogWarning(TravelDiagnostic, this);
            }
            else
            {
                InsideTrial = true;
                TravelDiagnostic = "inside trial at " + transform.position;
            }
            busy = false;
        }

        public bool Exit()
        {
            if (!InsideTrial || busy) return false;
            busy = true;
            TravelDiagnostic = "returning to Cornberg";
            motor.Stop();

            if (!motor.TeleportAcrossMaps(CornbergReturn))
            {
                TravelDiagnostic = "Cornberg return NavMesh transfer failed";
                Debug.LogWarning(TravelDiagnostic, this);
                busy = false;
                return false;
            }

            InsideTrial = false;
            var trial = SceneManager.GetSceneByName(sceneName);
            if (!trial.isLoaded)
            {
                busy = false;
                return true;
            }

            var unloading = SceneManager.UnloadSceneAsync(trial);
            if (unloading == null) busy = false;
            else unloading.completed += _ =>
            {
                busy = false;
                TravelDiagnostic = "returned to Cornberg";
            };
            return true;
        }

        private void Update()
        {
            if (!InsideTrial || busy || Keyboard.current == null) return;
            if (Keyboard.current.eKey.wasPressedThisFrame) Exit();
        }

        private void OnGUI()
        {
            if (!InsideTrial || busy) return;
            GUI.Box(new Rect(Screen.width*.5f-230,15,460,53),
                "Mountain Trial — choose your Way to advance\nPress E to return to Cornberg");
        }
    }

    /// <summary>Separate animatable gate leaves; door opens for eligible Novice.</summary>
    [DisallowMultipleComponent]
    public sealed class MountainAdvancementGate : MonoBehaviour
    {
        [SerializeField] private string trialSceneName = "NoviceMountainTrial";
        [SerializeField] private int minimumLevel = 10;
        [SerializeField] private Transform leftDoor;
        [SerializeField] private Transform rightDoor;
        private Quaternion leftClosed,rightClosed;
        private MountainTrialTraveller traveller;

        public void Configure(string scene,int level)
        {
            trialSceneName=scene;minimumLevel=Mathf.Max(1,level);
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                if (t.name=="DoorLeft") leftDoor=t;
                if (t.name=="DoorRight") rightDoor=t;
            }
        }

        private void Start()
        {
            leftClosed=leftDoor != null ? leftDoor.localRotation : Quaternion.identity;
            rightClosed=rightDoor != null ? rightDoor.localRotation : Quaternion.identity;
            traveller=FindAnyObjectByType<MountainTrialTraveller>();
        }

        private void Update()
        {
            if(traveller==null)return;
            bool open=traveller.Eligible;
            if (leftDoor!=null)
                leftDoor.localRotation=Quaternion.Slerp(leftDoor.localRotation,
                    leftClosed*Quaternion.Euler(0,open?-74f:0,0),Time.deltaTime*3f);
            if(rightDoor!=null)
                rightDoor.localRotation=Quaternion.Slerp(rightDoor.localRotation,
                    rightClosed*Quaternion.Euler(0,open?74f:0,0),Time.deltaTime*3f);
            if (!open || traveller.InsideTrial || traveller.Busy) return;
            float range=Vector3.Distance(
                traveller.transform.position,transform.position);
            if (range>25f)return;
            if (Keyboard.current!=null && Keyboard.current.eKey.wasPressedThisFrame)
                traveller.Enter(trialSceneName);
        }

        private void OnGUI()
        {
            if(traveller==null || traveller.InsideTrial || traveller.Busy)return;
            float range=Vector3.Distance(traveller.transform.position,transform.position);
            if (range>25f)return;
            string message=traveller.Eligible
                ? "Novice Mountain — Gate open. Press E to enter the separate advancement trial."
                : $"Novice Mountain — sealed. Reach level {minimumLevel} as a Novice to enter.";
            GUI.Box(new Rect(Screen.width*.5f-250,78,500,37),message);
        }
    }
}
