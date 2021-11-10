using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Build.Reporting;

namespace Filament.Editor.Build {
    public class CI {
        static string[] SCENES = FindEnabledEditorScenes();
        static string APP_NAME = PlayerSettings.applicationIdentifier.Split('.').Last();
        static string TARGET_DIR = "dist";

        [MenuItem("Filament/Build/CI/Win 64")]
        static void PerformWin64Build () {
            string target_dir = TARGET_DIR + "/Win64/" + APP_NAME + ".exe";
			BuildPlayerOptions options = new BuildPlayerOptions() {
				scenes = SCENES,
				locationPathName = target_dir,
				targetGroup = BuildTargetGroup.Standalone,
				target = BuildTarget.StandaloneWindows64,
				options = GetGenericBuildOptions()
			};
			GenericBuild(options);
		}

        [MenuItem("Filament/Build/CI/Win 32")]
        static void PerformWin32Build () {
            string target_dir = TARGET_DIR + "/Win32/" + APP_NAME + ".exe";
			BuildPlayerOptions options = new BuildPlayerOptions() {
				scenes = SCENES,
				locationPathName = target_dir,
				targetGroup = BuildTargetGroup.Standalone,
				target = BuildTarget.StandaloneWindows,
				options = GetGenericBuildOptions()
			};
			GenericBuild(options);
		}

        [MenuItem("Filament/Build/CI/Web GL")]
        static void PerformWebGLBuild () {
            string target_dir = TARGET_DIR + "/WebGL/" + APP_NAME;
			BuildPlayerOptions options = new BuildPlayerOptions() {
				scenes = SCENES,
				locationPathName = target_dir,
				targetGroup = BuildTargetGroup.WebGL,
				target = BuildTarget.WebGL,
				options = GetGenericBuildOptions()
			};
			GenericBuild(options);
		}

        [MenuItem("Filament/Build/CI/OSX")]
        static void PerformOSXBuild () {
            string target_dir = TARGET_DIR + "/OSX/" + APP_NAME + ".app";
			BuildPlayerOptions options = new BuildPlayerOptions() {
				scenes = SCENES,
				locationPathName = target_dir,
				targetGroup = BuildTargetGroup.Standalone,
				target = BuildTarget.StandaloneOSX,
				options = GetGenericBuildOptions()
			};
			GenericBuild(options);
		}

        [MenuItem("Filament/Build/CI/Linux")]
        static void PerformLinuxBuild () {
            string target_dir = TARGET_DIR + "/Linux/" + APP_NAME;
			BuildPlayerOptions options = new BuildPlayerOptions() {
				scenes = SCENES,
				locationPathName = target_dir,
				targetGroup = BuildTargetGroup.Standalone,
				target = BuildTarget.StandaloneLinuxUniversal,
				options = GetGenericBuildOptions()
			};
			GenericBuild(options);
		}

        [MenuItem("Filament/Build/CI/iPhone")]
        static void PerformiPhoneBuild () {
            string target_dir = TARGET_DIR + "/iPhone/" + APP_NAME + ".ipa";
			BuildPlayerOptions options = new BuildPlayerOptions() {
				scenes = SCENES,
				locationPathName = target_dir,
				targetGroup = BuildTargetGroup.iOS,
				target = BuildTarget.iOS,
				options = GetGenericBuildOptions()
			};
			GenericBuild(options);
		}

        [MenuItem("Filament/Build/CI/Android")]
        static void PerformAndroidBuild () {
            string target_dir = TARGET_DIR + "/Android/" + APP_NAME + ".apk";
            ParseAndroidFlags(ref target_dir);
			BuildPlayerOptions options = new BuildPlayerOptions() {
				scenes = SCENES,
				locationPathName = target_dir,
				targetGroup = BuildTargetGroup.Android,
				target = BuildTarget.Android,
				options = GetGenericBuildOptions()
			};
			GenericBuild(options);
		}

        private static BuildOptions GetGenericBuildOptions () {
            string[] args = Environment.GetCommandLineArgs();
            foreach (string arg in args) {
                if (arg == "-debug") {
                    return BuildOptions.Development | BuildOptions.AllowDebugging;
                }
            }

            return BuildOptions.None;
        }

        static void ParseAndroidFlags (ref string target_dir) {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; ++i) {
                string current = args[i];
                if (current == "-sign-apk") {
                    if (i + 2 < args.Length) {
                        string keystorePass = args[i + 1];
                        string keyaliasPass = args[i + 2];
                        PlayerSettings.keystorePass = keystorePass;
                        PlayerSettings.keyaliasPass = keyaliasPass;
                        i += 2;
                        Debug.Log("Detected passwords for keystore and alias.");
                    } else {
                        Debug.LogWarning("Not enough arguments provided to '-sign-apk'.");
                    }
                } else if (current == "-filename") {
                    if (i + 1 < args.Length) {
                        string oldTarget = target_dir;
                        string directory = Path.GetDirectoryName(oldTarget);
                        string extension = Path.GetExtension(oldTarget);
                        target_dir = Path.ChangeExtension(Path.Combine(directory, args[i + 1]), extension);
                        Debug.LogFormat("Switched output from '{0}' to '{1}'...", oldTarget, target_dir);
                    }
                } else if (current == "-versioncode") {
                    if (i + 1 < args.Length) {
                        int versionCode = -1;
                        if (int.TryParse(args[i + 1], out versionCode)) {
                            PlayerSettings.Android.bundleVersionCode = versionCode;
                            Debug.Log("Version code set to: " + versionCode);
                        } else {
                            Debug.LogError("Invalid version code!");
                        }
                    }
                }
            }
        }

        private static string[] FindEnabledEditorScenes () {
            List<string> EditorScenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
                if (!scene.enabled) continue;
                EditorScenes.Add(scene.path);
            }
            return EditorScenes.ToArray();
        }

        static void GenericBuild(BuildPlayerOptions options) {

			// sorta gross hack to reset the path
			options.locationPathName = Application.dataPath + "/../" + options.locationPathName;

			try {
                Directory.CreateDirectory(options.locationPathName);
            } catch { }

			EditorUserBuildSettings.SwitchActiveBuildTarget(options.targetGroup, options.target);
            BuildReport res = BuildPipeline.BuildPlayer(options);
            if (res.summary.result != BuildResult.Succeeded) {
                throw new Exception("BuildPlayer failure: " + res);
            }
        }
    }
}

