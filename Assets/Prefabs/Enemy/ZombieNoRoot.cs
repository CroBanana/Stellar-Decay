using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


namespace Game
{
    public class ZombieNoRoot : CoreCharacter
    {
        [SerializeField] protected Transform player, target;
        [SerializeField] protected float attackRange;
        [SerializeField] protected BoxCollider boxTrigger;
        [Range(0, 4.3f)][SerializeField] protected float moveSpeed;
        protected float moveSpeedOld;
        [SerializeField] protected LayerMask otherZombiesLayers;
        [SerializeField] protected float currentSpeed {get; set;}

        [SerializeField] protected List<ZombieNoRoot> zombiesInfront;
        protected override void Awake()
        {
            anim = GetComponentInChildren<Animator>();
            navAgent = GetComponent<NavMeshAgent>();
            navAgent.speed = moveSpeed;
            boxTrigger = GetComponent<BoxCollider>();
            boxTrigger.enabled = false;
        }

        protected override void Start()
        {
            base.Start();
            player = GameObject.FindWithTag("Player").transform;
            target = player;
            moveSpeedOld = moveSpeed;
            navAgent.SetDestination(target.position);

            // invoke funkcija koja će se pokretati uvijek s sve manjim tajmerom
            // u ovisnost s time koliko je bot daleko od playera.
            // ta funkcija bi trebala postavljati navAgent.SetDestination(player.position)
            // kada dođe pre blizu trebala bi se prestat zvat i setdestination bi trebo onda biti u updejtu
        }

        override protected void Update()
        {
            if (player == null)
            {
                return;
            }
            if (target == null)
            {
                ChangeTarget();
            }
            /* stari kod za zaustavljanje
            int rayDistance = 2;
            RaycastHit hit;
            Debug.DrawLine(transform.position + transform.up, transform.position + transform.up + (transform.forward * rayDistance),Color.blue);
            if (Physics.Raycast(transform.position+ transform.up,  
                                transform.position + transform.up + transform.forward, 
                                out hit, rayDistance, otherZombiesLayers))
            {
                if(hit.collider.transform == transform)
                    return;
                Debug.Log(hit.collider.name);
                if (hit.collider.GetComponent<NavMeshAgent>().velocity.magnitude < 0.5f)
                {
                    Debug.Log("NavAgent gotten");
                    moveSpeed = 0;
                    Debug.Log(navAgent.isStopped);
                    return;
                }
            }
            else{
                navAgent.SetDestination(target.position);
                moveSpeed = moveSpeedOld;
            }
            */
            
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Land"))
            {
                navAgent.isStopped = true;
                Debug.Log("Sould be stopped");
            }
            else if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Land") &&
                    navAgent.isStopped &&
                    target == player)
            {
                navAgent.isStopped = false;
            }


            ChangeSpeed();

            currentSpeed = navAgent.velocity.magnitude;

            if (navAgent.isStopped)
                navAgent.avoidancePriority = 0;
            else
            {
                if ((int)(currentSpeed * 10) < 5)
                    navAgent.avoidancePriority = 5;
                else
                    navAgent.avoidancePriority = (int)(currentSpeed * 10);
            }

            anim.SetFloat("Move Speed", currentSpeed);
            //Debug.Log(currentSpeed);

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
                AttackCkeck();
            }
            GroundCheck();

            // Use the current speed value as needed
            //Debug.Log("Current Speed: " + currentSpeed);
        }

        private void ChangeSpeed()
        {
            if (navAgent.isStopped == false)
                if (navAgent.speed != moveSpeed)
                {
                    navAgent.speed = moveSpeed;
                }
        }
        private void AttackCkeck()
        {
            if (Vector3.Distance(transform.position, target.transform.position) < attackRange)
            {
                anim.SetBool("Attack", true);
            }
            else if (anim.GetBool("Attack"))
                anim.SetBool("Attack", false);
        }

        override public void ChangeTarget(Transform newTarget)
        {
            target = newTarget;
            navAgent.isStopped = true;
        }
        override public void ChangeTarget()
        {
            target = player;
            navAgent.isStopped = false;
        }
        #region Triggers for dmg
        public void EventTrigger()
        {
            boxTrigger.enabled = true;
            StartCoroutine(DisableTrigger());
        }

        IEnumerator DisableTrigger()
        {
            yield return new WaitForSeconds(0.1f);
            boxTrigger.enabled = false;
        }

        #endregion

        #region Ground Check
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

        public void AddToZombieInFront(ZombieNoRoot newZombieInRange){
            zombiesInfront.Add(newZombieInRange);
        }
        public void RemoveFromZombiesInFront(ZombieNoRoot newZombieToRemove){
            zombiesInfront.Remove(newZombieToRemove);
        }


    }
}