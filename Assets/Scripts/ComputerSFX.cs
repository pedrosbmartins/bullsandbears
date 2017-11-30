using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerSFX : MonoBehaviour {

    [SerializeField] private AudioSource hummingSoundSource;
    [SerializeField] private AudioSource bootSoundSource;

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
        hummingSoundSource.Play();
        bootSoundSource.Play();
    }

    private void Play() {
        isEnabled = true;
        hummingSoundSource.Play();
    }
    private void Stop() {
        isEnabled = false;
        hummingSoundSource.Stop();
    }

}
