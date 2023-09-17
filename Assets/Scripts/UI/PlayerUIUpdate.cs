using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class PlayerUIUpdate : NetworkBehaviour {

    private void Start() {
        if(!IsOwner){
            this.enabled = false;
            return;
        }
        

    }


}
