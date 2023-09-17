using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Unity.Netcode;

namespace Game
{
    public class PlayerGuns : NetworkBehaviour
    {
        [SerializeField] private List<Weapon> equipedWeapons;
        [SerializeField] private Weapon usingWeapon;
        [SerializeField] private List<GameObject> startWeapon;
        private Animator anim;
        public Transform aimTarget;
        private Rig rig;
        public Transform leftHand;
        public TwoBoneIKConstraint gunHand;
        public float scrollDirection;
        public int maxWeaponCarry = 2;
        [SerializeField] private Transform gunPosition;
        [SerializeField] private List<Weapon> allWeapons;

        [Header("Grenade options")]
        public GameObject throwablePrefab;
        public List<GameObject> throwablePrefabsList;
        public NetworkVariable<int> curentThrowableIndex =  new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
        public Transform throwPoint;

        [SerializeField] private int throwAmountGrenadeMax;
        [SerializeField] private int throwAmountFlareMax;
        [SerializeField] private float grenadeRefilTimerConstant;
        [SerializeField] private float flareRefilTimerConstant;

        
        private float grenadeRefilTimer;
        private float flareRefilTimer;
        private int throwAmountGrenadeCurrent;
        private int throwAmountFlareCurrent;


        //
        public NetworkVariable<float> net_rigWeight = new NetworkVariable<float>(1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        public override void OnNetworkSpawn()
        {
            rig = GetComponentInChildren<Rig>();
            anim = GetComponent<Animator>();
            net_rigWeight.OnValueChanged += (float oldValue, float newValue) =>
            {
                if (!IsOwner)
                    rig.weight = newValue;
            };
        }


        void Start()
        {
            if (!IsOwner)
                return;

            grenadeRefilTimer = grenadeRefilTimerConstant;
            flareRefilTimer = flareRefilTimerConstant;
            throwAmountGrenadeCurrent = throwAmountGrenadeMax;
            throwAmountFlareCurrent = throwAmountFlareMax;
            UpdatePlayerUIThrowable();
            ActivateApropriateUIElements();

            AudioListener.volume = PlayerPrefs.GetFloat("masterAudioVolume");
            MenuManager.Instance.ActivatePlayerUI();
            foreach (Weapon weapon in GetComponentsInChildren<Weapon>())
            {
                allWeapons.Add(weapon);
                foreach (GameObject weaponGO in startWeapon)
                    if (weapon.name == weaponGO.name)
                    {
                        equipedWeapons.Add(weapon);
                        break;
                    }
                weapon.isEnabled.Value = false;
            }
            usingWeapon = equipedWeapons[0];
            usingWeapon.isEnabled.Value = true;

            PlayerUIManager.Instance.UpdateWeaponIcon(usingWeapon.weaponIcon);
            PlayerUIManager.Instance.UpdateAmmoMagazineText(usingWeapon.curentMagazineAmmoCount, usingWeapon.maxMagazineCount);
            PlayerUIManager.Instance.UpdateAmmoRemainingText(usingWeapon.curentAmmoCount);


            SwitchWeight();
            Invoke("SetAimTarget", 1f);
        }

        // Update is called once per frame
        void Update()
        {
            if (!IsOwner)
                return;

            RefilGranades();

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {

                if (hit.point.y > transform.position.y + 1.5f)
                    aimTarget.position = new Vector3(hit.point.x, transform.position.y + 1.5f, hit.point.z);
                else
                    aimTarget.position = hit.point;
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                SelectNewGrenade();
            }

            //throw Granade
            if (Input.GetKeyDown(KeyCode.Mouse1)
                && !AnimationPlayingCheck(new string[] { "Reload", "SwitchWeapon", "New Gun" }, 1)
                && !AnimationPlayingCheck(new string[] { "Run", "Falling", "Landing" }, 0))
            {
                bool canThrow= false;
                if(throwablePrefabsList[curentThrowableIndex.Value].CompareTag("Flare")){
                    if(throwAmountFlareCurrent>0){
                        canThrow = true;
                        throwAmountFlareCurrent--;
                    }

                }

                if(throwablePrefabsList[curentThrowableIndex.Value].CompareTag("Grenade")){
                    if(throwAmountGrenadeCurrent>0){
                        canThrow = true;
                        throwAmountGrenadeCurrent--;
                    }
                }

                if(canThrow){
                    UpdatePlayerUIThrowable();
                    ThrowObjectServerRpc(curentThrowableIndex.Value);
                }
            }

            //shoot
            if (Input.GetKey(KeyCode.Mouse0)
                && usingWeapon.fireRate <= 0
                && !AnimationPlayingCheck(new string[] { "Reload", "SwitchWeapon", "New Gun" }, 1)
                && !AnimationPlayingCheck(new string[] { "Run", "Falling", "Landing" }, 0))
            {

                usingWeapon.Shoot();
                PlayerUIManager.Instance.UpdateAmmoMagazineText(usingWeapon.curentMagazineAmmoCount,
                                                                usingWeapon.maxMagazineCount);
            }

            //reload
            if (Input.GetKeyDown(KeyCode.R)
                && usingWeapon.curentMagazineAmmoCount<usingWeapon.maxMagazineCount
                && !AnimationPlayingCheck(new string[] { "SwitchWeapon", "New Gun" }, 1)
                && !AnimationPlayingCheck(new string[] { "Run", "Falling", "Landing" }, 0))
            {
                if(usingWeapon.curentAmmoCount !=0)
                    anim.Play("Reload", 1);
            }

            if (Input.mouseScrollDelta.y != 0
                && !AnimationPlayingCheck(new string[] { "Falling", "Landing" }, 0)
                && equipedWeapons.Count == 2)
            {
                scrollDirection = Input.mouseScrollDelta.y;
                //Debug.Log(scrollDirection);
                //Debug.Log(Input.mouseScrollDelta);
                anim.Play("SwitchWeapon", 1);
            }

            if (AnimationPlayingCheck(new string[] { "SwitchWeapon", "New Gun", "Reload" }, 1)
                || AnimationPlayingCheck(new string[] { "Falling", "Landing", "Run" }, 0))
            {
                if (rig.weight > 0.2)
                {
                    rig.weight -= Time.deltaTime * 10;
                    net_rigWeight.Value = rig.weight;
                }
                else
                {
                    rig.weight = 0;
                    net_rigWeight.Value = rig.weight;
                }
                //Debug.Log("Some animations playing " + rig.weight);
            }
            else
            {
                if (rig.weight < 0.8)
                {
                    rig.weight += Time.deltaTime * 5;
                    net_rigWeight.Value = rig.weight;
                }
                else
                {
                    rig.weight = 1;
                    net_rigWeight.Value = rig.weight;
                    SetAimTarget();
                }
                //Debug.Log("Some animations playing " + rig.weight);
            }
        }

        void RefilGranades(){
            if(throwAmountGrenadeCurrent<throwAmountGrenadeMax){
                grenadeRefilTimer-=Time.deltaTime;
                if(grenadeRefilTimer<0){
                    throwAmountGrenadeCurrent++;
                    grenadeRefilTimer = grenadeRefilTimerConstant;
                    UpdatePlayerUIThrowable();
                }
            }

            if(throwAmountFlareCurrent < throwAmountFlareMax){
                flareRefilTimer-=Time.deltaTime;
                if(flareRefilTimer<0){
                    throwAmountFlareCurrent++;
                    flareRefilTimer = flareRefilTimerConstant;
                    UpdatePlayerUIThrowable();
                }
            }
        }

        void SelectNewGrenade()
        {
            curentThrowableIndex.Value++;
            if (curentThrowableIndex.Value >= throwablePrefabsList.Count)
                curentThrowableIndex.Value = 0;

            ActivateApropriateUIElements();

        }

        void ActivateApropriateUIElements(){
            PlayerUIManager.Instance.ActivateThrowableImages(throwablePrefabsList[curentThrowableIndex.Value].tag);
        }

        bool AnimationPlayingCheck(string[] names, int state)
        {
            foreach (string name in names)
            {
                //Debug.Log(name);
                if (anim.GetCurrentAnimatorStateInfo(state).IsName(name))
                    return true;
            }
            return false;
        }


        public void Reload()
        {
            if (!IsOwner)
                return;
            usingWeapon.Reload();
            UpdatePlayerUI();
            Debug.Log("Reloaded");
        }

        public void SwitchWeapon()
        {
            if (!IsOwner)
                return;
            Debug.Log("WeaponSwitched");
            int curentWeaponIndex = equipedWeapons.IndexOf(usingWeapon);
            curentWeaponIndex++;
            usingWeapon.isEnabled.Value = false;
            //usingWeapon.gameObject.SetActive(false);
            if (curentWeaponIndex >= equipedWeapons.Count)
            {
                usingWeapon = equipedWeapons[0];
            }
            else
            {
                usingWeapon = equipedWeapons[curentWeaponIndex];
            }
            usingWeapon.isEnabled.Value = true;
            UpdatePlayerUI();
            //usingWeapon.gameObject.SetActive(true);

            Invoke("SwitchWeight", 0.2f);
            SetLeftHandTarget();

        }

        void SwitchWeight()
        {
            if (usingWeapon.CompareTag("Pistol"))
            {
                //Debug.Log("Using pistol");
                gunHand.weight = 1;
            }
            else
                gunHand.weight = 0;

            SetLeftHandTarget();
        }

        public void NewWeapon(Weapon newWeapon)
        {
            bool isBeingUsed = false;
            //ako postoji gun napuni metke
            foreach (Weapon weapon in equipedWeapons)
            {
                if (weapon.name == newWeapon.name)
                {
                    //if exists replanishAmmo
                    isBeingUsed = true;
                    weapon.ReplanishAmmo();
                    break;
                }
            }
            //ako ne postoji gun treba se aktivirati
            if (!isBeingUsed)
            {
                Debug.Log("Old weapon sould be disabled");
                if(maxWeaponCarry== equipedWeapons.Count)
                    equipedWeapons.Remove(usingWeapon);
                usingWeapon.isEnabled.Value = false;
                foreach (Weapon weapon in allWeapons)
                {
                    if (newWeapon.name == weapon.name)
                    {
                        usingWeapon.isEnabled.Value = false;
                        usingWeapon = weapon;
                        equipedWeapons.Add(weapon);
                        break;
                    }
                }
            }
            
            usingWeapon.isEnabled.Value = true;
            UpdatePlayerUI();
            Invoke("SwitchWeight", 0.2f);
            Debug.Log("New weapon added");
            newWeapon.GetComponentInParent<IDestroy>().DestroyObject();
        }


        void SetAimTarget()
        {
            usingWeapon.AimAt(aimTarget);
        }

        void SetLeftHandTarget()
        {
            leftHand.position = usingWeapon.leftHandPosition.position;
        }

        void UpdatePlayerUI()
        {
            PlayerUIManager.Instance.UpdateWeaponIcon(usingWeapon.weaponIcon);
            PlayerUIManager.Instance.UpdateAmmoMagazineText(usingWeapon.curentMagazineAmmoCount, usingWeapon.maxMagazineCount);
            PlayerUIManager.Instance.UpdateAmmoRemainingText(usingWeapon.curentAmmoCount);

        }

        void UpdatePlayerUIThrowable(){
            PlayerUIManager.Instance.UpdateThrowableText(throwAmountGrenadeCurrent, throwAmountFlareCurrent);
        }

        [ServerRpc]
        void ThrowObjectServerRpc(int index)
        {
            ThrowObject(index);
        }

        void ThrowObject(int index)
        {
            //targetPosition.z = throwPoint.position.z; // Set the z-coordinate of the target position to match the throw point

            Vector3 direction = aimTarget.position - throwPoint.position;

            GameObject grenade = Instantiate(throwablePrefabsList[index], throwPoint.position, Quaternion.identity);
            grenade.GetComponent<NetworkObject>().Spawn();
            Rigidbody rb = grenade.GetComponent<Rigidbody>();

            // Calculate the initial velocity to achieve the desired time of flight
            float horizontalDistance = new Vector3(direction.x, 0f, direction.z).magnitude;
            float verticalDistance = direction.y;

            // Calculate the time it would take to free-fall from throw point's height to target's height
            float freeFallTime = Mathf.Sqrt(2 * Mathf.Abs(verticalDistance) / Mathf.Abs(Physics.gravity.y));

            // Set the dynamic timeToReachTarget based on distance
            float timeToReachTarget = freeFallTime + horizontalDistance * 0.1f; // You can adjust the factor (0.1f) as needed

            Vector3 horizontalDirection = new Vector3(direction.x, 0f, direction.z).normalized;
            Vector3 initialVelocity = horizontalDirection * horizontalDistance / timeToReachTarget;
            initialVelocity.y = (verticalDistance + 0.5f * Mathf.Abs(Physics.gravity.y) * timeToReachTarget * timeToReachTarget) / timeToReachTarget;
            rb.velocity = initialVelocity;
        }

        public void PlayReloadSound(){
            usingWeapon.SpawnAudio("Reload");
        }

    }
}