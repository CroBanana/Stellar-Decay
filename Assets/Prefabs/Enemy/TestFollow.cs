using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

namespace Game
{
    public class TestFollow : NetworkBehaviour
    {
        [SerializeField] private GameObject followTarget;
        [SerializeField] private NavMeshAgent navAgent;

        public float dmgInterval = 2f;
        float dmgIntervalConstant = 2f;
        private void Start()
        {
            Debug.Log("Host: " + IsHost + ", server:" + IsServer);
            if (!IsServer)
                Destroy(this);
            navAgent = GetComponent<NavMeshAgent>();
            InvokeRepeating("FollowPlayer", 0.5f, 0.5f);
        }

        void FollowPlayer()
        {

            List<GameObject> players = GameManager.Instance.allPlayers;
            if (players.Count == 0)
                return;
            Debug.Log(players.Count);
            float distance = 500;
            if (followTarget != null)
                distance = Vector3.Distance(transform.position, followTarget.transform.position);
            foreach (GameObject player in players)
            {
                if (player == followTarget && player == null)
                    continue;

                float newDistance = Vector3.Distance(transform.position, player.transform.position);
                if (Vector3.Distance(transform.position, player.transform.position) < distance)
                {
                    followTarget = player;
                    distance = newDistance;
                }
            }


        }

        private void Update() {
            if(dmgInterval>0){
                dmgInterval+=-Time.deltaTime;
            }
        }

        void FixedUpdate()
        {
            if (followTarget != null)
                navAgent.SetDestination(followTarget.transform.position);
        }


        private void DmgPlayer(GameObject player){
            if(dmgInterval<0){
                player.GetComponent<Stats>().TakeDmg(7);
                dmgInterval = dmgIntervalConstant;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                DmgPlayer(other.gameObject);
            }
        }

        private void OnTriggerStay(Collider other) {
            if (other.CompareTag("Player"))
            {
                DmgPlayer(other.gameObject);
            }
        }
    }

}