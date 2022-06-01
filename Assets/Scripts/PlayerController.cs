using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public static PlayerController instance;
  
    public Transform ViewPoint;
    public float MouseSensitivity = 1f;

    private float VerticalRotStore;
    private Vector2 MouseInput;

    public bool InvertLook;

    public float MoveSpeed = 5f, RunSpeed=8f;
    private float ActiveMoveSpeed;
    private Vector3 MoveDirection,Movement;
    public CharacterController CharCon;
    public Camera Cam;

    public float JumpForce = 12f, gravityMod = 2.5f;

    public Transform groundCheckPoint;
    private bool isGrounded;
    public LayerMask GroundLayers;

    public GameObject BulletImpact;

    public float MuzzleDisplayTime;
    private float MuzzleCounter;   

    private float ShotCounter;

    public float MaxHeat = 10f, CoolRate = 4f, OverHeatCoolRate = 5f;
    private float HeatCounter;
    private bool OverHeated;

    public Gun[] AllGuns;
    private int SelectedGun;

    public GameObject playerHitImpact;

    public int maxHealth = 100;
    private int currentHealth;

    public Animator anim;

    public GameObject playerModel;

    public Transform modelGunPoint, gunHolder;


    private void Awake()
    {
        instance = this;
    }

    void Start()
    {       
        Cursor.lockState = CursorLockMode.Locked;
   
        Cam = Camera.main;
        UIController.instance.WeaponTempSlider.maxValue = MaxHeat;

        //SwitchGun();
        photonView.RPC("SetGun", RpcTarget.All, SelectedGun);
        currentHealth = maxHealth;
       

        if(photonView.IsMine)
        {
            playerModel.SetActive(false);

            UIController.instance.healthSlider.maxValue = maxHealth;
            UIController.instance.healthSlider.value = currentHealth;
            UIController.instance.healthText.text = "HEALTH:" + currentHealth + "/" + maxHealth;
        }else
        {
            gunHolder.parent = modelGunPoint;
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            MouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * MouseSensitivity;
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + MouseInput.x, transform.rotation.eulerAngles.z);

            VerticalRotStore += MouseInput.y;
            VerticalRotStore = Mathf.Clamp(VerticalRotStore, -60f, 60f);

            if (InvertLook)
            {
                ViewPoint.rotation = Quaternion.Euler(VerticalRotStore, ViewPoint.rotation.eulerAngles.y, ViewPoint.rotation.eulerAngles.z);
            }
            else
            {
                ViewPoint.rotation = Quaternion.Euler(-VerticalRotStore, ViewPoint.rotation.eulerAngles.y, ViewPoint.rotation.eulerAngles.z);
            }

            MoveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

            if (Input.GetKey(KeyCode.LeftShift))
            {
                ActiveMoveSpeed = RunSpeed;
            }
            else
            {
                ActiveMoveSpeed = MoveSpeed;
            }
            float yvelocity = Movement.y;

            Movement = ((transform.forward * MoveDirection.z) + (transform.right * MoveDirection.x)).normalized * ActiveMoveSpeed;
            Movement.y = yvelocity;

            if (CharCon.isGrounded)
            {
                Movement.y = 0f;
            }
            isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, .25f, GroundLayers);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                Movement.y = JumpForce;
            }


            Movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;
            CharCon.Move(Movement * Time.deltaTime);



            if (AllGuns[SelectedGun].MuzzleFlash.activeInHierarchy)
            {
                MuzzleCounter -= Time.deltaTime;

                if (MuzzleCounter <= 0)
                {
                    AllGuns[SelectedGun].MuzzleFlash.SetActive(false);
                }
            }


            if (!OverHeated)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Shoot();
                }

                if (Input.GetMouseButton(0) && AllGuns[SelectedGun].IsAutomatic)
                {
                    ShotCounter -= Time.deltaTime;
                    if (ShotCounter <= 0)
                    {
                        Shoot();
                    }
                }
                HeatCounter -= CoolRate * Time.deltaTime;
            }
            else
            {
                HeatCounter -= OverHeatCoolRate * Time.deltaTime;
                if (HeatCounter <= 0)
                {
                    OverHeated = false;
                    UIController.instance.OverHeatedMessage.gameObject.SetActive(false);
                }
            }



            if (HeatCounter < 0)
            {
                HeatCounter = 0f;
            }
            UIController.instance.WeaponTempSlider.value = HeatCounter;

            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
            {
                SelectedGun++;
                if (SelectedGun >= AllGuns.Length)
                {
                    SelectedGun = 0;
                }
                photonView.RPC("SetGun", RpcTarget.All, SelectedGun);
                //SwitchGun();
            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
            {
                SelectedGun--;
                if (SelectedGun <= 0)
                {
                    SelectedGun = AllGuns.Length - 1;
                }
                photonView.RPC("SetGun", RpcTarget.All, SelectedGun);
                //SwitchGun();

            }

            anim.SetBool("grounded", isGrounded);
            anim.SetFloat("speed", MoveDirection.magnitude);


            if (Input.GetKey(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else if (Cursor.lockState == CursorLockMode.None)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }

        }
    }


    private void Shoot()
    {
        Ray ray = Cam.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        ray.origin = Cam.transform.position;

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject.tag == "Player")
            {
               
                PhotonNetwork.Instantiate(playerHitImpact.name, hit.point, Quaternion.identity);
                hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, AllGuns[SelectedGun].shotDamage);
             
               // view.RPC("DealDamage", RpcTarget.All /*,AllGuns[SelectedGun].shotDamage*/);
               
            }
            else
            {
                
                GameObject bulletImpactobject = Instantiate(BulletImpact, hit.point + (hit.normal * .002f), Quaternion.LookRotation(hit.normal, Vector3.up));

                Destroy(bulletImpactobject, 5f);
            }
        }

        ShotCounter = AllGuns[SelectedGun].TimeBetweenShots;


        HeatCounter += AllGuns[SelectedGun].HeatPerShot;
        if (HeatCounter >= MaxHeat)
        {
            HeatCounter = MaxHeat;
            OverHeated = true;
            UIController.instance.OverHeatedMessage.gameObject.SetActive(true);
        }
        AllGuns[SelectedGun].MuzzleFlash.SetActive(true);
        MuzzleCounter = MuzzleDisplayTime;


    }


    [PunRPC]

    public void DealDamage(int damageAmount)
    {
        if (photonView.IsMine)
        {
            currentHealth -= damageAmount;
            UIController.instance.healthSlider.value = currentHealth;
            UIController.instance.healthText.text = "HEALTH:" + currentHealth + "/" + maxHealth;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                SceneManager.LoadScene("gameover");
                PhotonNetwork.Destroy(photonView);

            }
            
        } 
    }
    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            Cam.transform.position = ViewPoint.position;
            Cam.transform.rotation = ViewPoint.rotation;
        }
    }


    void SwitchGun()
    {
        foreach(Gun gun in AllGuns )
        {
            gun.gameObject.SetActive(false);
        }
        AllGuns[SelectedGun].gameObject.SetActive(true);
        AllGuns[SelectedGun].MuzzleFlash.SetActive(false);
    }
    [PunRPC]
    public void SetGun(int gunToSwitchTo)
    {
        if(gunToSwitchTo < AllGuns.Length)
        {
            SelectedGun = gunToSwitchTo;
            SwitchGun();
        }
    }



}
