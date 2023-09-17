using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonClickSFX : MonoBehaviour
{
    
    public static ButtonClickSFX Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

    public List<GameObject> clickSounds;
    public List<GameObject> interactionSounds;

    public void PlayClickSound(){
        Instantiate(clickSounds[Random.Range(0, clickSounds.Count)]);
    }

    public void PlayInteractionSound(){
        Instantiate(interactionSounds[Random.Range(0, interactionSounds.Count)]);
    }
}
