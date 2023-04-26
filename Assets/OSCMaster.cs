using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using extOSC;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OSCMaster : MonoBehaviour
{
    private OSCTransmitter _transmitter;

    private OSCReceiver _receiver;
    private string IDadress;
    private GlobalData _data;

    [SerializeField] private GameObject button;

    [SerializeField] private InputField _input;

    [SerializeField] private Slider _slider;

    [SerializeField] private Text _currentScene;
    
    [SerializeField] Image _bg;
    // Start is called before the first frame update

    [SerializeField] private Text port;
    
    //[SerializeField] private Text micText;
    private bool isConnected;
    private int ID;

    private string sceneName;

    private Color c1;
    private Color c2;
    private Color c3;
    private Color c4;





    void Start()
    {
        _data = GameObject.FindWithTag("data").GetComponent<GlobalData>();
        Input.gyro.enabled = true;
        Input.gyro.updateInterval = 0.005f;

        _transmitter = gameObject.AddComponent<OSCTransmitter>();

        // Set remote host address.
        _transmitter.RemoteHost = "192.168.178.60";

        // Set remote port;
        _transmitter.RemotePort = 7204;

        _receiver = gameObject.AddComponent<OSCReceiver>();

        // Set local port.
        _receiver.LocalPort = 6969;

        // Bind "MessageReceived" method to special address.
        _receiver.Bind("/init", Init);
        _receiver.Bind("/scene", SceneChange);

        c1 = new Color(255, 196, 31,255) / 255;
        c2 = new Color(249, 2, 254,255) / 255;
        c3 = new Color(10, 232, 200,255) / 255;
        c4 = new Color(13, 89, 251,255) / 255;

        if (_data.ID != -1)
        {
            ID = _data.ID;
            IDadress = _data.IDAdress;
            _transmitter.RemoteHost = _data.IP;
            _receiver.LocalPort = _data.port;
            port.text = "Port: " + _receiver.LocalPort;
            _currentScene.text = "SpaceHub";
            isConnected = true;
            //setColor(ID);
        }
    }

    public string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }

    void setColor(int ID)
    {
        switch (ID)
        {
            case 1:
                _bg.color = c1;
                break;
            case 2:
                _bg.color = c2;
                break;
            case 3:
                _bg.color = c3;
                break;
            case 4:
                _bg.color = c4;
                break;
            default:
                _bg.color = Color.gray;
                break;
        }
    }

    private void Update()
    {
        if (isConnected)
        {
            switch (sceneName)
            {
                case "MiniValve":
                    SendValve();
                    break;
                case "2V2Fight":
                    if (ID == 1 || ID == 3) SendSpace1();
                    else if (ID == 2 || ID == 4) SendSpace2();
                    SendShake();
                    break;
                case "Lazer":
                    SendLaser();
                    break;
                case "Fire":
                    SceneManager.LoadScene(2);
                    break;
                default:
                    break;
            }
        }
        
    }


    public void Connect()
    {
        if (_receiver.LocalPort == 6969)
        {
            if (_input.text != "") _transmitter.RemoteHost = _input.text;
            _currentScene.text = _input.text;
            OSCMessage message = new OSCMessage("/init");
            message.AddValue(OSCValue.String(GetLocalIPv4()));
            message.AddValue(OSCValue.String(SystemInfo.deviceName));
            _transmitter.Send(message);
        }
    }

    public void Disconnect()
    {
        OSCMessage newmessage = new OSCMessage(IDadress + "/disc");
        newmessage.AddValue(OSCValue.String(IDadress));
        _transmitter.Send(newmessage);
        _receiver.LocalPort = 6969;
        IDadress = "";
        isConnected = false;
        port.text = "Unconnected";
    }

    void Init(OSCMessage message)
    {
        IDadress = "/player/" + message.Values[0].StringValue;
        OSCMessage newmessage = new OSCMessage(IDadress);
        newmessage.AddValue(OSCValue.String("Hiya"));
        _transmitter.Send(newmessage);
        _receiver.LocalPort = 6960 + message.Values[1].IntValue;
        ID = message.Values[1].IntValue;
        port.text = "Current Port: " + _receiver.LocalPort.ToString();
        isConnected = true;
        _data.Save(IDadress, _transmitter.RemoteHost, ID, _receiver.LocalPort);
        SceneManager.LoadScene("Hub");
        //setColor(ID);
    }

    void SceneChange(OSCMessage message)
    {
        sceneName = message.Values[0].StringValue;
        _currentScene.text = message.Values[0].StringValue;
    }


    #region Sends

    void SendAccelerometer()
    {
        OSCMessage message = new OSCMessage(IDadress + "/accel");
        Vector3 accel = Input.acceleration;
        message.AddValue(OSCValue.Float(accel.x));
        message.AddValue(OSCValue.Float(accel.y));
        message.AddValue(OSCValue.Float(accel.z));
        _transmitter.Send(message);
    }

    void SendQuat()
    {
        if (Input.gyro.enabled)
        {
            OSCMessage message = new OSCMessage(IDadress + "/quat");
            Gyroscope gyro = Input.gyro;
            message.AddValue(OSCValue.Float(gyro.attitude.x));
            message.AddValue(OSCValue.Float(gyro.attitude.y));
            message.AddValue(OSCValue.Float(gyro.attitude.z));
            message.AddValue(OSCValue.Float(gyro.attitude.w));
            _transmitter.Send(message);
        }
        else
        {
            port.text = "No Gyro";
        }
    }

    void SendValve()
    {
        if (Input.gyro.enabled)
        {
            OSCMessage message = new OSCMessage(IDadress + "/valve");
            Gyroscope gyro = Input.gyro;
            message.AddValue(OSCValue.Int(ID));
            message.AddValue(OSCValue.Float(gyro.rotationRate.z));
            //message.AddValue(OSCValue.Float(gyro.attitude.x));
            //message.AddValue(OSCValue.Float(gyro.attitude.y));
            //message.AddValue(OSCValue.Float(gyro.attitude.z));
            //message.AddValue(OSCValue.Float(gyro.attitude.w));
            _transmitter.Send(message);
        }
    }

    void SendLaser()
    {
        OSCMessage message = new OSCMessage(IDadress + "/laser");
        Vector3 accel = Input.acceleration;
        message.AddValue(OSCValue.Int(ID));
        message.AddValue(OSCValue.Float(accel.x));
        message.AddValue(OSCValue.Float(accel.y));
        message.AddValue(OSCValue.Float(accel.z));
        _transmitter.Send(message);
    }

    void SendSpace1()
    {
        if (Input.gyro.enabled)
        {
            OSCMessage message = new OSCMessage(IDadress + "/space1");
            Gyroscope gyro = Input.gyro;
            message.AddValue(OSCValue.Int(ID));
            message.AddValue(OSCValue.Float(gyro.attitude.x));
            message.AddValue(OSCValue.Float(gyro.attitude.y));
            message.AddValue(OSCValue.Float(gyro.attitude.z));
            message.AddValue(OSCValue.Float(gyro.attitude.w));
            _transmitter.Send(message);
        }
    }

    void SendSpace2()
    {
        OSCMessage message = new OSCMessage(IDadress + "/space2");
        message.AddValue(OSCValue.Int(ID));
        message.AddValue(OSCValue.Float(_slider.value));
        _transmitter.Send(message);
    }

    void SendShake()
    {
        OSCMessage message = new OSCMessage(IDadress + "/spaceshake");
        Vector3 accel = Input.acceleration;
        message.AddValue(OSCValue.Int(ID));
        message.AddValue(OSCValue.Float(accel.x));
        message.AddValue(OSCValue.Float(accel.y));
        message.AddValue(OSCValue.Float(accel.z));
        _transmitter.Send(message);
    }

    

    #endregion
}