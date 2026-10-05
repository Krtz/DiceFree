using DiceFree.Foundation;
using DiceFree.World;
using DiceFree.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.UI
{
    [DefaultExecutionOrder(-100)]
    public sealed class OptionsPanel : HudWidget
    {
        private bool open;
        private Vector2 scroll;
        private string feedback="Choose Rebind, then press a key or mouse button. Escape cancels.";
        private InputAction escape;
        private InputActionRebindingExtensions.RebindingOperation capture;
        private int suppressFrame=-1;
        public static string SaveFeedback { get; set; }
        public bool IsOpen => open;
        public bool Capturing => capture!=null;
        public override Rect Bounds => open ? new Rect(0,0,Screen.width,Screen.height) : new Rect(Screen.width-130,8,120,28);
        private void Awake() => escape=new InputAction("System menu",binding:"<Keyboard>/escape");
        protected override void OnEnable() { base.OnEnable(); escape.Enable(); }
        protected override void OnDisable() { CancelCapture(); open=false; ControlBindings.BlockGameplay=false; escape.Disable(); base.OnDisable(); }
        private void OnDestroy() => escape.Dispose();
        private void Update()
        {
            if(escape.WasPressedThisFrame()) RouteEscape();
            ControlBindings.BlockGameplay=open || capture!=null || Time.frameCount==suppressFrame;
        }
        public EscapeRoute RouteEscape()
        {
            var inventory=GetComponent<InventoryPanel>();
            var interaction=GetComponent<Interactor>();
            var selection=GetComponent<TargetSelection>();
            var route=EscapeRouting.Choose(Capturing,open,inventory!=null && inventory.IsOpen,interaction!=null && interaction.Active!=null,selection!=null && selection.Selected!=null);
            switch(route)
            {
                case EscapeRoute.CancelCapture: CancelCapture(); break;
                case EscapeRoute.CloseOptions: Close(); break;
                case EscapeRoute.CloseInventory: inventory.Close(); break;
                case EscapeRoute.CloseInteraction: interaction.Cancel(); break;
                case EscapeRoute.ClearTarget: selection.Select(null); GetComponent<BasicAttack>()?.Cancel(); break;
                case EscapeRoute.OpenOptions: Open(); break;
            }
            suppressFrame=Time.frameCount;
            return route;
        }
        public void Open() { open=true; ControlBindings.BlockGameplay=true; GetComponent<DiceFree.Characters.TraversalMotor>()?.Stop(); GetComponent<BasicAttack>()?.Cancel(); GetComponent<Interactor>()?.Cancel(); }
        public void Close() { CancelCapture(); open=false; suppressFrame=Time.frameCount; ControlBindings.BlockGameplay=true; }
        public void CancelCapture()
        {
            if(capture==null) return;
            var operation=capture; capture=null; operation.Dispose();
            feedback="Rebind cancelled."; suppressFrame=Time.frameCount;
        }
        public void BeginCapture(ControlBinding entry)
        {
            CancelCapture();
            feedback="Press a key or mouse button for " + entry.Label + ". Escape cancels.";
            capture=entry.Action.PerformInteractiveRebinding(entry.Index)
                .WithExpectedControlType("Button")
                .WithControlsExcluding("<Keyboard>/escape")
                .WithControlsExcluding("<Pointer>/position")
                .WithControlsExcluding("<Pointer>/delta")
                .WithControlsExcluding("<Mouse>/scroll")
                .OnApplyBinding((operation,path)=>
                {
                    if(ControlBindings.Set(entry,path,out var message)) feedback="Bound " + entry.Label + " to " + entry.Display + ".";
                    else feedback=message;
                })
                .OnComplete(operation=> { capture=null; operation.Dispose(); suppressFrame=Time.frameCount; });
            capture.Start();
        }
        private void OnGUI()
        {
            if(!open) { if(GUI.Button(Bounds,"Options [Esc]")) Open(); return; }
            GUI.Box(Bounds,GUIContent.none);
            float width=Mathf.Min(700,Screen.width-24), height=Mathf.Min(650,Screen.height-24);
            GUILayout.BeginArea(new Rect((Screen.width-width)/2,(Screen.height-height)/2,width,height),GUI.skin.box);
            GUILayout.BeginHorizontal(); GUILayout.Label("Options / Controls");
            if(GUILayout.Button("Close [Esc]",GUILayout.Width(110))) Close(); GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(); GUILayout.Label("Controls / Keybindings");
            GUI.enabled=false; GUILayout.Button("Audio (future)"); GUILayout.Button("Graphics (future)"); GUI.enabled=true; GUILayout.EndHorizontal();
            GUILayout.Label("Escape: cancel capture, close menu / inventory / interaction, clear target, then open Options.",new GUIStyle(GUI.skin.label){wordWrap=true});
            GUILayout.Label(feedback,new GUIStyle(GUI.skin.label){wordWrap=true});
            if(!string.IsNullOrEmpty(SaveFeedback)) GUILayout.Label(SaveFeedback);
            if(Capturing && GUILayout.Button("Cancel capture")) CancelCapture();
            scroll=GUILayout.BeginScrollView(scroll);
            foreach(var entry in ControlBindings.Entries)
            {
                GUILayout.BeginHorizontal(); GUILayout.Label(entry.Label,GUILayout.Width(width*0.43f)); GUILayout.Label(entry.Display,GUILayout.Width(width*0.22f));
                GUI.enabled=!Capturing && entry.Rebindable;
                if(GUILayout.Button("Rebind")) BeginCapture(entry);
                if(GUILayout.Button("Reset")) { if(ControlBindings.Reset(entry,out var message)) feedback="Reset " + entry.Label + "."; else feedback=message; }
                GUI.enabled=true; GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUI.enabled=!Capturing;
            if(GUILayout.Button("Reset all controls to defaults")) { ControlBindings.ResetAll(); feedback="All controls reset."; }
            GUI.enabled=true;
            GUILayout.EndArea();
        }
    }
}
