using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;
using System.IO;

// --- Minimal class for sending the request ---
[System.Serializable]
public class MinimalRequest
{
    public string audioData;
    public string inputLanguage;
    public string targetLanguage;
}

// --- Minimal class for receiving the response ---
[System.Serializable]
public class MinimalResponse
{
    public string audioReply; // We only care about the audio reply for this minimal version
}

public class SimpleBackendConnector : MonoBehaviour
{
    [Header("Backend Settings")]
    [Tooltip("The full URL of your backend endpoint")]
    public string backendUrl = "https://d35d111888cc.ngrok-free.app/api/converse";
    
    [Header("Components")]
    [Tooltip("AudioSource to play the AI's spoken response")]
    public AudioSource replyAudioSource;

    /// <summary>
    /// Public method to start the process of sending audio to the backend.
    /// </summary>
    public void SendAudioToBackend(AudioClip clip)
    {
        StartCoroutine(SendRequestCoroutine(clip));
    }

    private IEnumerator SendRequestCoroutine(AudioClip clip)
    {
        Debug.Log("Converting audio to WAV format...");
        byte[] wavData = WavUtility.FromAudioClip(clip);
        string base64Audio = Convert.ToBase64String(wavData);

        // Create the simple JSON payload
        MinimalRequest payload = new MinimalRequest
        {
            audioData = base64Audio,
            inputLanguage = "en", // Hardcoded for simplicity
            targetLanguage = "de" // Hardcoded for simplicity
        };
        string jsonPayload = JsonUtility.ToJson(payload);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(backendUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 25; // 25-second timeout

            Debug.Log("Sending audio data to backend...");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Successfully received response from backend.");
                MinimalResponse response = JsonUtility.FromJson<MinimalResponse>(request.downloadHandler.text);

                if (!string.IsNullOrEmpty(response.audioReply))
                {
                    // Convert the base64 audio reply back to an AudioClip and play it
                    byte[] audioBytes = Convert.FromBase64String(response.audioReply);
                    AudioClip replyClip = WavUtility.ToAudioClip(audioBytes);
                    
                    if (replyClip != null && replyAudioSource != null)
                    {
                        replyAudioSource.clip = replyClip;
                        replyAudioSource.Play();
                        Debug.Log("Playing AI audio reply.");
                    }
                }
            }
            else
            {
                Debug.LogError($"Error connecting to backend: {request.error}");
            }
        }
    }
}


/// <summary>
/// A utility class to handle conversion between Unity's AudioClip and WAV byte arrays.
/// This is placed in the same file for simplicity.
/// </summary>
public static class WavUtility
{
    public static byte[] FromAudioClip(AudioClip clip)
    {
        if (clip == null) { return null; }
        using (var memoryStream = new MemoryStream())
        {
            memoryStream.Write(new byte[44], 0, 44);
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);
            short[] intData = new short[samples.Length];
            byte[] bytesData = new byte[samples.Length * 2];
            float rescaleFactor = 32767;
            for (int i = 0; i < samples.Length; i++)
            {
                intData[i] = (short)(samples[i] * rescaleFactor);
                byte[] byteArr = BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }
            memoryStream.Write(bytesData, 0, bytesData.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);
            byte[] riff = Encoding.UTF8.GetBytes("RIFF"); memoryStream.Write(riff, 0, 4);
            byte[] chunkSize = BitConverter.GetBytes(memoryStream.Length - 8); memoryStream.Write(chunkSize, 0, 4);
            byte[] wave = Encoding.UTF8.GetBytes("WAVE"); memoryStream.Write(wave, 0, 4);
            byte[] fmt = Encoding.UTF8.GetBytes("fmt "); memoryStream.Write(fmt, 0, 4);
            byte[] subChunk1 = BitConverter.GetBytes(16); memoryStream.Write(subChunk1, 0, 4);
            byte[] audioFormat = BitConverter.GetBytes((ushort)1); memoryStream.Write(audioFormat, 0, 2);
            byte[] numChannels = BitConverter.GetBytes((ushort)clip.channels); memoryStream.Write(numChannels, 0, 2);
            byte[] sampleRate = BitConverter.GetBytes(clip.frequency); memoryStream.Write(sampleRate, 0, 4);
            byte[] byteRate = BitConverter.GetBytes(clip.frequency * clip.channels * 2); memoryStream.Write(byteRate, 0, 4);
            byte[] blockAlign = BitConverter.GetBytes((ushort)(clip.channels * 2)); memoryStream.Write(blockAlign, 0, 2);
            byte[] bitsPerSample = BitConverter.GetBytes((ushort)16); memoryStream.Write(bitsPerSample, 0, 2);
            byte[] dataString = Encoding.UTF8.GetBytes("data"); memoryStream.Write(dataString, 0, 4);
            byte[] subChunk2 = BitConverter.GetBytes(clip.samples * clip.channels * 2); memoryStream.Write(subChunk2, 0, 4);
            return memoryStream.ToArray();
        }
    }

    public static AudioClip ToAudioClip(byte[] fileBytes)
    {
        if (fileBytes == null || fileBytes.Length < 44) return null;
        int channels = BitConverter.ToInt16(fileBytes, 22);
        int frequency = BitConverter.ToInt32(fileBytes, 24);
        int sampleCount = (fileBytes.Length - 44) / 2 / channels;
        float[] samples = new float[sampleCount * channels];
        for (int i = 0; i < sampleCount * channels; i++)
        {
            short sample = BitConverter.ToInt16(fileBytes, 44 + i * 2);
            samples[i] = sample / 32767.0f;
        }
        AudioClip audioClip = AudioClip.Create("AI_Response", sampleCount, channels, frequency, false);
        audioClip.SetData(samples, 0);
        return audioClip;
    }
}