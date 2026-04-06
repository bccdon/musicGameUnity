using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System;
using System.Linq;

namespace PulseHighway.Editor
{
    public static class BuildScript
    {
        private static readonly string[] Scenes = { "Assets/Scenes/BootstrapScene.unity" };

        public static void PerformBuild()
        {
            var target = GetTargetFromArgs();
            var outputPath = GetArgValue("-outputPath") ?? GetDefaultOutputPath(target);

            Debug.Log($"Building for {target} to {outputPath}");

            var options = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = outputPath,
                target = target,
                options = BuildOptions.None
            };

            // Android-specific settings
            if (target == BuildTarget.Android)
            {
                PlayerSettings.Android.bundleVersionCode = 1;
                EditorUserBuildSettings.buildAppBundle = false; // APK by default
                EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            }

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            Debug.Log($"Build result: {summary.result}");
            Debug.Log($"Total time: {summary.totalTime}");
            Debug.Log($"Total size: {summary.totalSize} bytes");
            Debug.Log($"Warnings: {summary.totalWarnings}");
            Debug.Log($"Errors: {summary.totalErrors}");

            if (summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"Build failed with {summary.totalErrors} errors");
                EditorApplication.Exit(1);
            }
            else
            {
                Debug.Log("Build succeeded!");
                EditorApplication.Exit(0);
            }
        }

        private static BuildTarget GetTargetFromArgs()
        {
            var targetArg = GetArgValue("-buildTarget");
            if (string.IsNullOrEmpty(targetArg))
                return BuildTarget.StandaloneWindows64;

            return targetArg.ToLowerInvariant() switch
            {
                "standalonewindows64" or "win64" or "windows" => BuildTarget.StandaloneWindows64,
                "standalonelinux64" or "linux64" or "linux" => BuildTarget.StandaloneLinux64,
                "android" => BuildTarget.Android,
                "ios" => BuildTarget.iOS,
                "webgl" => BuildTarget.WebGL,
                _ => BuildTarget.StandaloneWindows64
            };
        }

        private static string GetDefaultOutputPath(BuildTarget target)
        {
            return target switch
            {
                BuildTarget.StandaloneWindows64 => "Builds/Windows/PulseHighway.exe",
                BuildTarget.StandaloneLinux64 => "Builds/Linux/PulseHighway",
                BuildTarget.Android => "Builds/Android/PulseHighway.apk",
                BuildTarget.iOS => "Builds/iOS",
                BuildTarget.WebGL => "Builds/WebGL",
                _ => "Builds/Default/PulseHighway"
            };
        }

        private static string GetArgValue(string argName)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == argName)
                    return args[i + 1];
            }
            return null;
        }
    }
}
