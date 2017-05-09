using UnityEngine;

public interface ITextureReceiver
{
    void textureReady(string textureID, Texture2D tex);
}
