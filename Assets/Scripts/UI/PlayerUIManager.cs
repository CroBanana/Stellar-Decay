using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager Instance { get; private set; }

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

    [Header("Menus")]
    [SerializeField] private GameObject menuUI;
    [SerializeField] private GameObject aliveUI;
    [SerializeField] private GameObject deadUI;
    [SerializeField] private GameObject endGameUi;
    [SerializeField] private GameObject controlsUI;

    [Header("Buttons, sliders and other stuff")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeSliderValue;

    [SerializeField] private GameObject quitGameUiOption;
    [SerializeField] private GameObject quitToMenuOption;

    [SerializeField] private  Image weaponIcon;
    [SerializeField] private TextMeshProUGUI ammoRemainingText;
    [SerializeField] private TextMeshProUGUI currentAmmoMagazineText;
    [SerializeField] private Image hpNotification;
    [SerializeField] private TextMeshProUGUI grenadesRemaining;
    [SerializeField] private TextMeshProUGUI flaresRemaining;
    [SerializeField] private Image grenadeImage;
    [SerializeField] private Image flareImage;

    [SerializeField] private TextMeshProUGUI respawnTimer;

    [SerializeField] private Button restartButton;
    [SerializeField] public TextMeshProUGUI notHostText;


    private void Start() {
        menuUI.SetActive(false);
        deadUI.SetActive(false);
        endGameUi.SetActive(false);
        quitGameUiOption.SetActive(false);
        quitToMenuOption.SetActive(false);

        if(PlayerPrefs.HasKey("masterAudioVolume")){
            volumeSlider.value = PlayerPrefs.GetFloat("masterAudioVolume");
            volumeSliderValue.text = volumeSlider.value.ToString();
            AudioListener.volume = volumeSlider.value;
        }
        else{
            PlayerPrefs.SetFloat("masterAudioVolume", 1);
            volumeSlider.value = PlayerPrefs.GetFloat("masterAudioVolume");
            AudioListener.volume = volumeSlider.value;
        }

        volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void OnSliderValueChanged(float value){
        Debug.Log(value);
        volumeSliderValue.text = value.ToString("0.00");
        AudioListener.volume = value;
        Options.Instance.ApplyVolume();
    }
    public void UpdateWeaponIcon(Sprite newWeaponIcon){
        weaponIcon.sprite = newWeaponIcon;
    }

    public void UpdateAmmoRemainingText(int ammoRemaining){
        ammoRemainingText.text = ammoRemaining.ToString();
    }

    public void UpdateAmmoMagazineText(int currentMagazine, int maxMagazine){
        currentAmmoMagazineText.text = currentMagazine+ " / "+ maxMagazine;
    }

    public void UpdateHpImage(int currentHP, int maxHP){
        Color imageColor = hpNotification.color;
        imageColor.a = (float) (maxHP- currentHP)/maxHP;
        //Debug.Log("Alpha is "+ imageColor.a);
        hpNotification.color = imageColor;
    }

    public void QuitGame(){
        Game.GameManager.Instance.ChangeNotHostTextClientRpc();
        Application.Quit();
    }

    public void ContinueGame(){
        menuUI.SetActive(false);
        quitGameUiOption.SetActive(false);
        quitToMenuOption.SetActive(false);
    }
    public void ActivateIngameMenu(){
        menuUI.SetActive(true);
    }

    public void UpdateThrowableText(int grenade, int flare){
        grenadesRemaining.text= grenade.ToString();
        flaresRemaining.text = flare.ToString();
    }

    public void ActivateThrowableImages(string tag){
        if(tag == "Grenade"){
            Color alphaChange = grenadeImage.color;

            //deactifate flare UI elements
            flaresRemaining.alpha =0;
            alphaChange.a =0;
            flareImage.color = alphaChange;

            //activate grenade ui elements
            grenadesRemaining.alpha =1;
            alphaChange.a =1;
            grenadeImage.color = alphaChange;
        }

        if(tag == "Flare"){
            Color alphaChange = grenadeImage.color;

            //deactifate grenades UI elements
            grenadesRemaining.alpha =0;
            alphaChange.a =0;
            grenadeImage.color = alphaChange;

            //activate flare ui elements
            flaresRemaining.alpha =1;
            alphaChange.a =1;
            flareImage.color = alphaChange;
        }
    }

    public void PlayerDeadUI(){
        aliveUI.SetActive(false);
        deadUI.SetActive(true);
    }

    public void PlayerAliveUI(){
        aliveUI.SetActive(true);
        deadUI.SetActive(false);
    }

    public void QuitButtonOption(){
        menuUI.SetActive(false);
        quitGameUiOption.SetActive(true);
        quitToMenuOption.SetActive(false);
    }

    public void QuitMenuButtonOption(){
        menuUI.SetActive(false);
        quitGameUiOption.SetActive(false);
        quitToMenuOption.SetActive(true);
    }

    public void QuitToMenu(){
        Game.GameManager.Instance.ChangeNotHostTextClientRpc();
        NetworkManager.Singleton.Shutdown();
        try
        {
            //ako je server ode na main menu
            NetworkManager.Singleton.SceneManager.LoadScene("Main menu",LoadSceneMode.Single);
        }
        catch
        {
            SceneManager.LoadScene("Main menu",LoadSceneMode.Single);
        }
        //StartCoroutine(HostQuit());
        //SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
    }

    IEnumerator HostQuit(){
        yield return new WaitForSeconds(0.15f);
        NetworkManager.Singleton.Shutdown();
        try
        {
            //ako je server ode na main menu
            NetworkManager.Singleton.SceneManager.LoadScene("Main menu",LoadSceneMode.Single);
        }
        catch
        {
            SceneManager.LoadScene("Main menu",LoadSceneMode.Single);
        }
    }

    public void GameOver(){
        aliveUI.SetActive(false);
        deadUI.SetActive(false);
        menuUI.SetActive(false);
        endGameUi.SetActive(true);

        if(Game.GameManager.Instance.CheckIfHost()){
            Debug.Log("He is host");
            restartButton.gameObject.SetActive(true);
        }else{
            notHostText.gameObject.SetActive(true);
        }
    }


    public void UpdateRespawnTime(int time){
        respawnTimer.text = time.ToString();
    }

    public void ChangeControlVisability(){
        controlsUI.SetActive(! controlsUI.activeSelf);
    }

    public void RestartGame(){
        NetworkManager.Singleton.SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    public void ThisIsHost(){
        restartButton.gameObject.SetActive(true);
        notHostText.gameObject.SetActive(false);
    }
    public void ThisIsClient(){
        restartButton.gameObject.SetActive(false);
        notHostText.gameObject.SetActive(true);
    }
}
