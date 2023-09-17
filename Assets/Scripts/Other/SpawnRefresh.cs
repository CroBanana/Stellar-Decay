using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class SpawnRefresh : MonoBehaviour
    {
        private void Start()
        {
            PlayerSpawnerNewScene.Instance.canSpawn = true;
        }
    }
}