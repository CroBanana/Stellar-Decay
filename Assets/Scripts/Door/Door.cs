using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Door : MonoBehaviour, IActivatable
    {
        [SerializeField] private Animator animator;


        [Header("If animator does not exist")]
        [SerializeField] private Transform part1;
        [SerializeField] private Transform part1Target;
        [SerializeField] private Transform part2;
        [SerializeField] private Transform part2Target;
        [SerializeField] private float timeToMove;
        [SerializeField] private List<GameObject> disableParts;

        private void Start() {
            animator = GetComponent<Animator>();
        }

        public void Activate()
        {
            if(animator == null)
                StartCoroutine(OpenDoor());
            else
                animator.SetBool("Open", true);
            Invoke("DisableParts",0.15f);
        }

        void DisableParts(){
            foreach(GameObject part in disableParts){
                part.SetActive(false);
            }
        }



        public void Deactivate()
        {

        }

        //ovo je ako ne postooji animator
        IEnumerator OpenDoor()
        {

            Vector3 initialPosition = transform.position;
            float elapsedTime = 0;

            while (elapsedTime < timeToMove)
            {
                float t = elapsedTime / timeToMove;
                if(part1 != null){
                    part1.position = Vector3.Lerp(initialPosition, part1Target.position, t);
                }

                if(part2 != null){
                    part2.position = Vector3.Lerp(initialPosition, part2Target.position, t);
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if(part1 != null)
                Destroy(part1);

            if(part2 != null)
                Destroy(part2);

        }

    }
}