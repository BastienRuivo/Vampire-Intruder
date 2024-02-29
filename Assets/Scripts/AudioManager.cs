
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    public AudioClip[] playlist;
    public AudioSource audioSource; 
    private int indexSong = 0;
    //public AudioMixerGroup soundEffectMixeur; 

    void Start()
    {
        if(playlist.Length == 0)
            return;
        
        System.Random rand = new System.Random();
        indexSong = rand.Next(0, playlist.Length);
        audioSource.clip = playlist[indexSong];
        audioSource.Play();
    }

    void Update()
    {
        if(!audioSource.isPlaying){
            playNext();
        } 
    }

    private void playNext(){
        if(playlist.Length == 0)
            return;
        indexSong = (indexSong + 1) % playlist.Length;
        audioSource.clip = playlist[indexSong];
        audioSource.Play();
    }

    public AudioSource playClip(AudioClip song, Vector3 pos, Transform parent = null){
        GameObject temp = new GameObject("TemAudio");
        temp.transform.position = pos;
        if(parent != null)
        {
            temp.transform.parent = parent;
        }
        AudioSource audioSource = temp.AddComponent<AudioSource>();
        audioSource.clip = song;
        //audioSource.outputAudioMixerGroup = soundEffectMixeur;
        audioSource.Play();
        Destroy(temp, song.length);
        return audioSource;
    }

}
