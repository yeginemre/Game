using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundEffect
{
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
}

public class AudioManager : MonoBehaviour
{
    [Header("==========Audio Sources==========")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource SFXSource;

    [Header("==========SFX==========")]
    public SoundEffect hover;
    public SoundEffect selectWarrior;
    public SoundEffect slide;
    public SoundEffect tacticalView;
    public SoundEffect spawnWarrior;
    public SoundEffect spawnArcher;
    public SoundEffect swordHitBlood;
    public SoundEffect arrowHitBlood;
    public void PlaySFX(SoundEffect sound)
    {
        if (sound != null && sound.clip != null)
        {
            SFXSource.PlayOneShot(sound.clip, sound.volume);
        }
    }

    public void PlayStoppableSFX(SoundEffect sound){
        if (sound != null && sound.clip != null)
            {
                SFXSource.clip = sound.clip;
                SFXSource.Play();
            }
    }


    public void StopSFX(SoundEffect sound)
    {
        if (sound != null && sound.clip != null && SFXSource.clip == sound.clip)
        {
            SFXSource.Stop();
        }
    }
}
