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
            if (x.GameAction.TimeLeft == 0 && y.GameAction.TimeLeft == 0)
            {
                int result =  (int) (x.FinishedAt - y.FinishedAt);
                if (result > 0)
                    return 1;
                else if (result == 0)
                    return 0;
                else
                    return -1;
            }
            int diff = x.GameAction.TimeLeft - y.GameAction.TimeLeft;
            if (diff > 0)
                return 1;
            else if (diff == 0)
                return 0;
            else
                return -1;
        }
        #endregion
    }

    //Set through Unity
    public Text CharacterName;
    public Text ActionName;
    public Text TimeLeft;
    public Button CancelActionButton;

    public Image Background;
    public Color HoverColor;
    //

//    public event Action<GameActionDisplay> Finish;
    public event Action<GameActionDisplay> Cancel;

    public float FinishedAt = -1f;

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
        if (this.gameAction == null || this.FinishedAt > 0f)
            return;        

        if (this.gameAction.TimeLeft == 0)
        {
            this.FinishedAt = Time.time;
            this.TimeLeft.text = "done";
//            this.Finish(this);
            return;
        }

        this.TimeLeft.text = GameActionDataExt.GetLengthInSecs( this.gameAction.TimeLeft  ); 

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
