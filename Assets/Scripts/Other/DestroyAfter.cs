using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
    public float destroyTimer;
    // Start is called before the first frame update
    void Start()
    {
        if(destroyTimer ==0)
            return;
        Invoke("DestroyAfterTime", destroyTimer);
    }

    public void DestroyAfterTime(){
        Destroy(gameObject);
    }
}
