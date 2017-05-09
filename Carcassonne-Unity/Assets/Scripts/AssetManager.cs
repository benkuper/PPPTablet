using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

public class AssetManager {

    static string basePath;
    static bool isInit;
    
	// Use this for initialization
	public static void init () {
#if UNITY_EDITOR
        basePath = Directory.GetCurrentDirectory().Replace('\\','/') + "/../Sync/";
#else
        basePath = "/storage/emulated/0/PPP/"; //Android sync folder
        
#endif
        Debug.Log("Base Path :" + basePath + ": exists ? " + Directory.Exists(basePath));

        isInit = true;
	}

    public static string getGameConfig(string gameID)
    {
        if (!isInit) init();
        return getGameAssetAsText(gameID,"config.json");
    }

    public static string getGameAssetAsText(string gameID, string assetPath)
    {
        if (!isInit) init();
        StreamReader s = new StreamReader(basePath + "jeux/" + gameID + "/"+assetPath, Encoding.Unicode);
        return s.ReadToEnd();
    }
        
    public static IEnumerator loadGameTexture(string gameID, string assetPath, string texID, ITextureReceiver receiver)
    {
        if (!isInit) init();
        string path = "file:///" + basePath + "jeux/" + gameID + "/" + assetPath;
        //Debug.Log("Loading game texture : " + path);
        WWW www = new WWW(path);
        yield return www;
        receiver.textureReady(texID, www.texture);
    }
}
