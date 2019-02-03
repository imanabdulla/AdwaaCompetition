using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class TextToSpeechCreatorWithAPI: MonoBehaviour
{
    public class AudioContent // class represents json object of output data  
    {
        public string audioContent = "";
    }
    public class Inputs // class represents json object of input data   
    {
        public AudioConfig audioConfig;
        public Input input;
        public Voice voice;
    }
    public class AudioConfig
    {
        string audioEncoding = "LINEAR16";
        [RangeAttribute(-20.00f, 20.00f)]
        public float pitch = 0.00f;
        [RangeAttribute(0.25f, 4.00f)]
        public float speakingRate = 1.00f;
    }
    public class Input
    {
        public string text;
    }
    public class Voice
    {
        string languageCode = "en-US";
        string name = "en-US-Wavenet-D";
    }

    public string url = "https://texttospeech.googleapis.com/v1/text:synthesize?key=";
    public string apiKey = "AIzaSyDNHvtjADoAqG_JCfTucyZIG-Fooyp2cVg";
    public TextMeshPro TextMesh;
    public string text;
    [RangeAttribute(-20.00f, 20.00f)]
    public float pitch = 0.00f;
    [RangeAttribute(0.25f, 4.00f)]
    public float speakingRate = 1.00f;

    private Dictionary<string, string> headers;

    private void Start()
    {
        // set headers
        headers = new Dictionary<string, string>();
        headers.Add("Content-Type", "application/json; charset=UTF-8");

        //check if api key not assigned
        if (apiKey == "")
            Debug.LogError("No API key. Please set your API key into the \"TextToSpeechCreatorWithAPI(Script)\" component.");
        
        // start text narration
        StartCoroutine(Narrate(text));
    }

    //read text
    private IEnumerator Narrate(string text)
    {
        if (this.apiKey == null)
            yield return null;
        
        // create jsonObject
        Inputs inputs = new Inputs();

        inputs.audioConfig = new AudioConfig();
        inputs.audioConfig.pitch = this.pitch;
        inputs.audioConfig.speakingRate = this.speakingRate;

        inputs.input = new Input();
        inputs.input.text = this.text;
        
        /*
         * el goz2 da has2al feh abuzueid ezay a7awel json string l json object?? 
         */
        //convert jsonObject to jsonString
        //string inputData = JsonUtility.ToJson(inputs);
        //print(inputData);
        string inputData = "{\"audioConfig\": {\"audioEncoding\": \"LINEAR16\",\"pitch\": \"0.00\",\"speakingRate\": \"1.00\"},\"input\": {\"text\": \""+text+"\"},\"voice\": {\"languageCode\": \"en-US\",\"name\": \"en-US-Wavenet-D\"}}";

        if (inputData != string.Empty)
        {

            //set url
            string url = this.url + this.apiKey;

            //set posteddData
            byte[] inputDataArray = System.Text.Encoding.Default.GetBytes(inputData);

            //retrieve postedData via WWW API request
            using (WWW outputData = new WWW(url, inputDataArray, headers))
            {
                yield return outputData;

                if (string.IsNullOrEmpty(outputData.error))
                {
                    // get voice file from url request results 
                    StartCoroutine(GetAudioFile(outputData));
                }
                else
                {
                    Debug.Log("Error: " + outputData.error);
                }
            }
        }
    }

    private IEnumerator GetAudioFile(WWW outputData)
    {
        //get audio content (synthesis data)
        AudioContent aC = JsonUtility.FromJson<AudioContent>(outputData.text);
        string audioContent = aC.audioContent;

        //create audio file
        using (Stream audioStream = File.Create("Assets/Resources/AudioFile.mp3"))
        {
            yield return audioStream;

            //fill audio file with audio content (synthesis data)
            byte[] audioContentArray = System.Convert.FromBase64String(audioContent);
            BinaryWriter binaryWriter = new BinaryWriter(audioStream);
            binaryWriter.Write(audioContentArray, 0, audioContentArray.Length);

            // appear text on screen and play audio file
            StartCoroutine(ShowTextAndPlayAudioFile());
        }
       // yield return null;
    }

    private IEnumerator ShowTextAndPlayAudioFile()
    {
        //load audio file
        AudioClip clip = Resources.Load<AudioClip>("AudioFile");
        yield return clip;

        if (clip != null)
        {
            //play audio file
            TextMesh.GetComponent<AudioSource>().clip = clip;
            TextMesh.GetComponent<AudioSource>().Play();

            //show text on screen
            TextMesh.text = text;
        }
        else
        {
            Debug.Log("Error: AudioFile.mp3 is Not Loaded");
        }

    }
}
