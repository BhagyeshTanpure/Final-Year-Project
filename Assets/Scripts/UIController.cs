using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviourPunCallbacks
{
    public static UIController instance;

    public Slider healthSlider;
    public TMP_Text healthText;
    public TMP_Text Timer;
    public float time;

    private void Awake()
    {
        instance = this;

    }
    public TMP_Text OverHeatedMessage;
    public Slider WeaponTempSlider;

    void Start()
    {
        
    }
    void Update()
    {
        if (GameObject.FindGameObjectsWithTag("Player") != null)
        {
            time -= 1 * Time.deltaTime;
            Timer.text = ((int)time).ToString();
        }
        if (time <= 0)
        {
            SceneManager.LoadScene("gameover");
           
        }
    }
}
