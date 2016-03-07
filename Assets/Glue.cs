using UnityEngine;
using System.Collections;
using RogueLib;

public class Glue : MonoBehaviour 
{
    public LevelMng LeveMng;
    public Camera MainCamera;
    public GameObject PlayerPrefab;

    public RadialActionMenu ActionMenu;
    public InputManager InputManager;

	void Start () 
    {
        var lvlGen = new BSPGenerator();
        var genOpts = new GenOptions()
        {
            Seed = 1,
            DUN_WIDTH = 50,
            DUN_HEIGHT= 50,

            MAX_LEAF_AREA = 1000,
            MIN_LEAF_SIDE = 10,
            MIN_ROOM_SIDE = 8
        };

        Dungeon dungeon = lvlGen.GenAndDrawRooms(genOpts);

        this.LeveMng.LoadLevel(dungeon);

        var playerGo = (GameObject)Instantiate(this.PlayerPrefab);
        this.MainCamera.transform.SetParent(playerGo.transform);
        var player = playerGo.GetComponent<Player>();
        this.LeveMng.AddCharacterOnPos(player as Character, dungeon.PlayerStartPos);
        playerGo.transform.localPosition = Vector3.zero;
        player.ActionMenu = this.ActionMenu;

        this.InputManager.Player = player;
	}
}
