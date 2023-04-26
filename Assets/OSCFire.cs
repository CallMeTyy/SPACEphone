using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class OSCFire : MonoBehaviour
{
    private OSCTransmitter _transmitter;
    private OSCReceiver _receiver;
    private GlobalData _data;
    [SerializeField] private AudioSource mic;
    public float updateStep = 0.01f;
    public int sampleDataLength = 1024;
 
    private float currentUpdateTime = 0f;
    private float clipLoudness;
    private float[] clipSampleData;
    private float loudest;

    [SerializeField] private GameObject[] _borders;

    private string IDadress = "";
    private int ID = -1;

    [SerializeField] private Text micText;

    private bool startAudio = true;
    
    // Start is called before the first frame update
    void Start()
    {
        _data = GameObject.FindWithTag("data").GetComponent<GlobalData>();
        mic = GetComponent<AudioSource>();
        
        
        

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
    
    public void GoBack()
    {
        if (_data.goBack) SceneManager.LoadScene(1);
        else _data.goBack = true;
    }
    
    void SceneChange(OSCMessage message)
    {
        SceneManager.LoadScene(3);
    }
    
    void SendMic()
    {
        currentUpdateTime += Time.deltaTime;
        if (currentUpdateTime >= updateStep) {
            clipSampleData = new float[sampleDataLength];
            currentUpdateTime = 0f;
            mic.clip.GetData(clipSampleData, mic.timeSamples); //I read 1024 samples, which is about 80 ms on a 44khz stereo clip, beginning at the current sample position of the clip.
            clipLoudness = 0f;
            foreach (var sample in clipSampleData) {
                clipLoudness += Mathf.Abs(sample);
            }
            clipLoudness /= sampleDataLength; //clipLoudness is what you are looking for
            //micText.text = "MIC: " + clipLoudness;
            OSCMessage message = new OSCMessage(IDadress + "/mic");
            message.AddValue(OSCValue.Int(ID));
            message.AddValue(OSCValue.Float(clipLoudness));
            if (_data != null) _transmitter.Send(message);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (startAudio)
        {
            mic.clip = Microphone.Start(Microphone.devices[0], true, 60, 44100);
            mic.loop = true;
            while (!(Microphone.GetPosition(Microphone.devices[0]) > 0))
            {
            }

            mic.Play();
            startAudio = false;
        }

        if (mic.clip != null)
        {
            SendMic();
        }
        
    }
}
