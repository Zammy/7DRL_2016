using UnityEngine;
using System.Collections;
using RogueLib;

public class Glue : MonoBehaviour 
{
    public LevelMng LeveMng;
    public Camera MainCamera;

    public GameObject PlayerPrefab;
    public GameObject DogPrefab;

    public RadialActionMenu ActionMenu;
    public InputManager InputManager;

    public int Seed;

	void Start () 
    {
        var lvlGen = new BSPGenerator();
        var genOpts = new GenOptions()
        {
            Seed = this.Seed,
            DUN_WIDTH = 50,
            DUN_HEIGHT= 50,

            MAX_LEAF_AREA = 650,
            MIN_LEAF_SIDE = 10,
            MIN_ROOM_SIDE = 6
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

//        for (int i = 1; i < dungeon.Rooms.Count; i++)
//        {
//            Room room = dungeon.Rooms[i];
//            var dogGo = (GameObject) Instantiate(this.DogPrefab);
//
//            Point p = room.GetRandomPointInsideRoom(1);
//            var dog = dogGo.GetComponent<Monster>();
//            this.LeveMng.AddCharacterOnPos(dog as Character, p);
//        }

	}
}
