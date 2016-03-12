using UnityEngine;
using System.Collections;

public class Glue : MonoBehaviour 
{
    //Set through Unity
    public LevelMng LeveMng;
    public Camera MainCamera;

    public GameObject PlayerPrefab;
    public GameObject DogPrefab;

    public ActionMenu ActionMenu;
    public InputManager InputManager;

    public int Seed;

    public StatInt HealthStat;
    public StatInt StaminaStat;

    public ItemData[] PlayerTestBuild;
    //

	void Start () 
    {
        ActionExecutor.Instance.ResetGameTime();

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

        ItemData[] build = LevelSettings.PlayerBuild;
        if (build == null)
        {
            build = PlayerTestBuild;
        }
        player.Build = build;

        this.LeveMng.AddCharacterOnPos(player as Character, dungeon.PlayerStartPos);
        playerGo.transform.localPosition = Vector3.zero;

        player.ActionMenu = this.ActionMenu;
        player.HealthStat = this.HealthStat;
        player.StaminaStat = this.StaminaStat;

        this.InputManager.Player = player;


//        Room room = dungeon.Rooms[0];

//        Point dogStartPos = dungeon.PlayerStartPos;
//        dogStartPos.Y++;dogStartPos.Y++;dogStartPos.Y++;
////        while(true)
////        {
////            dogStartPos = room.GetRandomPointInsideRoom(1);
////            if (dogStartPos != dungeon.PlayerStartPos)
////            {
////                break;
////            }
////        }

        for (int i = 0; i < dungeon.Rooms.Count; i++) 
        {
            var room = dungeon.Rooms[i];
            var dogGo = (GameObject) Instantiate(this.DogPrefab);
            var dog = dogGo.GetComponent<Monster>();
            this.LeveMng.AddCharacterOnPos(dog as Character, room.GetRandomPointInsideRoom(2));
        }

        LevelMng.Instance.ActivateRoom( dungeon.Rooms[0] );
	}
}
