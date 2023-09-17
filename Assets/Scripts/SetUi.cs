using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;


namespace Game
{
    public class SetUi : NetworkBehaviour
    {
        public GameObject playerUI;
        public GameObject otherUI;
        public Transform followThis;
        public TMP_Text serverCode;
        string code;
        private void Start()
        {
            if (IsOwner)
            {
                //PlayerCameraFollow.Instance.FollowPlayer(followThis);
                playerUI.SetActive(true);
                code = GameObject.Find("TestRelay").GetComponent<TestRelay>().serverCode;
                serverCode.text = code;
            }
            else
            {
                otherUI.SetActive(true);
            }
        }

    }
}