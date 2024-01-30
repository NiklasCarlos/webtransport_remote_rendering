using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataCollector : MonoBehaviour
{
    public FrameCountDisplay frameCountDisplay;
    public TimestampOverlay timestampOverlay;

    private List<string> dataEntries = new List<string>();

    void Start()
    {
        // Add header row
        dataEntries.Add("Frame Count,Start Time");
    }

    void Update()
    {
        Debug.LogError("test");
        string currentFrameCount = frameCountDisplay.GetCurrentFrameCount().ToString();
        string currentTimestamp = timestampOverlay.GetCurrentTimestamp();
        dataEntries.Add(currentFrameCount + "," + currentTimestamp);
    }

    void OnDestroy()
    {
        WriteDataToCSV();
    }

    private void WriteDataToCSV()
    {
        // Specify the relative path to the Assets folder
        string folderPath = "Assets/Results"; // Replace 'YourSubFolder' with your desired subfolder within Assets
        string fileName = "FrameData.csv";
        string filePath = Path.Combine(folderPath, fileName);

        // Ensure the directory exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        File.WriteAllLines(filePath, dataEntries);

    }
}
