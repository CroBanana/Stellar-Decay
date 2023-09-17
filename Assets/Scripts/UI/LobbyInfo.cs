using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameName;
    [SerializeField] private TextMeshProUGUI playerCount;
    [SerializeField] private Button joinButton;
    [SerializeField] private string lobbyID;

    // Start is called before the first frame update
    void Start()
    {
        joinButton.onClick.AddListener(JoinLobby);
    }

    private void JoinLobby(){
        GetComponentInParent<LobbySystem>().JoinLobbyFromListById(lobbyID);
    }

    public void SetGameName(string name){
        gameName.text = name;
    }

    public void SetPlayerCount(int currentPlayers, int maxPlayers){
        playerCount.text = currentPlayers+" / "+ maxPlayers;
    }

    public void SetLobbyID(string _lobbyID){
        lobbyID = _lobbyID;
    }
    
}
