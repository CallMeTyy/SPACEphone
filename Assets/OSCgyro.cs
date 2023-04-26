using System.Collections;
using System.Collections.Generic;
using extOSC;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OSCgyro : MonoBehaviour
{
    private OSCTransmitter _transmitter;
    private OSCReceiver _receiver;
    private GlobalData _data;

    [SerializeField] private GameObject[] _borders;

    private string IDadress = "";
    private int ID = -1;

    private bool accepted;

    [SerializeField] private GameObject button;

    // Start is called before the first frame update
    void Start()
    {
        Input.gyro.enabled = false;
        Input.gyro.updateInterval = 0.005f;
        _data = GameObject.FindWithTag("data")?.GetComponent<GlobalData>();

        if (_data != null)
        {
            _transmitter = gameObject.AddComponent<OSCTransmitter>();
            _transmitter.RemoteHost = _data.IP;
            _transmitter.RemotePort = 7204;
            _receiver = gameObject.AddComponent<OSCReceiver>();
            _receiver.LocalPort = _data.port;
        
            IDadress = _data.IDAdress;
            ID = _data.ID;
        }

        foreach (GameObject _b in _borders)
        {
            if (_b.name.Contains(ID.ToString()))
            {
                _b.SetActive(true);
            }
        }
        
        _receiver.Bind("/scene", SceneChange);
    }

    public void StartSending()
    {
        accepted = true;
        Input.gyro.enabled = true;
        button.GetComponent<Image>().color = Color.green;
    }
    void SceneChange(OSCMessage message)
    {
        SceneManager.LoadScene(3);
    }
    
    void SendGyro()
    {
        if (Input.gyro.enabled)
        {
            OSCMessage message = new OSCMessage(IDadress + "/gyro");
            Gyroscope gyro = Input.gyro;
            message.AddValue(OSCValue.Int(ID));
            //message.AddValue(OSCValue.Float(gyro.rotationRate.z));
            message.AddValue(OSCValue.Float(gyro.attitude.x));
            message.AddValue(OSCValue.Float(gyro.attitude.y));
            message.AddValue(OSCValue.Float(gyro.attitude.z));
            message.AddValue(OSCValue.Float(gyro.attitude.w));
            _transmitter.Send(message);
        }
    }
    public void GoBack()
    {
        if (_data.goBack) SceneManager.LoadScene(1);
        else _data.goBack = true;
    }
    

    // Update is called once per frame
    void Update()
    {
        if (accepted) SendGyro();
    }
}
