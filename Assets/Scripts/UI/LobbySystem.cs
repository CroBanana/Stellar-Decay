using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LobbySystem : MonoBehaviour
{
    [SerializeField] private List<string> names;
    [SerializeField] private Button listLobbies;
    [SerializeField] private Button joinLobby;
    [SerializeField] private Button quickJoin;
    [SerializeField] private TMP_InputField lobbyCode;
    [SerializeField] private PopUpMessage popUpMessage;
    [SerializeField] private Button privateLobby;
    [SerializeField] private TMP_InputField playerName;
    [SerializeField] private Button leaveLobby;


    [Header("List for lobies"), Space(20)]
    [SerializeField] private GameObject lobbyListContent;
    [SerializeField] private GameObject lobbyListPrefab;
    [SerializeField] private List<GameObject> allLobbiesListed;


    [Header("Create lobby options"), Space(20)]
    [SerializeField] private Button createLobby;
    private bool privateLobbyBool = false;
    [SerializeField] private Slider maxPlayersSlider;
    [SerializeField] private TMP_InputField lobbyNameField;


    [Header("Joined Lobby "), Space(20)]
    [SerializeField] private GameObject lobbyPlayerContent;
    [SerializeField] private GameObject lobbyPlayerPrefab;
    [SerializeField] private List<GameObject> allPlayersInLobby;

    [SerializeField] private TextMeshProUGUI lobbyNameCodeText;

    [SerializeField] private bool gameStarted = false;


    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTime = 20f;
    [SerializeField] private float lobbyUpdateTime = 2f;


    [SerializeField] private Button startGameButton;

    private async void Start()
    {

        createLobby.onClick.AddListener(CreateLobby);

        listLobbies.onClick.AddListener(() =>
        {
            ListLobbies();
        });
        joinLobby.onClick.AddListener(() =>
        {
            JoinLobbyByCode(lobbyCode.text);
        });
        privateLobby.onClick.AddListener(() =>
        {
            PrivateLobby();
        });
        quickJoin.onClick.AddListener(() =>
        {
            QuickJoinLobby();
        });
        leaveLobby.onClick.AddListener(() =>
        {
            LeaveLobby();
        });

        startGameButton.onClick.AddListener(() =>
        {
            StartGame();
        });


        await UnityServices.InitializeAsync();

        await AuthenticationService.Instance.SignInAnonymouslyAsync();


    }


    public string SetRandomName(){
        return names[Random.Range(0, names.Count)];
    }

    private async void CreateLobby()
    {
        try
        {

            string lobbyName;
            if (lobbyNameField.text != "")
                lobbyName = lobbyNameField.text;
            else
                lobbyName = "My lobby";

            int maxPlayers = (int)maxPlayersSlider.value;
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = privateLobbyBool,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>{
                    {"StartGameCode", new DataObject(DataObject.VisibilityOptions.Member, "0")}
                }
            };

            try
            {
                LeaveLobby();
            }
            catch (System.Exception)
            {
                Debug.Log("Either there is no lobby that has been jooined or this is the first create");
                throw;
            }

            hostLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, lobbyOptions);
            joinedLobby = hostLobby;

            CreatePlayerSlots(joinedLobby.MaxPlayers);

            Debug.Log("Created lobby = " + hostLobby.Name + " " + hostLobby.MaxPlayers + " " + hostLobby.Id + " " + hostLobby.LobbyCode);
            PrintPlayers(hostLobby);
            CancelInvoke();
            InvokeRepeating("HeartBeat", heartbeatTime, heartbeatTime);
            InvokeRepeating("UpdateLobby", 0.2f, lobbyUpdateTime);

            LobbyName();
            MenuManager.Instance.ActivateLobby();
            Debug.Log("Created lobby");
        }
        catch (LobbyServiceException e)
        {
            popUpMessage.MakePopUpMessage("Lobby creation failed.", 2);
            Debug.Log(e);
        }
    }

    private async void HeartBeat()
    {
        if (hostLobby != null)
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        }

    }

    private void LobbyName()
    {
        string text;
        text = joinedLobby.Name;
        if (joinedLobby.LobbyCode != "")
        {
            text += "  JoinCode: " + joinedLobby.LobbyCode;
        }
        lobbyNameCodeText.text = text;
    }
    private async void UpdateLobby()
    {
        try
        {
            //Debug.Log("Update");
            if (joinedLobby != null)
            {

                //Debug.Log("Joined lobby is not null " + joinedLobby.Players.Count);
                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);

                int i = 0;
                foreach (Player playerInLobby in joinedLobby.Players)
                {
                    allPlayersInLobby[i].GetComponent<PlayerInfo>().SetNameText(playerInLobby.Data["PlayerName"].Value);
                    allPlayersInLobby[i].GetComponent<PlayerInfo>().SetPlayerID(joinedLobby.Players[i].Id);
                    allPlayersInLobby[i].gameObject.SetActive(true);
                    i++;
                }
                for (; i < allPlayersInLobby.Count; i++)
                {
                    allPlayersInLobby[i].gameObject.SetActive(false);
                }

                if (joinedLobby.HostId == AuthenticationService.Instance.PlayerId)
                {
                    startGameButton.gameObject.SetActive(true);
                }
                else
                {
                    startGameButton.gameObject.SetActive(false);
                }

                if (joinedLobby.Data["StartGameCode"].Value != "0")
                {
                    if (joinedLobby.HostId != AuthenticationService.Instance.PlayerId)
                    {
                        TestRelay.Instance.JoinRelay(joinedLobby.Data["StartGameCode"].Value);
                    }
                    CancelInvoke();
                }

            }
        }
        catch (System.Exception)
        {
            popUpMessage.MakePopUpMessage("You where kicked from the lobby, or something else happend.", 3);
            CancelInvoke();
            MenuManager.Instance.LeaveLobby();
            throw;
        }

    }

    private void CreatePlayerSlots(int numberOfSlots)
    {
        if (allPlayersInLobby.Count > 0)
        {
            for (int i = allPlayersInLobby.Count - 1; i >= 0; i--)
            {
                Destroy(allPlayersInLobby[i]);
            }
            allPlayersInLobby.Clear();
        }

        for (int i = 0; i < numberOfSlots; i++)
        {

            Debug.Log("Slots created " + i);
            GameObject newSlot = Instantiate(lobbyPlayerPrefab, lobbyPlayerContent.transform.position, Quaternion.identity, lobbyPlayerContent.transform);
            allPlayersInLobby.Add(newSlot);


            if (i == 0)
            {
                newSlot.GetComponent<PlayerInfo>().IsKickButtonVisible(false);
            }
            else if (joinedLobby.HostId == AuthenticationService.Instance.PlayerId)
            {
                newSlot.GetComponent<PlayerInfo>().IsKickButtonVisible(true);
            }
        }
    }

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 10,
                Filters = new List<QueryFilter>{
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0",QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>{
                    new QueryOrder (false, QueryOrder.FieldOptions.Name)
                }
            };
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            if (queryResponse.Results.Count == 0)
            {
                popUpMessage.MakePopUpMessage("No lobbies found.", 2);
            }

            //brise listu ako vec postoji
            if (allLobbiesListed.Count > 0)
            {
                for (int i = allLobbiesListed.Count - 1; i >= 0; i--)
                {
                    Destroy(allLobbiesListed[i]);
                }
                allLobbiesListed.Clear();
            }

            foreach (Lobby lobby in queryResponse.Results)
            {
                LobbyInfo newLobbyInstance = Instantiate(lobbyListPrefab, lobbyListContent.transform.position, Quaternion.identity, lobbyListContent.transform).GetComponent<LobbyInfo>();
                newLobbyInstance.SetGameName(lobby.Name);
                newLobbyInstance.SetLobbyID(lobby.Id);
                Debug.Log(lobby.Id);
                newLobbyInstance.SetPlayerCount(lobby.Players.Count, lobby.MaxPlayers);
                allLobbiesListed.Add(newLobbyInstance.gameObject);
            }
        }
        catch (LobbyServiceException e)
        {
            popUpMessage.MakePopUpMessage("Cant list lobbies. Check internet connection.", 3f);
            Debug.Log(e);
        }
    }

    private async void JoinLobbyByCode(string lobbyCode)
    {
        if (lobbyCode == "")
        {
            popUpMessage.MakePopUpMessage("Code to join cannot be empty", 2f);
            return;
        }
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };
            joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            CreatePlayerSlots(joinedLobby.MaxPlayers);
            CancelInvoke();
            InvokeRepeating("UpdateLobby", 0.2f, lobbyUpdateTime);
            PrintPlayers(joinedLobby);
            LobbyName();
            MenuManager.Instance.ActivateLobby();
        }
        catch (LobbyServiceException e)
        {
            popUpMessage.MakePopUpMessage("Incorrect lobby code.", 2f);
            Debug.Log(e);
        }
    }

    private async void JoinLobbyById(string lobbyId)
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };
            joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId, joinLobbyByIdOptions);
            CreatePlayerSlots(joinedLobby.MaxPlayers);
            CancelInvoke();
            InvokeRepeating("UpdateLobby", 0.2f, lobbyUpdateTime);

            PrintPlayers(joinedLobby);
            LobbyName();
            MenuManager.Instance.ActivateLobby();
        }
        catch (LobbyServiceException e)
        {
            popUpMessage.MakePopUpMessage("Cannot join. Either the lobby doesnt exist anymore or its full.", 5);
            Debug.Log(e);
        }
    }

    public void JoinLobbyFromListById(string Id)
    {
        JoinLobbyById(Id);
    }

    void PrivateLobby()
    {
        Debug.Log("Test");
        if (privateLobbyBool)
        {
            privateLobbyBool = false;
            privateLobby.transform.GetComponentInChildren<TextMeshProUGUI>().text = "Public";
        }
        else
        {
            privateLobbyBool = true;
            privateLobby.transform.GetComponentInChildren<TextMeshProUGUI>().text = "Private";
        }
    }

    public async void QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
            {
                Player = GetPlayer()
            };
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            CreatePlayerSlots(joinedLobby.MaxPlayers);
            Debug.Log("Lobby joined succesfully");

            CancelInvoke();
            InvokeRepeating("UpdateLobby", 0.2f, lobbyUpdateTime);
            PrintPlayers(joinedLobby);
            LobbyName();
            MenuManager.Instance.ActivateLobby();
        }
        catch (LobbyServiceException e)
        {
            popUpMessage.MakePopUpMessage("Failed to join lobby, there are no lobbies available", 3);
            Debug.Log(e);
        }

    }


    private Player GetPlayer()
    {
        
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>{
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerPrefs.GetString("name"))}
                    }
        };
    }

    private void PrintPlayers()
    {
        PrintPlayers(joinedLobby);
    }

    private void PrintPlayers(Lobby lobby)
    {
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }

    public async void LeaveLobby()
    {

        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            MenuManager.Instance.LeaveLobby();
            CancelInvoke();
            Debug.Log("Lobby left successfully");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Leaving lobby unsuccessfull");
            Debug.Log(e);
        }
    }

    public async void KickPlayer(string id)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, id);
            Debug.Log("Lobby left successfully");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Leaving lobby unsuccessfull");
            Debug.Log(e);
        }
    }


    private async void StartGame()
    {
        if (joinedLobby.HostId == AuthenticationService.Instance.PlayerId)
        {
            try
            {
                if(gameStarted)
                    return;
                //NetworkManager.Singleton.SceneManager.LoadScene("SampleScene",LoadSceneMode.Single);
                gameStarted = true;
                string relayCode = await TestRelay.Instance.CreateRelay();

                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>{
                        {"StartGameCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode)}
                    }
                });

                joinedLobby = lobby;
            }
            catch (LobbyServiceException e)
            {
                popUpMessage.MakePopUpMessage("Cant start game.", 2);
                Debug.Log(e);
            }
        }
    }



}
