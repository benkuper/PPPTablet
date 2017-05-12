using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class MediaPlayer : OSCControllable {

    VideoPlayer player;

    public static MediaPlayer instance;
    public static IMediaListener currentListener;
    public static string currentVideoID;

    private void Awake()
    {
        instance = this;
        player = GetComponent<VideoPlayer>();
        player.loopPointReached += loopPointReached;
        hide();
    }

    public static void play(string mediaPath, bool exclusive = false, IMediaListener listener = null, string videoId = "")
    {
        if(exclusive)
        {
            GameMaster.instance.setCurrentGame(null);
            ScoreManager.instance.hideScore();
        }

        if (mediaPath == "")
        {
            Debug.Log("File does not exist : " + mediaPath);
            return;
        }

        instance.player.url = mediaPath;
        instance.player.Play();
        instance.player.enabled = true;

        currentListener = listener;
        currentVideoID = videoId;

    }

    [OSCMethod("go")]
    public void playMedia(string mediaFile)
    {
        currentListener = null;
        currentVideoID = "";
        play(AssetManager.getMediaPath(mediaFile), true);
    }

    [OSCMethod("pause")]
    public void pause()
    {
        instance.player.Pause();
    }

    [OSCMethod("play")]
    public void resume()
    {
        instance.player.Play();
    }

    [OSCMethod("stop")]
    public void stop()
    {
        hide();
    }

    public static void hide()
    {
        instance.player.Stop();
        instance.player.enabled = false;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.M)) playMedia("test.mp4");
	}


    void loopPointReached(VideoPlayer source)
    {
        Debug.Log("Media player finish");
        stop();

        if (currentListener != null)
        {
            currentListener.mediaFinished(currentVideoID);
        }
       
    }

    public bool mediaIsPlaying()
    {
        return instance.player.enabled;
    }
}
