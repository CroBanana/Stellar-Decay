using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Game
{
    public class AddPlayer : NetworkBehaviour
    {

        private void Start() {
            AddToPlayerList();
        }

        public void AddToPlayerList(){
            GameManager.Instance.AddPlayerToList(gameObject);
        }

    }
}