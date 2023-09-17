using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class LookAtCamera : MonoBehaviour
    {
        public Transform cam;
        // Start is called before the first frame update
        void Start()
        {
            Invoke("SetCamera", 0.5f);
        }

        void SetCamera(){
            cam = Camera.main.transform;
        }

        private void LateUpdate() {
            if(cam != null)
                transform.LookAt(transform.position+cam.forward);
        }
    }
}