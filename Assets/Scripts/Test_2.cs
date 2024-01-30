using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.IO;
using System.IO.Enumeration;
using System;
using UnityEditor;
using UnityEngine.UI;
using Unity.VisualScripting.FullSerializer;

public class Test_2 : MonoBehaviour
{

    //based on Test_1 with ffmpeg process
    //evtl buffern von meheren bildern


    public Text timestamp;

    public Camera captureCamera;           // The camera you want to capture frames from
    public int frameWidth = 320;          // Width of the captured frames 1280
    public int frameHeight = 240;         // Height of the captured frames 720

    //posible copy of rendertexture
    RenderTexture rTex;

    private RenderTexture renderTexture;
    public int frameCount;

    public int targetFrameRate = 30;

    //bashscriptprocess
    Process process = new Process();

    void Start()
    {
        UnityEngine.Debug.Log("start method executed");

       // LoadConfig();

        //start the bashscript
        // Specify the input format as rawvideo and pixel format as rgba for the ffmpeg process instead of png

        initBashScript();
   //   initFFmpegProcess();

        //limit framerate https://discussions.unity.com/t/how-to-limit-fps/189237/3
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrameRate;





        // Set the camera's resolution
       captureCamera.targetTexture = renderTexture = new RenderTexture(frameWidth, frameHeight, 24);
      captureCamera.targetTexture.Create();

        // Start capturing frames
       StartCoroutine(CaptureFrames());
    }

    /*
    private void Update()
    {
        //dont forget to put on main camera the timestamp text component!
        //parse timestamp to current time
        //timestamp.text = DateTime.UtcNow.Ticks.ToString();

        DateTime currentUtcTime = DateTime.UtcNow;
        string formattedTimestamp = currentUtcTime.ToString("yyyy-MM-dd HH:mm:ss.fff");

        // Update the text component with the formatted timestamp
        timestamp.text = formattedTimestamp;

    }
    */
    IEnumerator CaptureFrames()
    {
        while (true)
        {
            // Wait until the end of the frame rendering
            yield return new WaitForEndOfFrame();

            UnityEngine.Debug.Log("WaitForEndOfFrame executed");
            // Render the frame to the RenderTexture
            captureCamera.Render();


            //show robot in gameview
            Graphics.Blit(renderTexture, rTex);

            // Save the RenderTexture to a file
            // string fileName = System.IO.Path.Combine(outputDirectory, $"frame_{frameCount:D4}.png");
            //SaveRenderTextureToFile(renderTexture, fileName);

            //Encode frame with ffmpeg
            EncodeFrame(renderTexture);

            // Increment frame count
           // frameCount++;
        }
    }

    void OnDestroy()
    {
        // Cleanup resources
        captureCamera.targetTexture = null;
        renderTexture.Release();


        //TODO close everything- whats missing ->
        process.StandardInput.BaseStream.Close(); // Close the input stream

    }

    private void EncodeFrame(RenderTexture rt)
    {

        UnityEngine.Debug.Log("encode frame to png and start stdin");



        byte[] frameData = captureImage(rt);

        UnityEngine.Debug.Log(frameData.Length + "");

        UnityEngine.Debug.Log(process.StandardInput.BaseStream.CanWrite + "can write");
        UnityEngine.Debug.Log("hallo nach can write");



        process.StandardInput.BaseStream.Write(frameData, 0, frameData.Length);
        process.StandardInput.BaseStream.Flush();


        UnityEngine.Debug.Log("after encoding frame");

    }


    private byte[] captureImage(RenderTexture rt)
    {  //texT2 very slow better rt input
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height);
        //ReadPixel slows process downhttps://forum.unity.com/threads/rendertexture-to-texture2d-too-slow.693850/#:~:text=Render%20textures%20have%20the%20distinction,Normal%20textures%20cannot%20do%20this.
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;        // Provide the capturedFrame's raw image data to FFmpeg

       // byte[] frameData = tex.GetRawTextureData();
       //byte[] frameData = tex.EncodeToPNG();
        byte[] frameData = tex.EncodeToJPG();

      //  UnityEngine.Debug.Log("format: " + tex.format);
       
        return frameData;
    }


    
    
    private void initFFmpegProcess()
    {

        UnityEngine.Debug.Log("init ffmpeg");




        //1 Create a new process to run the Bash script
        process.StartInfo.FileName = "C://ffmpeg//bin//ffmpeg.exe";
        //works
             process.StartInfo.Arguments = "-framerate " + targetFrameRate + " -f image2pipe -i pipe:0 -c:v libx264 -pix_fmt yuv420p -update 1 C://Users//NIK//Desktop//test_frame//a_output30fps.mp4";

        //2 works mit png data
        // process.StartInfo.Arguments = "-framerate 1 -f image2pipe -i pipe:0 -filter:v \"drawtext=text='%{gmtime}.%{gmtime \\: %3N}':x=(w-text_w)/2:y=9*(h-text_h)/10:fontsize=48:fontcolor=white:fontfile='/Windows/Fonts/Calibri.ttf':box=1:boxcolor=black:boxborderw=5\" -c:v libx264 -pix_fmt yuv420p -update 1 C://Users//NIK//Desktop//test_frame//outputxyz.mp4";
        //process.StartInfo.Arguments = "-framerate 1 -f image2pipe -i pipe:0  -c:v libx264 -pix_fmt yuv420p -flush_packets 1 C://Users//NIK//Desktop//test_frame//outputPNG.mp4";


        //3works
          //process.StartInfo.Arguments = "-hide_banner -framerate 1 -f image2pipe -i - -c:v libx264 -an -f mp4 -movflags cmaf+separate_moof+delay_moov+skip_trailer -frag_duration 1  C://Users//NIK//Desktop//test_frame//output_"+ frameCount + ".mp4";
        //turn of png encoding


        //turn on rawdata

        //4test mit flip
        //rgba -> Invalid buffer size, packet size 306624 < expected frame_size 307200 manchmal framedata kleiner und missing bytes werden vom nächsten genommen??
        //process.StartInfo.Arguments = "-f rawvideo -pix_fmt rgba  -video_size 320x240 -framerate 1 -i - -vf vflip -c:v libx264 -pix_fmt yuv420p -update 1 C://Users//NIK//Desktop//test_frame//outputRaw_2.mp4";


        //5process.StartInfo.Arguments = "-f rawvideo -pix_fmt rgba  -video_size 320x240 -framerate 1 -i - -vf vflip -c:v libx264 -pix_fmt yuv420p -update 1 C://Users//NIK//Desktop//test_frame//flushpoutputRaw_2.mp4";


        //6 png test
        //ohne yuv funkt nicht
        //  process.StartInfo.Arguments = "-y -f rawvideo -vcodec rawvideo -pixel_format rgba -colorspace bt709 -video_size 320x240 -framerate 1 -i pipe:0 -vf vflip -c:v libx264 -flush_packets 1 C://Users//NIK//Desktop//test_frame//6outputRaw_2.mp4";


        //update
        //  process.StartInfo.Arguments = "-f rawvideo -pixel_format rgba -video_size 1280x720 -framerate 1 -i pipe:0 -c:v libx264 C://Users//NIK//Desktop//test_frame//outputRaw_2.mp4";


       // process.StartInfo.Arguments = "-y -f rawvideo -vcodec rawvideo -pixel_format rgba -colorspace bt709 -video_size 320x240 -framerate 1 -i - -c:v libx264 -pix_fmt yuv420p -an -f mp4 -movflags cmaf+separate_moof+delay_moov+skip_trailer -frag_duration 1  C://Users//NIK//Desktop//test_frame//output_" + frameCount + ".mp4";



        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        // Define event handlers for output and error
           process.OutputDataReceived += (sender, args) => UnityEngine.Debug.Log(args.Data);
           process.ErrorDataReceived += (sender, args) => UnityEngine.Debug.LogError(args.Data);

        // Start the process
        process.Start();

        // Begin reading output and error asynchronously
           process.BeginOutputReadLine();
           process.BeginErrorReadLine();

    }
    


    
    private void initBashScript()
    {

        string configPath = Path.Combine(Application.dataPath, "StreamingAssets", "config.json");

        if (File.Exists(configPath))
        {
            string jsonText = File.ReadAllText(configPath);
            Config config = JsonUtility.FromJson<Config>(jsonText);

            // Now you can use config.bashScriptPath and config.bashExecutable in your application
            UnityEngine.Debug.Log($"Bash Script Path: {config.bashScriptPath}");
            UnityEngine.Debug.Log($"Bash Executable: {config.bashExecutable}");
       


        //  string bashScriptPath = "C://Users//NIK//Desktop//Uni//Masterarbeit//rust_pj//rust_windows//moq-rs//dev//pubCopy_2_unity";
        string bashScriptPathLocal = config.bashScriptPath;

        // Create a new process to run the Bash script

        //process.StartInfo.FileName = "C://Program Files//Git//bin//bash.exe";

        UnityEngine.Debug.Log("t: " + config.bashExecutable);
        UnityEngine.Debug.Log("a: " + bashScriptPathLocal);

        process.StartInfo.FileName = config.bashExecutable;
       
        process.StartInfo.Arguments = $"-c \"{bashScriptPathLocal}\"";

        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        // Define event handlers for output and error
        process.OutputDataReceived += (sender, args) => UnityEngine.Debug.Log(args.Data);
        process.ErrorDataReceived += (sender, args) => UnityEngine.Debug.LogError(args.Data);

        // Start the process
        process.Start();

        // Begin reading output and error asynchronously
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();


        }
        else
        {
            UnityEngine.Debug.LogError("Cannot find config.json");
        }

        UnityEngine.Debug.Log("start process");


    }

    private void LoadConfig()
    {
        string configPath = Path.Combine(Application.dataPath, "StreamingAssets", "config.json");

        if (File.Exists(configPath))
        {
            string jsonText = File.ReadAllText(configPath);
            Config config = JsonUtility.FromJson<Config>(jsonText);

            // Now you can use config.bashScriptPath and config.bashExecutable in your application
            UnityEngine.Debug.Log($"Bash Script Path: {config.bashScriptPath}");
            UnityEngine.Debug.Log($"Bash Executable: {config.bashExecutable}");
        }
        else
        {
            UnityEngine.Debug.LogError("Cannot find config.json");
        }
    }

}

[System.Serializable]
public class Config
{
    public string bashScriptPath;
    public string bashExecutable;
}
