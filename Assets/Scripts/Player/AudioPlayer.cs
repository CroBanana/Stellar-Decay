using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class AudioPlayer : NetworkBehaviour
    {

        [SerializeField] private List<AudioClip> allAudiosForCurrentFloorRun;
        [SerializeField] private List<AudioClip> allAudiosForCurrentFloorWalk;

        [Header("Audio sources")]
        [SerializeField] private AudioSource leftLeg;
        [SerializeField] private AudioSource rightLeg;

        [Header("Zombie Noises")]
        [SerializeField] private bool isZombie;
        [SerializeField] private float minRepeatTime;
        [SerializeField] private float maxRepeatTime;
        [SerializeField] private List<GameObject> zombieNoises;
        
        // Start is called before the first frame update
        void Start()
        {
            allAudiosForCurrentFloorRun = AudioManager.Instance.audioMetalRun;
            allAudiosForCurrentFloorWalk = AudioManager.Instance.audioMetalWalk;
            //Debug.Log(gameObject.layer);
            if (gameObject.CompareTag("Player") && !IsOwner )
                GetComponentInChildren<AudioListener>().enabled = false;

            if(isZombie)
                Invoke("PlayZombieNoise", Random.Range(2,10));
        }

        private void PlayZombieNoise(){
            Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y+1, transform.position.z);
            Instantiate(zombieNoises[Random.Range( 0,zombieNoises.Count)],spawnPosition,Quaternion.identity,transform);
            Invoke("PlayZombieNoise", Random.Range(minRepeatTime,maxRepeatTime));
        }

        public void PlayLeftLegSound(string runOrWalk){
            if(runOrWalk == "Run"){
                leftLeg.clip = RandomAudio(allAudiosForCurrentFloorRun);
            }
            else{
                leftLeg.clip = RandomAudio(allAudiosForCurrentFloorWalk);
            }
            leftLeg.Play();
            //Debug.Log("LeftLeg Sound");
        }

        public void PlayRightLegSound(string runOrWalk){
            if(runOrWalk == "Run"){
                rightLeg.clip = RandomAudio(allAudiosForCurrentFloorRun);
            }
            else{
                rightLeg.clip = RandomAudio(allAudiosForCurrentFloorWalk);
            }
            rightLeg.Play();
            //Debug.Log("RightLeg Sound");
        }

        AudioClip RandomAudio(List<AudioClip> audios){
            return audios[Random.Range(0,audios.Count)];
        }


    }


    public class DiferentAudios{

    }
}