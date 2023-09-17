using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game{
    public class DestructableZombie : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other) {
            if(other.CompareTag("Zombie")){
                other.GetComponent<CoreCharacter>().ChangeTarget(transform);
                Debug.Log("Triggerd");
            }
        }
    }
}