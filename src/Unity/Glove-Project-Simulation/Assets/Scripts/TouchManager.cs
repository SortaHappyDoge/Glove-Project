using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchManager : MonoBehaviour
{
    public AudioSource[] audioSources = new AudioSource[37];

    public Transform soundPlayer;

    void Start(){
        audioSources = soundPlayer.GetComponents<AudioSource>(); 
    }

    void Command_4_8(){
        audioSources[0].Play(0);
    }
    void Command_4_12(){
        audioSources[1].Play(0);
    }
    void Command_4_16(){
        audioSources[2].Play(0);
    }
    void Command_4_20(){
        audioSources[3].Play(0);
    }
    void Command_4_7(){
        audioSources[4].Play(0);
    }
    void Command_4_11(){
        audioSources[5].Play(0);
    }
    void Command_4_15(){
        audioSources[6].Play(0);
    }
    void Command_4_19(){
        audioSources[7].Play(0);
    }


    void Command_22_29(){
        audioSources[8].Play(0);
    }
    void Command_22_33(){
        audioSources[9].Play(0);
    }
    void Command_22_37(){
        audioSources[10].Play(0);
    }
    void Command_22_41(){
        audioSources[11].Play(0);
    }
    void Command_22_28(){
        audioSources[12].Play(0);
    }
    void Command_22_32(){
        audioSources[13].Play(0);
    }
    void Command_22_36(){
        audioSources[14].Play(0);
    }
    void Command_22_40(){
        audioSources[15].Play(0);
    }
}
