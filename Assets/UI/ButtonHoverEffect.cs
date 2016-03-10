using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ButtonHoverEffect : MonoBehaviour 
{
    public Button Button;

    public void OnPointerEnter()
    {
        this.Button.image.color = Button.colors.highlightedColor;
    }

    public void OnPointerExit()
    {
        this.Button.image.color = Button.colors.normalColor;
    }
}
