using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Game
{
    public class PlayerInteraction : NetworkBehaviour, IInteractable
    {
        [SerializeField] private List<Transform> interactableObjects;
        [SerializeField] private Transform closestInteractable;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
                this.enabled = false;
        }

        private void Update()
        {
            if (interactableObjects.Count != 0)
            {
                SelectInteractableObject();

                //Debug.Log("Closest interactable " + closestInteractable.name);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    ButtonClickSFX.Instance.PlayInteractionSound();
                    closestInteractable.GetComponent<IInteractable>().Interact(transform);
                }
            }
            else
            {
                closestInteractable = null;
            }
        }

        public void SelectInteractableObject(){
            if (interactableObjects.Count > 1)
                {
                    float distance = 100;
                    Transform closest = interactableObjects[0];

                    for (int i = interactableObjects.Count - 1; i >= 0; i--)
                    {

                        if (interactableObjects[i].gameObject == null)
                        {
                            interactableObjects.RemoveAt(i);
                            Debug.Log("Object is null");
                            continue;
                        }

                        float newDistance = Vector3.Distance(transform.position, interactableObjects[i].position);
                        if (newDistance < distance)
                        {
                            closest = interactableObjects[i];
                            distance = newDistance;
                        }
                    }

                    if (closestInteractable != closest)
                    {
                        foreach (Transform interactable in interactableObjects)
                        {
                            if (interactable == closestInteractable)
                            {
                                try
                                {
                                    closestInteractable.GetComponent<IText>().ShowText();
                                }
                                catch
                                {
                                    closestInteractable.GetComponentInParent<IText>().ShowText();
                                }
                            }
                            try
                            {
                                closestInteractable.GetComponent<IText>().HideText();
                            }
                            catch
                            {
                                closestInteractable.GetComponentInParent<IText>().HideText();
                            }
                        }
                    }

                    closestInteractable = closest;

                    //needs to display the interact ui only on the closest to player
                    //and sets closest interactable
                }
                else
                {
                    if (interactableObjects[0] == null)
                    {
                        interactableObjects.RemoveAt(0);
                        return;
                    }
                    if (closestInteractable != interactableObjects[0])
                    {
                        closestInteractable = interactableObjects[0];
                        try
                        {
                            closestInteractable.GetComponent<IText>().ShowText();
                        }
                        catch
                        {
                            closestInteractable.GetComponentInParent<IText>().ShowText();
                        }
                    }

                }
        }

        public void Interact(Transform thisInteracted)
        {
            if (interactableObjects.Contains(thisInteracted))
                interactableObjects.Remove(thisInteracted);
            else
                interactableObjects.Add(thisInteracted);
        }
    }
}
