using System;
using System.Linq;
using DiceFree.Characters;
using DiceFree.Skills;
using DiceFree.UI;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.EditorTools
{
    internal static class SkillAndMinimapValidation
    {
        private static void Require(bool valid,string reason)
        {
            if(!valid) throw new InvalidOperationException(reason);
        }

        [CliCommand("dicefree.ui.skills.inspect",
            "Check every class skill's tooltip and one shared Skills HUD layout.")]
        public static object Inspect()
        {
            Require(!EditorApplication.isPlaying,"Edit Mode only");
            var novice=AssetDatabase.FindAssets("t:NoviceSkillDefinition",
                new[]{"Assets/_DiceFree/Settings/Skills"})
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<NoviceSkillDefinition>)
                .Where(x=>x!=null).ToArray();
            var magic=AssetDatabase.FindAssets("t:MagicalSkillDefinition",
                new[]{"Assets/_DiceFree/Settings/Skills"})
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<MagicalSkillDefinition>)
                .Where(x=>x!=null).ToArray();
            var physical=AssetDatabase.FindAssets("t:PhysicalSkillDefinition",
                new[]{"Assets/_DiceFree/Settings/Skills"})
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<PhysicalSkillDefinition>)
                .Where(x=>x!=null).ToArray();
            Require(novice.Length>=5&&magic.Length>=5&&physical.Length>=5,
                "All three skill definition sets must be loaded");
            foreach(var def in novice)
            {
                string tooltip=SkillTooltips.Describe(def,Mathf.Min(2,def.maxRank),"1");
                Require(tooltip.Contains(def.displayName)&&tooltip.Contains("Rank") &&
                    tooltip.Length>55,"Incomplete Novice tooltip: "+def.displayName);
            }
            foreach(var def in magic)
            {
                string tooltip=SkillTooltips.Describe(def,Mathf.Min(2,def.maxRank),"1");
                Require(tooltip.Contains(def.displayName)&&tooltip.Contains("Rank")&&
                    tooltip.Length>55,"Incomplete Magical tooltip: "+def.displayName);
            }
            foreach(var def in physical)
            {
                string tooltip=SkillTooltips.Describe(def,Mathf.Min(2,def.maxRank),"1");
                Require(tooltip.Contains(def.displayName)&&tooltip.Contains("Rank")&&
                    tooltip.Length>55,"Incomplete Physical tooltip: "+def.displayName);
            }
            var novicePanel=UnityEngine.Object.FindAnyObjectByType<NoviceSkillPanel>(
                FindObjectsInactive.Include);
            var magicPanel=UnityEngine.Object.FindAnyObjectByType<MagicalSkillPanel>(
                FindObjectsInactive.Include);
            var physicalPanel=UnityEngine.Object.FindAnyObjectByType<PhysicalSkillPanel>(
                FindObjectsInactive.Include);
            Require(novicePanel!=null&&magicPanel!=null&&physicalPanel!=null,
                "Skill panels missing from scene");
            foreach(var panel in new CustomizableHudWidget[]
                {novicePanel,magicPanel,physicalPanel})
                Require(panel.DisplayName=="Skills"&&panel.LayoutGroupId=="skills",
                    "All classes must share the Skills panel layout.");
            Require(new[]{novicePanel.LayoutId,magicPanel.LayoutId,
                physicalPanel.LayoutId}.Distinct().Count()==3,
                "Concrete skill panels still need independent component identities.");
            return new{success=true,tooltipDefinitions=novice.Length+magic.Length+
                physical.Length,sharedEditorWindow="Skills",
                concreteClassPanels=3};
        }

        [CliCommand("dicefree.ui.skills.hover-coords",
            "Return current Game view pixel coordinates for ability tooltip screenshots.")]
        public static object HoverCoords()
        {
            Require(EditorApplication.isPlaying,"Play Mode required");
            var bar=UnityEngine.Object.FindFirstObjectByType<GenericActionBar>();
            var source=UnityEngine.Object.FindFirstObjectByType<AbilityBarSource>();
            var hud=UnityEngine.Object.FindFirstObjectByType<HudLayoutManager>();
            Require(bar!=null&&source!=null&&hud!=null,"Action bar missing");
            var panel=bar.Bounds;
            float padding=hud.Theme.outerPadding;
            var inner=new Rect(panel.x+padding,panel.y+padding,
                panel.width-padding*2f,panel.height-padding*2f);
            float gap=hud.Theme.gap;
            float cellWidth=(inner.width-gap*5f)/6f;
            float cellHeight=(inner.height-gap)/2f;
            Vector2 point=new Vector2(inner.x+cellWidth/2f,
                inner.y+cellHeight/2f);
            var view=source.View(0);
            var activeSkillWindows=UnityEngine.Object.FindObjectsByType<CustomizableHudWidget>(
                FindObjectsInactive.Include,FindObjectsSortMode.None)
                .Where(w=>(w is NoviceSkillPanel || w is PhysicalSkillPanel ||
                    w is MagicalSkillPanel) && w.ShowInLayoutEditor).ToArray();
            Require(activeSkillWindows.Length==1 &&
                activeSkillWindows[0].LayoutGroupId=="skills" &&
                activeSkillWindows[0].DisplayName=="Skills",
                "HUD editor must show exactly one unified Skills window");
            Require(view.Exists&&!string.IsNullOrWhiteSpace(view.tooltip),
                "Action bar skill tooltip is missing in Play Mode");
            return new{success=true,unifiedSkillsWidgets=activeSkillWindows.Length,
                screenWidth=Screen.width,
                screenHeight=Screen.height,mouseX=point.x,
                mouseY=Screen.height-point.y,viewName=view.name,
                skillTooltip=view.tooltip,barBounds=panel.ToString()};
        }

        [CliCommand("dicefree.ui.minimap.travel-test",
            "Exercise minimap coordinate conversion and in-world NavMesh movement.")]
        public static object TravelTest()
        {
            Require(EditorApplication.isPlaying,"Play Mode only");
            var player=UnityEngine.Object.FindFirstObjectByType<TraversalInput>();
            var minimap=UnityEngine.Object.FindFirstObjectByType<MinimapHud>();
            var motor=player!=null?player.GetComponent<TraversalMotor>():null;
            var hud=UnityEngine.Object.FindFirstObjectByType<HudLayoutManager>();
            Require(player!=null&&minimap!=null&&motor!=null&&hud!=null,
                "Missing minimap/player/NavMesh/HUD");
            Require(motor.Ready,"Player NavMesh agent is not ready");
            var area=minimap.Bounds;
            var pad=hud.Theme.outerPadding;
            var inner=new Rect(area.x+pad,area.y+pad,
                area.width-2*pad,area.height-2*pad);
            float header=Mathf.Clamp(inner.height*.10f,18f,28f);
            var mapRect=new Rect(inner.x,inner.y+header,inner.width,
                inner.height-header);
            var center=MinimapHud.WorldPointForMapClick(player.transform.position,
                minimap.WorldRadius,mapRect,mapRect.center);
            Require(Vector3.Distance(center,player.transform.position)<.02f,
                "Center map click should resolve exactly to the player.");
            var north=MinimapHud.WorldPointForMapClick(player.transform.position,
                minimap.WorldRadius,mapRect,new Vector2(mapRect.center.x,mapRect.y));
            Require(north.z>player.transform.position.z+minimap.WorldRadius-.05f,
                "Northern minimap edge must map to positive world Z");
            var east=MinimapHud.WorldPointForMapClick(player.transform.position,
                minimap.WorldRadius,mapRect,new Vector2(mapRect.xMax,mapRect.center.y));
            Require(east.x>player.transform.position.x+minimap.WorldRadius-.05f,
                "Eastern minimap edge must map to positive world X");

            bool moved=false;
            Vector3 chosen=default;
            var failures=new System.Collections.Generic.List<string>();
            var navigator=player as IMinimapNavigator;
            Require(navigator!=null,"TraversalInput does not implement minimap navigator");
            foreach(var offset in new[]
                {new Vector3(2,0,0),new Vector3(0,0,2),
                 new Vector3(-2,0,0),new Vector3(0,0,-2),
                 new Vector3(4,0,0),new Vector3(0,0,5),
                 new Vector3(-5,0,0),new Vector3(0,0,-5),
                 new Vector3(7,0,3),new Vector3(-7,0,-3)})
            {
                var proposed=player.transform.position+offset;
                if(!NavMesh.SamplePosition(proposed,out var hit,1.5f,
                    NavMesh.AllAreas)){failures.Add("No NavMesh near "+proposed);continue;}
                var delta=hit.position-player.transform.position;
                if(delta.magnitude<1.3f || delta.magnitude>=minimap.WorldRadius*.8f)
                {failures.Add("Discarded "+offset+" hit="+hit.position+
                    " delta="+delta.magnitude+" radius="+minimap.WorldRadius);continue;}
                var gui=new Vector2(
                    mapRect.center.x+delta.x/(minimap.WorldRadius*2)*mapRect.width,
                    mapRect.center.y-delta.z/(minimap.WorldRadius*2)*mapRect.height);
                bool terrain=Physics.Raycast(hit.position+Vector3.up*80f,
                    Vector3.down,out var rayHit,160f,1<<8,
                    QueryTriggerInteraction.Ignore);
                bool movedFromMap=minimap.TryMoveFromMapClick(mapRect,gui);
                if(!movedFromMap)
                {
                    bool direct=navigator.TryMoveFromMinimap(hit.position);
                    failures.Add("At "+hit.position+" terrain="+terrain+
                        (terrain?" hit="+rayHit.collider.name+" ground="+rayHit.point:"")+
                        " map="+movedFromMap+" direct="+direct+
                        " motorReady="+motor.Ready+" mapRect="+mapRect);
                    motor.Stop();
                    continue;
                }
                moved=true;
                chosen=hit.position;
                break;
            }
            var agent=motor.GetComponent<NavMeshAgent>();
            bool destinationIssued=moved && agent!=null &&
                Vector3.Distance(agent.destination,chosen)<1.5f;
            Require(destinationIssued,
                "Minimap click did not issue a correct NavMeshAgent destination. "+
                " player="+player.transform.position+" mapRadius="+minimap.WorldRadius+
                " agentDestination="+(agent!=null?agent.destination.ToString():"NULL")+
                " chosen="+chosen+" hasPath="+motor.Travelling+
                " pathPending="+(agent!=null&&agent.pathPending)+
                " candidates: "+string.Join(" | ",failures));
            SessionState.SetFloat("dicefree.map.travel.startX",
                player.transform.position.x);
            SessionState.SetFloat("dicefree.map.travel.startZ",
                player.transform.position.z);
            SessionState.SetFloat("dicefree.map.travel.targetX",chosen.x);
            SessionState.SetFloat("dicefree.map.travel.targetZ",chosen.z);
            return new{success=true,
                centerAligned=true,northUp=true,eastRight=true,
                issuedValidNavMeshMove=destinationIssued,
                pathPending=agent.pathPending,
                activePath=motor.Travelling,
                target=chosen.ToString("F2")};
        }
        [CliCommand("dicefree.ui.minimap.travel-status",
            "After a short time in Play Mode confirm the minimap command physically moves the player.")]
        public static object TravelStatus()
        {
            Require(EditorApplication.isPlaying,"Play Mode only");
            var player=UnityEngine.Object.FindFirstObjectByType<TraversalInput>();
            var minimap=UnityEngine.Object.FindFirstObjectByType<MinimapHud>();
            var hud=UnityEngine.Object.FindFirstObjectByType<HudLayoutManager>();
            var motor=player.GetComponent<TraversalMotor>();
            var agent=motor.GetComponent<NavMeshAgent>();
            Vector3 start=new Vector3(
                SessionState.GetFloat("dicefree.map.travel.startX",0),
                0,SessionState.GetFloat("dicefree.map.travel.startZ",0));
            Vector3 end=new Vector3(
                SessionState.GetFloat("dicefree.map.travel.targetX",0),
                0,SessionState.GetFloat("dicefree.map.travel.targetZ",0));
            float moved=Vector2.Distance(new Vector2(player.transform.position.x,
                player.transform.position.z),new Vector2(start.x,start.z));
            float remaining=Vector2.Distance(
                new Vector2(player.transform.position.x,player.transform.position.z),
                new Vector2(end.x,end.z));
            Require(moved>.45f && remaining<2.0f,
                "Minimap movement debug: traveled "+moved+
                "m, still "+remaining+"m away; path="+agent.pathStatus+
                " initial="+start+" destination="+end+
                " player="+player.transform.position+
                " agentDest="+agent.destination+" nextPosition="+agent.nextPosition+
                " pending="+agent.pathPending+" hasPath="+agent.hasPath+
                " stopped="+agent.isStopped+" speed="+agent.speed+
                " velocity="+agent.velocity+" timeScale="+Time.timeScale+
                " editorPaused="+EditorApplication.isPaused);
            var bounds=minimap.Bounds;
            float pad=hud.Theme.outerPadding;
            var inner=new Rect(bounds.x+pad,bounds.y+pad,
                bounds.width-2*pad,bounds.height-2*pad);
            float header=Mathf.Clamp(inner.height*.10f,18f,28f);
            var mapRect=new Rect(inner.x,inner.y+header,inner.width,
                inner.height-header);
            hud.SetEditMode(true);
            try
            {
                Require(!minimap.TryMoveFromMapClick(mapRect,mapRect.center),
                    "HUD Edit Mode must block minimap movement");
            }
            finally{hud.SetEditMode(false);motor.Stop();}
            return new{success=true,actualDistanceMoved=moved,
                distanceFromDestination=remaining,
                navMeshPathStatus=agent.pathStatus.ToString(),
                editorBlocksClick=true};
        }
    }
}
