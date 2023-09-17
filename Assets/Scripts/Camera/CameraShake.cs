using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance {get;private set;}

    public CinemachineVirtualCamera cinemachineVirtualCamera;
    public CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;
    float shakeTimer;

    private void Awake() {
        Instance = this;
        cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
        cinemachineBasicMultiChannelPerlin = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void Shake(float intensity, float stopTime){
        try
        {
            if(cinemachineBasicMultiChannelPerlin == null)
                cinemachineBasicMultiChannelPerlin = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
        catch
        {
            Debug.LogError("Perlin noise is empty");
        }
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;

        CancelInvoke();
        Invoke("ShakeOver",stopTime);
    }
    
    private void ShakeOver(){
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0;
    }

    /*
    public IEnumerator Shake(float duration, float magnitude){
        Vector3 originalPos = transform.localPosition;
        float elapsedTime = 0;

        while (elapsedTime <duration){
            float x = Random.Range(-1f,1f) * magnitude;
            float y = Random.Range(-1f,1f) * magnitude;
            

            transform.localPosition = new Vector3(x,y, originalPos.z);
            elapsedTime +=Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPos;
    }
    */
}
