using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Flare : NetworkBehaviour
{
    [SerializeField] private float destroyAfterTime;

    private void Start() {
        if(IsServer){
            Invoke("DestroyAfter", destroyAfterTime);
        }
        else{
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    void DestroyAfter(){
        GetComponent<NetworkObject>().Despawn();
    }
}
