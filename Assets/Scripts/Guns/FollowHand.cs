using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class FollowHand : MonoBehaviour
    {
        public Transform followObject;

        // Update is called once per frame
        void Update()
        {
            if(followObject != null){
                transform.position = followObject.transform.position;
                transform.rotation = followObject.rotation;
            }
        }
    }
}