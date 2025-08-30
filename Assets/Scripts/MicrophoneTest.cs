using UnityEngine;

public class MicrophoneTest : MonoBehaviour
{
    [SerializeField]
    AudioReader audioReader;
    [SerializeField]
    string microphoneName = "";
    [SerializeField]
    bool looping = true;
    [SerializeField]
    int length = 10;
    [SerializeField]
    int frequency = 44100;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < Microphone.devices.Length; i++)
        {
            Debug.Log("Microphone " + i + ": " + Microphone.devices[i]);
        }

        // Check if any microphone is available
        if (Microphone.devices.Length > 0)
        {
            // Start recording from the first microphone
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = Microphone.Start(microphoneName == "" ? Microphone.devices[0] : microphoneName, looping, length, frequency);
            audioSource.loop = true;
            audioSource.Play();

            audioReader?.SetAudioClip(audioSource.clip);
        }
        else
        {
            Debug.LogWarning("No microphone devices found.");
        }
    }
}
