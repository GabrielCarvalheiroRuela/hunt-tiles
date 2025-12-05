using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class ChangeAudio : MonoBehaviour
{
    [SerializeField] private AudioMixer aMixer;
    [SerializeField] private Slider sliderVolume;
    
    void Start()
    {
        if (aMixer != null)
        {
            aMixer.SetFloat("Music", -25);
        }
        
        if (sliderVolume != null)
        {
            sliderVolume.value = 3;
        }
    }

    public void ChangeValue(Slider slider)  
    {
        switch (slider.value)
        {
            case 0:
                aMixer.SetFloat("Music", -88);
                break;
            case 1:
                aMixer.SetFloat("Music", -50);
                break;
            case 2:
                aMixer.SetFloat("Music", -35);
                break;
            case 3:
                aMixer.SetFloat("Music", -25);
                break;
            case 4:
                aMixer.SetFloat("Music", -15);
                break;
            case 5:
                aMixer.SetFloat("Music", -5);
                break;
        }
    }
}
