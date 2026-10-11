using System;
using DiceFree.Characters;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
namespace DiceFree.EditorTools
{
 internal static class IronpineRuntimeValidation
 {
  [CliCommand("dicefree.ironpine.runtime-start","Move test player into Ironpine and request the Village 2 approach path.")]
  public static object Start()
  {
   if(!EditorApplication.isPlaying)throw new InvalidOperationException("Enter Play Mode");
   var player=UnityEngine.Object.FindFirstObjectByType<TraversalInput>();
   if(player==null)throw new InvalidOperationException("No player");
   var motor=player.GetComponent<TraversalMotor>();
   var agent=player.GetComponent<NavMeshAgent>();
   if(!NavMesh.SamplePosition(new Vector3(182,0,99),out var origin,6,NavMesh.AllAreas)||
      !NavMesh.SamplePosition(new Vector3(223,0,149),out var goal,6,NavMesh.AllAreas))
       throw new InvalidOperationException("Ironpine endpoints not on NavMesh");
   if(!motor.Teleport(origin.position))throw new InvalidOperationException("Player cannot enter Ironpine");
   if(!motor.MoveTo(goal.position))throw new InvalidOperationException("Ironpine->village route rejected by motor");
   Application.runInBackground=true;
   SessionState.SetInt("IronpineTravelFrame",Time.frameCount);
   SessionState.SetFloat("IronpineTravelTime",Time.time);
   SessionState.SetFloat("IronpineTravelX",player.transform.position.x);
   SessionState.SetFloat("IronpineTravelZ",player.transform.position.z);
   return new{success=true,agentDestination=agent.destination.ToString("F1"),
       start=player.transform.position.ToString("F1"),routeStatus=agent.pathStatus.ToString()};
  }
  [CliCommand("dicefree.ironpine.runtime-status","Check live movement actually advances along Ironpine's route.")]
  public static object Status()
  {
   if(!EditorApplication.isPlaying)throw new InvalidOperationException("Enter Play Mode");
   var player=UnityEngine.Object.FindFirstObjectByType<TraversalInput>();
   var motor=player.GetComponent<TraversalMotor>();
   var agent=player.GetComponent<NavMeshAgent>();
   var p=player.transform.position;
   float traveled=Vector2.Distance(new Vector2(p.x,p.z),
     new Vector2(SessionState.GetFloat("IronpineTravelX",0),
                 SessionState.GetFloat("IronpineTravelZ",0)));
   if(traveled<2f)
       throw new InvalidOperationException("Player did not traverse Ironpine: "+
         traveled+"m; agent="+agent.pathStatus+" pending="+agent.pathPending+
         " hasPath="+agent.hasPath+" stopped="+agent.isStopped+
         " remaining="+agent.remainingDistance+" speed="+agent.speed+
         " velocity="+agent.velocity+" next="+agent.nextPosition+
         " canAct="+player.GetComponent<DiceFree.Combat.CombatActor>().CanAct+
         " timescale="+Time.timeScale+" paused="+EditorApplication.isPaused+
         " suppressed="+DiceFree.Input.InputBindings.Current.Suppressed+
         " motorReady="+motor.Ready+" travelling="+motor.Travelling+
         " frameDelta="+(Time.frameCount-SessionState.GetInt("IronpineTravelFrame",0))+
         " timeDelta="+(Time.time-SessionState.GetFloat("IronpineTravelTime",0f))+
         " runBackground="+Application.runInBackground);
   if(agent.pathStatus==NavMeshPathStatus.PathInvalid)
       throw new InvalidOperationException("NavMesh path invalid while traveling");
   motor.Stop();
   return new{success=true,movedMetres=traveled,
       pathStatus=agent.pathStatus.ToString(),physicsMovement=true};
  }
 }
}