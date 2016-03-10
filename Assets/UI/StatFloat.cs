using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StatFloat : MonoBehaviour 
{
    //Set through Unity
    public Text Text;
    //

    public float Value
    {
        set
        {
            this.Text.text = value.ToString("F");
        }
    }
}
