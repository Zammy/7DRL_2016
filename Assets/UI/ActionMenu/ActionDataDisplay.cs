using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ActionDataDisplay : MonoBehaviour 
{
    //Set through Unity
    public Text Name;
    public Text Stamina;
    public Text Time;
    public Text Range;

    public Text DamageText;
    public Text Damage;
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
            this.Stamina.text = value.StaminaCost.ToString();
            this.Time.text = GameActionDataExt.GetLengthInSecs(value.Length);
            this.Range.text = value.Range.ToString();
            if (value is AttackGameActionData)
            {
                this.DamageText.gameObject.SetActive(true);
                this.Damage.gameObject.SetActive(true);
                this.Damage.text = ((AttackGameActionData)value).Damage.ToString();
            }
            else
            {
                this.DamageText.gameObject.SetActive(false);
                this.Damage.gameObject.SetActive(false);
            }
        }
    }

}
