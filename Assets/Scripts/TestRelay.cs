using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Http;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;



    public class TestRelay : MonoBehaviour
    {
        [SerializeField] private Button serverButton, hostButton, clientButton;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Transform canvas;
        public string serverCode;
        public bool startInThisScene;

        [Header("Singleplayer button")]
        [SerializeField] private Button singleplayer;
        // Start is called before the first frame update


        public static TestRelay Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }


        private async void Start()
        {

            serverButton.onClick.AddListener(() =>
            {
                Debug.Log("Server button not working");
                //NetworkManager.Singleton.StartServer();
            });
            hostButton.onClick.AddListener(async () =>
            {
                await CreateRelay();
            });
            clientButton.onClick.AddListener(() =>
            {
                JoinRelay(inputField.text);
            });
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Singed in " + AuthenticationService.Instance.PlayerId);
            };

            PlayerPrefs.SetString("playerID", AuthenticationService.Instance.PlayerId.ToString());
            
            //CreateRelay();
        }




        public async Task<string> CreateRelay()
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);

                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                serverCode = joinCode;
                RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton.StartHost();
                Game.PlayerSpawnerNewScene.Instance.SceneLoadEvent();
                NetworkManager.Singleton.SceneManager.LoadScene("Map",LoadSceneMode.Single);

                //canvas.gameObject.SetActive(false);
                Debug.Log(serverCode);
                return serverCode;
            }
            catch (RelayServiceException e)
            {

                Debug.Log(e);
                return "0";
            }
        }

        public async void JoinRelay(string joinCode)
        {
            try
            {
                Debug.Log("Joining relay with " + joinCode);
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                serverCode = joinCode;
                RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton.StartClient();

                Game.PlayerSpawnerNewScene.Instance.SceneLoadEvent();

                //canvas.gameObject.SetActive(false);
            }
            catch (RelayServiceException e)
            {

                Debug.Log(e);
            }

        }

    }
