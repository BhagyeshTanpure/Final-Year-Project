using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class spawnplayers : MonoBehaviour
{
    public static spawnplayers instance;

    public GameObject player1;
    private GameObject cplayer;
    public GameObject DeathEffect;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        if(PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
        
       
    }
    public void SpawnPlayer()
    {

        Transform spawnPoint = spawnManager.instance.GetSpawnPoint();

        cplayer = PhotonNetwork.Instantiate(player1.name, spawnPoint.position, spawnPoint.rotation);
       

    }


}

