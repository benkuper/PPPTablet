using UnityEngine;

public interface IAudioReceiver
{
    void audioReady(string audioID, AudioClip clip);
}
