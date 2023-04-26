using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;
using extOSC.Examples;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    private OSCTransmitter _transmitter;
    private OSCReceiver _receiver;
    private string sceneName;
    private string IDadress;
    private GlobalData _data;
    private int ID = -1;
    [SerializeField] private GameObject[] _borders;
    [SerializeField] private Image _icon;
    [SerializeField] private Sprite[] _sprites;
    [SerializeField] private Text gameText;
    [SerializeField] private InputField _input;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _data = GameObject.FindWithTag("data").GetComponent<GlobalData>();
        Input.gyro.enabled = true;
        Input.gyro.updateInterval = 0.005f;
        _transmitter = gameObject.AddComponent<OSCTransmitter>();

        // Set remote host address.
        _transmitter.RemoteHost = "192.168.56.1";

        // Set remote port;
        _transmitter.RemotePort = 7204;

        _receiver = gameObject.AddComponent<OSCReceiver>();

        // Set local port.
        _receiver.LocalPort = 6969;

        // Bind "MessageReceived" method to special address.
        _receiver.Bind("/scene", SceneChange);
        

        if (_data.ID != -1)
        {
            ID = _data.ID;
            IDadress = _data.IDAdress;
            _transmitter.RemoteHost = _data.IP;
            _receiver.LocalPort = _data.port;
        }
        
        foreach (GameObject _b in _borders)
        {
            if (_b.name.Contains(ID.ToString()))
            {
                _b.SetActive(true);
            }
        }
    }

    
    
    void SceneChange(OSCMessage message)
    {
        sceneName = message.Values[0].StringValue;
        _icon.color = Color.white;
        _input.gameObject.SetActive(false);
        switch (sceneName)
        {
            case "MiniValve":
                _icon.sprite = _sprites[0];
                gameText.text = "Rotate your Phone!";
                break;
            case "Lazer":
                _icon.sprite = _sprites[1];
                gameText.text = "Shake your Phone!";
                break;
            case "Fire":
                _icon.sprite = _sprites[2];
                gameText.text = "Blow out the Fire!";
                break;
            case "Window":
                _icon.sprite = _sprites[3];
                gameText.text = "Clean the window!";
                break;
            case "Volcano":
                _icon.sprite = _sprites[0];
                gameText.text = "Balance your phone and avoid falling in lava!!";
                break;
            case "Win":
                SceneManager.LoadScene(1);
                break;
            case "WaterPlanet":
                _icon.sprite = _sprites[1];
                gameText.text = "Shake your Phone to Jump!";
                break;
            default:
                _icon.color = new Color(0, 0, 0, 0);
                gameText.text = "";
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (sceneName)
        {
            case "MiniValve":
                SendValve();
                break;
            case "2V2Fight":
                //if (ID == 1 || ID == 3) SendSpace1();
                //else if (ID == 2 || ID == 4) SendSpace2();
                //SendShake();
                break;
            case "Lazer":
                SendLaser();
                break;
            case "Fire":
                SceneManager.LoadScene(2);
                break;
            case "Window":
                SendWindow();
                break;
            case "Volcano":
                SceneManager.LoadScene(4);
                break;
            case "WaterPlanet":
                Vector3 accel = Input.acceleration;
                if (accel.magnitude > 1.4f)SendWater();
                break;
            default:
                break;
        }
    }
    
    public float Map(float x, float in_min, float in_max, float out_min, float out_max, bool clamp = false)
    {
        if (clamp) x = Mathf.Max(in_min, Mathf.Min(x, in_max));
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
    
    public void SendName()
    {
        OSCMessage message = new OSCMessage("/name");
        Vector3 accel = Input.acceleration;
        message.AddValue(OSCValue.Int(ID));
        message.AddValue(OSCValue.String(_input.text));
        _transmitter.Send(message);
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
    
    void SendWater()
    {
        OSCMessage message = new OSCMessage(IDadress + "/water");
        Vector3 accel = Input.acceleration;
        message.AddValue(OSCValue.Int(ID));
        message.AddValue(OSCValue.Float(accel.x));
        message.AddValue(OSCValue.Float(accel.y));
        message.AddValue(OSCValue.Float(accel.z));
        _transmitter.Send(message);
    }
    
    void SendWindow()
    {
        OSCMessage message = new OSCMessage(IDadress + "/touch");
        float x = Map(Input.GetTouch(0).position.x, 0, Screen.width, 0, 1);
        float y = Map(Input.GetTouch(0).position.y, 0, Screen.height, 0, 1);
        message.AddValue(OSCValue.Int(ID));
        message.AddValue(OSCValue.Float(x));
        message.AddValue(OSCValue.Float(y));
        _transmitter.Send(message);
    }

    public void GoBack()
    {
        if (_data.goBack) SceneManager.LoadScene(1);
        else _data.goBack = true;
    }
}
