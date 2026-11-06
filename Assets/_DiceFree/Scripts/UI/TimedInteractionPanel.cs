using DiceFree.World;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class TimedInteractionPanel : HudWidget
    {
        [SerializeField] Interactor interactor;
        TimedInteractionTarget Target=>interactor?.Active as TimedInteractionTarget;
        public override Rect Bounds
        {
            get{float width=Mathf.Min(370,Screen.width-24);return Target?.Worker!=null&&!HudPointerBlocker.ModalOpen?
                new Rect((Screen.width-width)*.5f,Screen.height*.57f,width,116):new Rect();}
        }
        void OnGUI()
        {
            var target=Target;var r=Bounds;if(target==null||r.width==0)return;
            var theme=HudLayoutManager.Current?.Theme??HudThemes.Get(HudThemes.CompactStone);
            int old=GUI.depth;GUI.depth=-120;
            try
            {
                HudChrome.DrawPanel(r,theme);HudChrome.DrawHeader(new Rect(r.x+5,r.y+5,r.width-10,27),theme,target.Verb);
                GUI.Box(new Rect(r.x+12,r.y+43,r.width-24,20),GUIContent.none);
                var previous=GUI.color;GUI.color=new Color(.27f,.77f,.62f);
                GUI.DrawTexture(new Rect(r.x+14,r.y+45,(r.width-28)*target.Progress,16),Texture2D.whiteTexture);GUI.color=previous;
                GUI.Label(new Rect(r.x+14,r.y+68,220,25),"Move or take damage to interrupt");
                if(GUI.Button(new Rect(r.xMax-112,r.y+73,100,28),"Cancel")){interactor.Cancel();target.CancelWork();}
            }
            finally{GUI.depth=old;}
        }
        public void Configure(Interactor value)=>interactor=value;
    }
}
