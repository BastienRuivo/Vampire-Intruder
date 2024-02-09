
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioClip[] playlist;
    public AudioSource audioSource; 
    //private int indexSong = 0;
    //public AudioMixerGroup soundEffectMixeur; 

    public static AudioManager instance;

    private void Awake(){
        if(instance != null){
            Debug.LogWarning("Multiple AudioManager");
            return;
        }
        instance = this;
    }

    // void Start()
    // {
    //     audioSource.clip = playlist[0];
    //     audioSource.Play();
    // }

    
    // void Update()
    // {
    //     if(!audioSource.isPlaying){
    //         playNext();
    //     } 
    // }

    // private void playNext(){
    //     indexSong = (indexSong + 1) % playlist.Length;
    //     audioSource.clip = playlist[indexSong];
    //     audioSource.Play();
    // }

    public AudioSource playClip(AudioClip song, Vector3 pos){
        GameObject temp = new GameObject("TemAudio");
        temp.transform.position = pos;
        AudioSource audioSource = temp.AddComponent<AudioSource>();
        audioSource.clip = song;
        //audioSource.outputAudioMixerGroup = soundEffectMixeur;
        audioSource.Play();
        Destroy(temp, song.length);
        return audioSource;
    }
}
