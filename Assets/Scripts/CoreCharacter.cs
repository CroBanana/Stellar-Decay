using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

namespace Game
{
    public class CoreCharacter : NetworkBehaviour
    {
        [SerializeField] protected bool run;
        [SerializeField] protected Animator anim;
        [SerializeField] protected NavMeshAgent navAgent;

        [Space(20), Header("Ground check options")]
        [SerializeField] protected bool grounded;
        protected float gravity;
        [SerializeField] protected float groundCheckRadius;
        [SerializeField] protected LayerMask layerMask;
        [SerializeField] protected float fallingTime;
        virtual protected void Awake() {
            anim = GetComponentInChildren<Animator>();
            gravity += Physics.gravity.y;
        }

        // Start is called before the first frame update
        virtual protected void Start()
        {

        }

        // Update is called once per frame
        virtual protected void Update()
        {
            Movement();
            SetAnimationParameters();
        }

        virtual protected  void FixedUpdate() {
            //MoveAnimations();
        }

        virtual protected void SetAnimationParameters(){
            float curentAgentVelocity =0;

            curentAgentVelocity = navAgent.velocity.magnitude;
            anim.SetFloat("Move Speed", curentAgentVelocity);
        }

        virtual protected void Movement()
        {
        }

        virtual public void ChangeTarget(Transform newTarget){}
        virtual public void ChangeTarget(){}
    }
}