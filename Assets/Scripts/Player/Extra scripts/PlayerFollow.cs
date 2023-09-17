using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game{
    public class PlayerFollow : MonoBehaviour
    {
        public Transform follow;

        // Update is called once per frame
        void Update()
        {
            if(follow != null)
                transform.position = follow.position; 
        }
    }
}