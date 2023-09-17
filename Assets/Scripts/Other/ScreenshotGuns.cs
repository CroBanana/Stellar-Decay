using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ScreenshotGuns : MonoBehaviour
{
    
    public string screenshotFolder = "Gun screenshots"; // Name of the folder where screenshots will be saved
    string savePath ;
    List<GameObject> weapons = new List<GameObject>();
    private void Start()
    {
        savePath = "Assets/" + screenshotFolder+"/"+screenshotFolder;
        //Debug.Log(Application.persistentDataPath);
        foreach (Transform child in transform){
            //Debug.Log(child.name);
            weapons.Add(child.gameObject);
            child.gameObject.SetActive(false);
        }
        // Create the screenshot folder if it doesn't exist
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        StartCoroutine(TakeScreenshots());
    }

    IEnumerator TakeScreenshots(){
        foreach (GameObject weapon in weapons){
            weapon.SetActive(true);
            yield return new WaitForEndOfFrame();
            Screenshot(weapon, weapon.name);
            weapon.SetActive(false);
        }
    }

    public void Screenshot(GameObject targetObject, string screenshotName)
    {

        // Capture the screenshot
        //string screenshotPath = Path.Combine(screenshotFolder, screenshotName + ".png");
        string screenshotPath = savePath +"/"+ screenshotName+".png";

        if(File.Exists(screenshotPath)){
            File.Delete(screenshotPath);
        }

        ScreenCapture.CaptureScreenshot(screenshotPath);

        // Deactivate the target object after capturing the screenshot
        targetObject.SetActive(false);

        Debug.Log("Screenshot captured and saved at: " + screenshotPath);
    }
}
