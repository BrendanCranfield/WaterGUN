using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    AudioSource source;
    [SerializeField] AudioClip carPass;

    private void Start() {
        source = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        source.PlayOneShot(carPass);
    }

    void PlaySoundClip()
    {
        source.PlayOneShot(carPass);
    }
}
