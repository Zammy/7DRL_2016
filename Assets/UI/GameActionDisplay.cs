using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class GameActionDisplay : MonoBehaviour 
{
    //Set through Unity
    public Text Name;
    public Text TimeLeft;
    public Button CancelActionButton;
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
            this.Name.text = value.ActionData.Name;
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

}
