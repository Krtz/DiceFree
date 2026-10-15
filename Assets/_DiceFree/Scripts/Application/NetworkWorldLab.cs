using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Persistence;
using DiceFree.UI;
using DiceFree.World;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace DiceFree.Development
{
    /// <summary>Bounded direct/LAN test. Transport carries intents; existing gameplay resolves on host.</summary>
    public sealed class NetworkWorldLab : MonoBehaviour
    {
        const string IntentChannel = "dicefree.lab.intent.v1", StateChannel = "dicefree.lab.state.v1";
        const string TravelLock = "network.lab.travel";
        const float ReconnectGrace = 30;
        const int MaxPlayers = 4;
        public static NetworkWorldLab Current { get; private set; }
        public bool Ready { get; private set; }
        public bool Connected => net != null && net.IsListening && (host || net.IsConnectedClient);
        public bool Host => host;
        public string Status { get; private set; } = "Choose host or join";
        public LabSnapshot Latest { get; private set; }
        public int ReceivedSnapshots { get; private set; }
        public int RejectedCommands { get; private set; }
        public int AcceptedCommands { get; private set; }

        [Serializable] public sealed class LabCommand
        {
            public int sequence;
            public string kind, target, face;
            public float x, z;
        }
        [Serializable] public sealed class LabActor
        {
            public string id, name, face, destination, runtimeId;
            public Vector3 position;
            public Quaternion rotation;
            public float hp, maximumHp, cooldown;
            public int hits, transitions;
            public bool connected, travelling, player;
        }
        [Serializable] public sealed class LabSnapshot
        {
            public int protocol = 1, tick;
            public string session, ownId;
            public float serverTime;
            public LabActor[] actors;
        }
        sealed class Participant
        {
            public string id, token, face = "world.test.1", destination;
            public CombatActor actor;
            public ulong connection;
            public bool connected, travelling;
            public Vector3 direction;
            public float inputUntil, graceUntil, arriveAt, budgetUntil;
            public int sequence, budget, transitions;
        }
        NetworkManager net;
        DevelopmentRoomController room;
        readonly Dictionary<string, DevelopmentMap> maps = new(StringComparer.Ordinal);
        readonly Dictionary<string, Participant> peers = new(StringComparer.Ordinal);
        readonly Dictionary<ulong, Participant> connections = new();
        readonly Dictionary<string, CombatActor> targets = new(StringComparer.Ordinal);
        readonly Dictionary<string, string> targetFaces = new(StringComparer.Ordinal);
        readonly Dictionary<string, GameObject> replicas = new(StringComparer.Ordinal);
        readonly SessionMapRegistry registry = new();
        string address = "127.0.0.1", portText = "7777", token, session, ownId, reportPath, commandPath;
        bool host, starting, selection = true, leaving, presenting, inputMoving;
        int tick, nextActor, localSequence, fileSequence;
        float nextSnapshot, nextReport, nextInput;
        MonoBehaviour menu;
        readonly List<Material> ownedMaterials = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => Current = null;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoLaunch()
        {
            string mode = Argument("-diceFreeNetwork");
            if (mode != "host" && mode != "client") return;
            var lab = Create();
            lab.address = Argument("-diceFreeNetworkAddress") ?? "127.0.0.1";
            lab.portText = Argument("-diceFreeNetworkPort") ?? "7777";
            lab.reportPath = Argument("-diceFreeNetworkReport");
            lab.commandPath = Argument("-diceFreeNetworkCommands");
            lab.Launch(mode == "host");
        }
        static string Argument(string key)
        {
            var args = Environment.GetCommandLineArgs(); int i = Array.IndexOf(args, key);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
        }
        static NetworkWorldLab Create()
        {
            if (Current != null) return Current;
            var go = new GameObject("DiceFree multiplayer development session");
            DontDestroyOnLoad(go); return go.AddComponent<NetworkWorldLab>();
        }
        public static void OpenSelection(MonoBehaviour currentMenu)
        {
            var lab = Create(); lab.menu = currentMenu; if(currentMenu!=null)currentMenu.enabled = false;
        }
        void Awake() { Current = this; token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"); }
        public void Launch(bool asHost)
        {
            if (starting || Ready) return;
            if (!ushort.TryParse(portText, out var port) || port < 1024 || string.IsNullOrWhiteSpace(address))
            { Status = "Use an address and a port between 1024 and 65535."; return; }
            host = asHost; starting = true; selection = false;
            StartCoroutine(Prepare(port));
        }
        IEnumerator Prepare(ushort port)
        {
            Status = "Preparing isolated multiplayer test";
            // A fresh process-specific dev profile; never load or flush the user's normal Echo.
            EchoSaveLocation.SetTemporaryRoot(
                Path.Combine(Path.GetTempPath(), "DiceFreeNetworkLab", Guid.NewGuid().ToString("N")));
            yield return SceneManager.LoadSceneAsync("DevelopmentRoom");
            float until = Time.realtimeSinceStartup + 30;
            while ((DevelopmentRoomController.Current == null || DevelopmentRoomController.Current.Map == null) &&
                   Time.realtimeSinceStartup < until) yield return null;
            room = DevelopmentRoomController.Current;
            if (room == null || room.Map == null) { Status = "Development room failed to initialize."; yield break; }
            room.enabled = false;
            room.SetLevel(1);
            var persistence = room.player.GetComponent<ManifestationPersistence>();
            persistence.Flush(); persistence.enabled = false;
            // Only the existing actor simulation and appearance remain active. Lab input owns all commands.
            foreach (var b in room.player.GetComponents<MonoBehaviour>())
                if (b is not CombatActor && b is not ActorStats && b is not Health &&
                    b is not TraversalMotor && b is not BasicAttack && b is not CombatStatusController &&
                    b is not ActorFootprintSeparation && b is not NovicePresentationDriver &&
                    b is not ClassPresentationSwitcher) b.enabled = false;
            room.fog.enabled = false;
            foreach (var r in room.fog.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
            yield return SceneManager.LoadSceneAsync("World2TransitionTest", LoadSceneMode.Additive);
            yield return null;
            foreach (var map in FindObjectsByType<DevelopmentMap>()) maps.Add(map.worldId, map);
            if (maps.Count != 2) { Status = "Both linked maps are required."; yield break; }
            foreach (var actor in CombatActor.All.ToArray())
                if (actor != room.player) actor.gameObject.SetActive(false); // local training fixtures are not network actors
            var transport = gameObject.AddComponent<UnityTransport>();
            transport.SetConnectionData(address, port, host ? "0.0.0.0" : null);
            net = gameObject.AddComponent<NetworkManager>();
            net.NetworkConfig = new NetworkConfig
            {
                NetworkTransport = transport, EnableSceneManagement = false,
                ConnectionApproval = true, ProtocolVersion = 1,
                ConnectionData = Encoding.ASCII.GetBytes("DiceFreeLab1|" + token),
                TickRate = 30
            };
            net.ConnectionApprovalCallback = Approve;
            net.OnClientConnectedCallback += ClientConnected;
            net.OnClientDisconnectCallback += ClientDisconnected;
            session = Guid.NewGuid().ToString("N");
            if (host)
            {
                var self = NewParticipant(token); self.actor = room.player; self.connected = true;
                self.connection = NetworkManager.ServerClientId; ownId = self.id;
                self.actor.Stats.SetEncounterMaximumHp(500); self.actor.Health.Restore();
                connections.Add(self.connection, self);
                registry.PlaceOnWorldFace(self.id, self.face);
                self.actor.Motor.TeleportAcrossMaps(maps[self.face].spawn + Vector3.left * 2);
                foreach (var map in maps.Values)
                {
                    string id = "sentinel." + map.worldId;
                    var enemy = CreateActor(id, map.spawn + new Vector3(-14, 0, -14), 1, false);
                    enemy.Stats.SetEncounterMaximumHp(2000); enemy.Health.Restore();
                    targets.Add(id, enemy); targetFaces.Add(id, map.worldId);
                }
            }
            else
            {
                room.player.GetComponent<BasicAttack>().enabled = false;
                room.player.Health.enabled = false;
                room.player.GetComponent<NavMeshAgent>().enabled = false;
                room.player.GetComponent<ActorFootprintSeparation>().enabled = false;
            }
            bool started = host ? net.StartHost() : net.StartClient();
            if (!started) { Status = "Could not start connection. Check address / port."; yield break; }
            net.CustomMessagingManager.RegisterNamedMessageHandler(IntentChannel, ReceiveIntent);
            net.CustomMessagingManager.RegisterNamedMessageHandler(StateChannel, ReceiveState);
            Ready = true; starting = false;
            Status = host ? "Hosting direct/LAN test on port " + port : "Connecting to " + address + ":" + port;
        }
        Participant NewParticipant(string credential)
        {
            var p = new Participant { id = "player." + (++nextActor), token = credential };
            peers.Add(p.id, p); return p;
        }
        void Approve(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            response.CreatePlayerObject = false; response.Pending = false;
            string payload = Encoding.ASCII.GetString(request.Payload ?? Array.Empty<byte>());
            string credential = payload.StartsWith("DiceFreeLab1|", StringComparison.Ordinal) ? payload.Substring(13) : "";
            if (credential.Length != 64 || credential.Any(c => !Uri.IsHexDigit(c)))
            { response.Approved = false; response.Reason = "Incompatible development test."; return; }
            if (request.ClientNetworkId == NetworkManager.ServerClientId)
            { response.Approved = true; return; }
            var p = peers.Values.FirstOrDefault(v => v.token == credential);
            if ((p != null && p.connected) || (p == null && peers.Count >= MaxPlayers))
            { response.Approved = false; response.Reason = "Session slot unavailable."; return; }
            p ??= NewParticipant(credential);
            p.connection = request.ClientNetworkId; connections[request.ClientNetworkId] = p;
            response.Approved = true;
        }
        void ClientConnected(ulong id)
        {
            if (!host) { Status = "Connected - waiting for authoritative state"; return; }
            if (!connections.TryGetValue(id, out var p)) return;
            p.connected = true; p.graceUntil = 0; p.direction = Vector3.zero; p.sequence = 0;
            if (p.actor == null)
                p.actor = CreateActor(p.id, maps[p.face].spawn + Vector3.right * (2 + peers.Count), 0, true);
            registry.PlaceOnWorldFace(p.id, p.face);
            Status = "Hosting - " + connections.Count + " connected";
        }
        void ClientDisconnected(ulong id)
        {
            if (leaving) return;
            if (!host) { Status = "Disconnected. Reconnect resumes the same actor for 30 seconds."; return; }
            if (!connections.Remove(id, out var p)) return;
            p.connected = false; p.graceUntil = Time.realtimeSinceStartup + ReconnectGrace;
            p.direction = Vector3.zero; p.actor?.GetComponent<BasicAttack>()?.Cancel(); p.actor?.Motor.Stop();
        }
        CombatActor CreateActor(string id, Vector3 position, int faction, bool copyAppearance)
        {
            var go = new GameObject(id); go.SetActive(false); go.transform.position = position; go.layer = faction == 0 ? 10 : 11;
            go.AddComponent<ActorStats>().Configure(room.player.Stats.Definition, 1);
            go.AddComponent<Health>();
            var agent = go.AddComponent<NavMeshAgent>(); agent.enabled = false; agent.height = 1.8f; agent.radius = .675f;
            go.AddComponent<TraversalMotor>();
            var actor = go.AddComponent<CombatActor>(); actor.Configure(faction, .45f);
            var capsule = go.AddComponent<CapsuleCollider>(); capsule.center = Vector3.up * .9f; capsule.height = 1.8f;
            go.AddComponent<BasicAttack>();
            GameObject visual;
            if (copyAppearance)
            {
                visual = Instantiate(room.player.GetComponent<ClassPresentationSwitcher>().ActiveVisualRoot.gameObject, go.transform);
                visual.transform.localPosition = Vector3.zero;
                foreach (var b in visual.GetComponentsInChildren<MonoBehaviour>(true)) b.enabled = false;
                var driver = go.AddComponent<NovicePresentationDriver>();
                driver.Configure(visual.transform, ClassPresentationSwitcher.FindVisualAnimator(visual.transform));
            }
            else
            {
                visual = GameObject.CreatePrimitive(PrimitiveType.Capsule); visual.transform.SetParent(go.transform, false);
                visual.transform.localPosition = Vector3.up; Destroy(visual.GetComponent<Collider>());
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.color = new Color(.8f,.25f,.45f); ownedMaterials.Add(material);
                visual.GetComponent<Renderer>().sharedMaterial = material;
            }
            go.SetActive(true);
            if(faction==0){actor.Stats.SetEncounterMaximumHp(500);actor.Health.Restore();}
            actor.Motor.TeleportAcrossMaps(position); return actor;
        }
        void Send(ulong id, string channel, string json)
        {
            using var writer = new FastBufferWriter(32768, Allocator.Temp);
            writer.WriteValueSafe(json);
            net.CustomMessagingManager.SendNamedMessage(channel, id, writer, NetworkDelivery.ReliableFragmentedSequenced);
        }
        void ReceiveIntent(ulong sender, FastBufferReader reader)
        {
            if (!host || !connections.ContainsKey(sender) || reader.Length > 4096) return;
            try { reader.ReadValueSafe(out string json); if (json.Length > 2048) { RejectedCommands++; return; }
                Execute(sender, JsonUtility.FromJson<LabCommand>(json)); }
            catch (Exception e) { RejectedCommands++; Debug.LogWarning("Invalid lab intent: " + e.GetType().Name); }
        }
        void ReceiveState(ulong sender, FastBufferReader reader)
        {
            if (host || sender != NetworkManager.ServerClientId || reader.Length > 32768) return;
            reader.ReadValueSafe(out string json);
            var snapshot = JsonUtility.FromJson<LabSnapshot>(json);
            if (snapshot == null || snapshot.protocol != 1 || snapshot.actors == null ||
                (Latest != null && snapshot.session == Latest.session && snapshot.tick <= Latest.tick)) return;
            Latest = snapshot; ownId = snapshot.ownId; ReceivedSnapshots++;
            Status = "Connected - " + snapshot.actors.Count(a=>a.player&&a.connected) + " participants";
        }
        public bool Submit(LabCommand command)
        {
            if (!Ready || !Connected) return false;
            command.sequence = ++localSequence;
            if (host) return Execute(NetworkManager.ServerClientId, command);
            Send(NetworkManager.ServerClientId, IntentChannel, JsonUtility.ToJson(command)); return true;
        }
        public bool RequestTravel(string scene)
        {
            if (room == null || room.Busy) return false;
            var destination = maps.Values.FirstOrDefault(m => m.gameObject.scene.name == scene);
            return destination != null && Submit(new LabCommand { kind = "travel", face = destination.worldId });
        }
        bool Execute(ulong sender, LabCommand cmd)
        {
            if (!connections.TryGetValue(sender, out var p) || !p.connected || p.actor == null || cmd == null ||
                cmd.sequence <= p.sequence || !float.IsFinite(cmd.x) || !float.IsFinite(cmd.z)) return Reject();
            p.sequence = cmd.sequence;
            if (Time.realtimeSinceStartup > p.budgetUntil) { p.budgetUntil = Time.realtimeSinceStartup + 1; p.budget = 0; }
            if (++p.budget > 60 || p.travelling || !p.actor.CanAct) return Reject();
            var map = maps[p.face]; var attack = p.actor.GetComponent<BasicAttack>();
            bool accepted = false;
            switch (cmd.kind)
            {
                case "direction":
                    if (Mathf.Abs(cmd.x) > 1 || Mathf.Abs(cmd.z) > 1) break;
                    attack.Cancel(); p.direction = Vector3.ClampMagnitude(new Vector3(cmd.x,0,cmd.z),1);
                    p.inputUntil = Time.realtimeSinceStartup + .3f; accepted = true; break;
                case "move":
                    p.direction = Vector3.zero; attack.Cancel();
                    accepted = map.bounds.Contains(new Vector2(cmd.x,cmd.z)) &&
                        p.actor.Motor.MoveTo(new Vector3(cmd.x,map.spawn.y,cmd.z)); break;
                case "stop":
                    p.direction = Vector3.zero; attack.Cancel(); p.actor.Motor.Stop(); accepted = true; break;
                case "attack":
                    p.direction = Vector3.zero;
                    accepted = cmd.target != null && targets.TryGetValue(cmd.target,out var target) &&
                        targetFaces[cmd.target] == p.face && attack.Order(target); break;
                case "travel":
                    var edge = map.GetComponent<CubeEdgeMap>();
                    if (cmd.face != edge.link.destinationFaceId || !maps.ContainsKey(cmd.face) ||
                        p.actor.InCombat || Vector3.Distance(p.actor.transform.position,edge.Approach) > 4) break;
                    attack.Cancel(); p.actor.Motor.Stop(); p.direction = Vector3.zero;
                    p.actor.SetActionBlocked(TravelLock,true); p.actor.Motor.SetMotionBlocked(TravelLock,true);
                    p.travelling = true; p.destination = cmd.face; p.arriveAt = Time.realtimeSinceStartup + 3.8f;
                    accepted = true; break;
            }
            if (!accepted) return Reject();
            AcceptedCommands++; return true;
        }
        bool Reject() { RejectedCommands++; return false; }
        void Update()
        {
            if (!Ready) return;
            if (host && Connected) Simulate();
            if (Connected && Time.realtimeSinceStartup >= nextSnapshot && host)
            { nextSnapshot = Time.realtimeSinceStartup + .1f; Broadcast(); }
            if (Latest != null) ApplyView();
            if(host)ApplyHostVisibility();
            InputCommands();
            ReadAutomation();
            if (Time.realtimeSinceStartup >= nextReport) { nextReport = Time.realtimeSinceStartup + .25f; WriteReport(); }
        }
        void Simulate()
        {
            foreach (var p in peers.Values.ToArray())
            {
                if (!p.connected && p.graceUntil > 0 && Time.realtimeSinceStartup > p.graceUntil)
                { if(p.actor!=null)Destroy(p.actor.gameObject); peers.Remove(p.id); registry.RemoveFromWorldFace(p.id); continue; }
                if (p.travelling && Time.realtimeSinceStartup >= p.arriveAt)
                {
                    var destination = maps[p.destination]; var edge = destination.GetComponent<CubeEdgeMap>();
                    if (p.actor.Motor.TeleportAcrossMaps(edge.Approach))
                    { p.face = p.destination; registry.PlaceOnWorldFace(p.id,p.face); p.transitions++; }
                    p.travelling = false; p.destination = null;
                    p.actor.SetActionBlocked(TravelLock,false); p.actor.Motor.SetMotionBlocked(TravelLock,false);
                }
                if (!p.travelling && p.connected && Time.realtimeSinceStartup < p.inputUntil && p.direction.sqrMagnitude > 0)
                    p.actor.Motor.MoveDirect(p.direction,Time.deltaTime);
            }
            foreach (var pair in targets)
            {
                var enemy = pair.Value; var attack = enemy.GetComponent<BasicAttack>();
                var victim = peers.Values.Where(p => p.face == targetFaces[pair.Key] && !p.travelling && p.actor != null && p.actor.Alive)
                    .OrderBy(p => Vector3.Distance(enemy.transform.position,p.actor.transform.position)).FirstOrDefault();
                if (victim != null && Vector3.Distance(enemy.transform.position,victim.actor.transform.position) < 3)
                    attack.Order(victim.actor);
                else attack.Cancel();
            }
        }
        LabActor ActorState(string id, CombatActor actor, string face, Participant p = null) => new()
        {
            id=id,runtimeId=actor.GetEntityId().ToString(),name=p != null ? id + " the Novice" : "Training sentinel",face=face,
            position=actor.transform.position,rotation=actor.transform.rotation,
            hp=actor.Health.Current,maximumHp=actor.Health.Maximum,
            cooldown=actor.GetComponent<BasicAttack>().CooldownRemaining,hits=actor.GetComponent<BasicAttack>().Hits,
            player=p!=null,connected=p?.connected??false,travelling=p?.travelling??false,
            destination=p?.destination,transitions=p?.transitions??0
        };
        void Broadcast()
        {
            var actors = peers.Values.Where(p=>p.actor!=null).Select(p=>ActorState(p.id,p.actor,p.face,p))
                .Concat(targets.Select(pair=>ActorState(pair.Key,pair.Value,targetFaces[pair.Key]))).ToArray();
            tick++;
            foreach (var pair in connections)
            {
                var snapshot = new LabSnapshot { session=session,tick=tick,ownId=pair.Value.id,
                    serverTime=Time.realtimeSinceStartup,actors=actors };
                if (pair.Key==NetworkManager.ServerClientId) Latest=snapshot;
                else Send(pair.Key,StateChannel,JsonUtility.ToJson(snapshot));
            }
        }
        LabActor Self => Latest?.actors?.FirstOrDefault(a=>a.id==ownId);
        void ApplyView()
        {
            var self = Self; if (self == null) return;
            if (!presenting && !room.Busy && (self.travelling || room.Map.worldId != self.face))
            {
                string destination = self.travelling ? self.destination : self.face;
                if (maps.TryGetValue(destination,out var map) && map != room.Map)
                    StartCoroutine(Present(map));
            }
            if (!host && !room.Busy)
            {
                room.player.GetComponent<NavMeshAgent>().enabled = false;
                room.player.transform.position = Vector3.Distance(room.player.transform.position,self.position)>20 ?
                    self.position : Vector3.Lerp(room.player.transform.position,self.position,Mathf.Min(1,Time.unscaledDeltaTime*20));
                room.player.transform.rotation = Quaternion.Slerp(room.player.transform.rotation,self.rotation,Time.unscaledDeltaTime*20);
            }
            if (!host)
            {
                foreach (var a in Latest.actors.Where(a=>a.id!=ownId))
                {
                    if (!replicas.TryGetValue(a.id,out var go))
                    {
                        // Replicas never execute Health/AI/attacks, own colliders, or write saves.
                        go=GameObject.CreatePrimitive(a.player?PrimitiveType.Capsule:PrimitiveType.Sphere);
                        go.name=a.name; Destroy(go.GetComponent<Collider>()); replicas.Add(a.id,go);
                        var mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        mat.color=a.player?new Color(.25f,.65f,.95f):new Color(.8f,.25f,.45f);
                        ownedMaterials.Add(mat);go.GetComponent<Renderer>().sharedMaterial=mat;
                        go.transform.position=a.position+Vector3.up;
                    }
                    go.SetActive(a.face==room.Map.worldId&&a.hp>0&&!a.travelling);
                    go.transform.position=Vector3.Lerp(go.transform.position,a.position+Vector3.up,Time.unscaledDeltaTime*20);
                    go.transform.rotation=a.rotation;
                }
                foreach(var id in replicas.Keys.Where(id=>!Latest.actors.Any(a=>a.id==id)).ToArray())
                {Destroy(replicas[id]);replicas.Remove(id);}
            }
        }
        void ApplyHostVisibility()
        {
            foreach(var p in peers.Values)
                if(p.actor!=null && p.id!=ownId)
                    foreach(var r in p.actor.GetComponentsInChildren<Renderer>(true))
                        r.enabled=p.face==room.Map.worldId&&!p.travelling&&!room.Busy&&p.actor.Alive;
            foreach(var pair in targets)
                foreach(var r in pair.Value.GetComponentsInChildren<Renderer>(true))
                    r.enabled=targetFaces[pair.Key]==room.Map.worldId&&!room.Busy&&pair.Value.Alive;
        }
        IEnumerator Present(DevelopmentMap destination)
        {
            presenting=true;
            yield return room.PresentNetworkTravel(destination,()=>Self!=null&&Self.face==destination.worldId&&!Self.travelling);
            if(!host)room.player.GetComponent<NavMeshAgent>().enabled=false;
            presenting=false;
        }
        void InputCommands()
        {
            if (!Connected || room.Busy || Self==null || Self.travelling || Self.hp<=0) return;
            var keyboard=Keyboard.current;
            if(keyboard!=null&&Time.realtimeSinceStartup>=nextInput)
            {
                nextInput=Time.realtimeSinceStartup+.05f;
                float x=(keyboard.dKey.isPressed?1:0)-(keyboard.aKey.isPressed?1:0);
                float z=(keyboard.wKey.isPressed?1:0)-(keyboard.sKey.isPressed?1:0);
                if(x!=0||z!=0||inputMoving)
                {
                    var camera=room.explorationCamera.transform;
                    var forward=Vector3.ProjectOnPlane(camera.forward,Vector3.up).normalized;
                    var right=Vector3.ProjectOnPlane(camera.right,Vector3.up).normalized;
                    var dir=Vector3.ClampMagnitude(right*x+forward*z,1);
                    Submit(new LabCommand{kind="direction",x=dir.x,z=dir.z});
                    inputMoving=x!=0||z!=0;
                }
                if(keyboard.spaceKey.wasPressedThisFrame)AttackNearest();
            }
            var mouse=Mouse.current;
            if(mouse==null||mouse.position.ReadValue().x<340) return;
            if(mouse.leftButton.wasPressedThisFrame)
            {
                var ray=room.explorationCamera.GetComponent<Camera>().ScreenPointToRay(mouse.position.ReadValue());
                if(Physics.Raycast(ray,out var hit,300,1<<8))
                    Submit(new LabCommand{kind="move",x=hit.point.x,z=hit.point.z});
            }
            if(mouse.rightButton.wasPressedThisFrame)AttackNearest();
        }
        void AttackNearest()
        {
            var target=Latest.actors.Where(a=>!a.player&&a.face==Self.face&&a.hp>0)
                .OrderBy(a=>Vector3.Distance(a.position,Self.position)).FirstOrDefault();
            if(target!=null)Submit(new LabCommand{kind="attack",target=target.id});
        }
        public void Disconnect()
        {
            if(host||net==null)return;
            net.Shutdown();Status="Disconnected - actor remains vulnerable during 30-second grace";
        }
        public void Reconnect()
        {
            if(host||net==null||net.IsListening||net.ShutdownInProgress)return;
            localSequence=0; Latest=null; net.StartClient();
            net.CustomMessagingManager.RegisterNamedMessageHandler(StateChannel,ReceiveState);
            Status="Reconnecting";
        }
        void ReadAutomation()
        {
            if (!Debug.isDebugBuild && !UnityEngine.Application.isEditor) return;
            if(string.IsNullOrEmpty(commandPath)||!File.Exists(commandPath))return;
            try
            {
                var cmd=JsonUtility.FromJson<LabCommand>(File.ReadAllText(commandPath));
                if(cmd==null||cmd.sequence<=fileSequence)return;
                fileSequence=cmd.sequence;
                if(cmd.kind=="disconnect")Disconnect();
                else if(cmd.kind=="reconnect")Reconnect();
                else if(cmd.kind=="quit")UnityEngine.Application.Quit();
                else Submit(cmd);
            }
            catch(IOException){} // writer can atomically replace on its next tick

        }
        void WriteReport()
        {
            if(string.IsNullOrEmpty(reportPath))return;
            try
            {
                string json=JsonUtility.ToJson(new LabReport{ready=Ready,connected=Connected,host=host,
                    status=Status,snapshots=ReceivedSnapshots,accepted=AcceptedCommands,rejected=RejectedCommands,
                    transitions=room.CompletedTransitions,flips=room.GetComponent<CubeEdgePresentation>().CompletedFlips,
                    localFace=room.Map.worldId,busy=room.Busy,snapshot=Latest,loadedFaces=maps.Keys.ToArray(),
                    unityVersion=UnityEngine.Application.unityVersion},true);
                File.WriteAllText(reportPath+".tmp",json);
                File.Copy(reportPath+".tmp",reportPath,true);
            }
            catch(IOException){}
        }
        [Serializable] sealed class LabReport
        {
            public bool ready,connected,host,busy;
            public string status,localFace,unityVersion;
            public int snapshots,accepted,rejected,transitions,flips;
            public string[] loadedFaces;
            public LabSnapshot snapshot;
        }
        public void Leave()
        {
            leaving=true;if(net!=null)net.Shutdown();
            if(room!=null)room.GetComponent<CubeEdgePresentation>()?.End();
            DiceFree.Foundation.DevelopmentSession.End();
            Destroy(gameObject);
            SceneManager.LoadScene("StartMenu");
        }
        void OnDestroy()
        {
            if(Current==this){Current=null;EchoSaveLocation.SetTemporaryRoot(null);}
            foreach(var material in ownedMaterials)if(material!=null)Destroy(material);
            foreach(var go in replicas.Values)if(go!=null)Destroy(go);
        }
        void OnGUI()
        {
            if(selection)
            {
                GUILayout.BeginArea(new Rect((Screen.width-460)/2,(Screen.height-330)/2,460,330),GUI.skin.box);
                GUILayout.Label("MULTIPLAYER DEVELOPMENT TEST");
                GUILayout.Label("Isolated Novices. Movement, basic attacks and two linked faces.");
                GUILayout.Label("Host address (join only)");address=GUILayout.TextField(address,128);
                GUILayout.Label("Port");portText=GUILayout.TextField(portText,5);
                if(GUILayout.Button("Host LAN test"))Launch(true);
                if(GUILayout.Button("Join test"))Launch(false);
                if(GUILayout.Button("Back")){if(menu!=null)menu.enabled=true;Destroy(gameObject);}
                GUILayout.Label(Status);GUILayout.EndArea();return;
            }
            GUILayout.BeginArea(new Rect(10,10,320,480),GUI.skin.box);
            GUILayout.Label("MULTIPLAYER DEVELOPMENT TEST");
            GUILayout.Label(Status);
            if(Self!=null)
            {
                GUILayout.Label(Self.name+" - "+Self.face);
                GUILayout.Label("HP "+Self.hp.ToString("F1")+" / "+Self.maximumHp.ToString("F0"));
                GUILayout.Label("WASD / click: move. Space / right click: attack.");
                GUILayout.Label("Basic attacks only in this test.");
                if(GUILayout.Button("Walk to cube edge"))
                {var edge=maps[Self.face].GetComponent<CubeEdgeMap>();Submit(new LabCommand{kind="move",x=edge.Approach.x,z=edge.Approach.z});}
                if(GUILayout.Button("Cross edge (when nearby)"))RequestTravel(maps[Self.face].GetComponent<CubeEdgeMap>().destinationScene);
                if(GUILayout.Button("Stop"))Submit(new LabCommand{kind="stop"});
                GUILayout.Label("Session roster");
                foreach(var a in Latest.actors.Where(a=>a.player))
                    GUILayout.Label(a.name+" | "+a.face+(a.travelling?" | turning":a.connected?"":" | reconnect grace"));
            }
            if(!host){if(Connected){if(GUILayout.Button("Disconnect"))Disconnect();}else if(GUILayout.Button("Reconnect"))Reconnect();}
            if(GUILayout.Button("Main menu"))Leave();
            GUILayout.Label("Prototype: host migration / skills / progression are pending.");
            GUILayout.EndArea();
        }
    }
}
