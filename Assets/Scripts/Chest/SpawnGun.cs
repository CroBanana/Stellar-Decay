using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Game
{
    public class SpawnGun : NetworkBehaviour, IInteractable
    {
        private float angleOpend = -90;
        private float angleClosed =0;
        private SphereCollider triggerCollider;
        public NetworkVariable<float> sphereRadius =  new NetworkVariable<float>(1f,
                                                                                NetworkVariableReadPermission.Everyone,
                                                                                NetworkVariableWritePermission.Server);
        public Transform topPart;
        public List<GameObject> gunsToSpawn;

        public override void OnNetworkSpawn()
        {
            sphereRadius.OnValueChanged += (float oldValue, float newValue)=>{
                triggerCollider.radius = newValue;
            };
        }

        private void Start() {
            triggerCollider = GetComponent<SphereCollider>();
            triggerCollider.radius = sphereRadius.Value;
        }


        public void Interact(Transform player){
            OpenCrateServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void OpenCrateServerRpc()
        {
            if(!IsServer)
                return;

            sphereRadius.Value = 0.1f;
            //Debug.Log("Opening Crate");
            StartCoroutine(Rotate(1f));

        }


        IEnumerator Rotate(float inTime)
        {
            var fromAngle = topPart.localRotation;
            var toAngle = Quaternion.Euler(new Vector3(angleOpend, 0,0));
            for (var t = 0f; t < 1; t += Time.deltaTime / inTime)
            {
                topPart.localRotation = Quaternion.Lerp(fromAngle, toAngle, t);
                yield return null;
            }
            NewGunSpawned();
        }

        public void NewGunSpawned()
        {
            if(!IsServer)
                return;

            GameObject newGun = gunsToSpawn[Random.Range(0,gunsToSpawn.Count)];
            newGun = Instantiate(newGun,transform.position,transform.rotation);
            newGun.GetComponent<NetworkObject>().Spawn();
        }


        private void OnTriggerEnter(Collider other)
        {
            //display UI for possible interaction
            if(!other.CompareTag("Player")){
                return;
            }
            other.GetComponent<IInteractable>().Interact(transform);
            //Debug.Log("Can Interact");
        }
        private void OnTriggerExit(Collider other)
        {
            //remove UI display for possible interaction
            if(!other.CompareTag("Player")){
                return;
            }
            other.GetComponent<IInteractable>().Interact(transform);
            transform.GetComponent<IText>().HideText();
            //Debug.Log("Cant Interact anymore");
        }


    }
}