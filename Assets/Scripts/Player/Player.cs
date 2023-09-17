using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game
{
    public class Player : CoreCharacter
    {


        //combined vector of horizontal and vectical inputs
        protected Vector3 movement;

        protected CharacterController characterController;

        [Space(20), Header("Controller movement speeds")]
        [SerializeField] protected float moveSpeed;
        [SerializeField] protected float runSpeed;
        [SerializeField] protected float moveSpeedWhileFalling;

        Vector3 relative;


        [Space(20), Header("Rotation Speed")]
        [SerializeField] protected float rotationSpeed;


        protected RaycastHit hit;

        protected override void Awake()
        {

            base.Awake();
            characterController = GetComponent<CharacterController>();
        }

        protected override void Start()
        {
            base.Start();
            grounded = true;
            if(IsOwner)
                PlayerCameraFollow.Instance.FollowPlayer(transform);
        }

        protected override void Update()
        {
            if(!IsOwner)
                return;
            if( AnimationPlayingCheck("Landing")){
                return;
            }

            RunInputCheck();
            MovementInput();
            Movement();
        }



        protected override void Movement()
        {
            SetAnimationParameters();

            Move();

        }
        void Move(){

            if(!grounded || AnimationPlayingCheck("Falling")){
                fallingTime += Time.deltaTime;
                MoveController(moveSpeedWhileFalling);
                FaceMouse();
                return;
            }

            if(run){
                MoveController(runSpeed);
                if(movement.magnitude <=0.05f)
                    FaceMouse();
                else
                    FaceDirection();
                return;
            }

            FaceMouse();
            MoveController(moveSpeed);
        }

        void MoveController(float usedSpeed){
            Vector3 velocity;
            velocity = movement.normalized * usedSpeed;
            velocity.y = gravity;
            characterController.Move(velocity * Time.deltaTime);
        }
        

        #region Input
        void MovementInput(){
            movement.x = Input.GetAxis("Horizontal");
            movement.z = Input.GetAxis("Vertical");
            relative = transform.InverseTransformDirection(movement);
        }


        void RunInputCheck()
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if(run == false){
                    run = true;
                    anim.SetBool("Running", true);
                }
            }
            else
            {
                if(run ){
                    run = false;
                    anim.SetBool("Running", false);
                }
            }
        }


        #endregion


        #region Animations
        protected override void SetAnimationParameters()
        {

            anim.SetFloat("Side Movement", (float)decimal.Round((decimal)relative.z, 2));
            anim.SetFloat("Front Movement", (float)decimal.Round((decimal)relative.x, 2));
            anim.SetFloat("Falling Time", fallingTime);
            anim.SetBool("Grounded", grounded);
            anim.SetFloat("Run Magnitude", relative.magnitude);

            //post https://stackoverflow.com/questions/57174293/get-unity-blend-tree-to-update-based-on-player-rotation-mouse-position related to the  other solution

        }

        bool AnimationPlayingCheck(string animation){
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            if(stateInfo.IsName(animation)){
                return true;
            }
            return false;
        }
        #endregion


        #region Face Mouse location or movement vector

        void FaceMouse()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 100))
            {
                Vector3 facingPoint = new Vector3(hit.point.x, 
                                            transform.position.y, 
                                            hit.point.z);
                /*transform.LookAt(new Vector3(hit.point.x, 
                                            transform.position.y, 
                                            hit.point.z));*/

                var q = Quaternion.LookRotation(facingPoint-transform.position);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, q,
                                                                rotationSpeed * Time.deltaTime);
            }
        }

        void FaceDirection(){
            /*
            transform.LookAt(new Vector3(movement.x + transform.position.x,
                                        transform.position.y,
                                        movement.z + transform.position.z)); */

            var q = Quaternion.LookRotation((transform.position+movement) - transform.position);
            transform.rotation = Quaternion.RotateTowards(transform.rotation,q ,
                                                            rotationSpeed * Time.deltaTime);
        }
        #endregion



         private void OnDrawGizmosSelected() {
                Gizmos.color = Color.red;
                //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
                Gizmos.DrawWireSphere (transform.position, groundCheckRadius);
            }
        }
}