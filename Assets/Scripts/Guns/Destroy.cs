using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Game
{
    public class Destroy : NetworkBehaviour, IDestroy
    {
        public void DestroyObject()
        {
            DestoryObjectOnServerRpc();
        }

        [ServerRpc(RequireOwnership =false)]
        void DestoryObjectOnServerRpc(){
            GetComponent<NetworkObject>().Despawn();
        }
    }
}