using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private AudioSource audioSource;
    private LevelController levelController;

    private void Awake()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();
    }

    private IEnumerator PlaySoundCoroutine(AudioClip audio, Bool isPlayed, float volume = 0.3f)
    {
        audioSource.PlayOneShot(audio, volume);
        isPlayed.Value = true;
        yield return new WaitForSeconds(audio.length);
        isPlayed.Value = false;
    }

    public void StopAllAudio()
    {
        audioSource.Stop();
    }

    public void PlaySoundWithoutBlocking(AudioClip audio, Bool isPlayed, float volume = 0.3f)
    {
        StartCoroutine(PlaySoundCoroutine(audio, isPlayed, volume));
    }

    public Bool PlaySoundWithoutBlocking(AudioClip audio, float volume = 0.3f)
    {
        Bool isPlayed = new Bool();
        StartCoroutine(PlaySoundCoroutine(audio, isPlayed, volume));

        return isPlayed;
    }

    public class Bool
    {
        private bool value;

        public Bool() {}

        public Bool(bool value)
        {
            this.value = value;
        }

        public bool Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }
}
