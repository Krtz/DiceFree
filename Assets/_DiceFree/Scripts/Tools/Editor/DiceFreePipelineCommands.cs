using System;
using System.Diagnostics;
using DiceFree.EditorTools;
using Unity.Pipeline.Commands;
using UnityEngine;

namespace DiceFree.EditorTools
{
    /// <summary>Stable, machine-readable validation commands for Unity CLI + Pipeline.</summary>
    internal static class DiceFreePipelineCommands
    {
        [CliCommand("dicefree.architecture.validate", "Validate DiceFree assembly boundaries and first-party source ownership.", Tags = new[] { "tests", "dicefree/architecture" })]
        private static object ArchitectureValidate([CliArg("test_git_sha", "Git SHA being validated, when known.")] string testGitSha = "") =>
            RunSynchronous("architecture", "DICEFRE_ARCHITECTURE_OK", ArchitectureValidation.Run, testGitSha);

        [CliCommand("dicefree.architecture.policy", "Run DiceFree architecture dependency-policy self-tests.", Tags = new[] { "tests", "dicefree/architecture" })]
        private static object ArchitecturePolicy([CliArg("test_git_sha", "Git SHA being validated, when known.")] string testGitSha = "") =>
            RunSynchronous("architecture-policy", "DICEFRE_ARCH_POLICY_SELFTEST_OK", ArchitectureValidation.RunPolicySelfTests, testGitSha);

        [CliCommand("dicefree.managed-references.validate", "Validate first-party SerializeReference data and migration compatibility.", Tags = new[] { "tests", "dicefree/architecture" })]
        private static object ManagedReferences([CliArg("test_git_sha", "Git SHA being validated, when known.")] string testGitSha = "") =>
            RunSynchronous("managed-references", "DICEFREE_MANAGED_REFERENCE_OK", ManagedReferenceValidation.Run, testGitSha);

        [CliCommand("dicefree.issue36.route12", "Start focused Issue 36 route-12 traversal validation. Poll dicefree.issue36.status until complete.", Tags = new[] { "tests", "dicefree/issue36" })]
        private static object StartRoute12([CliArg("test_git_sha", "Git SHA being validated, when known.")] string testGitSha = "") =>
            StartIssue36("route12", testGitSha);

        [CliCommand("dicefree.issue36.runner", "Start the existing dedicated Runner Returns harness under a fresh isolated save root. Poll dicefree.issue36.status until complete.", Tags = new[] { "tests", "dicefree/issue36" })]
        private static object StartRunner([CliArg("test_git_sha", "Git SHA being validated, when known.")] string testGitSha = "") =>
            StartIssue36("runner", testGitSha);

        [CliCommand("dicefree.issue36.save-reload", "Start the existing two-phase save/reload harness under one fresh isolated save root. Poll dicefree.issue36.status until complete.", Tags = new[] { "tests", "dicefree/issue36" })]
        private static object StartSaveReload([CliArg("test_git_sha", "Git SHA being validated, when known.")] string testGitSha = "") =>
            StartIssue36("save", testGitSha);

        [CliCommand("dicefree.issue36.status", "Read the current or most recently completed Issue 36 validation task result.", Tags = new[] { "tests", "dicefree/issue36" })]
        private static object Issue36Status(
            [CliArg("task_id", "Optional task ID returned by an Issue 36 start command.")] string taskId = "") =>
            Issue36ManualValidation.GetPipelineTaskStatus(taskId);

        private static object RunSynchronous(string name, string expectedMarker, Action validate, string testGitSha)
        {
            var timer = Stopwatch.StartNew();
            bool markerSeen = false;
            string error = null;
            Application.LogCallback capture = (message, stack, type) =>
            {
                if (message.StartsWith(expectedMarker, StringComparison.Ordinal)) markerSeen = true;
                if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                {
                    if (!string.IsNullOrEmpty(error)) error += "\n";
                    error += message + (string.IsNullOrEmpty(stack) ? string.Empty : "\n" + stack);
                }
            };

            Application.logMessageReceived += capture;
            try { validate(); }
            catch (Exception exception) { error = exception.ToString(); }
            finally { Application.logMessageReceived -= capture; }
            timer.Stop();

            bool success = markerSeen && string.IsNullOrEmpty(error);
            if (!success && string.IsNullOrEmpty(error)) error = "Expected final marker was not emitted: " + expectedMarker;
            return new
            {
                success,
                validation = name,
                finalMarker = success ? expectedMarker : null,
                elapsedSeconds = timer.Elapsed.TotalSeconds,
                error,
                temporarySaveRoot = (string)null,
                logPath = Application.consoleLogPath,
                testedGitSha = testGitSha ?? string.Empty
            };
        }

        private static object StartIssue36(string kind, string testGitSha)
        {
            if (Issue36ManualValidation.IsPipelineValidationBusy())
                return new { success = false, status = "rejected", validation = kind, taskId = string.Empty,
                    finalMarker = (string)null, elapsedSeconds = 0d, error = "Another Issue 36 check or Play Mode transition is already active.",
                    temporarySaveRoot = (string)null, logPath = Application.consoleLogPath, testedGitSha = testGitSha ?? string.Empty };

            Issue36ManualValidation.StartFromPipeline(kind, testGitSha);
            return Issue36ManualValidation.GetPipelineTaskStatus(string.Empty);
        }
    }
}
