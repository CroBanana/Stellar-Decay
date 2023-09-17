using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.Audio;

namespace Game
{
    public class PowerGenerator : NetworkBehaviour, IInteractable
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

        [SerializeField,Space(20), Header("Activate Objects/lights")]
        private List<GameObject> activatableObjects;
        private List<IActivatable> iActivatableList;
        [SerializeField] private Transform segmentObject;
        [SerializeField] public IActivatable iActivatable;
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
            /*
            foreach(GameObject activatable in activatableObjects){
                try
                {
                    iActivatableList.Add(activatable.GetComponent<IActivatable>());
                }
                catch
                {
                    Debug.LogWarning(activatable.name +" Does not have an IActivatable interface on it, you need to add it");
                }
            }
            */
            iActivatable = segmentObject.GetComponent<IActivatable>();
            
            audioSource = GetComponent<AudioSource>();
            isActivated = false;

            Activate();
        }

        #region Generator interaction and starting

        public void Interact(Transform interactingPlayer){
            //later neki minigame
            //trenutno deaktivirati interaction kod svih
            StartGeneratorServerRpc();
            xScaleTriggerCollider.Value = 1;
        }

        [ServerRpc(RequireOwnership = false)]
        void StartGeneratorServerRpc(){
            StartCoroutine( GeneratorStarting());
        }

        IEnumerator GeneratorStarting(){
            StartSoundClientRpc();

            int timeToGO = (int) rebootingTime;
            while(timeToGO >=0){
                yield return new WaitForSeconds(1);
                currentRebootingTime.Value += 1;
                timeToGO--;
            }
            WorkSoundClientRpc();
            if(IsServer)
                Activate();

        }

        [ClientRpc]
        void StartSoundClientRpc(){
            PlayAudio(startSound);
        }

        [ClientRpc]
        void WorkSoundClientRpc(){
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

        #region Activate/Deactivate objects/scripts
        private void DeactivateOnStart(){
            iActivatable.Deactivate();
        }

        private void Activate(){
            iActivatable.Activate();
            if(IsServer)
                ActivateClientRpc();
        }
        [ClientRpc]
        private void ActivateClientRpc(){
            Activate();
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