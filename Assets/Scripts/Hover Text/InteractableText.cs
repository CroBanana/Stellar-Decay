using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game
{
    public class InteractableText : MonoBehaviour, IText
    {

        public TextMeshProUGUI interactText;
        public Image image;
        public string textIs = "Open Box";
        public bool useNameInText;
        public bool setimage;



        private void Awake()
        {
            if (interactText == null)
            {
                interactText = transform.GetComponentInChildren<TextMeshProUGUI>();
            }
            if (useNameInText)
            {
                char breakCharacter = '(';
                int breakIndex = transform.name.IndexOf(breakCharacter);
                interactText.text = textIs + transform.name.Substring(0, breakIndex);
            }
            else
                interactText.text = textIs;
            HideText();
        }
        private void Start() {
            if(image == null){
                image = GetComponentInChildren<Image>();
            }
            if (setimage)
            {

                    image.sprite =  GetComponentInChildren<Weapon>().weaponIcon;
                    

            }
        }

        public void ShowText()
        {
            interactText.gameObject.SetActive(true);
            if (image != null)
            {
                image.gameObject.SetActive(true);
            }
        }

        public void HideText()
        {
            interactText.gameObject.SetActive(false);
            if (image != null)
            {
                image.gameObject.SetActive(false);
            }
        }
    }
}
