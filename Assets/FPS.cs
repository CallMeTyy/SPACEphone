using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPS : MonoBehaviour
{
    private Text _text;
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 90;
        _text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        _text.text = "FPS: " + (int)(1 / Time.deltaTime);
    }
}
