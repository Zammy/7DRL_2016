using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GameActionDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public class Comparer : IComparer<GameActionDisplay>
    {
        #region IComparer implementation
        public int Compare(GameActionDisplay x, GameActionDisplay y)
        {
            int diff = x.GameAction.TimeLeft - y.GameAction.TimeLeft;
            if (diff == 0)
            {
                return x.GameAction.TimeFinished - y.GameAction.TimeFinished;
            }
            return diff;
        }
        #endregion
    }

    //Set through Unity
    public Text CharacterName;
    public Text ActionName;

    public Text TimeLeftLabel;
    public Text TimeFinishedLabel;
    public Text TimeLeft;


    public GameObject IsHit;

    public Image Background;
    public Color HoverColor;
    //

    public event Action<GameActionDisplay> MouseHoverIn;
    public event Action<GameActionDisplay> MouseHoverOut;


    GameAction gameAction;
    public GameAction GameAction
    {
        get
        {
            return this.gameAction;
        }
        set
        {
            this.gameAction = value;
            this.ActionName.text = value.ActionData.Name;

            this.CharacterName.text = value.Character.Name;

            this.CharacterName.color = value.Character.GetComponent<LightTarget>().Color;
        }
    }

	void Update ()
    {
        if (this.gameAction == null && this.TimeFinishedLabel.gameObject.activeSelf)
            return;

        if (this.gameAction.TimeLeft == 0)
        {
            this.TimeLeftLabel.gameObject.SetActive(false);
            this.TimeFinishedLabel.gameObject.SetActive(true);
            this.TimeLeft.text = GameActionDataExt.GetLengthInSecs( this.gameAction.TimeFinished ).ToString("F");

            var attack = this.gameAction.GetComponent<Attack>();
            if (attack != null)
            {
                this.IsHit.SetActive( attack.TargetsHit.Count > 0 );
            }
            return;
        }

        this.TimeLeft.text = GameActionDataExt.GetLengthInSecs( this.gameAction.TimeLeft  ).ToString("F"); 
	}

    #region IPointerEnterHandler implementation
    public void OnPointerEnter(PointerEventData eventData)
    {
        this.Background.color = this.HoverColor;

        this.MouseHoverIn(this);
    }
    #endregion

    #region IPointerExitHandler implementation

    public void OnPointerExit(PointerEventData eventData)
    {
        this.Background.color = Color.black;

        this.MouseHoverOut(this);
    }

    #endregion
}
