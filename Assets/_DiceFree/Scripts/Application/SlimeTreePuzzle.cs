using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace DiceFree.Dungeons
{
    public sealed class SlimeTreePuzzle : MonoBehaviour
    {
        public SlimeDungeonRun Run;
        public Transform centralTree;
        public Transform[] rings;
        public Material blueFoliage;
        readonly List<DungeonSlime> free=new();
        readonly List<DungeonSlime> captured=new();
        bool running,highLevel;
        public int Filled=>captured.Count;
        public void Begin(bool blue){running=true;highLevel=blue;Replenish();}
        void Update()
        {
            if(!running||!Run.Active)return;
            free.RemoveAll(s=>s==null||!s.Actor.Alive);
            foreach(var slime in free.ToArray())
            {
                var ring=rings.FirstOrDefault(r=>!captured.Any(s=>Vector3.Distance(s.transform.position,r.position)<.7f)&&Vector3.Distance(slime.transform.position,r.position)<1.15f);
                if(ring==null)continue;
                slime.Capture(ring.position,centralTree.position);captured.Add(slime);free.Remove(slime);
                if(captured.Count==5)
                {
                    running=false;bool secret=highLevel&&captured.All(s=>s.Blue);
                    centralTree.localScale*=1.45f;
                    if(secret)foreach(var r in centralTree.GetComponentsInChildren<Renderer>()){var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++)if(mats[i]!=null&&mats[i].name.Contains("Foliage"))mats[i]=blueFoliage;r.sharedMaterials=mats;}
                    foreach(var s in free)if(s!=null)Destroy(s.gameObject);free.Clear();Run.SolvePuzzle(secret);return;
                }
            }
            Replenish();
        }
        void Replenish()
        {
            while(free.Count<3)
            {
                bool blue=highLevel&&!free.Any(s=>s.Blue);int slot=0;
                // Separate spawn slots from rings so no replacement auto-captures.
                while(free.Any(s=>Vector3.Distance(s.GetComponent<DiceFree.AI.AggroBehaviour>().Home,transform.position+new Vector3(-8+slot*8,0,-9))<1))slot++;
                var p=transform.position+new Vector3(-8+slot*8,0,-9);
                free.Add(Run.Spawn(blue?Run.Tuning.blue:Run.Tuning.puzzleGreen,p,1,SlimeRole.Puzzle,blue));
            }
        }
    }
}
