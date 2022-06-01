using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class startgame : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void start()
    {       
       SceneManager.LoadScene("loading");
    }
    public void exit()
    {
     
        Application.Quit();
    }

    public void mainmenu()
    {
        PhotonNetwork.LeaveRoom();
        //SceneManager.LoadScene("Mainmenu");


    }
}
