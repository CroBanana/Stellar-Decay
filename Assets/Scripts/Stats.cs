using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;

namespace Game
{
    public class Stats : NetworkBehaviour, IDamageable
    {
        public NetworkVariable<int> hp = new NetworkVariable<int>(100,
                                                                NetworkVariableReadPermission.Everyone,
                                                                NetworkVariableWritePermission.Server);


        public NetworkVariable<PlayerNameStruct> playerName = new NetworkVariable<PlayerNameStruct>(new PlayerNameStruct{
                                                                                                            _playerName = ""
                                                                                                        },
                                                                                                        NetworkVariableReadPermission.Everyone,
                                                                                                        NetworkVariableWritePermission.Owner);

        public struct PlayerNameStruct : INetworkSerializable
        {
            public FixedString64Bytes _playerName;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter{
                serializer.SerializeValue(ref _playerName);
            }
        }
        private int maxHP;

        public GameObject heartbeataudioPrefab;
        private AudioSource heartbeatAudioSource;

        [SerializeField]
        private TextMeshProUGUI nameText;
        public override void OnNetworkSpawn()
        {
            maxHP = hp.Value;
            hp.OnValueChanged += (int oldHP, int newHP) =>
            {

                UpdateHPImage(newHP);


                if (newHP <= 0)
                {
                    if (heartbeatAudioSource != null)
                        heartbeatAudioSource.Stop();

                    GetComponent<IDied>().Died();
                }
            };

            playerName.OnValueChanged += (PlayerNameStruct oldName, PlayerNameStruct newName) =>
            {
                nameText.text = newName._playerName.ToString();

                if (IsOwner)
                {
                    nameText.gameObject.SetActive(false);
                }
            };

        }

        [ServerRpc(RequireOwnership = false)]
        void SetNameServerRpc(string playerName)
        {
            SetNameClientRpc(playerName);
        }

        [ClientRpc]
        void SetNameClientRpc(string playerName)
        {
            //nameText.text = playerName;
            if (IsOwner)
                nameText.gameObject.SetActive(false);
        }

        void CheckSpawnPosition()
        {
            if (IsOwner)
            {
                Vector3 newPos = GameManager.Instance.CheckSpawnPosition(transform);
                Debug.LogWarning(newPos);
                if (newPos != Vector3.zero)
                    transform.position = newPos;
            }
        }

        private void Start()
        {
            if(IsOwner){

            if (heartbeataudioPrefab == null)
                return;

            heartbeataudioPrefab = Instantiate(heartbeataudioPrefab, transform.position, Quaternion.identity, transform);
            heartbeatAudioSource = heartbeataudioPrefab.GetComponent<AudioSource>();
            UpdateHPImage(maxHP);
            
            playerName.Value = new PlayerNameStruct{_playerName = PlayerPrefs.GetString("name")};
            nameText.text = playerName.Value._playerName.ToString();
            }

            if(!IsOwner){
                try
                {
                    nameText.text = playerName.Value._playerName.ToString();
                }
                catch 
                {
                    //Debug.Log("No name text object");
                }
            }
        }

        void UpdateHPImage(int newHP)
        {
            Debug.Log("Shake");
            if (!IsOwner)
                return;
            if (gameObject.CompareTag("Player"))
            {
                //Debug.Log("Changing image hp");
                CameraShake.Instance.Shake(3f, 0.1f);
                PlayerUIManager.Instance.UpdateHpImage(newHP, maxHP);

                float precentageHpLeft = (float)newHP / maxHP;
                if (precentageHpLeft <= 0.3f)
                {


                    if (!heartbeatAudioSource.isPlaying)
                        heartbeatAudioSource.Play();
                    if (precentageHpLeft < 0.15f)
                        heartbeatAudioSource.pitch = 1.4f;
                    else
                        heartbeatAudioSource.pitch = 1f;
                }
                else
                    heartbeatAudioSource.Stop();
            }
        }

        public void TakeDmg(int dmgTaken)
        {
            TakeDmgServerRpc(dmgTaken);
        }

        [ServerRpc(RequireOwnership = false)]
        public void TakeDmgServerRpc(int dmgTaken)
        {
            if (IsServer)
                hp.Value -= dmgTaken;
        }

    }
}