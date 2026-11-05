using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.ApplicationFlow
{
    /// <summary>Shared presentation for startup work and full-scene transitions.</summary>
    public sealed class LoadingScreen : MonoBehaviour
    {
        static readonly Color Gold = new(.87f, .72f, .44f);
        static readonly Color Teal = new(.40f, .88f, .80f);
        static LoadingScreen current;
        Texture2D backdrop;
        GUIStyle title, caption, detail;
        string message;
        float? progress;
        float openedAt;
        const float MinimumDisplaySeconds = .25f;

        public static bool Busy => current != null;

        public static bool TryLoadAdditive(string scene,string message,Action<Scene> ready,Action<Exception> failed)
        {
            if(Busy)return false;
            var screen=Open(message);screen.progress=0;
            screen.StartCoroutine(screen.LoadAdditive(scene,ready,failed));return true;
        }
        IEnumerator LoadAdditive(string scene,Action<Scene> ready,Action<Exception> failed)
        {
            yield return null;AsyncOperation operation=null;Exception error=null;
            try{operation=SceneManager.LoadSceneAsync(scene,LoadSceneMode.Additive);}catch(Exception e){error=e;}
            if(operation!=null)
            {
                while(!operation.isDone){progress=Mathf.Clamp01(operation.progress/.9f);yield return null;}
                while(Time.unscaledTime-openedAt<MinimumDisplaySeconds)yield return null;
                var loaded=SceneManager.GetSceneByName(scene);
                if(!loaded.isLoaded)error=new InvalidOperationException("Scene did not finish loading: "+scene);
                else try{ready?.Invoke(loaded);}catch(Exception e){error=e;}
            }
            if(operation==null&&error==null)error=new InvalidOperationException("Scene could not be loaded: "+scene);
            Close();if(error!=null)failed?.Invoke(error);
        }

        public static void Initialize(string message, Action initialize)
        {
            // Returning to the menu already has a screen covering initialization.
            if (Busy) { initialize(); return; }
            var screen = Open(message);
            screen.StartCoroutine(screen.InitializeAfterPaint(initialize));
        }

        public static bool TryLoadScene(string scene, string message, Action<Exception> onFailure)
        {
            if (Busy) return false;
            if (!Application.CanStreamedLevelBeLoaded(scene))
            {
                onFailure?.Invoke(new ArgumentException($"Scene '{scene}' is unavailable in this build."));
                return false;
            }

            var screen = Open(message);
            screen.progress = 0;
            screen.StartCoroutine(screen.Load(scene, onFailure));
            return true;
        }

        static LoadingScreen Open(string message)
        {
            var root = new GameObject("DiceFree Loading Screen");
            DontDestroyOnLoad(root);
            current = root.AddComponent<LoadingScreen>();
            current.message = message;
            current.openedAt = Time.unscaledTime;
            return current;
        }

        IEnumerator InitializeAfterPaint(Action initialize)
        {
            yield return null;
            try
            {
                initialize();
                while (Time.unscaledTime - openedAt < MinimumDisplaySeconds) yield return null;
            }
            finally { Close(); }
        }

        IEnumerator Load(string scene, Action<Exception> onFailure)
        {
            yield return null;
            AsyncOperation operation = null;
            Exception failure = null;
            try { operation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single); }
            catch (Exception exception) { failure = exception; }
            if (operation == null)
            {
                Close();
                onFailure?.Invoke(failure ?? new InvalidOperationException($"Could not load '{scene}'."));
                yield break;
            }

            while (!operation.isDone)
            {
                // Unity reserves the final portion for scene activation.
                progress = Mathf.Min(.9f, operation.progress);
                yield return null;
            }
            progress = 1;
            message = "Ready";
            // Let destination Start methods run behind the cover.
            yield return null;
            while (Time.unscaledTime - openedAt < MinimumDisplaySeconds) yield return null;
            Close();
        }

        void Close()
        {
            enabled = false;
            if (current == this) current = null;
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            if (current == this) current = null;
            if (backdrop != null) Destroy(backdrop);
        }

        void OnGUI()
        {
            var input = Event.current;
            if (input.isKey || input.isMouse || input.type == EventType.ScrollWheel)
            {
                input.Use();
                return;
            }
            if (input.type != EventType.Repaint) return;

            int depth = GUI.depth;
            Matrix4x4 matrix = GUI.matrix;
            Color tint = GUI.color;
            GUI.depth = -10000;
            GUI.color = Color.white;
            try
            {
                if (backdrop == null) MakeBackdrop();
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), backdrop);
                float scale = Mathf.Min(Screen.width / 800f, Screen.height / 520f);
                GUI.matrix = Matrix4x4.TRS(new Vector3(Screen.width / 2f, Screen.height / 2f),
                    Quaternion.identity, Vector3.one * scale);
                title ??= Text(76, new Color(.96f, .92f, .84f), true);
                caption ??= Text(14, Gold);
                detail ??= Text(18, new Color(.76f, .84f, .86f));

                Fill(new Rect(-220, -133, 440, 1), new Color(.30f, .29f, .22f));
                Fill(new Rect(-26, -159, 52, 52), Gold);
                Fill(new Rect(-24, -157, 48, 48), new Color(.07f, .14f, .17f));
                Fill(new Rect(-5, -138, 10, 10), Teal);
                GUI.Label(new Rect(-350, -91, 700, 88), "DiceFree", title);
                GUI.Label(new Rect(-300, 0, 600, 25), "A N   E C H O   B E Y O N D   T H E   D I C E", caption);

                const float width = 440;
                Fill(new Rect(-221, 75, 442, 10), new Color(.28f, .33f, .34f));
                Fill(new Rect(-220, 76, width, 8), new Color(.036f, .055f, .067f));
                if (progress.HasValue)
                    Fill(new Rect(-220, 76, width * progress.Value, 8), Teal);
                else
                    Fill(new Rect(-220 + Mathf.PingPong(Time.unscaledTime * 240, width - 96), 76, 96, 8), Teal);
                string suffix = progress.HasValue ? $"  ·  {Mathf.FloorToInt(progress.Value * 100)}%" : "";
                GUI.Label(new Rect(-350, 102, 700, 30), message + suffix, detail);
            }
            finally
            {
                GUI.depth = depth;
                GUI.matrix = matrix;
                GUI.color = tint;
            }
        }

        void MakeBackdrop()
        {
            backdrop = new Texture2D(1, 128, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            for (int i = 0; i < backdrop.height; i++)
            {
                float light = Mathf.Sin(i / 127f * Mathf.PI);
                backdrop.SetPixel(0, i, Color.Lerp(new Color(.014f, .022f, .036f),
                    new Color(.052f, .116f, .142f), light));
            }
            backdrop.Apply(false, true);
        }

        static GUIStyle Text(int size, Color color, bool bold = false) => new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = size,
            fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
            normal = { textColor = color }
        };

        static void Fill(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
