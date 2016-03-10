using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StatString : MonoBehaviour 
{
    //Set through Unity
    public Text Text;
    //

    public string Value
    {
        set
        {
            this.Text.text = value;
        }
    }
}
