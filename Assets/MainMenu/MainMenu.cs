using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour 
{
    public Canvas Title;
    public Canvas ItemSelection;

    public GameObject ItemSelectorPrefab;
    public Transform Items;

    public ItemData[] ItemsAvialable;
    public ItemData BaseActions;

    List<ItemData> selectedItems = new List<ItemData>(3);

    void Start()
    {
        foreach (var item in ItemsAvialable)
        {
            var itemGo = (GameObject) Instantiate(this.ItemSelectorPrefab);

            itemGo.transform.SetParent( this.Items );
            itemGo.transform.localScale = Vector3.one;
            itemGo.GetComponent<ItemSelector>().ItemData = item ;
        }
    }

    void Update()
    {
        this.selectedItems.Clear();

        foreach (Transform itemTrans in Items)
        {
            var itemSelector = itemTrans.GetComponent<ItemSelector>();
            if (itemSelector.Selected.activeSelf)
            {
                this.selectedItems.Add(itemSelector.ItemData);
            }
        }

        if (this.selectedItems.Count > 2)
        {
            this.selectedItems.Add(this.BaseActions);

            LevelSettings.PlayerBuild = this.selectedItems.ToArray();

            SceneManager.LoadScene(1);
        }
    }

	public void DescendClicked()
    {
        this.Title.gameObject.SetActive(false);
        this.ItemSelection.gameObject.SetActive(true);
    }
}
