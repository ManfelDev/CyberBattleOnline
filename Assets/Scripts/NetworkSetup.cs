using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Collections;

using Debug = UnityEngine.Debug;
#if UNITY_STANDALONE_WIN
using System.Runtime.InteropServices;
#endif
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
#endif


public class NetworkSetup : MonoBehaviour
{
    public class RelayHostData
    {
        public string   JoinCode;
        public string   IPv4Address;
        public ushort   Port;
        public Guid     AllocationID;
        public byte[]   AllocationIDBytes;
        public byte[]   ConnectionData;
        public byte[]   HostConnectionData;
        public byte[]   Key;
    }

    [SerializeField] private bool            forceLocalServer = false;
    [SerializeField] private List<Player>    playerPrefabs;
    [SerializeField] private List<Transform> playerSpawnLocations;
    [SerializeField] private int             maxPlayers = 2;
    [SerializeField] private string          joinCode;
    [SerializeField] private TextMeshProUGUI textJoinCode;

    private bool            isServer = false;
    private int             playerPrefabIndex = 0;
    private bool            isRelay;
    private UnityTransport  transport;
    private RelayHostData   relayData;

    void Start()
    {
        // Parse command line arguments
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--server")
            {
                // --server found, this should be a server application
                isServer = true;
            }
            else if (args[i] == "--code")
            {
                joinCode = ((i + 1) < args.Length) ? (args[i + 1]) : ("");
            }
        }

        // Check if the join code was set in the JoinManager
        if (string.IsNullOrEmpty(joinCode) && !string.IsNullOrEmpty(JoinManager.joinCode))
        {
            joinCode = JoinManager.joinCode;
        }

        if (forceLocalServer) isServer = true;

        transport = GetComponent<UnityTransport>();
        if (transport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport)
        {
            isRelay = true;
        }
        else
        {
            textJoinCode.gameObject.SetActive(false);
        }

        if (isServer)
            StartCoroutine(StartAsServerCR());
        else
            StartCoroutine(StartAsClientCR());
    }
    
    IEnumerator StartAsServerCR()
    {
        SetWindowTitle("CyberBattle (server mode)");

        var networkManager = GetComponent<NetworkManager>();
        networkManager.enabled = true;
        transport.enabled = true;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        // Wait a frame for setups to be done
        yield return null;

        if (isRelay)
        {
            // Vou chamar uma função que é async (ver abaixo)
            // Isso devolve um Task 
            var loginTask = Login();

            // Fico à espera que a Task acabe, verificando o IsComplete
            yield return new WaitUntil(() => loginTask.IsCompleted);

            // Verifico se houve um exception na tarefa. Esta foi 
            // executada numa tarefa distinta, por isso não é propagada, e é normalmente
            // como se propaga erros
            if (loginTask.Exception != null)
            {
                Debug.LogError("Login failed: " + loginTask.Exception);
                yield break;
            }

            // Tarefa foi concluída, podiamos agora ir buscar o resultado (já vamos ver)
            Debug.Log("Login successfull!");

            var allocationTask = CreateAllocationAsync(maxPlayers);

            yield return new WaitUntil(() => allocationTask.IsCompleted);

            if (allocationTask.Exception != null)
            {
                Debug.LogError("Allocation failed: " + allocationTask.Exception);
                yield break;
            }
            else
            {
                Debug.Log("Allocation successfull!");

                Allocation allocation = allocationTask.Result;

                relayData = new RelayHostData();

                // Find the appropriate endpoint, just select the first one and use it
                foreach (var endpoint in allocation.ServerEndpoints)
                {
                    relayData.IPv4Address = endpoint.Host;
                    relayData.Port = (ushort)endpoint.Port;
                    break;
                }

                relayData.AllocationID = allocation.AllocationId;
                relayData.AllocationIDBytes = allocation.AllocationIdBytes;
                relayData.ConnectionData = allocation.ConnectionData;
                relayData.Key = allocation.Key;

                var joinCodeTask = GetJoinCodeAsync(relayData.AllocationID);

                yield return new WaitUntil(() => joinCodeTask.IsCompleted);

                if (joinCodeTask.Exception != null)
                {
                    Debug.LogError("Join code failed: " + joinCodeTask.Exception);
                    yield break;
                }
                else
                {
                    Debug.Log("Code retrieved!");

                    relayData.JoinCode = joinCodeTask.Result;

                    if (textJoinCode != null)
                    {
                        textJoinCode.text = $"JoinCode:{relayData.JoinCode}";
                        textJoinCode.gameObject.SetActive(true);
                    }

                    transport.SetRelayServerData(relayData.IPv4Address, relayData.Port, relayData.AllocationIDBytes, 
                                                 relayData.Key, relayData.ConnectionData);
                }
            }
        }

        if (networkManager.StartServer())
        {
            Debug.Log($"Serving on port {transport.ConnectionData.Port}...");
        }
        else
        {
            Debug.LogError($"Failed to serve on port {transport.ConnectionData.Port}...");
        }
    }

    private async Task<Allocation> CreateAllocationAsync(int maxPlayers)
    {
        try
        {
            // This requests space for maxPlayers + 1 connections (the +1 is for the server itself)
            Allocation allocation = await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(maxPlayers);
            return allocation;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error creating allocation: " + e);
            throw;
        }
    }

    private async Task Login()
    {
        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error login: " + e);
            throw;
        }
    }

    private async Task<string> GetJoinCodeAsync(Guid allocationID)
    {
        try
        {
            string code = await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocationID);

            return code;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error retrieving join code: " + e);
            throw;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.LogError($"Player {clientId} connected, prefab index = {playerPrefabIndex}!");

        // Get a free spot for this player from the SpawnManager
        var spawnPos = FindObjectOfType<SpawnManager>().GetSpawnPosition();

        // Spawn player object
        var spawnedObject = Instantiate(playerPrefabs[playerPrefabIndex], spawnPos, Quaternion.identity);
        var prefabNetworkObject = spawnedObject.GetComponent<NetworkObject>();
        prefabNetworkObject.SpawnAsPlayerObject(clientId, true);
        prefabNetworkObject.ChangeOwnership(clientId);

        // Set player name
        var playerComponent = spawnedObject.GetComponent<Player>();
        if (playerComponent != null && !string.IsNullOrEmpty(JoinManager.playerName.ToString()))
        {
            playerComponent.PlayerName.Value = JoinManager.playerName;
        }

        playerPrefabIndex = (playerPrefabIndex + 1) % playerPrefabs.Count;
    }


    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Player {clientId} disconnected!");
    }

    IEnumerator StartAsClientCR()
    {
        SetWindowTitle("CyberBattle (client mode)");

        var networkManager = GetComponent<NetworkManager>();
        networkManager.enabled = true;
        transport.enabled = true;

        // Wait a frame for setups to be done
        yield return null;

        if (isRelay)
        {
            var loginTask = Login();

            yield return new WaitUntil(() => loginTask.IsCompleted);

            if (loginTask.Exception != null)
            {
                Debug.LogError("Login failed: " + loginTask.Exception);
                yield break;
            }

            Debug.Log("Login successfull!");

            //Ask Unity Services for allocation data based on a join code
            var joinAllocationTask = JoinAllocationAsync(joinCode);

            yield return new WaitUntil(() => joinAllocationTask.IsCompleted);

            if (joinAllocationTask.Exception != null)
            {
                Debug.LogError("Join allocation failed: " + joinAllocationTask.Exception);
                yield break;
            }
            else
            {
                Debug.Log("Allocation joined!");

                relayData = new RelayHostData();

                var allocation = joinAllocationTask.Result;

                // Find the appropriate endpoint, just select the first one and use it
                foreach (var endpoint in allocation.ServerEndpoints)
                {
                    relayData.IPv4Address = endpoint.Host;
                    relayData.Port = (ushort)endpoint.Port;
                    break;
                }

                relayData.AllocationID = allocation.AllocationId;
                relayData.AllocationIDBytes = allocation.AllocationIdBytes;
                relayData.ConnectionData = allocation.ConnectionData;
                relayData.HostConnectionData = allocation.HostConnectionData;
                relayData.Key = allocation.Key;

                transport.SetRelayServerData(relayData.IPv4Address, relayData.Port, 
                                             relayData.AllocationIDBytes, relayData.Key, relayData.ConnectionData, 
                                             relayData.HostConnectionData);
            }
        }

        if (networkManager.StartClient())
        {
            Debug.Log($"Connecting on port {transport.ConnectionData.Port}...");
        }
        else
        {
            Debug.LogError($"Failed to connect on port {transport.ConnectionData.Port}...");
        }
    }

    private async Task<JoinAllocation> JoinAllocationAsync(string joinCode)
    {
        try
        {
            // This requests space for maxPlayers + 1 connections (the +1 is for the server itself)
            var allocation = await Unity.Services.Relay.RelayService.Instance.JoinAllocationAsync(joinCode);
            
            return allocation;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error joining allocation: " + e);
            throw;
        }
    }


#if UNITY_STANDALONE_WIN
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowText(IntPtr hWnd, string lpString);

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    static extern IntPtr EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    // Delegate to filter windows
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    private static IntPtr FindWindowByProcessId(uint processId)
    {
        IntPtr windowHandle = IntPtr.Zero;
        EnumWindows((hWnd, lParam) =>
        {
            uint windowProcessId;
            GetWindowThreadProcessId(hWnd, out windowProcessId);
            if (windowProcessId == processId)
            {
                windowHandle = hWnd;
                return false; // Found the window, stop enumerating
            }
            return true; // Continue enumerating
        }, IntPtr.Zero);
        return windowHandle;
    }

    static void SetWindowTitle(string title)
    {
#if !UNITY_EDITOR
        uint processId = (uint)Process.GetCurrentProcess().Id;
        IntPtr hWnd = FindWindowByProcessId(processId);
        if (hWnd != IntPtr.Zero)
        {
            SetWindowText(hWnd, title);
        }
#endif
    }
#else
    static void SetWindowTitle(string title)
    {
    }
#endif

#if UNITY_EDITOR
    [MenuItem("Tools/Build Windows (x64)", priority = 0)]
    public static bool BuildGameServer()
    {
        // Specify build options
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        var enabledScenes = EditorBuildSettings.scenes.Where(s => s.enabled).ToArray();

        if (enabledScenes.Length > 1)
        {
            buildPlayerOptions.scenes = new[] { enabledScenes[1].path };
        }
        else
        {
            Debug.LogError("There are not enough enabled scenes to select index 1.");
            return false;
        }

        buildPlayerOptions.locationPathName = Path.Combine("Builds", "CyberBattle.exe");
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;

        // Perform the build
        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        // Output the result of the build
        Debug.Log($"Build ended with status: {report.summary.result}");

        // Check if the build was successful
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build was successful!");
        }
        else if (report.summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed.");
        }
        else if (report.summary.result == BuildResult.Cancelled)
        {
            Debug.Log("Build was cancelled.");
        }
        else if (report.summary.result == BuildResult.Unknown)
        {
            Debug.Log("Build result is unknown.");
        }

        // Additional information about the build can be logged
        Debug.Log($"Total errors: {report.summary.totalErrors}");
        Debug.Log($"Total warnings: {report.summary.totalWarnings}");

        return report.summary.result == BuildResult.Succeeded;
    }

    public static bool BuildGame()
    {
        // Specify build options
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();
        buildPlayerOptions.locationPathName = Path.Combine("Builds", "CyberBattle.exe");
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;


        // Perform the build
        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        // Output the result of the build
        Debug.Log($"Build ended with status: {report.summary.result}");

        // Check if the build was successful
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build was successful!");
        }
        else if (report.summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed.");
        }
        else if (report.summary.result == BuildResult.Cancelled)
        {
            Debug.Log("Build was cancelled.");
        }
        else if (report.summary.result == BuildResult.Unknown)
        {
            Debug.Log("Build result is unknown.");
        }

        // Additional information about the build can be logged
        Debug.Log($"Total errors: {report.summary.totalErrors}");
        Debug.Log($"Total warnings: {report.summary.totalWarnings}");

        return report.summary.result == BuildResult.Succeeded;
    }

    [MenuItem("Tools/Build and Launch (Server + Client)", priority = 12)]
    public static void BuildAndLaunch2()
    {
        CloseAll();
        if (BuildGameServer())
        {
            Launch1();
        }
        if (BuildGame())
        {
            Launch1_NoServer();
        }
    }

    private static void Launch1()
    {
        Run("Builds\\CyberBattle.exe", "--server");
    }

    [MenuItem("Tools/Launch (Client)", priority = 31)]
    public static void Launch1_NoServer()
    {
        StartClient();
    }

    static void StartClient()
    {
#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying)
        {
            NetworkSetup networkSetup = FindObjectOfType<NetworkSetup>();
            if (networkSetup)
            {
                if ((networkSetup.isRelay) && (networkSetup.isServer) && (networkSetup.relayData != null))
                {
                    Run("Builds\\CyberBattle.exe", $"--code {networkSetup.relayData.JoinCode}");
                }
                else
                {
                    Run("Builds\\CyberBattle.exe", "");
                }
            }
            else
            {
                Run("Builds\\CyberBattle.exe", "");
            }
        }
        else
        {
            Run("Builds\\CyberBattle.exe", "");
        }
#else
        Run("Builds\\CyberBattle.exe", "");
#endif
    }

    [MenuItem("Tools/Close All", priority = 100)]
    public static void CloseAll()
    {
        // Get all processes with the specified name
        Process[] processes = Process.GetProcessesByName("CyberBattle");

        foreach (var process in processes)
        {
            try
            {
                // Close the process
                process.Kill();
                // Wait for the process to exit
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                // Handle exceptions, if any
                // This could occur if the process has already exited or you don't have permission to kill it
                Debug.LogWarning($"Error trying to kill process {process.ProcessName}: {ex.Message}");
            }
        }
    }

    private static void Run(string path, string args)
    {
        // Start a new process
        Process process = new Process();

        // Configure the process using the StartInfo properties
        process.StartInfo.FileName = path;
        process.StartInfo.Arguments = args;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Normal; // Choose the window style: Hidden, Minimized, Maximized, Normal
        process.StartInfo.RedirectStandardOutput = false; // Set to true to redirect the output (so you can read it in Unity)
        process.StartInfo.UseShellExecute = true; // Set to false if you want to redirect the output

        // Run the process
        process.Start();
    }
#endif
}