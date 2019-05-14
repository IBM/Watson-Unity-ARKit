using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IBM.Watson.Assistant.V2;
using IBM.Watson.Assistant.V2.Model;
using IBM.Watson.SpeechToText.V1;
using IBM.Watson.TextToSpeech.V1;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK.DataTypes;

public class WatsonLogic : MonoBehaviour
{
    #region PLEASE SET THESE VARIABLES IN THE INSPECTOR
    [Header("Watson Assistant")]
    [Tooltip("The service URL (optional). This defaults to \"https://gateway.watsonplatform.net/assistant/api\"")]
    [SerializeField]
    private string AssistantURL;
    [SerializeField]
    private string assistantId;
    [Tooltip("The apikey.")]
    [SerializeField]
    private string assistantIamApikey;

    [Header("Speech to Text")]
    [Tooltip("The service URL (optional). This defaults to \"https://stream.watsonplatform.net/speech-to-text/api\"")]
    [SerializeField]
    private string SpeechToTextURL;
    [Tooltip("The apikey.")]
    [SerializeField]
    private string SpeechToTextIamApikey;

    [Header("Text to Speech")]
    [SerializeField]
    [Tooltip("The service URL (optional). This defaults to \"https://stream.watsonplatform.net/text-to-speech/api\"")]
    private string TextToSpeechURL;
    [Tooltip("The apikey.")]
    [SerializeField]
    private string TextToSpeechIamApikey;

#endregion

    private int _recordingRoutine = 0;
    private string _microphoneID = null;
    private AudioClip _recording = null;
    private int _recordingBufferSize = 2;
    private int _recordingHZ = 22050;
    //private string initialMessage = "Hello";
    private AssistantService _assistant;
    private SpeechToTextService _speechToText;
    private TextToSpeechService _textToSpeech;
    private string sessionId;
    private bool firstMessage;
    private bool _stopListeningFlag = false;
    private bool sessionCreated = false;

    Animator animator;

    //Get your services up and running
    void Awake()
    {
        Runnable.Run(InitializeServices());
    }

    // Use this for initialization
    private void Start()
    {
        LogSystem.InstallDefaultReactors();
        animator = gameObject.GetComponent<Animator>();
    }

    private IEnumerator InitializeServices()
    {

        Credentials asst_credentials = null;
        TokenOptions asst_tokenOptions = new TokenOptions()
        {
            IamApiKey = assistantIamApikey,
        };

        asst_credentials = new Credentials(asst_tokenOptions, AssistantURL);

        while (!asst_credentials.HasIamTokenData())
            yield return null;

        _assistant = new AssistantService("2019-02-08", asst_credentials);

        _assistant.CreateSession(OnCreateSession, assistantId);

        while (!sessionCreated)
            yield return null;


        Credentials tts_credentials = null;
        TokenOptions tts_tokenOptions = new TokenOptions()
        {
            IamApiKey = TextToSpeechIamApikey
        };

        tts_credentials = new Credentials(tts_tokenOptions, TextToSpeechURL);

        while (!tts_credentials.HasIamTokenData())
            yield return null;

        _textToSpeech = new TextToSpeechService(tts_credentials);

        Credentials stt_credentials = null;
        TokenOptions stt_tokenOptions = new TokenOptions()
        {
            IamApiKey = SpeechToTextIamApikey
        };

        stt_credentials = new Credentials(stt_tokenOptions, SpeechToTextURL);

        while (!stt_credentials.HasIamTokenData())
            yield return null;

        _speechToText = new SpeechToTextService(stt_credentials);

        Active = true;

        // Send first message, create inputObj w/ no context
        Message0();

        StartRecording();   // Setup recording

    }

    //  Initiate a conversation
    private void Message0()
    {
        firstMessage = true;
        var input = new MessageInput()
        {
            Text = "Hello"
        };

        _assistant.Message(OnMessage, assistantId, sessionId, input);
    }


    private void OnMessage(DetailedResponse<MessageResponse> response, IBMError error)
    {
        if (!firstMessage)
        {
            //getIntent
            string intent = response.Result.Output.Intents[0].Intent;


            //Trigger the animation
            MakeAMove(intent);


            //get Watson Output
            string outputText2 = response.Result.Output.Generic[0].Text;

            
            CallTextToSpeech(outputText2);
        }

        firstMessage = false;

    }

    private void MakeAMove(string intent)
    {
        if (intent.ToLower() == "forward")
        {
            animator.SetBool("isIdle", false);
            animator.SetBool("isWalkingBackward", false);
            animator.SetBool("isWalkingForward", true);
        }
        else if (intent.ToLower() == "backward")
        {
            animator.SetBool("isIdle", false);
            animator.SetBool("isWalkingForward", false);
            animator.SetBool("isWalkingBackward", true);
        }
        else if (intent.ToLower() == "idle")
        {
            animator.SetBool("isIdle", true);
            animator.SetBool("isWalkingBackward", false);
            animator.SetBool("isWalkingForward", false);
        }
        else
        {
            animator.SetBool("isIdle", true);
            animator.SetBool("isWalkingBackward", false);
            animator.SetBool("isWalkingForward", false);
        }

    }

    private void BuildSpokenRequest(string spokenText)
    {
        var input = new MessageInput()
        {
            Text = spokenText
        };

        _assistant.Message(OnMessage, assistantId, sessionId, input);
    }

    private void CallTextToSpeech(string outputText)
    {
        Debug.Log("Sent to Watson Text To Speech: " + outputText);

        byte[] synthesizeResponse = null;
        AudioClip clip = null;

        _textToSpeech.Synthesize(
            callback: (DetailedResponse<byte[]> response, IBMError error) =>
            {
                synthesizeResponse = response.Result;
                clip = WaveFile.ParseWAV("myClip", synthesizeResponse);
                PlayClip(clip);

            },
            text: outputText,
            voice: "en-US_AllisonVoice",
            accept: "audio/wav"
        );
    }

    private void PlayClip(AudioClip clip)
    {
        Debug.Log("Received audio file from Watson Text To Speech");

        if (Application.isPlaying && clip != null)
        {
            GameObject audioObject = new GameObject("AudioObject");
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.spatialBlend = 0.0f;
            source.volume = 1.0f;
            source.loop = false;
            source.clip = clip;
            source.Play();

            Invoke("RecordAgain", source.clip.length);
            Destroy(audioObject, clip.length);
        }
    }

    private void RecordAgain()
    {
        Debug.Log("Played Audio received from Watson Text To Speech");
        if (!_stopListeningFlag)
        {
            OnListen();
        }
    }

    private void OnListen()
    {
        Log.Debug("AvatarPattern.OnListen", "Start();");

        Active = true;

        StartRecording();
    }

    public bool Active
    {
        get { return _speechToText.IsListening; }
        set
        {
            if (value && !_speechToText.IsListening)
            {
                _speechToText.DetectSilence = true;
                _speechToText.EnableWordConfidence = false;
                _speechToText.EnableTimestamps = false;
                _speechToText.SilenceThreshold = 0.03f;
                _speechToText.MaxAlternatives = 1;
                _speechToText.EnableInterimResults = true;
                _speechToText.OnError = OnError;
                _speechToText.StartListening(OnRecognize);
            }
            else if (!value && _speechToText.IsListening)
            {
                _speechToText.StopListening();
            }
        }
    }

    private void OnRecognize(SpeechRecognitionEvent result)
    {
        if (result != null && result.results.Length > 0)
        {
            foreach (var res in result.results)
            {
                foreach (var alt in res.alternatives)
                {
                    if (res.final && alt.confidence > 0)
                    {
                        StopRecording();
                        string text = alt.transcript;
                        Debug.Log("Watson hears : " + text + " Confidence: " + alt.confidence);
                        BuildSpokenRequest(text);
                    }
                }
            }
        }

    }


    private void StartRecording()
    {
        if (_recordingRoutine == 0)
        {
            Debug.Log("Started Recording");
            UnityObjectUtil.StartDestroyQueue();
            _recordingRoutine = Runnable.Run(RecordingHandler());
        }
    }

    private void StopRecording()
    {
        if (_recordingRoutine != 0)
        {
            Debug.Log("Stopped Recording");
            Microphone.End(_microphoneID);
            Runnable.Stop(_recordingRoutine);
            _recordingRoutine = 0;
        }
    }

    private void OnError(string error)
    {
        Active = false;

        Log.Debug("AvatarPatternError.OnError", "Error! {0}", error);
    }

    private void OnCreateSession(DetailedResponse<SessionResponse> response, IBMError error)
    {
        Log.Debug("AvatarPatternError.OnCreateSession()", "Session: {0}", response.Result.SessionId);
        sessionId = response.Result.SessionId;
        sessionCreated = true;
    }

    private IEnumerator RecordingHandler()
    {
        _recording = Microphone.Start(_microphoneID, true, _recordingBufferSize, _recordingHZ);
        yield return null;      // let m_RecordingRoutine get set..

        if (_recording == null)
        {
            StopRecording();
            yield break;
        }

        bool bFirstBlock = true;
        int midPoint = _recording.samples / 2;
        float[] samples = null;

        while (_recordingRoutine != 0 && _recording != null)
        {
            int writePos = Microphone.GetPosition(_microphoneID);
            if (writePos > _recording.samples || !Microphone.IsRecording(_microphoneID))
            {
                Log.Error("MicrophoneWidget", "Microphone disconnected.");

                StopRecording();
                yield break;
            }

            if ((bFirstBlock && writePos >= midPoint)
                || (!bFirstBlock && writePos < midPoint))
            {
                // front block is recorded, make a RecordClip and pass it onto our callback.
                samples = new float[midPoint];
                _recording.GetData(samples, bFirstBlock ? 0 : midPoint);

                AudioData record = new AudioData();
                record.MaxLevel = Mathf.Max(samples);
                record.Clip = AudioClip.Create("Recording", midPoint, _recording.channels, _recordingHZ, false);
                record.Clip.SetData(samples, 0);

                _speechToText.OnListen(record);

                bFirstBlock = !bFirstBlock;
            }
            else
            {
                // calculate the number of samples remaining until we ready for a block of audio,
                // and wait that amount of time it will take to record.
                int remaining = bFirstBlock ? (midPoint - writePos) : (_recording.samples - writePos);
                float timeRemaining = (float)remaining / (float)_recordingHZ;

                yield return new WaitForSeconds(timeRemaining);
            }

        }

        yield break;
    }
}