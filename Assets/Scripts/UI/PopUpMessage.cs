using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PopUpMessage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Button exitPopUp;
    private void Start() {
        gameObject.SetActive(false);

        exitPopUp.onClick.AddListener(ExitPopUp);
    }

    public void MakePopUpMessage(string message, float stopTime){
        text.text = message;
        gameObject.SetActive(true);
        CancelInvoke();
        Invoke("DeactivateObject",stopTime);
    }

    private void DeactivateObject(){
        gameObject.SetActive(false);
    }

    private void ExitPopUp(){
        gameObject.SetActive(false);
    }
}
