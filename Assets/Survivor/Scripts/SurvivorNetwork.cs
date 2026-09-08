using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Collections;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace BonkSurvivor
{
    // Bootstraps Unity Gaming Services + Relay and starts/joins a Netcode session.
    // The host keeps playing through SurvivorGame exactly as in single-player; a joining
    // client gets a RemotePlayerNet body instead. Stage 1 of the multiplayer plan in AGENTS.md.
    public sealed class SurvivorNetwork : MonoBehaviour
    {
        public static SurvivorNetwork Instance { get; private set; }

        public enum State { Offline, SigningIn, Hosting, Joining, Connected, Error }
        public State CurrentState { get; private set; } = State.Offline;
        public string JoinCode { get; private set; } = "";
        public string StatusMessage { get; private set; } = "";
        public int ConnectedFriendCount => networkManager && networkManager.IsServer ? Mathf.Max(0, networkManager.ConnectedClientsIds.Count - 1) : 0;

        [SerializeField] GameObject remotePlayerPrefab;

        NetworkManager networkManager;
        UnityTransport transport;
        bool servicesReady;

        void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            networkManager = GetComponent<NetworkManager>();
            transport = GetComponent<UnityTransport>();
            networkManager.OnClientConnectedCallback += OnClientConnected;
            networkManager.OnClientDisconnectCallback += OnClientDisconnected;
            networkManager.OnClientStarted += RegisterMapMessages;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

        void OnDestroy()
        {
            if(networkManager)
            {
                networkManager.OnClientConnectedCallback -= OnClientConnected;
                networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
                networkManager.OnClientStarted -= RegisterMapMessages;
            }
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
        }

        // Shut the network session down in an orderly way (all objects still alive) before Unity's
        // own teardown starts destroying scene objects in an unspecified order - otherwise NetworkObjects
        // destroyed after NetworkManager hit NREs against its already-cleared internal state.
        void OnApplicationQuit() => Disconnect();

#if UNITY_EDITOR
        void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode) Disconnect();
        }
#endif

        // Both networked prefabs (enemy + RemotePlayerNet) are already registered via
        // Unity's auto-generated Assets/DefaultNetworkPrefabs.asset - no manual AddNetworkPrefab needed.

        void OnClientConnected(ulong clientId)
        {
            if (!networkManager.IsServer || clientId == NetworkManager.ServerClientId || !remotePlayerPrefab) return;
            var game=FindFirstObjectByType<SurvivorGame>();
            var instance = Instantiate(remotePlayerPrefab, game ? game.MapSpawn : Vector3.zero, Quaternion.identity);
            var rp = instance.GetComponent<RemotePlayerNet>();
            if (game && rp) rp.FriendState = game.CreateFriendCombatant(instance.transform);
            instance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            SendMap(clientId);
            StatusMessage = "Kaveri liittyi peliin!";
        }

        const string MapMessage = "Survivor.Map.v1";
        void RegisterMapMessages() => networkManager.CustomMessagingManager.RegisterNamedMessageHandler(MapMessage, ReceiveMap);

        void ReceiveMap(ulong sender, FastBufferReader reader)
        {
            if(networkManager.IsServer || sender!=NetworkManager.ServerClientId) return;
            reader.ReadValueSafe(out int version); reader.ReadValueSafe(out int seed);
            reader.ReadValueSafe(out int area); reader.ReadValueSafe(out int runSeed);
            if(version!=ProceduralMapLayout.GeneratorVersion) { StatusMessage="Karttageneraattorin versiot eroavat. Päivitä peli."; Disconnect(); return; }
            FindFirstObjectByType<SurvivorGame>()?.ApplyNetworkMap(seed, area, runSeed);
        }

        void SendMap(ulong clientId)
        {
            var game=FindFirstObjectByType<SurvivorGame>(); if(!game) return;
            using(var writer=new FastBufferWriter(16,Allocator.Temp))
            {
                writer.WriteValueSafe(ProceduralMapLayout.GeneratorVersion); writer.WriteValueSafe(game.CurrentMapSeed);
                writer.WriteValueSafe(Mathf.Max(1,game.Area)); writer.WriteValueSafe(game.RunSeed);
                networkManager.CustomMessagingManager.SendNamedMessage(MapMessage,clientId,writer,NetworkDelivery.ReliableSequenced);
            }
        }

        public void BroadcastMap()
        {
            if(!networkManager || !networkManager.IsServer || !networkManager.IsListening) return;
            foreach(var id in networkManager.ConnectedClientsIds) if(id!=NetworkManager.ServerClientId) SendMap(id);
        }

        void OnClientDisconnected(ulong clientId)
        {
            if (!networkManager.IsServer && clientId == networkManager.LocalClientId)
            {
                CurrentState = State.Offline; StatusMessage = "Yhteys isäntään katkesi.";
            }
        }

        async System.Threading.Tasks.Task<bool> EnsureServicesAsync()
        {
            if (servicesReady) return true;
            try
            {
                if (UnityServices.State != ServicesInitializationState.Initialized)
                    await UnityServices.InitializeAsync();
                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                servicesReady = true;
                return true;
            }
            catch (Exception e)
            {
                CurrentState = State.Error; StatusMessage = "Kirjautuminen epäonnistui: " + e.Message;
                return false;
            }
        }

        static RelayServerEndpoint DtlsEndpoint(List<RelayServerEndpoint> endpoints)
        {
            foreach (var e in endpoints) if (e.ConnectionType == "dtls") return e;
            return endpoints.Count > 0 ? endpoints[0] : null;
        }

        public async void HostGame()
        {
            if (CurrentState == State.SigningIn || CurrentState == State.Hosting || CurrentState == State.Connected) return;
            CurrentState = State.SigningIn; StatusMessage = "Kirjaudutaan...";
            if (!await EnsureServicesAsync()) return;
            CurrentState = State.Hosting; StatusMessage = "Luodaan Relay-yhteyttä...";
            try
            {
                var allocation = await RelayService.Instance.CreateAllocationAsync(1);
                JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                var endpoint = DtlsEndpoint(allocation.ServerEndpoints);
                if (endpoint == null) { CurrentState = State.Error; StatusMessage = "Relay ei tarjonnut yhteyspistettä."; return; }
                transport.SetHostRelayData(endpoint.Host, (ushort)endpoint.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, true);
                if (!networkManager.StartHost())
                {
                    CurrentState = State.Error; StatusMessage = "Isännöinti epäonnistui.";
                    return;
                }
                CurrentState = State.Connected; StatusMessage = "Isännöit peliä - anna koodi kaverille.";
            }
            catch (Exception e)
            {
                CurrentState = State.Error; StatusMessage = "Relay-virhe: " + e.Message;
            }
        }

        public async void JoinGame(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) { StatusMessage = "Anna liittymiskoodi."; return; }
            if (CurrentState == State.SigningIn || CurrentState == State.Joining || CurrentState == State.Connected) return;
            CurrentState = State.SigningIn; StatusMessage = "Kirjaudutaan...";
            if (!await EnsureServicesAsync()) return;
            CurrentState = State.Joining; StatusMessage = "Liitytään...";
            try
            {
                var allocation = await RelayService.Instance.JoinAllocationAsync(code.Trim());
                var endpoint = DtlsEndpoint(allocation.ServerEndpoints);
                if (endpoint == null) { CurrentState = State.Error; StatusMessage = "Relay ei tarjonnut yhteyspistettä."; return; }
                transport.SetClientRelayData(endpoint.Host, (ushort)endpoint.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData, true);
                if (!networkManager.StartClient())
                {
                    CurrentState = State.Error; StatusMessage = "Liittyminen epäonnistui.";
                    return;
                }
                CurrentState = State.Connected; StatusMessage = "Yhdistetty isäntään.";
            }
            catch (Exception e)
            {
                CurrentState = State.Error; StatusMessage = "Liittyminen epäonnistui: " + e.Message;
            }
        }

        public void Disconnect()
        {
            if (networkManager && networkManager.IsListening) networkManager.Shutdown();
            CurrentState = State.Offline; StatusMessage = ""; JoinCode = "";
        }
    }
}
