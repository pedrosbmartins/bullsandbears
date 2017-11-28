using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerSFX : MonoBehaviour {

    public AudioSource HummingSoundSource;
    public AudioSource BootSoundSource;

    private bool isEnabled;

    private void Start() {
        isEnabled = GameData.GetSFXOn();

        if (isEnabled) {
            PlayBootSFX();
        }
    }

    private void Update() {
        if (isEnabled && !GameData.GetSFXOn()) {
            Stop();
        }
        else if (!isEnabled && GameData.GetSFXOn()) {
            Play();
        }
    }

    private void PlayBootSFX() {
        HummingSoundSource.Play();
        BootSoundSource.Play();
    }

    private void Play() {
        isEnabled = true;
        HummingSoundSource.Play();
    }
    private void Stop() {
        isEnabled = false;
        HummingSoundSource.Stop();
    }

}
