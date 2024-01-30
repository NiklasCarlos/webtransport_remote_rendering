using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.IO;
using System.IO.Enumeration;
using System;

public class Test_1 : MonoBehaviour
{

    //based on Test_1 with ffmpeg process


    public Camera captureCamera;           // The camera you want to capture frames from
    public int frameWidth = 1280;          // Width of the captured frames
    public int frameHeight = 720;         // Height of the captured frames
    public string outputDirectory = "Frames_1";  // Output directory for saved frames

    private RenderTexture renderTexture;
    private int frameCount;

    public int targetFrameRate = 1;

    void Start()
    {
        //limit framerate https://discussions.unity.com/t/how-to-limit-fps/189237/3
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrameRate;


       
        // Ensure the output directory exists
        System.IO.Directory.CreateDirectory(outputDirectory);

        // Set the camera's resolution
        captureCamera.targetTexture = renderTexture = new RenderTexture(frameWidth, frameHeight, 24);
        captureCamera.targetTexture.Create();

        // Start capturing frames
        StartCoroutine(CaptureFrames());
    }

    IEnumerator CaptureFrames()
    {
        while (true)
        {
            // Wait until the end of the frame rendering
            yield return new WaitForEndOfFrame();

            UnityEngine.Debug.Log("after render");
            // Render the frame to the RenderTexture
            captureCamera.Render();

            // Save the RenderTexture to a file
            // string fileName = System.IO.Path.Combine(outputDirectory, $"frame_{frameCount:D4}.png");
            //SaveRenderTextureToFile(renderTexture, fileName);

            //Encode frame with ffmpeg
            string fileName = System.IO.Path.Combine(outputDirectory, "frame_");
            EncodeFrame(renderTexture, fileName);

            // Increment frame count
            frameCount++;
        }
    }

    void OnDestroy()
    {
        // Cleanup resources
        captureCamera.targetTexture = null;
        renderTexture.Release();
    }

    private void EncodeFrame(RenderTexture rt, string outputVideoPath)
    {

        UnityEngine.Debug.Log("start new process");


        // Encode the captured frame with FFmpeg
        Process ffmpegProcess = new Process();
        ffmpegProcess.StartInfo.FileName = "C://ffmpeg//bin//ffmpeg.exe";

        // Specify the input format as rawvideo and pixel format as rgba
        // Set the resolution based on the capturedFrame dimensions
        //ffmpeg -i input.png -preset ultrafast output.jpg

        ffmpegProcess.StartInfo.Arguments = "-framerate 1 -f image2pipe -i - " + outputVideoPath + frameCount + ".jpeg";

        // Set other process properties as needed (e.g., redirecting output)
        // ...


        initFFmpegProcess(ffmpegProcess);



        ffmpegProcess.Start();

        byte[] frameData = captureImage(rt);
        ffmpegProcess.StandardInput.BaseStream.Write(frameData, 0, frameData.Length);
        ffmpegProcess.StandardInput.BaseStream.Close(); // Close the input stream

        ffmpegProcess.WaitForExit();

        UnityEngine.Debug.Log("after encoding frame");

    }

    private void initFFmpegProcess(Process ffmpegProcess)
    {
        ffmpegProcess.StartInfo.UseShellExecute = false;
        ffmpegProcess.StartInfo.CreateNoWindow = true;
        ffmpegProcess.StartInfo.RedirectStandardOutput = true;
        ffmpegProcess.StartInfo.RedirectStandardInput = true;
    }

    private byte[] captureImage(RenderTexture rt)
    {  //texT2 very slow better rt input
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height);
        //ReadPixel slows process downhttps://forum.unity.com/threads/rendertexture-to-texture2d-too-slow.693850/#:~:text=Render%20textures%20have%20the%20distinction,Normal%20textures%20cannot%20do%20this.
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;        // Provide the capturedFrame's raw image data to FFmpeg
        byte[] frameData = tex.EncodeToPNG();
        return frameData;
    }

}
