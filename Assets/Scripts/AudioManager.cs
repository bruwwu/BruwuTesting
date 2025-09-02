using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource SFXSource;

    [Header("AudioClips")]
    public AudioClip EActive;
    public AudioClip EOff;
    public AudioClip QFirstSlash;
    public AudioClip QBuildUpForTicks;
    public AudioClip QTicksBurst;

    public AudioClip RActive;
    public AudioClip RBuildUp;
    

    // Start is called before the first frame update
    public void PlaySFX(AudioClip audioClip)
    {
        SFXSource.PlayOneShot(audioClip);
    }
}
