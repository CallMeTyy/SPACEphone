using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalData : MonoBehaviour
{
    public string IDAdress;
    public string IP;
    public int ID = -1;
    public int port;
    public bool goBack = false;
    private float time;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if (goBack)
        {
            time += Time.deltaTime;
            if (time > 0.5f)
            {
                time = 0;
                goBack = false;
            }
        }
    }

    public void Save(string IDA, string IPa, int IDn, int PortN)
    {
        IDAdress = IDA;
        IP = IPa;
        ID = IDn;
        port = PortN;
        
    }
}
