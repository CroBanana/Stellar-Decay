using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class LoadNextScene : MonoBehaviour
{
    public  VideoPlayer video;
    public GameObject skipMessage;
    bool startedLoadingNextScene;
    float skipTimer = 2f;
    bool canBeSkipped;

    // Start is called before the first frame update
    void Start()
    {
        skipMessage.SetActive(false);
        Invoke("SkipCheck",2f);
    }

    void SkipCheck(){
        if(PlayerPrefs.GetInt("watchedBefore") ==1){
            skipMessage.SetActive(true);
            canBeSkipped = true;
        }
        else{
            skipMessage.SetActive(false);
            canBeSkipped = false;
        }
    }

    private void Update() {
        if(startedLoadingNextScene)
            return;

        skipTimer-=Time.deltaTime;

        if(video.isPlaying == false && skipTimer<0){
            Debug.Log("Video isnt playing anything");
            PlayerPrefs.SetInt("watchedBefore", 1);
            SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
            startedLoadingNextScene = true;
        }
        
        if(canBeSkipped && skipTimer<0){
            if(Input.anyKeyDown){
                SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
                startedLoadingNextScene = true;
            }
        }


    }
}
