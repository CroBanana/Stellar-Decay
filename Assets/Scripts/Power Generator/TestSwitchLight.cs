using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSwitchLight : MonoBehaviour
{
    public bool textureLights1, textureLights2;
    private void Update() {
        if(Input.GetKeyDown(KeyCode.Space)){
            GetComponentInParent<IActivatable>().Activate();
        }
        if(Input.GetKeyDown(KeyCode.LeftAlt)){
            GetComponentInParent<IActivatable>().Deactivate();
        }
        if(textureLights1){
            GetComponentInParent<IActivatable>().Activate();
            textureLights1 = false;
        }
        if(textureLights2){
            GetComponentInParent<IActivatable>().Deactivate();
            textureLights2 = false;
        }
    }
}
