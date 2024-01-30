using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class TimestampOverlay : MonoBehaviour
{
    // Start is called before the first frame update

    public TMP_Text timestamp;



    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //dont forget to put on main camera the timestamp text component!
        //parse timestamp to current time
        //timestamp.text = DateTime.UtcNow.Ticks.ToString();

        DateTime currentUtcTime = DateTime.UtcNow;
        string formattedTimestamp = currentUtcTime.ToString("yyyy-MM-dd HH:mm:ss.fff");

        // Update the text component with the formatted timestamp
        timestamp.text = formattedTimestamp;
    }

    public string GetCurrentTimestamp()
    {
        return timestamp.text;
    }

}
