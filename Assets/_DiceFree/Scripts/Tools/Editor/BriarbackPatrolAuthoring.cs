using System;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Briarback;
using DiceFree.Combat;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DiceFree.EditorTools
{
    /// <summary>Additive, guarded authoring for two small road patrol prototypes.</summary>
    public static class BriarbackPatrolAuthoring
    {
        private const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
        private const string RegionName = "14 - Briarback quilboar frontier";
        private const string RoleGroupName = "Approved Quillboar role prototypes - additive";
        private const string PatrolGroupName = "Briarback road patrols - additive";
        private const string RecoveryPath = "Assets/_DiceFree/Settings/Recovery/Cornberg-before-road-patrols-20261028.unity";
        private const string RepairRecoveryPath = "Assets/_DiceFree/Settings/Recovery/Cornberg-before-road-patrols-formation-20261028.unity";

        private sealed class Plan
        {
            public string name;
            public Vector2 first;
            public Vector2 second;
            public QuilboarRole[] roles;
            public Vector3[] centers;
            public Vector3[] targets;
            public Vector3[] spawns;
            public Vector3 spawn;
            public float maxPathRadius;
        }

        private static readonly Plan[] Plans =
        {
            new Plan { name = "Southern road patrol", first = new Vector2(248, 263), second = new Vector2(278, 260),
                roles = new[] { QuilboarRole.Warrior, QuilboarRole.SpearThrower, QuilboarRole.FireShaman } },
            new Plan { name = "Western road patrol", first = new Vector2(211, 289), second = new Vector2(204, 311),
                roles = new[] { QuilboarRole.Warrior, QuilboarRole.SpearThrower, QuilboarRole.SpearThrower } }
        };

        private static void Need(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("BRIARBACK ROAD PATROLS: " + message);
        }

        private static Scene RequireScene()
        {
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "stop Play Mode first.");
            var scene = SceneManager.GetActiveScene();
            Need(scene.path == ScenePath, "open the saved Cornberg scene first; no other scene will be loaded.");
            Need(!scene.isDirty, "save existing Cornberg scene edits before proceeding.");
            Need(GameObject.Find(RegionName) != null, "the existing Briarback root is missing.");
            return scene;
        }

        private static Vector3 Ground(Vector2 point)
        {
            return CornbergLandscape.Ground(point.x, point.y);
        }

        private static Vector3 Sample(Vector3 point, string label)
        {
            Need(NavMesh.SamplePosition(point, out var hit, 2.5f, NavMesh.AllAreas), label + " is not on the saved NavMesh at " + point);
            return hit.position;
        }

        private static bool StaticCapsuleClear(Vector3 point)
        {
            var colliders = Physics.OverlapCapsule(point + Vector3.up * .45f, point + Vector3.up * 1.55f, .42f,
                Physics.AllLayers, QueryTriggerInteraction.Ignore);
            return colliders.All(c => c == null || c.GetComponentInParent<CombatActor>() != null ||
                                      c.transform.root.name == "Cornberg - World 1 starter pocket" && c is TerrainCollider);
        }

        private static Plan[] Preflight(Scene scene, out NavMeshDataInstance navigation)
        {
            navigation = default;
            var world = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<WorldNavigation>(true)).SingleOrDefault();
            Need(world != null && world.Data != null, "saved World 1 navigation data is missing.");
            navigation = NavMesh.AddNavMeshData(world.Data);
            Physics.SyncTransforms();
            var roleRoot = GameObject.Find(RegionName).transform.Find(RoleGroupName);
            Need(roleRoot != null, "the approved role-prototype group is missing.");
            var roleActors = roleRoot.GetComponentsInChildren<QuilboarCombat>(true);
            foreach (var plan in Plans)
            {
                Need(plan.roles.Length == 3, plan.name + " must remain a three-member group.");
                foreach (var role in plan.roles)
                    Need(roleActors.Any(a => a.role == role && a.gameObject.activeInHierarchy),
                        "an active " + role + " role prototype is required for " + plan.name + ".");

                Vector3 first = Sample(Ground(plan.first), plan.name + " first camp stop");
                Vector3 second = Sample(Ground(plan.second), plan.name + " second camp stop");
                Need(Vector3.Distance(first, second) > 12 && Vector3.Distance(first, second) < 35,
                    plan.name + " camp stops should form a short, readable local loop.");
                plan.centers = new[] { first, second };
                plan.spawn = Sample((first + second) * .5f, plan.name + " spawn/home point");
                var tangent = Vector3.ProjectOnPlane(second - first, Vector3.up).normalized;
                var side = Vector3.Cross(Vector3.up, tangent).normalized;
                plan.targets = new Vector3[6];
                for (int stop = 0; stop < 2; stop++)
                for (int member = 0; member < 3; member++)
                {
                    float lateral = (member - 1) * 1.7f;
                    var requested = plan.centers[stop] + side * lateral;
                    var target = Sample(requested, plan.name + " stop " + stop + " member " + member);
                    Need(StaticCapsuleClear(target), plan.name + " has a world-collider obstruction at " + target.ToString("F1"));
                    var fromSpawn = new NavMeshPath();
                    Need(NavMesh.CalculatePath(plan.spawn, target, NavMesh.AllAreas, fromSpawn) && fromSpawn.status == NavMeshPathStatus.PathComplete,
                        plan.name + " has no complete spawn-to-stop path for member " + member + ".");
                    Need(Vector3.Distance(plan.spawn, target) < 19f,
                        plan.name + " stop exceeds the existing 20m aggro leash from the patrol home.");
                    plan.targets[stop * 3 + member] = target;
                }

                plan.spawns = new Vector3[3];
                for (int member = 0; member < 3; member++)
                {
                    var spawn = Sample(plan.spawn + side * ((member - 1) * 1.7f), plan.name + " member spawn " + member);
                    Need(StaticCapsuleClear(spawn), plan.name + " spawn has a world-collider obstruction at " + spawn.ToString("F1"));
                    plan.spawns[member] = spawn;
                }
                foreach (var spawn in plan.spawns)
                    foreach (var target in plan.targets)
                    {
                        var memberPath = new NavMeshPath();
                        Need(NavMesh.CalculatePath(spawn, target, NavMesh.AllAreas, memberPath) && memberPath.status == NavMeshPathStatus.PathComplete,
                            plan.name + " has no complete member-spawn-to-stop path from " + spawn.ToString("F1") + " to " + target.ToString("F1") + ".");
                        plan.maxPathRadius = Mathf.Max(plan.maxPathRadius,
                            memberPath.corners.Max(corner => Vector3.Distance(plan.spawn, corner)));
                    }
            }

            var existingPatrolGroup = GameObject.Find(PatrolGroupName)?.transform;
            var existingActors = Object.FindObjectsByType<CombatActor>(FindObjectsInactive.Include)
                .Where(actor => existingPatrolGroup == null || !actor.transform.IsChildOf(existingPatrolGroup)).ToArray();
            foreach (var plan in Plans)
            {
                foreach (var actor in existingActors)
                    foreach (var spawn in plan.spawns)
                        Need(Vector3.Distance(spawn, actor.transform.position) > 1.6f,
                            plan.name + " member spawn overlaps existing actor " + actor.name + ".");
                Need(Vector3.Distance(plan.spawns[0], plan.spawns[1]) > 1.3f &&
                     Vector3.Distance(plan.spawns[1], plan.spawns[2]) > 1.3f,
                    plan.name + " member spawn footprints overlap.");
            }
            return Plans;
        }

        [CliCommand("dicefree.briarback.patrols.preview",
            "Check two three-member road patrol routes against saved terrain, static collision, actor spacing and the existing NavMesh.")]
        public static object Preview()
        {
            var scene = RequireScene();
            var navigation = Preflight(scene, out var nav);
            try
            {
                return new { marker = "DICEFREE_BRIARBACK_PATROLS_PREFLIGHT_OK", groups = navigation.Length,
                    members = navigation.Sum(p => p.roles.Length), stopsPerGroup = 2, pauseSeconds = "random 3-8",
                    routes = navigation.Select(p => new { p.name, spawn = p.spawn.ToString("F2"),
                        campStops = string.Join(" -> ", p.centers.Select(point => point.ToString("F2"))),
                        roles = string.Join(", ", p.roles), memberTargets = string.Join("; ", p.targets.Select(point => point.ToString("F2"))),
                        maxPathCornerRadius = p.maxPathRadius.ToString("F2") }).ToArray(),
                    scene = ScenePath, navMeshChanged = false };
            }
            finally { if (nav.valid) nav.Remove(); }
        }

        [CliCommand("dicefree.briarback.patrols.author",
            "Add two guarded three-member Briarback patrol groups to the saved Cornberg scene without replacing existing content.")]
        public static object Author()
        {
            var scene = RequireScene();
            Need(GameObject.Find(PatrolGroupName) == null, "the additive patrol group already exists; refusing to duplicate it.");
            var plans = Preflight(scene, out var nav);
            GameObject root = null;
            try
            {
                Need(!AssetDatabase.LoadMainAssetAtPath(RecoveryPath), "the patrol recovery scene already exists; inspect it before authoring.");
                Directory.CreateDirectory(Path.GetDirectoryName(RecoveryPath));
                Need(AssetDatabase.CopyAsset(ScenePath, RecoveryPath), "Unity could not create the pre-patrol scene recovery copy.");

                var region = GameObject.Find(RegionName).transform;
                var roleRoot = region.Find(RoleGroupName);
                var templates = roleRoot.GetComponentsInChildren<QuilboarCombat>(true);
                root = new GameObject(PatrolGroupName);
                Undo.RegisterCreatedObjectUndo(root, "Add Briarback road patrols");
                root.transform.SetParent(region, false);

                int groupCount = 0;
                foreach (var plan in plans)
                {
                    var group = new GameObject(plan.name);
                    Undo.RegisterCreatedObjectUndo(group, "Add Briarback road patrol group");
                    group.transform.SetParent(root.transform, false);
                    group.transform.position = plan.spawn;
                    var members = new CombatActor[3];
                    for (int i = 0; i < 3; i++)
                    {
                        var template = templates.First(a => a.role == plan.roles[i] && a.gameObject.activeInHierarchy);
                        var tangent = Vector3.ProjectOnPlane(plan.centers[1] - plan.centers[0], Vector3.up).normalized;
                        var side = Vector3.Cross(Vector3.up, tangent).normalized;
                        var spawn = plan.spawns[i];
                        var clone = Object.Instantiate(template.gameObject, spawn, Quaternion.LookRotation(tangent), group.transform);
                        Need(clone != null, "Unity could not clone the " + plan.roles[i] + " prototype.");
                        Undo.RegisterCreatedObjectUndo(clone, "Add Briarback patrol member");
                        clone.name = plan.roles[i] + " - " + plan.name + " " + (i + 1) + " (Lv 25)";
                        clone.transform.position = spawn;
                        clone.transform.rotation = Quaternion.LookRotation(tangent);
                        var aggro = clone.GetComponent<AggroBehaviour>();
                        Need(aggro != null, "Patrol clone lost its AggroBehaviour component.");
                        aggro.Configure(8f, 20f);
                        aggro.ConfigureHomePosition(plan.spawn);
                        aggro.ConfigurePatrolMovement(true);
                        aggro.ConfigureAutoAggro(new AutoAggroPolicy { alwaysAutoAggro = true });
                        members[i] = clone.GetComponent<CombatActor>();
                        Need(members[i] != null, "Patrol clone lost its CombatActor component.");
                    }
                    var patrol = group.AddComponent<QuilboarRoadPatrol>();
                    patrol.Configure(members, plan.targets, 3f, 8f);
                    groupCount++;
                }

                EditorSceneManager.MarkSceneDirty(scene);
                Need(EditorSceneManager.SaveScene(scene), "Unity could not save the Cornberg scene after patrol authoring.");
                return new { marker = "DICEFREE_BRIARBACK_PATROLS_ADDED", groups = groupCount,
                    members = groupCount * 3, scene = ScenePath, recovery = RecoveryPath, existingRegionPreserved = true };
            }
            catch
            {
                if (root != null) Undo.DestroyObjectImmediate(root);
                throw;
            }
            finally { if (nav.valid) nav.Remove(); }
        }

        [CliCommand("dicefree.briarback.patrols.repair",
            "Repair the existing additive patrol actors in place after verifying their exact roster, route, home, leash and recovery copy.")]
        public static object Repair()
        {
            var scene = RequireScene();
            var plans = Preflight(scene, out var nav);
            try
            {
                var region = GameObject.Find(RegionName).transform;
                var groupRoot = region.Find(PatrolGroupName);
                Need(groupRoot != null, "the existing additive patrol group is missing.");
                var groups = groupRoot.Cast<Transform>().OrderBy(t => t.name).ToArray();
                Need(groups.Length == 2, "expected exactly two existing patrol groups.");
                Need(!AssetDatabase.LoadMainAssetAtPath(RepairRecoveryPath), "the repair recovery scene already exists; inspect it before repairing.");
                Directory.CreateDirectory(Path.GetDirectoryName(RepairRecoveryPath));
                Need(AssetDatabase.CopyAsset(ScenePath, RepairRecoveryPath), "Unity could not create the pre-repair scene recovery copy.");

                foreach (var plan in plans)
                {
                    var groupTransform = groupRoot.Find(plan.name);
                    Need(groupTransform != null, "existing patrol group is missing: " + plan.name);
                    var patrol = groupTransform.GetComponent<QuilboarRoadPatrol>();
                    Need(patrol != null, plan.name + " is missing its runtime patrol controller.");
                    var members = patrol.Members;
                    Need(members.Length == 3, plan.name + " must still contain exactly three members.");
                    groupTransform.position = plan.spawn;
                    var tangent = Vector3.ProjectOnPlane(plan.centers[1] - plan.centers[0], Vector3.up).normalized;
                    for (int i = 0; i < 3; i++)
                    {
                        Need(members[i] != null && members[i].GetComponent<QuilboarCombat>()?.role == plan.roles[i],
                            plan.name + " role/order mismatch at member " + i + "; refusing to replace actors.");
                        members[i].transform.position = plan.spawns[i];
                        members[i].transform.rotation = Quaternion.LookRotation(tangent);
                        var aggro = members[i].GetComponent<AggroBehaviour>();
                        Need(aggro != null, members[i].name + " lost its ordinary aggro behavior.");
                        aggro.Configure(8f, 20f);
                        aggro.ConfigureHomePosition(plan.spawn);
                        aggro.ConfigurePatrolMovement(true);
                        aggro.ConfigureAutoAggro(new AutoAggroPolicy { alwaysAutoAggro = true });
                        EditorUtility.SetDirty(aggro);
                        EditorUtility.SetDirty(members[i].transform);
                    }
                    patrol.Configure(members, plan.targets, 3f, 8f);
                    EditorUtility.SetDirty(patrol);
                    EditorUtility.SetDirty(groupTransform);
                }
                EditorSceneManager.MarkSceneDirty(scene);
                Need(EditorSceneManager.SaveScene(scene), "Unity could not save the repaired Cornberg patrol groups.");
                return new { marker = "DICEFREE_BRIARBACK_PATROLS_REPAIRED", groups = plans.Length, members = 6,
                    scene = ScenePath, recovery = RepairRecoveryPath, actorsReplaced = false };
            }
            finally { if (nav.valid) nav.Remove(); }
        }

        [CliCommand("dicefree.briarback.patrols.validate",
            "Validate the saved three-member patrol compositions, camp pause bounds, NavMesh paths and leash-safe routes.")]
        public static object Validate()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var region = GameObject.Find(RegionName);
            Need(region != null, "Briarback region is missing.");
            var patrolRoot = region.transform.Find(PatrolGroupName);
            Need(patrolRoot != null, "The additive patrol group has not been authored.");
            var patrols = patrolRoot.GetComponentsInChildren<QuilboarRoadPatrol>(true);
            Need(patrols.Length == 2, "Expected two local patrol prototypes.");
            var world = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<WorldNavigation>(true)).Single();
            var nav = NavMesh.AddNavMeshData(world.Data);
            try
            {
                foreach (var patrol in patrols)
                {
                    Need(patrol.Members.Length == 3 && patrol.Members.All(a => a != null), patrol.name + " is not a complete three-member group.");
                    Need(patrol.StopCount == 2 && patrol.RouteTargets.Length == 6, patrol.name + " requires two three-position camp stops.");
                    Need(patrol.MinimumStopSeconds == 3f && patrol.MaximumStopSeconds == 8f, patrol.name + " pause window differs from design.");
                    Need(patrol.Members.Count(a => a.GetComponent<QuilboarCombat>().role == QuilboarRole.Warrior) >= 1 &&
                         patrol.Members.Count(a => a.GetComponent<QuilboarCombat>().role == QuilboarRole.SpearThrower) >= 1,
                        patrol.name + " must mix Warrior and Spear Thrower roles.");
                    foreach (var actor in patrol.Members)
                    {
                        var aggro = actor.GetComponent<AggroBehaviour>();
                        Need(actor.gameObject.activeInHierarchy && aggro != null && aggro.Awareness == 8 && aggro.Leash == 20 && aggro.PatrolControlled,
                            actor.name + " must retain ordinary 8m awareness and 20m leash.");
                        Need(actor.GetComponent<OverworldRespawn>()?.Definition?.delaySeconds == 90,
                            actor.name + " must use the ordinary 90-second regional respawn.");
                    }
                    var home = patrol.transform.position;
                    foreach (var target in patrol.RouteTargets)
                    {
                        Need(NavMesh.SamplePosition(target, out var hit, .5f, NavMesh.AllAreas) && Vector3.Distance(hit.position, target) < .15f,
                            patrol.name + " stop is no longer on the saved NavMesh.");
                        var path = new NavMeshPath();
                        Need(NavMesh.CalculatePath(home, target, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete,
                            patrol.name + " no longer has a complete route from its home.");
                        Need(Vector3.Distance(home, target) < 19f, patrol.name + " exceeds the existing aggro leash.");
                    }
                }
                var allCreatures = region.GetComponentsInChildren<QuilboarCombat>(true);
                Need(allCreatures.Length == 26, "Expected 20 existing creatures plus six patrol actors; found " + allCreatures.Length);
                return new { marker = "DICEFREE_BRIARBACK_PATROLS_VALID", groups = patrols.Length,
                    patrolMembers = patrols.Sum(p => p.Members.Length), regionalCreatures = allCreatures.Length,
                    completeNavMeshTargets = patrols.Sum(p => p.RouteTargets.Length), scene = ScenePath };
            }
            finally { if (nav.valid) nav.Remove(); }
        }
    }
}
