using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Game
{
    public class FinishGameGenerator : NetworkBehaviour, IInteractable
    {
        [SerializeField] private float rebootingTime;
        private bool isActivated;

        [SerializeField] private BoxCollider triggerCollider;

        private NetworkVariable<int> xScaleTriggerCollider = new NetworkVariable<int>(4,
                                                                                    NetworkVariableReadPermission.Everyone,
                                                                                    NetworkVariableWritePermission.Server);
        public NetworkVariable<int> currentRebootingTime = new NetworkVariable<int>(0,
                                                                                    NetworkVariableReadPermission.Everyone,
                                                                                    NetworkVariableWritePermission.Server);

        [SerializeField] private AudioClip startSound;
        [SerializeField] private AudioClip workingSound;
        [SerializeField] private AudioMixerGroup generatorWorkingGroup;
        private AudioSource audioSource;

        [SerializeField] private Image loadingBar;
        [SerializeField] public IActivatable activateLights;

        [Space(20), Header("Activate objects, and start spawners\nTries to acces IActivatable interface\nIf it does not exist it will just activate the object")]
        [SerializeField] private List<GameObject> activateAtStartingGenerator;
        [SerializeField] private List<GameObject> activateAtEndGenerator;
        [SerializeField] private List<GameObject> spawners;


        public override void OnNetworkSpawn()
        {
            xScaleTriggerCollider.OnValueChanged +=(int oldValue, int newValue)=>{
                triggerCollider.size = new Vector3(newValue,newValue,newValue);
            };

            currentRebootingTime.OnValueChanged += (int oldValue, int newValue)=>{
                loadingBar.fillAmount = newValue/rebootingTime;
            };
        }


        private void Start() {
            audioSource = GetComponent<AudioSource>();
            isActivated = false;
            activateLights = GetComponentInParent<IActivatable>();
            
        }


        #region Generator interaction and starting

        public void Interact(Transform interactingPlayer){
            //later neki minigame
            //trenutno deaktivirati interaction kod svih
            
            StartGeneratorServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        void StartGeneratorServerRpc(){
            xScaleTriggerCollider.Value = 1;
            StartCoroutine( GeneratorStarting());
        }

        IEnumerator GeneratorStarting(){
            StartSoundClientRpc();

            ActivateAtStart();
            int timeToGO = (int) rebootingTime;
            while(timeToGO >=0){
                yield return new WaitForSeconds(1);
                currentRebootingTime.Value += 1;
                timeToGO--;
            }
            ActivateAtEnd();

        }

        void ActivateAtStart(){
            ActivateAtStartClientRpc();



            if(spawners != null){
                foreach(GameObject spawner in spawners){
                    spawner.GetComponent<IActivatable>().Activate();
                }
            }
        }

        [ClientRpc]
        void ActivateAtStartClientRpc(){
            activateLights.Activate();
            foreach(GameObject go in activateAtStartingGenerator){
                try
                {
                    go.GetComponent<IActivatable>().Activate();
                }
                catch (System.Exception)
                {
                    go.SetActive(true);
                    throw;
                }
            }

            if(spawners != null){
                foreach(GameObject spawner in spawners){
                    spawner.GetComponent<IActivatable>().Activate();
                }
            }
        }

        void ActivateAtEnd(){
            ActivateAtEndClientRpc();
        }
        [ClientRpc]
        void ActivateAtEndClientRpc(){
            SceneManager.LoadScene("EndGame", LoadSceneMode.Single);
        }



        [ClientRpc]
        void StartSoundClientRpc(){
            PlayAudio(startSound);
        }

        void WorkSound(){
            PlayAudio(workingSound);
            StartCoroutine(RemoveLoadingBar());
        }

        IEnumerator RemoveLoadingBar(){
            Color color;
            while (loadingBar.color.a >0){
                yield return null;
                color = loadingBar.color;
                color.a += -Time.deltaTime;
                loadingBar.color = color;
            }
            color = loadingBar.color;
            color.a += 0;
            loadingBar.color = color;
        }

        void PlayAudio(AudioClip soundToPlay){
            audioSource.clip = soundToPlay;
            audioSource.outputAudioMixerGroup = generatorWorkingGroup; 
            audioSource.Play();
        }

        #endregion





        #region Trigger

        private void OnTriggerEnter(Collider other)
        {
            if(!other.CompareTag("Player")){
                return;
            }
            //display UI for possible interaction
            other.GetComponent<IInteractable>().Interact(transform);
            Debug.Log("Can Interact");
        }
        private void OnTriggerExit(Collider other)
        {
            if(!other.CompareTag("Player")){
                return;
            }
            //remove UI display for possible interaction
            other.GetComponent<IInteractable>().Interact(transform);
            transform.GetComponent<IText>().HideText();
            Debug.Log("Cant Interact anymore");
        }
        #endregion
    }
}
