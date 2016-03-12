using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StatInt : MonoBehaviour 
{
    //Set through Unity
    public Text Text;
    public bool IsPrecent;
    //

    public int Value
    {
        set
        {
            if (IsPrecent)
            {
                this.Text.text = value.ToString() + "%";
            }
            else
            {
                this.Text.text = value.ToString();
            }
        }
    }
}
