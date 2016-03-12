using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ActionDisplay : MonoBehaviour 
{
    //Set through Unity
    public Text Name;
    public Text Description;

    public StatFloat Time;
    public StatInt Stamina;
    public StatInt Range;
    public StatInt Damage;
    public StatString AttackPattern;
    public StatString Status;
    public StatFloat StatusDuration;
    public StatInt Recharge;
    public StatInt FlatDefend;
    public StatInt HitDefend;
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

            this.Name.text = gameAction.Name;
            this.Stamina.Value = gameAction.StaminaCost;
            this.Time.Value = GameActionDataExt.GetLengthInSecs(gameAction.Length);
            this.Range.Value = gameAction.Range;


            this.Damage.gameObject.SetActive(false);
            this.AttackPattern.gameObject.SetActive(false);

            var attackCmp = gameAction.GetComponent<AttackComponent>();
            if (attackCmp != null)
            {
                this.Damage.gameObject.SetActive(true);
                this.Damage.Value = attackCmp.Damage;
                this.AttackPattern.gameObject.SetActive(true);
                this.AttackPattern.Value = attackCmp.Pattern.ToString();
            }

            this.Status.gameObject.SetActive(false);
            this.StatusDuration.gameObject.SetActive(false);

            var attackStatus = gameAction.GetComponent<AttackStatusComponent>();
            if (attackStatus != null)
            {
                this.SetStatus(attackStatus.StatusEffect, attackStatus.Duration);
            }
            var statusCmp = gameAction.GetComponent<StatusComponent>();
            if (statusCmp != null)
            {
                SetStatus(statusCmp.StatusEffect, statusCmp.Duration);
            }

            this.Recharge.gameObject.SetActive(false);

            var rechargeCmp = gameAction.GetComponent<RechargeComponent>();
            if (rechargeCmp != null)
            {
                this.Recharge.gameObject.SetActive(true);
                this.Recharge.Value = rechargeCmp.Recharge;
            }


            this.FlatDefend.gameObject.SetActive(false);
            this.HitDefend.gameObject.SetActive(false);

            var defendCmp = gameAction.GetComponent<DefendComponent>();
            if (defendCmp != null)
            {
                if (defendCmp.FlatDamageReduction > 0)
                {
                    this.FlatDefend.gameObject.SetActive(true);
                    this.FlatDefend.Value = defendCmp.FlatDamageReduction;
                }
                if (defendCmp.ChanceToHitReduction > 0)
                {
                    this.HitDefend.gameObject.SetActive(true);
                    this.HitDefend.Value = defendCmp.ChanceToHitReduction;
                }
            }
        }
    }

    void SetStatus(StatusEffect effect, int duration)
    {
        this.Status.gameObject.SetActive(true);
        this.Status.Value = effect.ToString(); 

        this.StatusDuration.gameObject.SetActive(true);
        this.StatusDuration.Value = GameActionDataExt.GetLengthInSecs(duration);
    }

}
