﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerStat : MonoBehaviour 
{
    //Set through Unity
    public Text Value;
    //

    public int Stat
    {
        set
        {
            this.Value.text = value.ToString();
        }
    }
}