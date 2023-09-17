using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
namespace Game
{
    //cilj ove skripte je da updejta y position od aimtarget objekta
    //jer se zbog nekog razloga ne updejta pravilno kada koristim clientNetworkTransform
    public class TargetYPosition : NetworkBehaviour
    {

        private void Start() {
            if(IsOwner)
                StartCoroutine(SlightYTransformMove());
        }

        IEnumerator SlightYTransformMove(){
            while (true){
                yield return new WaitForSeconds(0.3f);
                transform.position = new Vector3(transform.position.x,
                                                transform.position.y+0.02f,
                                                transform.position.z);

                //Debug.Log("Repeating");
            }
        }
    }
}