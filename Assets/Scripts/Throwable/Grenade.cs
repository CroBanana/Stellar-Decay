using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Game
{
    public class Grenade : NetworkBehaviour
    {
        // Start is called before the first frame update
        [SerializeField] private float explodeTimer;
        [SerializeField] private float effectRadius;
        [SerializeField] private int maxDmg;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private GameObject explosionParticlePrefab;

        private void Start()
        {
            if (IsServer)
            {
                Debug.LogWarning("This is the explosions erver");
                Invoke("DestroyAfter", explodeTimer);
            }
            else
            {
                gameObject.GetComponent<Rigidbody>().isKinematic = true;
            }
        }

        void DestroyAfter()
        {
            Explode();
            GetComponent<NetworkObject>().Despawn();
        }

        void Explode()
        {
            SpawnParticleAndAudioClientRpc();
            RaycastHit[] all = Physics.SphereCastAll(transform.position, effectRadius, transform.up, 0f, layerMask);

            foreach (RaycastHit hit in all)
            {
                // Check if the hit object has the IDamagable interface
                IDamageable damagable = hit.collider.GetComponent<IDamageable>();
                if (damagable != null)
                {
                    int dmgAmount = (int)(maxDmg * ((effectRadius - Vector3.Distance(transform.position, hit.transform.position)) / effectRadius));
                    damagable.TakeDmg(dmgAmount);
                    Debug.Log("Dmged target for " + dmgAmount);
                }
            }
        }

        [ClientRpc]
        void SpawnParticleAndAudioClientRpc(){
            Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
        }

        #region Gizmos
        public Color sphereColor = Color.red;

        private void OnDrawGizmos()
        {
            Gizmos.color = sphereColor;
            Gizmos.DrawWireSphere(transform.position, effectRadius);
        }

        #endregion
    }
}