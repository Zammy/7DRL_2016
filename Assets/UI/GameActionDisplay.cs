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
    public GameObject IsHit;

    public Image Background;
    public Color HoverColor;
    //

    public event Action<GameActionDisplay> MouseHoverIn;
    public event Action<GameActionDisplay> MouseHoverOut;


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

            var attack = this.gameAction.GetComponent<Attack>();
            if (attack != null)
            {
                this.IsHit.SetActive( attack.TargetHit != null  );
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
