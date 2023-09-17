using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Options : MonoBehaviour
{
    public static Options Instance { get; private set; }

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

    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeSliderValue;
    [SerializeField] private AudioListener audioListenerTemp;
    [SerializeField] private Button fullscreenButton;
    [SerializeField] private TMP_Dropdown resolutionSettings;
    [SerializeField] private TMP_Dropdown qualitySettings;

    [SerializeField] private bool fullscreen;
    [SerializeField] private int qualityLevel;
    [SerializeField] private Resolution[] resolutions;





    private void Start() {
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

        
        if(PlayerPrefs.HasKey("fullscreen")){
            if(PlayerPrefs.GetInt("fullscreen")==1){
                fullscreen = true;
                fullscreenButton.image.color = Color.green;
            }
            else{
                fullscreen = false;
                fullscreenButton.image.color = Color.red;
            }
        }

        if(PlayerPrefs.HasKey("masterQuality")){

            qualityLevel = PlayerPrefs.GetInt("masterQuality");
            qualitySettings.value=qualityLevel;
        }

        resolutions = Screen.resolutions;
        resolutionSettings.ClearOptions();
        List<string> _resolutions = new List<string>();

        int currentResolutionIndex =0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x "+resolutions[i].height;
            _resolutions.Add(option);
            if(resolutions[i].width  == Screen.width && resolutions[i].height == Screen.height){
                currentResolutionIndex = i;
            }
        }

        resolutionSettings.AddOptions(_resolutions);
        resolutionSettings.value = currentResolutionIndex;
        resolutionSettings.RefreshShownValue();
        
        audioListenerTemp.gameObject.SetActive(false);

        volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);
        fullscreenButton.onClick.AddListener(ChangeFullscreen);
        resolutionSettings.onValueChanged.AddListener(SetNewResolution);
        qualitySettings.onValueChanged.AddListener(SetQuality);
    }

    public void SetNewResolution(int _resolutionIndex){
        Resolution resolution = resolutions[_resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    void OnSliderValueChanged(float value){
        Debug.Log(value);
        volumeSliderValue.text = value.ToString("0.00");
        AudioListener.volume = value;
        ApplyVolume();
    }

    public void ActivateTempListener(){
        audioListenerTemp.gameObject.SetActive(true);
    }
    public void DeactivateTempListener(){
        audioListenerTemp.gameObject.SetActive(false);
    }

    public void ApplyVolume(){
        PlayerPrefs.SetFloat("masterAudioVolume", AudioListener.volume);
    }

    private void ChangeFullscreen(){
        fullscreen =!fullscreen;
        if(fullscreen ){
            fullscreenButton.image.color = Color.green;
        }
        else{
            fullscreenButton.image.color = Color.red;
        }
        PlayerPrefs.SetInt("fullscreen", (fullscreen ?1:0));
        Screen.fullScreen = fullscreen;
    }

    private void SetQuality(int qualityIndex){
        qualityLevel = qualityIndex;
        PlayerPrefs.SetInt("masterQuality", qualityLevel);
        QualitySettings.SetQualityLevel(qualityLevel);
    }





}
