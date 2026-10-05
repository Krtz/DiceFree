using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.Pipeline.Commands;

namespace DiceFree.EditorTools
{
    /// <summary>Expose the unchanged existing item/Q4 harnesses through the connected Editor.</summary>
    [InitializeOnLoad]
    internal static class WorkGlovesRegressionValidation
    {
        private const string Key = "DiceFree.WorkGlovesRegression.";
        static WorkGlovesRegressionValidation() => Application.logMessageReceived += Capture;

        [CliCommand("dicefree.art.gloves.regression", "Start an existing isolated regression harness (items or q4); poll regression-status.")]
        private static object Start([CliArg("suite", "Existing suite: items or q4.")] string suite)
        {
            WorkGlovesArtValidation.Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Requires idle Edit Mode.");
            WorkGlovesArtValidation.Require(suite == "items" || suite == "q4", "Unknown suite.");
            string root = Path.Combine(Path.GetTempPath(), "DiceFree-GlovesRegression-" + suite + "-" + Guid.NewGuid().ToString("N"));
            SessionState.SetString(Key + "root", root); SessionState.SetString(Key + "suite", suite);
            SessionState.SetString(Key + "status", "running"); SessionState.SetString(Key + "error", "");
            try { if (suite == "items") ItemValidation.RunFromPipeline(root); else CornbergSurgeValidation.RunFromPipeline(root); }
            catch (Exception e) { SessionState.SetString(Key + "error", e.ToString()); Complete(1); }
            return Status();
        }

        [CliCommand("dicefree.art.gloves.regression-status", "Read the most recent existing item/Q4 regression result.")]
        private static object Status() => new { status = SessionState.GetString(Key + "status", "idle"),
            success = SessionState.GetString(Key + "status", "idle") == "passed", suite = SessionState.GetString(Key + "suite", ""),
            error = SessionState.GetString(Key + "error", ""), temporarySaveRoot = SessionState.GetString(Key + "root", "") };

        private static void Capture(string message, string stack, LogType type)
        {
            if (SessionState.GetString(Key + "status", "") != "running" ||
                (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)) return;
            if (message.Contains("ArgumentOutOfRangeException") && stack.Contains("SearchDatabase") && stack.Contains("IndexationOnStartup")) return;
            SessionState.SetString(Key + "error", message + "\n" + stack);
        }

        internal static void Complete(int code)
        {
            SessionState.SetString(Key + "status", code == 0 ? "passed" : "failed");
            Debug.Log("WORK_GLOVES_EXISTING_REGRESSION_" + (code == 0 ? "OK " : "FAIL ") + SessionState.GetString(Key + "suite", ""));
        }
    }
}
