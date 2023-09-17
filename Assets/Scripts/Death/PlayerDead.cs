using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Animations.Rigging;
using System.Linq;
namespace Game
{
    public class PlayerDead : NetworkBehaviour, IDied
    {
        public GameObject playerPrefab;
        public List<Rigidbody> ragdollRigidbodies;
        public int respawnTimerConstant = 10;
        public bool isDead;
        private void Start()
        {

            Rigidbody[] ragdoll = GetComponentsInChildren<Rigidbody>();
            ragdollRigidbodies = ragdoll.ToList();

            DisableRagdoll();
            if(IsOwner)
                ActivateUI();

        }

        void ActivateUI(){
            PlayerUIManager.Instance.PlayerAliveUI();
        }

        void DisableRagdoll()
        {
            foreach (Rigidbody rig in ragdollRigidbodies)
            {
                rig.isKinematic = true;
            }
        }


        public void Died()
        {
            DisableComponents();
            ActivateRagdoll();
            AcivateRagdollClientRpc();

            if (!IsOwner )
                return;
            Debug.LogWarning("Indeed it is the owner");
            StopAllCoroutines();
            StartCoroutine(RespawnAfter(respawnTimerConstant));
        }

        void DisableComponents()
        {
            GameManager.Instance.allPlayers.Remove(gameObject);
            GetComponent<Animator>().enabled = false;
            GetComponent<CharacterController>().enabled = false;
            GetComponent<PlayerGuns>().enabled = false;
            GetComponent<Player>().enabled = false;
            GetComponent<Stats>().enabled = false;
            GetComponent<PlayerInteraction>().enabled = false;
            GetComponentInChildren<Rig>().weight = 0;

            if(IsOwner){
                PlayerUIManager.Instance.PlayerDeadUI();
                if(GameManager.Instance.allPlayers.Count != 0)
                    PlayerCameraFollow.Instance.FollowPlayer(GameManager.Instance.allPlayers[Random.Range(0, GameManager.Instance.allPlayers.Count)].transform);
            }
        }

        void ActivateRagdoll()
        {
            foreach (Rigidbody rig in ragdollRigidbodies)
            {
                if (rig == null)
                    continue;
                rig.isKinematic = false;
            }
            //after 5-10s become kinematic
        }

                [ServerRpc(RequireOwnership = false)]
        void DespawnInServerRpc()
        {
            GetComponent<NetworkObject>().Despawn();
        }

        [ClientRpc]
        void AcivateRagdollClientRpc()
        {
            DisableComponents();
            ActivateRagdoll();
        }

        /////////////////////////////////////////////////////
        IEnumerator RespawnAfter(int time){
            PlayerUIManager.Instance.UpdateRespawnTime(time);
            while(time>=0){
                yield return new WaitForSeconds(1);
                time--;
                PlayerUIManager.Instance.UpdateRespawnTime(time);
            }

            RespawnServerRpc(OwnerClientId);
            Invoke("DespawnInServerRpc", 2f);
        }
        [ServerRpc(RequireOwnership = false)]
        void RespawnServerRpc(ulong clientID){

            Debug.Log("New player spawining, if not WHY NOT???");
            GameObject newPlayer = Instantiate(GameManager.Instance.playerPrefab,
                                                GameManager.Instance.allPlayers[Random.Range(0,GameManager.Instance.allPlayers.Count)].transform.position,
                                                Quaternion.identity);
            newPlayer.GetComponent<NetworkObject>().SpawnWithOwnership(clientID);

        }
    }
}