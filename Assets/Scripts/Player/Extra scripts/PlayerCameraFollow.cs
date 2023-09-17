using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Game
{
    public class PlayerCameraFollow : MonoBehaviour
    {
        public static PlayerCameraFollow Instance { get; private set; }
        private void Awake()
        {
            // If there is an instance, and it's not me, delete myself.

            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }

            cinemachineVirtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        }

        private Stats followTargetStats;

        private void Start()
        {
            InvokeRepeating("CheckPlayerHP", 5f, 1f);
        }

        private void CheckPlayerHP()
        {
            if (followTargetStats.hp.Value <= 0)
            {
                FollowPlayer(GameManager.Instance.allPlayers[Random.Range(0, GameManager.Instance.allPlayers.Count)].transform);
            }
        }

        private CinemachineVirtualCamera cinemachineVirtualCamera;
        [SerializeField] private PlayerFollow playerFollow;
        public void FollowPlayer(Transform followTransform)
        {
            try
            {
                followTargetStats.gameObject.GetComponentInChildren<AudioListener>().enabled = false;
            }
            catch
            {
                Debug.Log("No audio listener exists");
            }
            followTargetStats = followTransform.GetComponent<Stats>();
            try
            {
                followTargetStats.gameObject.GetComponentInChildren<AudioListener>().enabled = true;
            }
            catch
            {
                Debug.Log("No audio listener exists");
            }
            playerFollow.follow = followTransform;
            cinemachineVirtualCamera.Follow = playerFollow.transform;
            cinemachineVirtualCamera.LookAt = playerFollow.transform;
            cinemachineVirtualCamera.enabled = true;
        }
    }
}