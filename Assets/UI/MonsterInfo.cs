using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MonsterInfo : MonoBehaviour 
{
    //Set through Unity
    public Text Name;
    public Text Description;
    public StatInt Health;
    public StatInt Stamina;
    //

    public void ShowMonsterInfo(Monster monster)
    {
        this.gameObject.SetActive(true);

        Name.text = monster.Name;
        Name.color = monster.GetComponent<LightTarget>().Color;
        Description.text = monster.Description;
        Health.Value = monster.Health;
        Stamina.Value = monster.Stamina;
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
