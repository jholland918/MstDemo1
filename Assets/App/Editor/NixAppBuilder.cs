using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Utils.Editor;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Assets.App.Editor
{
    public class NixAppBuilder
    {
        public const string ToolMenu = "Tools/App Builds (Nix)/";

        [MenuItem(ToolMenu + "Room(Headless)")]
        private static void BuildRoomForLinuxHeadless()
        {
            BuildRoomForLinux(true);
        }

        [MenuItem(ToolMenu + "Room(Normal)")]
        private static void BuildRoomForLinuxNormal()
        {
            BuildRoomForLinux(false);
        }

        private static void BuildRoomForLinux(bool isHeadless)
        {
            string buildFolder = Path.Combine("Builds", "App", "Nix", "Room");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] {
                    "Assets/App/Scenes/Room/Room.unity"
                },
                locationPathName = Path.Combine(buildFolder, "Room.x86_64"),
                target = BuildTarget.StandaloneLinux64,
#if UNITY_2021_1_OR_NEWER
                options = BuildOptions.ShowBuiltPlayer | BuildOptions.Development,
                subtarget = isHeadless ? (int)StandaloneBuildSubtarget.Server : (int)StandaloneBuildSubtarget.Player
#else
                options = isHeadless ? BuildOptions.ShowBuiltPlayer | BuildOptions.EnableHeadlessMode : BuildOptions.ShowBuiltPlayer
#endif
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                string appConfig = Mst.Args.AppConfigFile(buildFolder);

                MstProperties properties = new MstProperties();
                properties.Add(Mst.Args.Names.StartClientConnection, true);
                properties.Add(Mst.Args.Names.MasterIp, Mst.Args.MasterIp);
                properties.Add(Mst.Args.Names.MasterPort, Mst.Args.MasterPort);
                properties.Add(Mst.Args.Names.RoomIp, Mst.Args.RoomIp);
                properties.Add(Mst.Args.Names.RoomPort, Mst.Args.RoomPort);

                File.WriteAllText(appConfig, properties.ToReadableString("\n", "="));

                Debug.Log("Room build succeeded: " + summary.totalSize / 1024 + " kb");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Room build failed");
            }
        }

        [MenuItem(ToolMenu + "Master Server and Spawner")]
        private static void BuildMasterAndSpawnerForLinux()
        {
            string buildFolder = Path.Combine("Builds", "App", "Nix", "MasterAndSpawner");
            string roomExePath = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "App", "Nix", "Room", "Room.x86_64");

            roomExePath = roomExePath.Replace("\\", "/");
            roomExePath = roomExePath.Replace("C:/", "/");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/App/Scenes/Master/Master.unity" },
                locationPathName = Path.Combine(buildFolder, "MasterAndSpawner.x86_64"),
                target = BuildTarget.StandaloneLinux64,
#if UNITY_2021_1_OR_NEWER
                options = BuildOptions.ShowBuiltPlayer | BuildOptions.Development,
                subtarget = (int)StandaloneBuildSubtarget.Server
#else
                options = BuildOptions.EnableHeadlessMode | BuildOptions.ShowBuiltPlayer | BuildOptions.Development
#endif
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                MstProperties properties = new MstProperties();
                properties.Add(Mst.Args.Names.StartMaster, true);
                properties.Add(Mst.Args.Names.StartSpawner, true);
                properties.Add(Mst.Args.Names.StartClientConnection, true);
                properties.Add(Mst.Args.Names.MasterIp, Mst.Args.MasterIp);
                properties.Add(Mst.Args.Names.MasterPort, Mst.Args.MasterPort);
                properties.Add(Mst.Args.Names.RoomExecutablePath, roomExePath);
                properties.Add(Mst.Args.Names.RoomIp, Mst.Args.RoomIp);
                properties.Add(Mst.Args.Names.RoomRegion, Mst.Args.RoomRegion);

                File.WriteAllText(Path.Combine(buildFolder, "application.cfg"), properties.ToReadableString("\n", "="));

                Debug.Log("Master Server build succeeded: " + summary.totalSize / 1024 + " kb");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Master Server build failed");
            }
        }

        [MenuItem(ToolMenu + "Spawner")]
        private static void BuildSpawnerForLinux()
        {
            string buildFolder = Path.Combine("Builds", "App", "Nix", "Spawner");
            string roomExePath = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "App", "Nix", "Room", "Room.x86_64");

            roomExePath = roomExePath.Replace("\\", "/");
            roomExePath = roomExePath.Replace("C:/", "/");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] {
                    "Assets/App/Scenes/Spawner/Spawner.unity"
                },
                locationPathName = Path.Combine(buildFolder, "Spawner.x86_64"),
                target = BuildTarget.StandaloneLinux64,
#if UNITY_2021_1_OR_NEWER
                options = BuildOptions.ShowBuiltPlayer | BuildOptions.Development,
                subtarget = (int)StandaloneBuildSubtarget.Server
#else
                options = BuildOptions.EnableHeadlessMode | BuildOptions.ShowBuiltPlayer | BuildOptions.Development
#endif
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                string appConfig = Mst.Args.AppConfigFile(buildFolder);

                MstProperties properties = new MstProperties();
                properties.Add(Mst.Args.Names.StartSpawner, true);
                properties.Add(Mst.Args.Names.StartClientConnection, true);
                properties.Add(Mst.Args.Names.MasterIp, Mst.Args.MasterIp);
                properties.Add(Mst.Args.Names.MasterPort, Mst.Args.MasterPort);
                properties.Add(Mst.Args.Names.RoomExecutablePath, roomExePath);
                properties.Add(Mst.Args.Names.RoomIp, Mst.Args.RoomIp);
                properties.Add(Mst.Args.Names.RoomRegion, Mst.Args.RoomRegion);

                File.WriteAllText(appConfig, properties.ToReadableString("\n", "="));

                Debug.Log("Spawner build succeeded: " + summary.totalSize / 1024 + " kb");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Spawner build failed");
            }
        }

        [MenuItem(ToolMenu + "Client")]
        private static void BuildClientForLinux()
        {
            string buildFolder = Path.Combine("Builds", "App", "Nix", "Client");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] {
                    "Assets/App/Scenes/Client/Client.unity",
                    "Assets/App/Scenes/Room/Room.unity"
                },
                locationPathName = Path.Combine(buildFolder, "Client.x86_64"),
                target = BuildTarget.StandaloneLinux64,
                options = BuildOptions.ShowBuiltPlayer | BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                string appConfig = Mst.Args.AppConfigFile(buildFolder);

                MstProperties properties = new MstProperties();
                properties.Add(Mst.Args.Names.StartClientConnection, true);
                properties.Add(Mst.Args.Names.MasterIp, Mst.Args.MasterIp);
                properties.Add(Mst.Args.Names.MasterPort, Mst.Args.MasterPort);

                File.WriteAllText(appConfig, properties.ToReadableString("\n", "="));

                Debug.Log("Client build succeeded: " + summary.totalSize / 1024 + " kb");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Client build failed");
            }
        }
    }
}
