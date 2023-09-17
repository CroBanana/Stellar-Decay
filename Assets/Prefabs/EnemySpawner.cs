using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class EnemySpawner : MonoBehaviour, IActivatable
    {
        [Tooltip("When its true, it will start spawning")]
        [SerializeField] private bool startSpawning;
        [Tooltip("Amount of zombies/enemies that will be spawned in a group")]
        [SerializeField] private int spawnAmountMin=0;
        [SerializeField] private int spawnAmountMax=10;

        [Tooltip("Time when zombies/enemies will respawn")]
        [SerializeField] private float spawnTime;
        private float spawnRateConstant;

        [Tooltip("How many times will the group of zombies/enemies be spawned \n If set to a negative number it will spawn infinitly")]
        [SerializeField] private int spawnTimes;
        [SerializeField] private bool multiplyByPlayingPlayers;
        // Start is called before the first frame update
        private void Start() {
            spawnRateConstant = spawnTime;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if(spawnTime>0){
                spawnTime -=Time.deltaTime;
            }

            if(startSpawning && spawnTime<=0){
                Debug.LogWarning("Spawning");
                if(spawnTimes<0){
                    //spawn infinitly
                    GameManager.Instance.SpawnEnemies(Random.Range(spawnAmountMin, spawnAmountMax),transform, multiplyByPlayingPlayers);
                }
                if(spawnTimes>0){
                    //spawn
                    GameManager.Instance.SpawnEnemies(Random.Range(spawnAmountMin, spawnAmountMax),transform, multiplyByPlayingPlayers);
                    spawnTimes--;
                }
                if(spawnTimes ==0)
                    startSpawning = false;
                spawnTime= spawnRateConstant;
            }
        }

        public void Activate(){
            Debug.LogWarning("Changed spawner so it can spawn");
            startSpawning = true;
        }
        public void Deactivate(){
            startSpawning = false;
        }
    }
}