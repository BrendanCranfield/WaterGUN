using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    public AudioSource audio;

    private void OnTriggerEnter(Collider other)
    {
        audio.PlayOneShot(audio.clip);
    }

    void PlaySoundClip()
    {
        audio.PlayOneShot(audio.clip);
    }
}
