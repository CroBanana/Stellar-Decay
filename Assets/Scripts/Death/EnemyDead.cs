using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using System.Linq;

namespace Game
{
    public class EnemyDead : NetworkBehaviour, IDied
    {
        public List< Rigidbody> ragdollRigidbodies;

        private void Start()
        {
            ragdollRigidbodies = gameObject.GetComponentsInChildren<Rigidbody>().ToList();
            ragdollRigidbodies.RemoveAt(0);
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
            if (!IsServer)
                return;
            //make object rigidbody
            DisableComponents();
            ActivateRagdoll();
            AcivateRagdollClientRpc();
        }

        void DisableComponents()
        {
            GetComponent<Animator>().enabled = false;
            GetComponent<NavMeshAgent>().enabled = false;
            Destroy(GetComponent<Rigidbody>());
            GetComponent<CapsuleCollider>().enabled = false;
            GetComponent<BoxCollider>().enabled = false;
            GetComponent<Zombie>().enabled = false;
            GetComponent<Stats>().enabled = false;

            AudioPlayer aPlayer = GetComponent<AudioPlayer>();
            aPlayer.CancelInvoke();
            aPlayer.enabled = false;

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
            Invoke("DisableRagdollAfterTime", 10);
        }

        void Flick(){
            
        }

        void DisableRagdollAfterTime()
        {
            bool bodiesDontMove = true;
            foreach (Rigidbody rig in ragdollRigidbodies)
            {
                if (Mathf.Approximately(0, rig.velocity.magnitude))
                {
                    continue;
                }
                bodiesDontMove = false;
                break;
            }

            if (bodiesDontMove)
            {
                foreach (Rigidbody rig in ragdollRigidbodies)
                {
                    rig.isKinematic = true;
                    Destroy( rig.transform.GetComponent<CharacterJoint>());
                    Destroy( rig.transform.GetComponent<Collider>());
                }
            }
            else{
                Invoke("DisableRagdollAfterTime",5f);
            }
        }


        [ClientRpc]
        void AcivateRagdollClientRpc()
        {
            DisableComponents();
            ActivateRagdoll();
        }
    }
}