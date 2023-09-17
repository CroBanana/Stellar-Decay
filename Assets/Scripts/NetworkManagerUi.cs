using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

namespace Game
{
    public class NetworkManagerUi : MonoBehaviour
    {

        [SerializeField] private Button serverButton, hostButton, clientButton;
        // Start is called before the first frame update

        private void Start()
        {
            
            serverButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartServer();
            });
            hostButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartHost();
            });
            clientButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartClient();
            });
            
        }

    }
}
