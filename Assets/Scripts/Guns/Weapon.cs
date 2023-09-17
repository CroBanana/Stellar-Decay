using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.VFX;


namespace Game
{
    [RequireComponent(typeof(LineRenderer))]
    public class Weapon : NetworkBehaviour, IWeapon
    {
        public NetworkVariable<bool> isEnabled = new NetworkVariable<bool>(true,
                                                                            NetworkVariableReadPermission.Everyone,
                                                                            NetworkVariableWritePermission.Owner);

        [SerializeField] private int dmg;

        [Header("Ui settings")]
        public Sprite weaponIcon;

        [Header("Ammo settings")]
        [SerializeField] private int maxAmmoCount;
        public int curentAmmoCount;

        public int curentMagazineAmmoCount;
        public int maxMagazineCount;

        [Header("Shoot settings")]
        [SerializeField] private float fireRateConstant;
        public float fireRate;
        [SerializeField] private int numberOfProjectiles;

        [SerializeField] private LayerMask hitLayers;

        [Header("Projectile settings")]
        public Transform projectileSpawnLocation;
        [SerializeField] private GameObject projectile;
        [SerializeField] private VisualEffect muzzleFlashEffect;
        [SerializeField] private Light muzzlePointLight;
        [SerializeField] private float startLightIntensity;
        [SerializeField] private TrailRenderer bulletTrail;
        [SerializeField] private GameObject impactBloodParticles;
        [SerializeField] private LineRenderer laser;

        public Transform leftHandPosition;

        [Header("Audio settings")]
        [SerializeField] private List<AudioSource> shootAudioPrefabs;
        [SerializeField] private List<AudioSource> reloadAudioPrefabs;
        [SerializeField] private List<AudioSource> noAmmoAudioPrefabs;

        [Header("Bullet spread"), Space(20)]
        [SerializeField] private List<Transform> shootDirections;
        List<Transform> tempShootDirections;
        [SerializeField] private float minimumSpread;
        [SerializeField] private float currentSpread;
        [SerializeField] private float maximumSpread;
        [SerializeField] private float spreadNormalizationSpeed;
        [SerializeField] private Transform spreadTransform;
        [SerializeField] private float spreadAddPerShot = 0.1f;


        public override void OnNetworkSpawn()
        {
            isEnabled.OnValueChanged += (bool oldValue, bool newValue) =>
            {
                transform.gameObject.SetActive(newValue);
            };
        }

        // Start is called before the first frame update
        void Start()
        {
            fireRate = 0.5f;
            currentSpread = minimumSpread;
            spreadTransform.localScale = new Vector3(currentSpread,
                                                        currentSpread,
                                                        spreadTransform.localScale.z);
            laser = GetComponent<LineRenderer>();
            if (!IsOwner)
                gameObject.SetActive(isEnabled.Value);
            if (shootDirections.Count > 0)
                shootDirections.Clear();

            foreach (Transform child in spreadTransform)
            {
                shootDirections.Add(child);
            }
            tempShootDirections = shootDirections;

            muzzleFlashEffect = GetComponentInChildren<VisualEffect>();
            muzzlePointLight = muzzleFlashEffect.transform.parent.GetComponentInChildren<Light>();
            startLightIntensity = muzzlePointLight.intensity;
            muzzlePointLight.intensity = 0;
            impactBloodParticles = GameManager.Instance.bloodParticlePrefab;
        }

        // Update is called once per frame
        void Update()
        {

            Laser();
            if (fireRate > 0)
                fireRate -= Time.deltaTime;

            if (IsOwner)
            {
                if (currentSpread > minimumSpread)
                {
                    if (currentSpread > maximumSpread)
                        currentSpread = maximumSpread;
                    currentSpread -= Time.deltaTime * spreadNormalizationSpeed;
                    spreadTransform.localScale = new Vector3(currentSpread,
                                                            currentSpread,
                                                            spreadTransform.localScale.z);
                }
            }
        }

        public void AimAt(Transform aimTowards)
        {
            projectileSpawnLocation.LookAt(aimTowards);
        }

        public void Shoot()
        {

            if (fireRate > 0)
            {
                return;
            }
            if (curentMagazineAmmoCount <= 0)
            {
                Debug.Log("No ammo sound");
                fireRate = 0.4f;
                SpawnAudio("NoAmmo");
                if (IsHost)
                {
                    SpawnAudioClientRpc("NoAmmo");
                }
                else
                {
                    SpawnAudioServerRpc("NoAmmo");
                }
                return;
            }
            currentSpread += spreadAddPerShot;

            curentMagazineAmmoCount--;
            Debug.Log("Shoot");


            bool useAudio = true;


            for (int i = 0; i < numberOfProjectiles; i++)
            {
                int shootAtIndex = Random.Range(0, shootDirections.Count);
                ShootRaycast(true, shootAtIndex, useAudio);

                if (IsHost)
                    ShootSyncClientRpc(shootAtIndex, useAudio);
                else
                    ServerShootServerRpc(shootAtIndex, useAudio);


                useAudio = false;
            }


            fireRate = fireRateConstant;
        }

        void ShootRaycast(bool canDmg, int _shootAtIndex, bool _useAudio)
        {
            if (_useAudio)
            {
                SpawnAudio("");
                muzzleFlashEffect.Play();
                StopCoroutine(MuzzleLightFlash());
                StartCoroutine(MuzzleLightFlash());
            }

            RaycastHit hit;

            Vector3 direction = shootDirections[_shootAtIndex].position - projectileSpawnLocation.position;

            if (Physics.Raycast(projectileSpawnLocation.position, direction, out hit, Mathf.Infinity, hitLayers))
            {
                TrailRenderer trail = Instantiate(bulletTrail, projectileSpawnLocation.position, Quaternion.identity);
                if (canDmg)
                    StartCoroutine(SpawnTrailAndDMG(trail, hit));
                else
                    StartCoroutine(SpawnTrail(trail, hit));
                Debug.Log("Did Hit");
            }
        }

        //salje hostu da puca i puca host tj server
        [ServerRpc(RequireOwnership = false)]
        public void ServerShootServerRpc(int _shootAtIndex, bool _useAudio)
        {
            //Debug.DrawRay(projectileSpawnLocation.position, Camera.main.ScreenPointToRay (Input.mousePosition).direction * 100, Color.green,1f);
            ShootRaycast(false, _shootAtIndex, _useAudio);
            ShootSyncClientRpc(_shootAtIndex, _useAudio);

        }

        //Server salje ostalima da player puca
        [ClientRpc]
        public void ShootSyncClientRpc(int _shootAtIndex, bool _useAudio)
        {
            if (IsOwner) return;

            //Debug.DrawRay(projectileSpawnLocation.position, Camera.main.ScreenPointToRay (Input.mousePosition).direction * 100, Color.green,1f);
            ShootRaycast(false, _shootAtIndex, _useAudio);
        }


        public void SpawnAudio(string audioList)
        {
            switch (audioList)
            {
                case "Reload":
                    Instantiate(reloadAudioPrefabs[Random.Range(0, reloadAudioPrefabs.Count)], projectileSpawnLocation.position, Quaternion.identity, projectileSpawnLocation);
                    break;
                case "NoAmmo":
                    Instantiate(noAmmoAudioPrefabs[Random.Range(0, noAmmoAudioPrefabs.Count)], projectileSpawnLocation.position, Quaternion.identity, projectileSpawnLocation);
                    break;
                default:
                    Instantiate(shootAudioPrefabs[Random.Range(0, shootAudioPrefabs.Count)], projectileSpawnLocation.position, Quaternion.identity, projectileSpawnLocation);
                    break;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnAudioServerRpc(string playAudio)
        {
            SpawnAudio(playAudio);
            SpawnAudioClientRpc(playAudio);
        }

        [ClientRpc]
        public void SpawnAudioClientRpc(string playAudio)
        {
            if (IsOwner) return;

            SpawnAudio(playAudio);

        }

        private Vector3 GetDirection()
        {
            Vector3 direction = projectileSpawnLocation.forward;

            return direction;
        }

        IEnumerator SpawnTrail(TrailRenderer trail, RaycastHit hit)
        {
            float time = 0;
            Vector3 startPosition = trail.transform.position;
            while (time < 1)
            {
                trail.transform.position = Vector3.Lerp(startPosition, hit.point, time);
                time += Time.deltaTime / trail.time;
                yield return null;
            }
            trail.transform.position = hit.point;
            //kasnije osposobiti kada nađem neki article za to
            //Instantiate(impactParticles, hit.point, Quaternion.LookRotation(hit.normal));

            Destroy(trail.gameObject, trail.time);

            //spawns blood
            if (hit.transform.CompareTag("Zombie"))
            {
                Instantiate(impactBloodParticles, hit.point, Quaternion.LookRotation(-hit.normal));
            }
        }

        IEnumerator SpawnTrailAndDMG(TrailRenderer trail, RaycastHit hit)
        {
            float time = 0;
            Vector3 startPosition = trail.transform.position;
            while (time < 1)
            {
                trail.transform.position = Vector3.Lerp(startPosition, hit.point, time);
                time += Time.deltaTime / trail.time;
                yield return null;
            }
            trail.transform.position = hit.point;
            //instantiate impact particles
            //Instantiate(impactParticles, hit.point, Quaternion.LookRotation(hit.normal));

            Destroy(trail.gameObject, trail.time);

            try
            {
                hit.transform.GetComponent<IDamageable>().TakeDmg(dmg);

                //spawns blood
                if (hit.transform.CompareTag("Zombie"))
                {
                    Instantiate(impactBloodParticles, hit.point, Quaternion.LookRotation(-hit.normal));
                }
            }
            catch
            {
                try
                {
                    hit.transform.parent.root.GetComponent<IDamageable>().TakeDmg(dmg);
                }
                catch
                {
                    Debug.Log("Object does not contain any IDamagable interface");
                }
            }

            Debug.Log("tEST");
        }



        public void Reload()
        {
            int ammoNeededToReloadFully = maxMagazineCount - curentMagazineAmmoCount;

            if (ammoNeededToReloadFully > curentAmmoCount)
            {
                curentMagazineAmmoCount += curentAmmoCount;
                curentAmmoCount = 0;
            }
            else
            {
                curentMagazineAmmoCount = maxMagazineCount;
                curentAmmoCount -= ammoNeededToReloadFully;
            }
            Debug.Log("Reloaded");
        }

        public void ReplanishAmmo()
        {
            curentAmmoCount = maxAmmoCount;
            curentMagazineAmmoCount = maxMagazineCount;
        }

        public void Laser()
        {
            laser.SetPosition(0, projectileSpawnLocation.position);
            RaycastHit hit;
            if (Physics.Raycast(projectileSpawnLocation.position, projectileSpawnLocation.forward, out hit, Mathf.Infinity, hitLayers))
            {
                laser.SetPosition(1, hit.point);
            }
            else
            {
                laser.SetPosition(1, projectileSpawnLocation.forward * 100);
            }

        }

        IEnumerator MuzzleLightFlash()
        {

            float randomTime = Random.Range(0.1f, 0.2f);
            float elapsed = 0.0f;
            while (elapsed < randomTime)
            {
                muzzlePointLight.intensity = Mathf.Lerp(startLightIntensity, 0, elapsed / randomTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

        }

        public void DisableOnStart()
        {
            Invoke("Disable",0.1f);
        }
        public void Disable(){
            foreach (Transform child in transform)
            {
                if(child == transform)
                    continue;
                child.gameObject.SetActive(false);
            }
            laser.enabled = false;
        }
    }

}

