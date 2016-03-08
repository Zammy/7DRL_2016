using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class GameActionDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //Set through Unity
    public Text CharacterName;
    public Text ActionName;
    public Text TimeLeft;
    public Button CancelActionButton;

    public Image Background;
    public Color HoverColor;
    //

    public event Action<GameActionDisplay> Finish;
    public event Action<GameActionDisplay> Cancel;

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
        }
    }

	void Update ()
    {
        if (this.gameAction == null)
            return;

        if (this.gameAction.TimeLeft == 0)
        {
            this.Finish(this);
            return;
        }

        this.TimeLeft.text = GameActionDataExt.GetLengthInSecs( this.gameAction.TimeLeft );

        bool canCancel = this.GameAction.Character is Player && !this.GameAction.Started;
        this.CancelActionButton.gameObject.SetActive( canCancel );
	}

    public void CancelGameAction()
    {
        this.Cancel(this);
    }

    #region IPointerEnterHandler implementation
    public void OnPointerEnter(PointerEventData eventData)
    {
        this.GameAction.Target.IsHighlighted = true;
        this.Background.color = this.HoverColor;
    }
    #endregion

    #region IPointerExitHandler implementation

    public void OnPointerExit(PointerEventData eventData)
    {
        this.GameAction.Target.IsHighlighted = false;
        this.Background.color = Color.black;
    }

    #endregion
}
