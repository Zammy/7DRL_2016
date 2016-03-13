using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum MonsterType
{
    FeralDog,
    RottingCorpse
}

public class MonsterChanceSpawn
{
    public MonsterType Monster;
    public int Minimum;
    public float ExtraChance;
    public int Maximum;
}

public class LevelSettings 
{
    static MonsterChanceSpawn[][] mobChances = new MonsterChanceSpawn[][]
    {
        //1
        new MonsterChanceSpawn[] 
        {
            new MonsterChanceSpawn()
            {
                Monster = MonsterType.FeralDog,
                Minimum = 1,
                ExtraChance = .80f,
                Maximum = 3
            },
            new MonsterChanceSpawn()
            {
                Monster = MonsterType.RottingCorpse,
                Minimum = 0,
                ExtraChance = .50f,
                Maximum = 1
            }
        },

        //2
        new MonsterChanceSpawn[]
        {
            new MonsterChanceSpawn()
            {
                Monster = MonsterType.FeralDog,
                Minimum = 0,
                ExtraChance = .25f,
                Maximum = 3
            },
            new MonsterChanceSpawn()
            {
                Monster = MonsterType.RottingCorpse,
                Minimum = 1,
                ExtraChance = .75f,
                Maximum = 4
            }
        }
    };


    static ItemData[] playerBuild;

    static int level = 0;

    static LevelSettings()
    {
    }

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

    public static Dungeon GenerateDungeon()
    {
        var bspGen = new BSPGenerator();
        var genOpts = new GenOptions()
        {
            DUN_WIDTH = 50 + level*10,
            DUN_HEIGHT= 50 + level*10,

            MAX_LEAF_AREA = 650,
            MIN_LEAF_SIDE = 10,
            MIN_ROOM_SIDE = 6
        };

        Dungeon dungeon = bspGen.GenDungeon(genOpts);

        SpawnMonsters(dungeon);

        return dungeon;
    }

    public static void SpawnMonsters(Dungeon dungeon)
    {
        Func<List<Point>, Point> getRndPoint = (List<Point> roomPoints) =>
        {
            int index = UnityEngine.Random.Range(0, roomPoints.Count);
            Point p = roomPoints[index];
            roomPoints.RemoveAt(index);
            return p;
        };

        MonsterChanceSpawn[] levelChances = mobChances[level];

        foreach(Room room in dungeon.Rooms)
        {
            List<Point> roomPoints = room.GetPointsInside();

            foreach (var mobChance in levelChances)
            {
                MonsterType mob = mobChance.Monster;
                for (int i = 0; i < mobChance.Minimum; i++)
                {
                    Point p = getRndPoint(roomPoints);
                    dungeon.monsters.Add(p, mob);
                }

                for (int i = 0; i < mobChance.Maximum - mobChance.Minimum; i++)
                {
                    if (UnityEngine.Random.value > mobChance.ExtraChance)
                    {
                        break;
                    }
                    Point p = getRndPoint(roomPoints);
                    dungeon.monsters.Add(p, mob);
                }
            }
        }
    }

    public static void FinishedLevel()
    {
        level++;
    }

}
