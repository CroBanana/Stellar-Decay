using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game
{
    public class Zombie : CoreCharacter
    {

        [Space(30), Header("Zombie Region")]
        [SerializeField] protected float attackRange;
        [SerializeField] protected Transform followTarget;
        [Range(0, 4.3f)][SerializeField] protected float moveSpeed;
        protected float moveSpeedOld;
        [SerializeField] protected float currentSpeed { get; set; }

        [SerializeField] protected List<Zombie> zombiesInfront;
        private Transform otherZombieTarget { get; set; }
        [SerializeField] protected Collider dmgZone;
        [SerializeField] protected int dmg;
        bool canDmg = true;

        public override void OnNetworkSpawn()
        {
            /*
            if(!IsServer){
                Destroy(this);
            }
            */
        }


        protected override void Start()
        {
            dmgZone = GetComponent<BoxCollider>();
            dmgZone.enabled = false;
            if(!IsServer){
                canDmg = false;
                return;
            }
            base.Start();
            anim = GetComponentInChildren<Animator>();
            navAgent = GetComponent<NavMeshAgent>();
            SetMovementSpeed();

            InvokeRepeating( "FollowPlayer",Random.Range(0.1f,1f),1f);

            // invoke funkcija koja će se pokretati uvijek s sve manjim tajmerom
            // u ovisnost s time koliko je bot daleko od playera.
            // ta funkcija bi trebala postavljati navAgent.SetDestination(player.position)
            // kada dođe pre blizu trebala bi se prestat zvat i setdestination bi trebo onda biti u updejtu
        }

        public void SetMovementSpeed(){
            moveSpeed = Random.Range(0.5f,4.3f);
            navAgent.speed = moveSpeed;
            moveSpeedOld = moveSpeed;
        }

        void FollowPlayer()
        {
            /*
            for (int i =GameManager.Instance.allPlayers.Count; i>=0;){
                i--;
                if(GameManager.Instance.allPlayers[i].gameObject == null)
                    GameManager.Instance.allPlayers.RemoveAt(i);
            }*/
            List<GameObject> players = GameManager.Instance.allPlayers;
            if (players.Count == 0)
                return;
            //Debug.Log(players.Count);
            float distance = Mathf.Infinity;
            Transform tempFollowTarget = null;

            foreach (GameObject player in players)
            {
                if (player == followTarget || player == null)
                    continue;

                float newDistance = Vector3.Distance(transform.position, player.transform.position);
                if (newDistance < distance)
                {
                    tempFollowTarget = player.transform;
                    distance = newDistance;
                }
            }
            followTarget = tempFollowTarget;



        }



        protected override void Update()
        {
            if(!IsServer)
                return;
            if (followTarget == null)
            {
                Debug.LogWarning("No Player");
                return;
            }


            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Land"))
            {
                navAgent.isStopped = true;
                Debug.Log("Sould be stopped");
            }
            else if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Land") &&
                    navAgent.isStopped)
            {
                navAgent.isStopped = false;
            }

            //navAgent.SetDestination(target.position);

            //ChangeSpeed();
            //SlowBasedOnObsticles();

            ChangeAgentPriority();

            anim.SetFloat("Move Speed", currentSpeed);

            currentSpeed = navAgent.velocity.magnitude;

            if (anim.GetBool("Off Mesh Link") != navAgent.isOnOffMeshLink)
            {
                anim.SetBool("Off Mesh Link", navAgent.isOnOffMeshLink);
                if (navAgent.currentOffMeshLinkData.startPos.y != navAgent.currentOffMeshLinkData.endPos.y)
                {
                    anim.SetBool("Falling", true);
                }
                else
                {
                    anim.SetBool("Falling", false);
                }
            }
            else
            {
                AttackCheck();
            }
            GroundCheck();
        }

        override protected void FixedUpdate() {
            if(!IsServer)
                return;
            if (followTarget != null)
                navAgent.SetDestination(followTarget.transform.position);
        }


        private void ChangeAgentPriority(){
            if (navAgent.isStopped)
                navAgent.avoidancePriority = 0;
            else
            {
                if ((int)(currentSpeed * 10) < 5)
                    navAgent.avoidancePriority = 5;
                else
                    navAgent.avoidancePriority = (int)(currentSpeed * 10);
            }
        }

        

        private void ChangeSpeed()
        {
            //OtherZombiesSpeedCheck();
            if (navAgent.isStopped == false)
                if (navAgent.speed != moveSpeed)
                {
                    navAgent.speed = moveSpeed;
                }
        }

        #region Zombies infront 
        public void AddToZombieInFront(Zombie newZombieInRange)
        {
            zombiesInfront.Add(newZombieInRange);
        }
        public void RemoveFromZombiesInFront(Zombie newZombieToRemove)
        {
            zombiesInfront.Remove(newZombieToRemove);
        }




        private void OtherZombiesSpeedCheck()
        {
            float minSpeed = 100;
            foreach (Zombie z in zombiesInfront)
            {
                if (z == null)
                    continue;
                if (z.currentSpeed < minSpeed)
                    minSpeed = z.currentSpeed;

            }
            if (minSpeed < moveSpeed)
                moveSpeed = 1;
            else
            {
                moveSpeed = moveSpeedOld;
            }
        }

        #endregion

        private void AttackCheck()
        {
            if (Vector3.Distance(transform.position, followTarget.transform.position) < attackRange)
            {
                anim.SetBool("Attack", true);
            }
            else if (anim.GetBool("Attack"))
                anim.SetBool("Attack", false);
        }

        void GroundCheck()
        {
            //makes a OverlapSphere witch checks if there are any ground colliders interacting with player
            //if there are colliders player is on ground
            if (Physics.OverlapSphere(transform.position,
                                        groundCheckRadius,
                                        layerMask).Length == 0)
            {
                if (grounded)
                {
                    grounded = false;
                    Debug.Log("Not on ground");
                    anim.SetBool("Grounded", grounded);
                    fallingTime = 0;
                }
            }
            else
            {
                grounded = true;
                anim.SetBool("Grounded", grounded);
            }
        }

        #region Animation Events
        void Event_ActivateDmgZone(){
            if(!IsServer)
                return;
            dmgZone.enabled = true;
            float deactivationTime = 0.1f;
            Invoke("DeactivateDmgZone", deactivationTime);
        }

        void DeactivateDmgZone(){
            dmgZone.enabled = false;
        }
        
        #endregion

        #region On trigger
        private void OnTriggerEnter(Collider other) {
            if(other.CompareTag("Player")){
                other.GetComponent<IDamageable>().TakeDmg(dmg);
            }
        }
        #endregion


        #region Gizmos
        void OnDrawGizmosSelected()
        {

            var nav = GetComponent<NavMeshAgent>();
            if (nav == null || nav.path == null)
                return;

            var line = GetComponent<LineRenderer>();
            if (line == null)
            {
                line = gameObject.AddComponent<LineRenderer>();
                line.material = new Material(Shader.Find("Sprites/Default")) { color = Color.yellow };
                line.startWidth = 0.5f;
                line.startColor = Color.yellow;
            }

            var path = nav.path;

            line.positionCount = path.corners.Length;

            for (int i = 0; i < path.corners.Length; i++)
            {
                line.SetPosition(i, path.corners[i]);
            }

            Gizmos.color = Color.red;
            //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
            Gizmos.DrawWireSphere(transform.position, groundCheckRadius);

            //Gizmos.color = Color.blue;
            //Vector3 position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
            //Gizmos.DrawLine(position, position + transform.forward * 1f);

        }
        #endregion
    }
}