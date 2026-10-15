using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Foundation;
using DiceFree.Input;
using DiceFree.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.Development
{
    /// <summary>Visual-only fold between independently navigated test faces.</summary>
    [DefaultExecutionOrder(600),DisallowMultipleComponent]
    public sealed class CubeEdgePresentation : MonoBehaviour
    {
        const string LockId="presentation.cube-edge";
        public float turnSeconds=2.2f;
        public bool Turning {get;private set;}
        public bool VistaVisible=>vistaWeight>.8f&&!Turning;
        public float Angle {get;private set;}
        public int RotationSamples {get;private set;}
        public int CompletedFlips {get;private set;}
        public float LastSignedTurn {get;private set;}
        public CubeEdgeMap Edge {get;private set;}
        public Transform Neighbour=>neighbour!=null?neighbour.transform:null;
        public bool ControlsLocked=>Turning&&!room.player.CanAct&&!room.player.Motor.MotionAllowed;
#if UNITY_EDITOR
        public bool CaptureHold {get;set;}
#endif
        DevelopmentRoomController room;
        GameObject neighbour,pivot,avatar;
        float vistaWeight;
        bool armed=true,committed;
        string turningFace;
        readonly Dictionary<Renderer,bool> renderStates=new();
        readonly Dictionary<Behaviour,bool> behaviourStates=new();
        readonly Dictionary<InputAction,bool> inputStates=new();
        RunLoadoutLock loadout;
        bool priorLoadout,createdLoadout;
        InputBindings bindings;
        Vector3 heldPosition;
        Quaternion heldRotation;
        float heldFov;
        private void Awake()=>room=GetComponent<DevelopmentRoomController>();
        public void Bind(CubeEdgeMap edge,bool arm=true)
        {
            if(neighbour!=null)Destroy(neighbour);
            Edge=edge;armed=arm;vistaWeight=0;
            if(edge==null||edge.adjacentPreview==null)return;
            neighbour=Instantiate(edge.adjacentPreview,transform);
            neighbour.name="Visible neighbouring cube face (visual only)";
            neighbour.transform.SetPositionAndRotation(edge.Origin+new Vector3(0,-edge.halfSize,edge.Sign*edge.halfSize),
                Quaternion.Euler(edge.Sign*90,0,0));
        }
        public bool CanStart(string destination)=>!Turning&&Edge!=null&&Edge.facePreview!=null&&
            Edge.adjacentPreview!=null&&destination==Edge.destinationScene;
        private void Update()
        {
            if(room==null||room.Map==null||room.Busy||Edge==null)return;
            float local=Edge.Sign*(room.player.transform.position.z-Edge.Origin.z);
            if(local<Edge.halfSize-4.5f)armed=true;
            if(armed&&Edge.AtCrossing(room.player.transform.position))room.Travel(Edge.destinationScene);
        }
        void VistaPose(out Vector3 position,out Quaternion rotation,out float fov)
        {
            var focus=Edge.Pivot+Vector3.down*8;
            focus.x+=Mathf.Clamp(room.player.transform.position.x-Edge.Origin.x,-9,9);
            rotation=Quaternion.Euler(30,Edge.Sign>0?180:0,0);
            position=focus-rotation*Vector3.forward*58;fov=60;
        }
        private void LateUpdate()
        {
            if(room==null||room.explorationCamera==null||Edge==null)return;
            if(Turning){room.explorationCamera.SetPresentationPose(heldPosition,heldRotation,heldFov);return;}
            float local=Edge.Sign*(room.player.transform.position.z-Edge.Origin.z);
            float desired=armed?Mathf.InverseLerp(Edge.halfSize-12,Edge.halfSize-3,local):0;
            vistaWeight=Mathf.MoveTowards(vistaWeight,desired,Time.unscaledDeltaTime*1.5f);
            if(vistaWeight<.001f){if(room.explorationCamera.PresentationOverride)room.explorationCamera.ClearPresentationPose();return;}
            room.explorationCamera.GetFollowPose(room.player.transform,out var normalPosition,out var normalRotation,out var normalFov);
            VistaPose(out var edgePosition,out var edgeRotation,out var edgeFov);
            float t=Mathf.SmoothStep(0,1,vistaWeight);
            room.explorationCamera.SetPresentationPose(Vector3.Lerp(normalPosition,edgePosition,t),Quaternion.Slerp(normalRotation,edgeRotation,t),Mathf.Lerp(normalFov,edgeFov,t));
        }
        void SaveBehaviour(Behaviour value)
        {
            if(value==null||behaviourStates.ContainsKey(value))return;
            behaviourStates[value]=value.enabled;value.enabled=false;
        }
        void HideRenderer(Renderer value)
        {
            if(value==null||renderStates.ContainsKey(value))return;
            renderStates[value]=value.enabled;value.enabled=false;
        }
        public void Begin()
        {
            if(Edge==null||Turning)throw new InvalidOperationException("Cube edge is not ready.");
            committed=false;Turning=true;Angle=0;RotationSamples=0;turningFace=Edge.link.destinationFaceId;
            heldPosition=room.explorationCamera.transform.position;heldRotation=room.explorationCamera.transform.rotation;
            heldFov=room.explorationCamera.GetComponent<Camera>().fieldOfView;
            bindings=InputBindings.Current;
            foreach(var mapName in new[]{"Gameplay","Camera"})
                foreach(var action in bindings.Asset.FindActionMap(mapName,true).actions){inputStates[action]=action.enabled;action.Disable();}
            room.player.SetActionBlocked(LockId,true);room.player.Motor.SetMotionBlocked(LockId,true);
            loadout=room.player.GetComponent<RunLoadoutLock>();createdLoadout=loadout==null;
            if(createdLoadout)loadout=room.player.gameObject.AddComponent<RunLoadoutLock>();
            priorLoadout=loadout.Locked;loadout.Locked=true;
            foreach(var component in room.player.GetComponents<MonoBehaviour>())
            {
                if(component is IQueuedOrders queue)queue.ClearOrders();
                if(component is TraversalInput||component is CombatInput||component is GenericActionBar||component is SkillTargetingController||component is InventoryPanel)SaveBehaviour(component);
            }
            room.player.GetComponent<InventoryPanel>()?.Close();
            room.player.GetComponent<ReturnHomeAbility>()?.Cancel();
            var visual=room.player.GetComponent<ClassPresentationSwitcher>().ActiveVisualRoot;
            avatar=Instantiate(visual.gameObject,transform);avatar.name="Crossing Echo (presentation only)";
            foreach(var b in avatar.GetComponentsInChildren<MonoBehaviour>(true))if(b!=null)b.enabled=false;
            foreach(var a in avatar.GetComponentsInChildren<Animator>(true))a.enabled=false;
            foreach(var c in avatar.GetComponentsInChildren<Collider>(true)){c.enabled=false;Destroy(c);}
            avatar.transform.position=Edge.Pivot-Vector3.forward*(Edge.Sign*2);
            var parentScale=transform.lossyScale;
            avatar.transform.localScale=new Vector3(visual.lossyScale.x/parentScale.x,visual.lossyScale.y/parentScale.y,visual.lossyScale.z/parentScale.z);
            foreach(var feedback in room.Map.GetComponentsInChildren<ActorFeedback>(true))SaveBehaviour(feedback);
            SaveBehaviour(room.player.GetComponent<ActorFeedback>());
            foreach(var bars in room.gameObject.scene.GetRootGameObjects().SelectMany(root=>root.GetComponentsInChildren<OverheadUnitHealthBars>(true)))SaveBehaviour(bars);
            foreach(var r in room.Map.GetComponentsInChildren<Renderer>(true))HideRenderer(r);
            foreach(var r in room.player.GetComponentsInChildren<Renderer>(true).Where(r=>r.gameObject.layer!=31))HideRenderer(r);
            foreach(var r in room.fog.GetComponentsInChildren<Renderer>(true))HideRenderer(r);
            SaveBehaviour(room.fog);
            pivot=new GameObject("Cube quarter-turn pivot");pivot.transform.SetParent(transform);pivot.transform.position=Edge.Pivot;
            var source=Instantiate(Edge.facePreview,pivot.transform);source.name="Source face surrogate";
            source.transform.SetPositionAndRotation(Edge.Origin,Quaternion.identity);
            neighbour.transform.SetParent(pivot.transform,true);

        }
        public IEnumerator Warmup()
        {
            var startPosition=heldPosition;var startRotation=heldRotation;float startFov=heldFov;
            VistaPose(out var targetPosition,out var targetRotation,out var targetFov);
            float elapsed=0;while(elapsed<.55f){elapsed+=Time.unscaledDeltaTime;float t=Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/.55f));
                heldPosition=Vector3.Lerp(startPosition,targetPosition,t);heldRotation=Quaternion.Slerp(startRotation,targetRotation,t);heldFov=Mathf.Lerp(startFov,targetFov,t);yield return null;}
        }
        public void SuppressDestination(DevelopmentMap destination)
        {
            foreach(var light in destination.GetComponentsInChildren<Light>(true))SaveBehaviour(light);
            foreach(var feedback in destination.GetComponentsInChildren<ActorFeedback>(true))SaveBehaviour(feedback);
        }
        public IEnumerator Rotate()
        {
            float elapsed=0;
            while(elapsed<turnSeconds)
            {
                elapsed+=Time.unscaledDeltaTime;float t=Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/turnSeconds));
#if UNITY_EDITOR
                if(CaptureHold&&t>=.5f){t=.5f;elapsed=turnSeconds*.5f;}
#endif
                Angle=90*t;pivot.transform.rotation=Quaternion.AngleAxis(-Edge.Sign*Angle,Vector3.right);
                var step=t<=.5f?Quaternion.AngleAxis(-Edge.Sign*Angle,Vector3.right)*(Vector3.forward*(-Edge.Sign*2*(1-2*t))):
                    Quaternion.AngleAxis(Edge.Sign*(90-Angle),Vector3.right)*(Vector3.forward*(Edge.Sign*3*(2*t-1)));
                avatar.transform.position=Edge.Pivot+step;RotationSamples++;yield return null;
            }
            Angle=90;pivot.transform.rotation=Quaternion.AngleAxis(-Edge.Sign*90,Vector3.right);
            LastSignedTurn=-Edge.Sign*90;
        }
        public void Commit(CubeEdgeMap destination)
        {
            var syntheticArrival=Edge.Pivot+Vector3.forward*(Edge.Sign*3);
            heldPosition+=room.player.transform.position-syntheticArrival;
            foreach(var light in Edge.GetComponentsInChildren<Light>(true))SaveBehaviour(light);
            foreach(var pair in renderStates)if(pair.Key!=null)pair.Key.enabled=pair.Value;
            renderStates.Clear();
            // Destination lights and feedback may resume; source objects are about to unload.
            foreach(var pair in behaviourStates.Where(pair=>pair.Key!=null&&pair.Key.transform.IsChildOf(destination.transform)).ToArray())
                {pair.Key.enabled=pair.Value;behaviourStates.Remove(pair.Key);}
            if(pivot!=null)Destroy(pivot);pivot=null;neighbour=null;
            if(avatar!=null)Destroy(avatar);avatar=null;
            Bind(destination,false);committed=true;
        }
        public IEnumerator Settle()
        {
            var startPosition=heldPosition;var startRotation=heldRotation;float startFov=heldFov;
            room.explorationCamera.GetFollowPose(room.player.transform,out var endPosition,out var endRotation,out var endFov);
            float elapsed=0;while(elapsed<.7f){elapsed+=Time.unscaledDeltaTime;float t=Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/.7f));
                heldPosition=Vector3.Lerp(startPosition,endPosition,t);heldRotation=Quaternion.Slerp(startRotation,endRotation,t);heldFov=Mathf.Lerp(startFov,endFov,t);yield return null;}
            CompletedFlips++;
        }
        public void End()
        {
            foreach(var pair in renderStates)if(pair.Key!=null)pair.Key.enabled=pair.Value;renderStates.Clear();
            foreach(var pair in behaviourStates)if(pair.Key!=null)pair.Key.enabled=pair.Value;behaviourStates.Clear();
            if(bindings!=null&&bindings.Asset!=null&&!bindings.Suppressed)
                foreach(var pair in inputStates){if(pair.Value)pair.Key.Enable();else pair.Key.Disable();}
            inputStates.Clear();
            if(room!=null&&room.player!=null){room.player.SetActionBlocked(LockId,false);room.player.Motor.SetMotionBlocked(LockId,false);}
            if(loadout!=null){loadout.Locked=priorLoadout;if(createdLoadout)Destroy(loadout);}loadout=null;
            if(avatar!=null)Destroy(avatar);avatar=null;
            if(pivot!=null){if(neighbour!=null)neighbour.transform.SetParent(transform,true);Destroy(pivot);}pivot=null;
            if(!committed&&Edge!=null)Bind(Edge,false);
            Turning=false;
            if(room!=null&&room.explorationCamera!=null)room.explorationCamera.ClearPresentationPose();
        }
        private void OnDisable(){if(Turning)End();else if(room!=null&&room.explorationCamera!=null)room.explorationCamera.ClearPresentationPose();}
        private void OnGUI()
        {
            if(!VistaVisible&&!Turning)return;
            string face=Turning?turningFace:Edge?.link.destinationFaceId;
            string world=face=="world.test.2"?"World 2":"World 1";
            GUI.Box(new Rect(Screen.width*.30f,15,Screen.width*.40f,45),Turning?"Turning toward "+world:world+" is on the neighbouring face\nCross the glowing seam to travel.");
        }
    }
}
