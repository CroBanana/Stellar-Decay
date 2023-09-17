using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateProjector : MonoBehaviour, IActivatable
{
    [SerializeField] private List<GameObject> objectsToActivate;
    // Start is called before the first frame update
    void Start()
    {
        Deactivate();
    }

    public void Activate(){
        foreach(GameObject obj in objectsToActivate)
            obj.SetActive(true);
    }
    public void Deactivate(){
        foreach(GameObject obj in objectsToActivate)
            obj.SetActive(false);
    }

}
