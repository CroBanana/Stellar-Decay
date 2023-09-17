using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameName;
    [SerializeField] private string playerId;
    [SerializeField] private Button kickPlayer;
    // Start is called before the first frame update


    private void Start() {
        kickPlayer.onClick.AddListener(KickPlayer);
    }

    public void SetNameText(string text){
        gameName.text = text;
    }

    public void SetPlayerID(string id){
        playerId = id;
    }

    private void KickPlayer(){
        Debug.Log("Kick Player");
        GetComponentInParent<LobbySystem>().KickPlayer(playerId);
    }

    public void IsKickButtonVisible(bool visible){
        kickPlayer.gameObject.SetActive(visible);
    }
}
