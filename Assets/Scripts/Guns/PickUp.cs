using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{

    [RequireComponent(typeof(SphereCollider))]
    public class PickUp : MonoBehaviour, IInteractable
    {
        [SerializeField] private SphereCollider sphereCollider;
        [SerializeField] private float interactionRadius = 2f;
        // Start is called before the first frame update
        void Start()
        {
            sphereCollider = GetComponent<SphereCollider>();
            sphereCollider.radius = interactionRadius;
            GetComponentInChildren<Weapon>().DisableOnStart();
            GetComponent<IText>().HideText();
            /*
            if (transform.parent.root.CompareTag("Player"))
            {
                EnableDisable(false);
            }
            */
        }

        /*
        void EnableDisable(bool tf)
        {
            this.enabled = tf;
            transform.GetComponent<Weapon>().enabled = !tf;
            Destroy(sphereCollider);
        }
        */

        public void Interact(Transform player)
        {
            player.GetComponent<PlayerGuns>().NewWeapon(GetComponentInChildren<Weapon>());
            //EnableDisable(false);
        }
        

        /*
        private void OnTriggerEnter(Collider other)
        {
            //display UI for possible interaction
            other.GetComponent<IInteractable>().Interact(transform);
            Debug.Log("Can Interact");
        }
        private void OnTriggerExit(Collider other)
        {
            //remove UI display for possible interaction
            other.GetComponent<IInteractable>().Interact(transform);
            try
            {
                transform.GetComponent<IText>().HideText();
            }
            catch (System.Exception)
            {
                transform.parent.GetComponent<IText>().HideText();
            }

            Debug.Log("Cant Interact anymore");
        }
        */
        private void OnTriggerEnter(Collider other)
        {
            //display UI for possible interaction
            if(!other.CompareTag("Player")){
                return;
            }
            other.GetComponent<IInteractable>().Interact(transform);
            Debug.Log("Can Interact");
        }
        private void OnTriggerExit(Collider other)
        {
            //remove UI display for possible interaction
            if(!other.CompareTag("Player")){
                return;
            }
            other.GetComponent<IInteractable>().Interact(transform);
            transform.GetComponent<IText>().HideText();
            Debug.Log("Cant Interact anymore");
        }
    }
}