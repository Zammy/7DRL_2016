using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ActionDataDisplay : MonoBehaviour 
{
    //Set through Unity
    public Text Name;
    public Text StaminaCost;
    public Text TimeCost;
    public Text Range;
    //

    GameActionData gameAction;
    public GameActionData GameActionData
    {
        get
        {
            return this.gameAction;
        }
        set
        {
            this.gameAction = value;

            this.Name.text = value.Name;
            this.StaminaCost.text = value.StaminaCost.ToString();
            this.TimeCost.text = GameActionDataExt.GetLengthInSecs(value.Length);
            this.Range.text = value.Range.ToString();
        }
    }

}
