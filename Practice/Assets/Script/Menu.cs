using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{

    public GameObject mainMenuHolder;
    public GameObject optionMenuHolder;
    public GameObject selectMenuHolder;

    public Slider[] volumeSliders;
    public Toggle[] resolutionToggles;
    public Toggle fullScreenToggle;
    public int[] screenWidth;
    int activeScreenResIndex;

    void Start() {
        activeScreenResIndex = PlayerPrefs.GetInt("screen res index");
        bool isFullScreen = (PlayerPrefs.GetInt("fullscreen") == 1) ? true : false;

        volumeSliders[0].value = AudioManager.instance.masterVolumePercent;
        volumeSliders[1].value = AudioManager.instance.sfxVolumePercent;
        volumeSliders[2].value = AudioManager.instance.musicVolumePercent;

        for (int i = 0; i < resolutionToggles.Length; i++) {
            resolutionToggles [i].isOn = (i == activeScreenResIndex);
        }

        fullScreenToggle.isOn = isFullScreen;
    }

    public void Play() {
        mainMenuHolder.SetActive(false);
        selectMenuHolder.SetActive(true);
    }

    public void PlayTutorial() {
        SceneManager.LoadScene("Tutorial");
    }

    public void PlayInfinite() {
        SceneManager.LoadScene("Infinite");
    }

    public void Quit() {
        Application.Quit();
    }
    public void OptionsMenu() {
        mainMenuHolder.SetActive(false);
        optionMenuHolder.SetActive(true);
    }

    public void MainMenu() {
        selectMenuHolder.SetActive(false);
        optionMenuHolder.SetActive(false);
        mainMenuHolder.SetActive(true);
    }

    public void SetScreenResolution(int i) {
        if (resolutionToggles[i].isOn) {
            activeScreenResIndex = i;
            float aspecRatio = 16 / 9f;
            Screen.SetResolution(screenWidth[i], (int)(screenWidth[i] / aspecRatio), false);
            PlayerPrefs.SetInt("screen res index", activeScreenResIndex);
            PlayerPrefs.Save();
        }
    }

    public void SetFullScreen(bool isFullScreen) {
        for (int i = 0; i < resolutionToggles.Length; i++) {
            resolutionToggles[i].interactable = !isFullScreen;
        }

        //Debug.Log(isFullScreen);

        if (isFullScreen) {
            Resolution[] allResolutions = Screen.resolutions;
            Resolution maxResolution = allResolutions[allResolutions.Length - 1];
            Screen.SetResolution(maxResolution.width, maxResolution.height, true);
        }
        else {
            SetScreenResolution(activeScreenResIndex);
        }

        PlayerPrefs.SetInt("fullscreen", (isFullScreen ? 1 : 0));
        PlayerPrefs.Save();
    }

    public void SetMasterVolume(float value) {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Master);
    }

    public void SetSfxVolume(float value) {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Sfx);
    }

    public void SetMusicVolume(float value) {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Music);
    }
}
