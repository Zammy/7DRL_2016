using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Text Name;
    public Text Description;

    public Transform Actions;
    public GameObject ActionDisplayPrefab;

    public Color HighlightColor;
    public Image Background;

    public GameObject Selected;

    ItemData item;
    public ItemData ItemData
    {
        get
        {
            return item;
        }
        set
        {
            item = value;

            this.Name.text = item.Name;
            this.Description.text = item.Description;

            foreach (var action in item.Actions) 
            {
                var actionGo = (GameObject) Instantiate( ActionDisplayPrefab );

                actionGo.GetComponent<ActionDisplay>().GameActionData = action;

                actionGo.transform.SetParent(Actions);
                actionGo.transform.localScale = Vector3.one;

            }
        }
    }

    #region IPointerEnterHandler implementation

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.Background.color = this.HighlightColor;
    }

    #endregion

    #region IPointerExitHandler implementation

    public void OnPointerExit(PointerEventData eventData)
    {
        this.Background.color = Color.black;
    }

    #endregion

    #region IPointerClickHandler implementation

    public void OnPointerClick(PointerEventData eventData)
    {
        this.Selected.SetActive( !this.Selected.activeSelf );
    }

    #endregion
}
