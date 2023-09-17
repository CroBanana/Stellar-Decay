using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class PlayerController : CoreCharacter
    {
        protected Vector3 movement, relative;
        protected CharacterController controller;
        protected float gravityConstant = 9.8f, gravity;

        protected override void Awake()
        {
            base.Awake();
            controller = GetComponent<CharacterController>();
        }

        protected override void Update()
        {
            Movement();
        }

        protected override void Movement()
        {
            movement.x = Input.GetAxis("Horizontal");
            movement.z = Input.GetAxis("Vertical");
            relative = transform.InverseTransformDirection(movement.normalized);
            anim.SetFloat("Side Movement", (float)decimal.Round((decimal)relative.z, 2));
            anim.SetFloat("Front Movement", (float)decimal.Round((decimal)relative.x, 2));
        }

        private void OnAnimatorMove() {
            Vector3 velocity = anim.deltaPosition;
            controller.Move(velocity);
        }
    }
}