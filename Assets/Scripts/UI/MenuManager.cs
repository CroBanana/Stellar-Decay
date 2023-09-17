using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

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

    [SerializeField]
    private GameObject main,
                                        createLobby,
                                        listLobbies,
                                        privateLobby,
                                        gameLobby,
                                        ingamePlayerUI,
                                        optionsMenu,
                                        controls,
                                        about,
                                        playerName,
                                        quitGame;
    [SerializeField] private List<GameObject> allMenus;
    [SerializeField] private Button backButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button listLobbiesButton;
    [SerializeField] private Button joinPrivateButton;
    [SerializeField] private Button startSingleplayer;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button aboutPageButton;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button playerNameConfirmButton;
    [SerializeField] private Button quitGameButton;

    [SerializeField] private bool gameStarted;

    private void Start()
    {
        allMenus.Add(main);
        allMenus.Add(createLobby);
        allMenus.Add(listLobbies);
        allMenus.Add(privateLobby);
        allMenus.Add(gameLobby);
        allMenus.Add(ingamePlayerUI);
        allMenus.Add(optionsMenu);
        allMenus.Add(controls);
        allMenus.Add(about);
        allMenus.Add(playerName);
        allMenus.Add(quitGame);
        ActivateMenu(playerName, false);
        if(PlayerPrefs.GetString("name") != null){
            playerNameInput.text = PlayerPrefs.GetString("name");
        }

        backButton.onClick.AddListener(() =>
        {
            ActivateMenu(main, false);
        });
        createLobbyButton.onClick.AddListener(() =>
        {
            ActivateMenu(createLobby, true);
        });
        listLobbiesButton.onClick.AddListener(() =>
        {
            ActivateMenu(listLobbies, true);
        });
        joinPrivateButton.onClick.AddListener(() =>
        {
            ActivateMenu(privateLobby, true);
        });
        startSingleplayer.onClick.AddListener(() =>
        {
            StartSingleplayer();
        });
        optionsButton.onClick.AddListener(() =>
        {
            ActivateMenu(optionsMenu, true);
        });
        controlsButton.onClick.AddListener(() =>
        {
            ActivateMenu(controls, true);
        });
        aboutPageButton.onClick.AddListener(() =>
        {
            ActivateMenu(about, true);
        });

        playerNameConfirmButton.onClick.AddListener(() =>
        {
            SetNewName();
            ActivateMenu(main, false);
        });

        quitGameButton.onClick.AddListener(() =>
        {
            ActivateMenu(quitGame, false);
        });


    }

    private void ActivateMenu(GameObject menuToActivate,bool addBackButton)
    {
        Debug.Log("Test");
        foreach (GameObject menu in allMenus)
        {
            menu.SetActive(false);
        }
        menuToActivate.SetActive(true);

        if(addBackButton)
            backButton.gameObject.SetActive(true);
        else
            backButton.gameObject.SetActive(false);

        if(menuToActivate == optionsMenu){
            Options.Instance.ActivateTempListener();
        }
        else{
            Options.Instance.DeactivateTempListener();
        }
    }

    public void ActivateLobby()
    {
        ActivateMenu(gameLobby, false);
    }

    public void LeaveLobby()
    {
        ActivateMenu(main, false);
    }

    public void ActivatePlayerUI(){
        ActivateMenu(ingamePlayerUI, false);
    }



    public async void StartSingleplayer(){
        if(gameStarted)
            return;
        gameStarted = true;
        await TestRelay.Instance.CreateRelay();
    }

    public void SetNewName(){
        string newName = playerNameInput.text;
        if(newName==""){
            PlayerPrefs.SetString("name", GetComponentInChildren<LobbySystem>().SetRandomName());
        }
        else
            PlayerPrefs.SetString("name", playerNameInput.text);

        Debug.Log("New name is "+PlayerPrefs.GetString("name"));
    }

    public void QuitGame(){
        Application.Quit();
    }

}
