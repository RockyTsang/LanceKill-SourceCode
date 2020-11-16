﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class GameMainControl : MonoBehaviour
{
    public enum GameModeSelect{
        Rounded4v4,
        KillCount4v4
    }
    public GameModeSelect Mode;
    public int WinUnit;
    public enum MapSelect
    {
        Map1
    }
    public MapSelect Map;
    public CharacterPreset.TeamSelect MyTeam;
    public CheckTeammate Team1;
    public CharacterPreset.TeamSelect EnemyTeam;
    public CheckTeammate Team2;
    public CharacterPreset.WeaponSelect MyWeapon;
    public GameObject[] Maps;
    public GameObject[] SpawnPoints;
    public GameObject Avatar;
    public GameObject[] Players;
    public Camera GameCamera;

    public Announcement AnnouncementWindow;

    private int team1wincount;
    private int team2wincount;

    // Start is called before the first frame update
    void Start()
    { 
        // Set map
        switch (Map)
        {
            case MapSelect.Map1:
                Instantiate(Maps[0], transform);
                break;
        }

        // instantiate avatars to each spawnpoints
        SpawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        foreach(GameObject SpawnPoint in SpawnPoints)
        {
            switch(SpawnPoint.transform.parent.name)
            {
                case "BottomSpawnFeild":
                    GameObject newPlayer1 = Instantiate(Avatar, SpawnPoint.transform.position, Quaternion.Euler(new Vector3(0, 0, 0)), transform.Find("Team1"));
                    newPlayer1.GetComponent<CharacterPreset>().Team = MyTeam;
                    newPlayer1.GetComponent<CharacterPreset>().SpawnPosition = SpawnPoint.transform;
                    break;
                case "TopSpawnFeild":
                    GameObject newPlayer2 = Instantiate(Avatar, SpawnPoint.transform.position, Quaternion.Euler(new Vector3(0, 0, 0)), transform.Find("Team2"));
                    newPlayer2.GetComponent<CharacterPreset>().Team = EnemyTeam;
                    newPlayer2.GetComponent<CharacterPreset>().SpawnPosition = SpawnPoint.transform;
                    break;
            }
        }

        // Set identity of avatars
        Players = GameObject.FindGameObjectsWithTag("Player");
        System.Random RandomSeed = new System.Random(System.DateTime.Now.Minute);
        int RandNumber = RandomSeed.Next(0, 4);
        int PointerOfMe = 0;
        for (int i = 0; i < Players.Length; i++)
        {
            if(Players[i].GetComponent<CharacterPreset>().Team == MyTeam)
            {
                if (PointerOfMe == RandNumber)
                {
                    Players[i].GetComponent<CharacterPreset>().Type = CharacterPreset.Identity.Me;
                    Players[i].GetComponent<CharacterPreset>().WeaponType = MyWeapon;
                }
                else
                {
                    GenerateIDAndWeapon(i);
                }
                PointerOfMe++;
            }
            else
            {
                GenerateIDAndWeapon(i);
            }
            Players[i].GetComponent<CharacterPreset>().enabled = true;
        }
        GameCamera.gameObject.GetComponent<CameraFollowAvatar>().enabled = true;

        team1wincount = 0;
        team2wincount = 0;
        Team1.GetComponent<CheckTeammate>().enabled = true;
        Team2.GetComponent<CheckTeammate>().enabled = true;
        Invoke("EngageAllPlayer", 3f);
    }

    // Update is called once per frame
    void Update()
    {
        // Turn-based mode
        if(Mode == GameModeSelect.Rounded4v4)
        {
            if(Team1.surviving != Team2.surviving)
            {
                // Pause
                foreach (GameObject player in Players)
                {
                    if(player.GetComponent<CharacterPreset>().Type == CharacterPreset.Identity.Me)
                    {
                        player.GetComponent<AvatarControl>().enabled = false;
                    }else if(player.GetComponent<CharacterPreset>().Type == CharacterPreset.Identity.AI)
                    {
                        player.GetComponent<NonPlayerAI>().enabled = false;
                    }
                }
                EditorApplication.isPaused = true;
                
                // Determine win and lose
                if (Team1.surviving && !Team2.surviving)
                {
                    team1wincount++;
                    AnnouncementWindow.ShowResult(Team1.myTeam.ToString(), team1wincount, team2wincount);
                    AnnouncementWindow.gameObject.SetActive(true);
                    StartCoroutine(WaitFunction(3f));
                    AnnouncementWindow.HideWindow();
                }else if(!Team1.surviving && Team2.surviving)
                {
                    team2wincount++;
                    AnnouncementWindow.ShowResult(Team2.myTeam.ToString(), team1wincount, team2wincount);
                    AnnouncementWindow.gameObject.SetActive(true);
                    StartCoroutine(WaitFunction(3f));
                    AnnouncementWindow.HideWindow();
                }
                if(team1wincount >= Mathf.Round(WinUnit / 2))
                {
                    EndGame(Team1.myTeam);
                }
                else if(team2wincount >= Mathf.Round(WinUnit / 2))
                {
                    EndGame(Team2.myTeam);
                }
                else
                {
                    ResetRound();
                }
            }
        }

        // Deadmatch mode
        if(Mode == GameModeSelect.KillCount4v4)
        {

        }
    }

    // Random set weapon
    void GenerateIDAndWeapon(int i)
    {
        System.Random WeaponRandomSeed = new System.Random(System.DateTime.Now.Millisecond);
        int RandWeapon = WeaponRandomSeed.Next(0, 3);
        switch (RandWeapon)
        {
            case 0:
                Players[i].GetComponent<CharacterPreset>().WeaponType = CharacterPreset.WeaponSelect.Knife;
                break;
            case 1:
                Players[i].GetComponent<CharacterPreset>().WeaponType = CharacterPreset.WeaponSelect.Sword;
                break;
            case 2:
                Players[i].GetComponent<CharacterPreset>().WeaponType = CharacterPreset.WeaponSelect.Spear;
                break;
        }
        Players[i].GetComponent<CharacterPreset>().Type = CharacterPreset.Identity.AI;
    }

    // Start all player's movement
    void EngageAllPlayer()
    {
        foreach(GameObject player in Players)
        {
            switch(player.GetComponent<CharacterPreset>().Type)
            {
                case CharacterPreset.Identity.Me:
                    player.GetComponent<AvatarControl>().enabled = true;
                    break;
                case CharacterPreset.Identity.TestObject:
                    break;
                case CharacterPreset.Identity.AI:
                    player.GetComponent<NonPlayerAI>().enabled = true;
                    break;
            }
        }
    }

    void ResetRound()
    {
        Team1.surviving = true;
        Team2.surviving = true;
        foreach (GameObject player in Players)
        {
            player.transform.position = player.GetComponent<CharacterPreset>().SpawnPosition.position;
            player.SetActive(true);
            Debug.Log(player.activeInHierarchy);
        }
        EditorApplication.isPaused = false;
        Invoke("EngageAllPlayer", 3f);
    }

    void EndGame(CharacterPreset.TeamSelect WinTeam)
    {

    }

    IEnumerator WaitFunction(float time)
    {
        Debug.Log("Waiting");
        yield return new WaitForSecondsRealtime(time);
        Debug.Log("Waited");
    }
}
