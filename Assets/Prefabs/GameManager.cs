using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

namespace Game
{
    public class GameManager : NetworkBehaviour, IActivatable
    {
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        public List<NetworkObject> dinamiclySpawnedObjects;

        [Header("Player spawning")]
        public GameObject playerPrefab;
        public List<Transform> startingSpawnLocations;
        public float gameTimer;

        [Header("Chest spawning")]
        [SerializeField] private GameObject weaponChestPrefab;
        [SerializeField] private List<Transform> weaponChestSpawnLocations;
        [SerializeField] bool chestSpawned = false;

        [Header("Enemy spawning")]
        [SerializeField] private bool canSpawn;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private int randomSpawnAmountMin;
        [SerializeField] private int randomSpawnAmountMax;
        [SerializeField] private float randomSpawnTimeMin, randomSpawnTimeMax;
        [SerializeField] private List<Transform> spawnLocationsEnemy;

        [SerializeField] public GameObject bloodParticlePrefab;


        private bool escPressed;

        public List<GameObject> allPlayers;
        public int playersPlaying;
        public List<Light> listFlashLights;

        public bool notSent = true;

        [Header("Audio based on generators")]
        [SerializeField] private AudioSource audioSourceGameManager;
        [SerializeField] private List<AudioClip> audioClipsDuringGame;
        public NetworkVariable<int> audioClipIndex = new NetworkVariable<int>(0,
                                                                NetworkVariableReadPermission.Everyone,
                                                                NetworkVariableWritePermission.Server);

        private void Start()
        {
            //Debug.Log("Owner id:"+ OwnerClientId);
            InvokeRepeating("SpawnEnemies", 1f, Random.Range(randomSpawnTimeMin, randomSpawnTimeMax));
            if (!IsServer)
            {
                Invoke("SearchForPlayers", 1f);
            }
            if (IsServer)
            {
                InvokeRepeating("AllPlayersDeadCheck", 5, 0.2f);
            }

            audioSourceGameManager = GetComponent<AudioSource>();
            audioClipIndex.OnValueChanged += (int oldIndex, int newIndex) =>
            {
                audioSourceGameManager.clip = audioClipsDuringGame[newIndex];
                audioSourceGameManager.Play();
            };

            audioSourceGameManager.clip = audioClipsDuringGame[0];
            audioSourceGameManager.Play();

            //string playerIdString = PlayerPrefs.GetString("playerID");
            //ulong playerIDValue = ulong.Parse(playerIdString);
            //NewPlayerSpawn(playerIDValue);

            if (IsHost)
            {
                PlayerUIManager.Instance.ThisIsHost();
            }
            else
            {
                PlayerUIManager.Instance.ThisIsClient();
            }



        }


        void SearchForPlayers()
        {
            Debug.Log("GM: Search for players function");
            Player[] playersInScene = FindObjectsOfType(typeof(Player)) as Player[];

            foreach (Player p in playersInScene)
            {
                if (allPlayers.Contains(p.gameObject))
                    continue;

                allPlayers.Add(p.gameObject);
            }
        }

        private void AllPlayersDeadCheck()
        {
            if (allPlayers.Count == 0 && notSent)
            {
                DespawnEverything();
                EnableRespawnOptionClientRpc();
                GameOverClientRpc();
                notSent = false;
            }
        }

        public void NewPlayerSpawn(ulong _clientID)
        {
            SpawnPlayerOnSceneLoadServerRpc(_clientID);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnPlayerOnSceneLoadServerRpc(ulong _clientID)
        {
            StartCoroutine(EndOfFrameSpawn(_clientID));
            /*
            //Debug.LogWarning("Creating new player in game manager "+ _clientID);
            GameObject newPlayer;

            Vector3 position = Vector3.zero;

            if(gameTimer <10 && startingSpawnLocations != null){
                //Debug.LogError("game 1 spawner");
                position = startingSpawnLocations[Random.Range(0,startingSpawnLocations.Count)].transform.position;
                newPlayer = Instantiate(playerPrefab,position,Quaternion.identity);
            }
            else{
                //Debug.LogError("game 2 spawner");
                position = allPlayers[Random.Range(0, allPlayers.Count)].transform.position;
                newPlayer = Instantiate(playerPrefab, position,Quaternion.identity);
            }


            //Debug.LogWarning("Starting a corutine for "+ _clientID);
            newPlayer.GetComponent<NetworkObject>().SpawnWithOwnership(_clientID);

            /*
            if(_clientID == 0)
                newPlayer.GetComponent<NetworkObject>().SpawnWithOwnership(_clientID);
            else
                StartCoroutine(CheckCorectPosition(position, newPlayer.transform,_clientID));

                */


        }

        IEnumerator EndOfFrameSpawn(ulong _clientID)
        {
            yield return new WaitForEndOfFrame();
            GameObject newPlayer;

            Vector3 position = Vector3.zero;

            if (gameTimer < 10 && startingSpawnLocations != null)
            {
                //Debug.LogError("game 1 spawner");
                position = startingSpawnLocations[Random.Range(0, startingSpawnLocations.Count)].transform.position;
                newPlayer = Instantiate(playerPrefab, position, Quaternion.identity);
            }
            else
            {
                //Debug.LogError("game 2 spawner");
                position = allPlayers[Random.Range(0, allPlayers.Count)].transform.position;
                newPlayer = Instantiate(playerPrefab, position, Quaternion.identity);
            }


            //Debug.LogWarning("Starting a corutine for "+ _clientID);
            newPlayer.GetComponent<NetworkObject>().SpawnWithOwnership(_clientID);
            dinamiclySpawnedObjects.Add(newPlayer.GetComponent<NetworkObject>());
        }



        public Vector3 CheckSpawnPosition(Transform playerPosition)
        {
            if (gameTimer < 10 && startingSpawnLocations != null)
            {
                bool hasSpawnedProperly = false;
                foreach (Transform location in startingSpawnLocations)
                {

                    if (Vector3.Distance(playerPosition.transform.position, location.position) < 1f)
                    {
                        hasSpawnedProperly = true;
                        break;
                    }
                }
                if (!hasSpawnedProperly)
                    return startingSpawnLocations[Random.Range(0, startingSpawnLocations.Count)].transform.position;
            }
            else
            {
                //player respaened
                //this works for now so its fine
                return Vector3.zero;
            }

            return Vector3.zero;

        }

        IEnumerator CheckCorectPosition(Vector3 pos, Transform newPlayer, ulong _clientID)
        {

            Debug.Log(pos);
            yield return new WaitForSeconds(0.4f);
            if (Vector3.Distance(pos, newPlayer.position) > 1f)
                newPlayer.position = pos;
            newPlayer.position = pos;

            /*
            yield return new WaitForSeconds(0.3f);
            newPlayer.GetComponent<NetworkObject>().SpawnWithOwnership(_clientID);
            */
        }



        private void SpawnEnemies()
        {
            if (!canSpawn)
                return;

            if (allPlayers.Count == 0)
                return;

            if (allPlayers.Count > 0)
            {
                if (!IsServer)
                    return;
                int spawning = 0;
                int spawningCount = Random.Range(randomSpawnAmountMin, randomSpawnAmountMax);
                while (spawning < randomSpawnAmountMax)
                {
                    GameObject newEnemy = Instantiate(enemyPrefab,
                                                spawnLocationsEnemy[Random.Range(0, spawnLocationsEnemy.Count)].position,
                                                Quaternion.identity);

                    NavMeshAgent newEnemyAgent = newEnemy.GetComponent<NavMeshAgent>();

                    newEnemy.GetComponent<NetworkObject>().Spawn();

                    dinamiclySpawnedObjects.Add(newEnemy.GetComponent<NetworkObject>());
                    spawning++;
                }

            }
        }

        public void SpawnEnemies(int enemyCount, Transform spawnLocation, bool playerMultiplayer)
        {
            if (!IsServer)
                return;

            if (allPlayers.Count == 0)
                return;

            if (playerMultiplayer)
            {
                enemyCount = (int)(enemyCount * 1.5);
                //enemyCount = enemyCount * allPlayers.Count;
            }

            for (int i = 0; i < enemyCount; i++)
            {
                GameObject newEnemy = Instantiate(enemyPrefab,
                                            spawnLocation.position,
                                            Quaternion.identity);

                NavMeshAgent newEnemyAgent = newEnemy.GetComponent<NavMeshAgent>();

                newEnemy.GetComponent<NetworkObject>().Spawn();
                dinamiclySpawnedObjects.Add(newEnemy.GetComponent<NetworkObject>());
            }
        }

        private void FixedUpdate()
        {
            if (!IsServer)
            {

                return;
            }

            if (playersPlaying < allPlayers.Count)
            {
                playersPlaying = allPlayers.Count;
            }

            /*
            if (!chestSpawned)
            {
                Debug.Log("Test");
                foreach (Transform chest in weaponChestSpawnLocations)
                {
                    GameObject go = Instantiate(weaponChestPrefab, chest.transform.position, chest.transform.rotation);
                    go.GetComponent<NetworkObject>().Spawn();
                    chestSpawned = true;
                }
            }
            */
        }

        private void Update()
        {
            if (gameTimer < 10)
            {
                gameTimer += Time.deltaTime;
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Esc pressed:" + escPressed);
                if (!escPressed)
                {
                    escPressed = true;
                    PlayerUIManager.Instance.ActivateIngameMenu();
                }
                else if (escPressed)
                {
                    escPressed = false;
                    PlayerUIManager.Instance.ContinueGame();
                }

            }
        }


        public void AddPlayerToList(GameObject newPlayer)
        {
            if (allPlayers.Contains(newPlayer))
                return;


            allPlayers.Add(newPlayer);
            Light[] childrenLights = newPlayer.GetComponentsInChildren<Light>();
            foreach (Light light in childrenLights)
            {
                if (light.transform.CompareTag("Flashlight"))
                {
                    listFlashLights.Add(light);
                    break;
                }
            }
            SyncFlashLightsClientRpc();
        }

        [ClientRpc]
        private void SyncFlashLightsClientRpc()
        {
            GameObject[] flashlights = GameObject.FindGameObjectsWithTag("Flashlight");
            foreach (GameObject flashlight in flashlights)
            {
                if (listFlashLights == null)
                {
                    listFlashLights.Add(flashlight.GetComponent<Light>());
                    continue;
                }
                if (listFlashLights.Contains(flashlight.GetComponent<Light>()))
                    continue;
                listFlashLights.Add(flashlight.GetComponent<Light>());
            }
        }


        public void Respawn(ulong clientID, int respawnTime)
        {
            if (!IsServer)
                return;
            Debug.Log("Making new player spawned");
            StartCoroutine(RespawnAfter(clientID, respawnTime));
        }

        IEnumerator RespawnAfter(ulong clientID, int time)
        {
            yield return new WaitForSeconds(time);
            RespawnServerRpc(clientID);
        }
        [ServerRpc]
        void RespawnServerRpc(ulong clientID)
        {
            Debug.Log("New player spawining, if not WHY NOT???");
            GameObject newPlayer = Instantiate(playerPrefab,
                                                allPlayers[Random.Range(0, allPlayers.Count)].transform.position,
                                                Quaternion.identity);


            newPlayer.GetComponent<NetworkObject>().SpawnWithOwnership(clientID);
            dinamiclySpawnedObjects.Add(newPlayer.GetComponent<NetworkObject>());
        }

        [ClientRpc]
        public void GameOverClientRpc()
        {
            PlayerUIManager.Instance.GameOver();
        }

        public void Activate()
        {
            if (IsServer)
                audioClipIndex.Value++;
        }
        public void Deactivate()
        {

        }


        public bool CheckIfHost()
        {
            return IsHost;
        }

        public void DespawnEverything()
        {
            foreach (NetworkObject no in dinamiclySpawnedObjects)
            {
                try
                {
                    no.Despawn();
                }
                catch
                {
                    Debug.Log("NO does not exist");
                }
            }

            PlayerDead[] activePlayerDeadScripts = FindObjectsOfType(typeof(PlayerDead)) as PlayerDead[];
            foreach (PlayerDead pd in activePlayerDeadScripts)
            {
                pd.StopAllCoroutines();
                pd.gameObject.GetComponent<NetworkObject>().Despawn();
            }
        }

        [ClientRpc]
        public void EnableRespawnOptionClientRpc()
        {
            PlayerSpawnerNewScene.Instance.canSpawn = true;
        }

        [ClientRpc]
        public void HostLeftGameClientRpc()
        {
            
        }

        [ClientRpc]
        public void ChangeNotHostTextClientRpc(){
            PlayerUIManager.Instance.notHostText.text = "Host has left the game";
        }

    }
}