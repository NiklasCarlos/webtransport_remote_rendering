using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FrameCountDisplay : MonoBehaviour
{
    // Start is called before the first frame update


    public int frameCount = 0;
    public TMP_Text frameCountTmp;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       frameCountTmp.text = "" + frameCount++;
    }

    public int GetCurrentFrameCount()
    {
        return frameCount;
    }

}
