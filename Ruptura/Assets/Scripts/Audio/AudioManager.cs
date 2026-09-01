using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    private EventInstance ambienceEventInstance;
    private EventInstance musicEventInstance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log(" foi encontrado mais de um Audio Manager na cena.");
            
        }   
        instance = this;
        
    }


    public void PlayOneShot(EventReference sound, Vector3 worldPosition)
    {
        RuntimeManager.PlayOneShot(sound, worldPosition);
    }

    public EventInstance CreateEventInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        return eventInstance;
    }

    private void Start()
    {
        InitializeAmbience(FMODEvent.instance.ambience);
        InitializeMusic(FMODEvent.instance.music);
    }



    private void InitializeAmbience(EventReference ambienceEvent)
    {
        ambienceEventInstance = CreateEventInstance(ambienceEvent);
        ambienceEventInstance.start();
    }

    private void InitializeMusic(EventReference musicEvent)
    {
        musicEventInstance = CreateEventInstance(musicEvent);
        musicEventInstance.start();
    }

    public void SetAmbienceParameter(string parameterName, float value)
    {
        ambienceEventInstance.setParameterByName(parameterName, value);
    }
    

}
