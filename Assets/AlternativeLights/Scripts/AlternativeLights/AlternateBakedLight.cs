using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlternateBakedLight : MonoBehaviour, IActivatable
{
    [SerializeField]
    private MeshRenderer[] renderers;
    [SerializeField]
    public int currentLightState;
    [SerializeField]
    public int[] linkedLightProbes; 
    
    public struct Indexes{
        public Indexes (List<int> indexList){
            indexesList = indexList;
        }

        public List<int> indexesList {get;}
    }

    private AlternativeLightsManager manager;

    public List<Indexes> indexesList;

    private void Start()
    {
        manager = FindObjectOfType<AlternativeLightsManager>();
        renderers = GetComponentsInChildren<MeshRenderer>();

        //Set lightprobe and lightmap at start of game
        Invoke("ChangelightStateAfter", 0.5f);

    }
    void ChangelightStateAfter(){
        ChangeLightState(currentLightState);
    }

    //Call the Manager script to change the linked lightprobes and then changes the objects lightmap texture locally
    public void ChangeLightState(int Value)
    {
        //Make sure target lightmap texture is in the valid range
        Value = Mathf.Clamp(Value, 0, manager.maxStatesCount);
        currentLightState = Value;

        //Call the manager to change the selected lightprobes settings
        manager.AssignLightProbesSegment(linkedLightProbes, currentLightState);

        //Switch to a diffrent lightmap texture

        int skipCount = manager.l_light.Length/manager.lightStates.Length; 
        int startIndex = skipCount * currentLightState;

        foreach (Renderer rend in renderers)
        {
            int selectedIndex = (rend.lightmapIndex % skipCount)+ startIndex;

            rend.lightmapIndex = selectedIndex;
        }
    }
    public void Activate(){
        Debug.Log("light change has been called");
        currentLightState = 1;
        ChangeLightState(1);
    }
    public void Deactivate(){
        Debug.Log("light change has been called");
        currentLightState = 0;
        ChangeLightState(0);
    }

    #region Intraction
    private void OnMouseDown()
    {
        Debug.LogError("On mouse is called");
        ToggleLights();
    }

    public void ToggleLights()
    {
        currentLightState += 1;
        if (currentLightState > manager.maxStatesCount)
        {
            currentLightState = 0;
        }
        ChangeLightState(currentLightState);
    }
    #endregion Intraction

#if UNITY_EDITOR
    //Used by the editor script for the selection tool
    [HideInInspector]
    public List<int> inRangeProbes;
    [HideInInspector]
    public List<int> tempList;
#endif
}
