using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public Sound[] sounds;

    public static AudioManager instance;
    // Awake is start, maar dan wordt het voor Start uitgevoerd
    void Awake()
    {
        // controleer of er al een AudioManager is
        // zo ja, s e l f    d e s t r u c t
        if (instance == null) instance = this;
        else {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();

            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void Play(string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        // als name niet in de sound array voorkomt, valt er niks af te spelen, dus stoppen we de functie
        if (s == null) {
            Debug.LogWarning("Sound \"" + name + "\" doesn't exist!");
            return;
        }

        s.source.Play();
    }
}
