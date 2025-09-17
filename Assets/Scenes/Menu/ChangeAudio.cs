using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class ChangeAudio : MonoBehaviour
{
    [SerializeField] private AudioMixer aMixer;

    public void ChangeValue(Slider slider)  
    {
        switch (slider.value)
        {
            case 0:
                aMixer.SetFloat("Music", -88);
                break;
            case 1:
                aMixer.SetFloat("Music", -40);
                break;
            case 2:
                aMixer.SetFloat("Music", -20);
                break;
            case 3:
                aMixer.SetFloat("Music", -10);
                break;
            case 4:
                aMixer.SetFloat("Music", 0);
                break;
            case 5:
                aMixer.SetFloat("Music", 10);
                break;
        }
    }
}
