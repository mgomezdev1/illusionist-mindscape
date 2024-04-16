using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeUIAgent : MonoBehaviour
{
    public void SetMasterVolume(float v)
    {
        SoundManager.Instance.SetChannelVolume(SoundManager.Channel.Master, v);
    }
}
