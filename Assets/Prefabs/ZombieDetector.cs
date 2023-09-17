using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ZombieDetector : MonoBehaviour
    {
        //whole script is for detecting other zombies that are in front of this one to know 
        //if any of them is standing still and if soo this one will move slover/stand still too
        [SerializeField] private Zombie mainZombie;

        private void Start()
        {
            mainZombie = GetComponentInParent<Zombie>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.gameObject.layer == 10)
            {
                try
                {
                    mainZombie.AddToZombieInFront(other.gameObject.GetComponent<Zombie>());
                }
                catch
                {
                    Debug.LogWarning("Does not contain zombieRoots");
                }
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.transform.gameObject.layer == 10)
            {
                try
                {
                    mainZombie.RemoveFromZombiesInFront(other.gameObject.GetComponent<Zombie>());
                }
                catch
                {
                    Debug.LogWarning(other.name+ "    Does not contain zombieRoots");
                }
            }
        }
    }
}