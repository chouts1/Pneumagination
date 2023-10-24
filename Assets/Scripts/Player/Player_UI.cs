using SFB;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Player_UI : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] UIDocument document;
    [SerializeField] DEAD_Interface deadInterface;
    [SerializeField] FirstPersonController controller;

    [Header("Animation Curves")]
    [SerializeField] AnimationCurve hotkeyPress;
    [SerializeField] AnimationCurve hotkeyRelease;
    [SerializeField] AnimationCurve uiMove;

    [Header("Data")]
    [SerializeField] HotKeyIcons[] hotkeyIcons = new HotKeyIcons[10];

    //UI Objects
    VisualElement[] hotBarVisualElements = new VisualElement[10];
    VisualElement showInfoPopup;
    ProgressBar playbackBar;
    Label playbackTime;

    //UI Values
    float[] hotBarKeyScale = new float[10];
    float showInfoPopupPosition = 0;
    bool showInfoPopupMoving;

    //Objects
    [HideInInspector] DEAD_Showtape showtape;

    //Consts
    const float hotBarKeyAnimationSpeed = 8;
    const float hotBarKeyminSize = 0.8f;
    const float hotBarKeyYOffset = 10f;

    void OnEnable()
    {
        document.rootVisualElement.Q<Button>("LoadShow").clicked += () => LoadFile();
        document.rootVisualElement.Q<Button>("SaveShow").clicked += () => SaveFile();
        document.rootVisualElement.Q<Button>("ShowInfo").clicked += () => StartCoroutine(VisualizeShowInfoPopup(Convert.ToBoolean(showInfoPopupPosition)));
        document.rootVisualElement.Q<Button>("ClearAllData").clicked += () => CreateNewShowtape();
        document.rootVisualElement.Q<Button>("EditScreenBack").clicked += () => StartCoroutine(VisualizeShowInfoPopup(true));
        document.rootVisualElement.Q<Button>("BrowseAudio").clicked += () => InjectAudioData();
        document.rootVisualElement.Q<Button>("Play").clicked += () => SendCommand("Play");
        document.rootVisualElement.Q<Button>("Pause").clicked += () => SendCommand("Pause");
        document.rootVisualElement.Q<Button>("Rewind").clicked += () => SendCommand("Rewind");
    }

    private void Start()
    {
        for (int i = 0; i < hotBarVisualElements.Length; i++)
        {
            hotBarVisualElements[i] = document.rootVisualElement.Q<VisualElement>("Hotbar" + i);
        }
        showInfoPopup = document.rootVisualElement.Q<VisualElement>("ShowInfoPopup");
        playbackBar = document.rootVisualElement.Q<ProgressBar>("PlaybackBar");
        playbackTime = document.rootVisualElement.Q<Label>("PlaybackTime");
        if (deadInterface.GetShowtape(0) == null)
        {
            CreateNewShowtape();
        }

        UpdateHotbarIcons();
    }

    private void Update()
    {
        PressHotbarKey(0, Input.GetKey(KeyCode.Alpha1));
        PressHotbarKey(1, Input.GetKey(KeyCode.Alpha2));
        PressHotbarKey(2, Input.GetKey(KeyCode.Alpha3));
        PressHotbarKey(3, Input.GetKey(KeyCode.Alpha4));
        PressHotbarKey(4, Input.GetKey(KeyCode.Alpha5));
        PressHotbarKey(5, Input.GetKey(KeyCode.Alpha6));
        PressHotbarKey(6, Input.GetKey(KeyCode.Alpha7));
        PressHotbarKey(7, Input.GetKey(KeyCode.Alpha8));
        PressHotbarKey(8, Input.GetKey(KeyCode.Alpha9));
        PressHotbarKey(9, Input.GetKey(KeyCode.Alpha0));

        UpdateTabPositions();
        UpdatePlaybackBar();
    }

    void SendCommand(string command)
    {
        deadInterface.SendCommand(command, true);
        if (command == "Play")
        {
            StartCoroutine(UpdateShowMaxLength());
        }
    }

    IEnumerator UpdateShowMaxLength()
    {
        while (deadInterface.getLoadingState() == DEAD_Interface.LoadingState.loading)
        {
            yield return null;
        }
        AudioClip clip = deadInterface.GetAudioClip(0);
        if (clip != null)
        {
            showtape.endOfTapeTime = clip.length;
        }
    }

    void PressHotbarKey(int number, bool down)
    {
        hotBarKeyScale[number] = Mathf.Clamp01(hotBarKeyScale[number] + ((down ? hotBarKeyAnimationSpeed : -hotBarKeyAnimationSpeed) * Time.deltaTime));
        hotBarVisualElements[number].style.scale = Vector2.Lerp(Vector2.one, Vector2.one * hotBarKeyminSize, down ? hotkeyPress.Evaluate(hotBarKeyScale[number]) : hotkeyRelease.Evaluate(hotBarKeyScale[number]));
        hotBarVisualElements[number].style.translate = new StyleTranslate() { value = new Translate(0, Mathf.Lerp(0, hotBarKeyYOffset, down ? hotkeyPress.Evaluate(hotBarKeyScale[number]) : hotkeyRelease.Evaluate(hotBarKeyScale[number]))) };
    }

    void UpdatePlaybackBar()
    {
        playbackTime.text = TimeSpan.FromSeconds(deadInterface.GetCurrentTapeTime()).ToString(@"mm\:ss") + "/" + TimeSpan.FromSeconds(showtape.endOfTapeTime).ToString(@"mm\:ss");
        playbackBar.highValue = showtape.endOfTapeTime;
        playbackBar.value = deadInterface.GetCurrentTapeTime();
    }

    void UpdateTabPositions()
    {
        showInfoPopup.style.translate = new StyleTranslate() { value = new Translate(0, Mathf.Lerp(1111, 86, uiMove.Evaluate(showInfoPopupPosition))) };
    }

    void UpdateHotbarIcons()
    {
        for (int i = 0; i < hotBarVisualElements.Length; i++)
        {
            VisualElement key = hotBarVisualElements[i].Q<VisualElement>("Icon");
            key.style.backgroundImage = hotkeyIcons[i].icon;
            key.style.scale = new Vector2(hotkeyIcons[i].flippedX ? -1 : 1, 1);
        }
    }

    void UpdateShowtapeText()
    {
        document.rootVisualElement.Q<TextField>("ShowtapeName").SetValueWithoutNotify(showtape.name);
        document.rootVisualElement.Q<TextField>("ShowtapeAuthor").SetValueWithoutNotify(showtape.author);
        document.rootVisualElement.Q<TextField>("ShowtapeDescription").SetValueWithoutNotify(showtape.description);
        Label fileName = document.rootVisualElement.Q<Label>("ShowAudioFileName");
        if (showtape.audioClips != null && showtape.audioClips.Length > 0 && showtape.audioClips[0].fileName != null)
        {
            if (showtape.audioClips[0].fileName == "")
            {
                fileName.text = "(Audio present with no name)";
            }
            else
            {
                fileName.text = showtape.audioClips[0].fileName;
            }
        }
        else
        {
            fileName.text = "No Show Audio";
        }
    }

    IEnumerator VisualizeShowInfoPopup(bool hide)
    {
        if (!showInfoPopupMoving)
        {
            showInfoPopupMoving = true;
            if (hide)
            {
                while (showInfoPopupPosition > 0)
                {
                    showInfoPopupPosition -= Time.deltaTime * 5;
                    yield return null;
                }
                showInfoPopupPosition = 0;
            }
            else
            {
                while (showInfoPopupPosition < 1)
                {
                    showInfoPopupPosition += Time.deltaTime * 5;
                    yield return null;
                }
                showInfoPopupPosition = 1;
            }
            showInfoPopupMoving = false;
        }
    }

    void InjectAudioData()
    {
        var extensions = new[] { new ExtensionFilter("Audio Files", "wav", "aiff", "mp3", "wma"), };
        string[] files = StandaloneFileBrowser.OpenFilePanel("Load Showtape Audio", "", extensions, false);
        if (files != null && files.Length != 0)
        {
            if (File.Exists(files[0]))
            {
                byte[] filestream = File.ReadAllBytes(files[0]);
                showtape.audioClips = new DEAD_ByteArray[] { new DEAD_ByteArray() { fileName = Path.GetFileName(files[0]), array = filestream } };
                UpdateShowtapeText();
            }
        }
    }

    void CreateNewShowtape()
    {
        showtape = new DEAD_Showtape();
        showtape.name = "My Showtape";
        showtape.author = "Me";
        showtape.description = "A showtape.";
        showtape.timeCreated = new UDateTime() { dateTime = DateTime.Now };
        deadInterface.SetShowtape(0, showtape);
        UpdateShowtapeText();
    }

    void SaveFile()
    {
        //Save
        string path = StandaloneFileBrowser.SaveFilePanel("Save Showtape File", "", "MyShowtape", new[] { new ExtensionFilter("Showtape Files", "showtape"), });
        if (path != "")
        {
            DEAD_Save_Load.SaveShowtape(path, showtape);
        }
    }

    void LoadFile()
    {
        string[] files = StandaloneFileBrowser.OpenFilePanel("Load Showtape File", "", "showtape", false);
        if (files != null && files.Length != 0)
        {
            showtape = DEAD_Save_Load.LoadShowtape(files[0]);
            deadInterface.SetShowtape(0, showtape);
            UpdateShowtapeText();

        }
    }

}

[System.Serializable]
struct HotKeyIcons
{
    public Texture2D icon;
    public bool flippedX;
}
