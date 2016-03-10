using UnityEngine;
using System.Collections;

public class LevelSettings 
{
    static ItemData[] playerBuild;

    public static ItemData[] PlayerBuild
    {
        get
        {
            return playerBuild;
        }
        set
        {
            playerBuild = value;
        }
    }

}
