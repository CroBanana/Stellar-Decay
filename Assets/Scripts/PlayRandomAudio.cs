using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class PlayRandomAudio : MonoBehaviour
    {
        [SerializeField] private List<AudioClip> audioClipsList;
        [SerializeField] public AudioSource audioSource;
        private void Start() {

            audioSource =  GetComponent<AudioSource>();
            audioSource.clip = audioClipsList[Random.Range(0, audioClipsList.Count)];
            audioSource.Play();
        }
    }
}