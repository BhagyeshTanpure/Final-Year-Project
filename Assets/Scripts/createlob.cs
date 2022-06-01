using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
public class createlob : MonoBehaviourPunCallbacks
{
   
    public TMP_InputField create;
    public TMP_InputField join;
    // Start is called before the first frame update

    public void Createroom()
    {
       
        if(create.text.Length >=1 )
        {
            PhotonNetwork.CreateRoom(create.text);
        }
    }

    public void Joinroom()
    {
        PhotonNetwork.JoinRoom(join.text);

    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Map1");       

    }




}
