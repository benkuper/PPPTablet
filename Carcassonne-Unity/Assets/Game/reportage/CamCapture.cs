using UnityEngine;
using System.Collections;
using System.IO;
using System;

[RequireComponent(typeof(Camera))]
public class CamCapture : MonoBehaviour
{
    public bool enableCapture = true;

    public RenderTexture rt;
    public int videoWidth = 720;
    public int videoHeight = 1094;
    public int videoFrameRate = 15;
    public int videoBitRate = 3000;

    public string videoDir;
    public string fileName = "game_capturing-";

    private IntPtr capturingObject = IntPtr.Zero;
    private float startTime = 0.0f;
    private float nextCaptureTime = 0.0f;
    public bool isRunning { get; private set; }

    private AndroidJavaObject playerActivityContext = null;

    private static IntPtr constructorMethodID = IntPtr.Zero;
    private static IntPtr initCapturingMethodID = IntPtr.Zero;
    private static IntPtr startCapturingMethodID = IntPtr.Zero;
    private static IntPtr captureFrameMethodID = IntPtr.Zero;
    private static IntPtr stopCapturingMethodID = IntPtr.Zero;

    private static IntPtr getDirectoryDCIMMethodID = IntPtr.Zero;

    GameObject plane;

    private void Awake()
    {
        plane = transform.Find("Plane").gameObject;
        plane.SetActive(false);

        rt = new RenderTexture(videoWidth, videoHeight, 1);
        GetComponent<Camera>().targetTexture = rt;
    }
    void Start()
    {
        if (!Application.isEditor && enableCapture)
        {
            // First, obtain the current activity context
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                playerActivityContext = jc.GetStatic<AndroidJavaObject>("currentActivity");
            }

            // Search for our class
            IntPtr classID = AndroidJNI.FindClass("com/ppp/tablet/Capturing"); // com.intel.inde.mp.samples.unity // com/intel/penelope/Capturing

            // Search for it's contructor
            constructorMethodID = AndroidJNI.GetMethodID(classID, "<init>", "(Landroid/content/Context;II)V");
            if (constructorMethodID == IntPtr.Zero)
            {
                Debug.LogError("Can't find Capturing constructor.");
                return;
            }

            // Register our methods
            initCapturingMethodID = AndroidJNI.GetMethodID(classID, "initCapturing", "(IIII)V");
            if (initCapturingMethodID == IntPtr.Zero)
            {
                Debug.LogError("Can't find initCapturing() method.");
                return;
            }
            startCapturingMethodID = AndroidJNI.GetMethodID(classID, "startCapturing", "(Ljava/lang/String;)V");
            if (startCapturingMethodID == IntPtr.Zero)
            {
                Debug.LogError("Can't find startCapturing() method.");
                return;
            }
            captureFrameMethodID = AndroidJNI.GetMethodID(classID, "captureFrame", "(I)V");
            if (captureFrameMethodID == IntPtr.Zero)
            {
                Debug.LogError("Can't find captureFrame() method.");
                return;
            }
            stopCapturingMethodID = AndroidJNI.GetMethodID(classID, "stopCapturing", "()V");
            if (stopCapturingMethodID == IntPtr.Zero)
            {
                Debug.LogError("Can't find stopCapturingMethodID() method."); 
                return;
            }

            // Register and call our static method
            getDirectoryDCIMMethodID = AndroidJNI.GetStaticMethodID(classID, "getDirectoryDCIM", "()Ljava/lang/String;");
            jvalue[] args = new jvalue[0];

            if(videoDir == "") videoDir = AndroidJNI.CallStaticStringMethod(classID, getDirectoryDCIMMethodID, args);

           
            // Create Capturing object
            jvalue[] constructorParameters = AndroidJNIHelper.CreateJNIArgArray(new object[] {
                playerActivityContext, Screen.width, Screen.height
            });
            IntPtr local_capturingObject = AndroidJNI.NewObject(classID, constructorMethodID, constructorParameters);
            if (local_capturingObject == IntPtr.Zero)
            {
                Debug.LogError("--- Can't create Capturing object.");
                return;
            }
            // Keep a global reference to it
            capturingObject = AndroidJNI.NewGlobalRef(local_capturingObject);
            AndroidJNI.DeleteLocalRef(local_capturingObject);

            AndroidJNI.DeleteLocalRef(classID);
        }

        isRunning = false;
        Debug.Log("--- videoFrameRate = " + videoFrameRate);
        Debug.Log("--- 1.0f / videoFrameRate = " + 1.0f / videoFrameRate);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        /*
        if (!enableCapture) return;

        Graphics.Blit(src, dest);
        if (isRunning)
        {
            float elapsedTime = Time.time - startTime;
            if (elapsedTime >= nextCaptureTime)
            {
                CaptureFrame(rt.GetNativeTexturePtr().ToInt32());// src.GetNativeTexturePtr().ToInt32());
                nextCaptureTime += 1.0f / videoFrameRate;
            }
        }
        */
    }

    void OnPostRender()
    {
        if (!enableCapture) return;

        if (isRunning)
        {
            float elapsedTime = Time.time - startTime;
            if (elapsedTime >= nextCaptureTime)
            {
                CaptureFrame(rt.GetNativeTexturePtr().ToInt32());// src.GetNativeTexturePtr().ToInt32());
                nextCaptureTime += 1.0f / videoFrameRate;
            }
        }
    }
    
    public void StartCapturing()
    {
        plane.SetActive(true);

        Debug.Log("Start capturing");

        if (capturingObject == IntPtr.Zero)
            return;

        if (!enableCapture) return;

        jvalue[] videoParameters = new jvalue[4];
        videoParameters[0].i = videoWidth;
        videoParameters[1].i = videoHeight;
        videoParameters[2].i = videoFrameRate;
        videoParameters[3].i = videoBitRate;
        AndroidJNI.CallVoidMethod(capturingObject, initCapturingMethodID, videoParameters);
        DateTime date = DateTime.Now;
        string fullFileName = fileName + date.ToString("ddMMyy-HHmmss.fff") + ".mp4";
        jvalue[] args = new jvalue[1];
        args[0].l = AndroidJNI.NewStringUTF(videoDir + fullFileName);
        AndroidJNI.CallVoidMethod(capturingObject, startCapturingMethodID, args);

        Debug.Log("CamCapture :: Capture to file : "+videoDir+fullFileName);

        startTime = Time.time;
        nextCaptureTime = 0.0f;
        isRunning = true;
    }

    private void CaptureFrame(int textureID)
    {
        if (capturingObject == IntPtr.Zero)
            return;

        if (!enableCapture) return;

        jvalue[] args = new jvalue[1];
        args[0].i = textureID;
        AndroidJNI.CallVoidMethod(capturingObject, captureFrameMethodID, args);
    }

    public void StopCapturing()
    {
        plane.SetActive(false);

        isRunning = false;


        if (capturingObject == IntPtr.Zero)
            return;

        if (!enableCapture) return;

        Debug.Log("CamCapture Stop Capturing");

        jvalue[] args = new jvalue[0];
        AndroidJNI.CallVoidMethod(capturingObject, stopCapturingMethodID, args);
    }
}