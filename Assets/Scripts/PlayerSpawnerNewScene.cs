using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

namespace Game
{
    public class PlayerSpawnerNewScene : MonoBehaviour
    {

        public static PlayerSpawnerNewScene Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }

            canSpawn = true;
        }

        public string playerID;
        public bool canSpawn = false;

        void OnSceneLoaded(ulong _playerID, string scene, LoadSceneMode mode)
        {
            if(canSpawn){
                StartCoroutine(WaitForRespawn(_playerID, 0.2f));
                canSpawn = false;
            }
        }

        IEnumerator WaitForRespawn(ulong _playerID, float waitTime){
            yield return new WaitForSeconds(waitTime);
            Debug.LogWarning("Creating player in corutine  "+_playerID);
            try
            {
                GameManager.Instance.NewPlayerSpawn(_playerID);
            }
            catch (System.Exception)
            {
                Debug.Log("No gamemanager instance");
                throw;
            }
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
        public void SceneLoadEvent(){
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoaded;
        }
    }
}
