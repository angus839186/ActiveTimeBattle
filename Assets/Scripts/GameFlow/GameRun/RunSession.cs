using System.Collections.Generic;
using UnityEngine;

public class RunSession
{
    public string SelectedClassId { get; private set; }

    public string CurrentExploreRoomId { get; private set; }
    public int Seed { get; private set; }
    public bool IsActive { get; private set; }
    public int BattlesWon { get; private set; }

    public PlayerClassDefinition SelectedClass { get; private set; }

    public ExploreMapData ExploreMap { get; private set; }
    public string PendingBattleRoomId { get; private set; }
    public string PendingBattleNodeId { get; private set; }
    public bool HasPendingBattle => !string.IsNullOrEmpty(PendingBattleNodeId);
    private readonly HashSet<string> completedRooms = new HashSet<string>();

    private readonly HashSet<string> completedNodes = new HashSet<string>();

    public bool HasExploreReturnPosition { get; private set; }
    public Vector3 ExploreReturnPosition { get; private set; }



    public void StartRun(string selectedClassId, int seed)
    {
        ResetRunState();
        SelectedClassId = selectedClassId;
        Seed = seed;
        IsActive = true;

    }

    public void StartRun(PlayerClassDefinition selectedClass, int seed)
    {
        ResetRunState();
        SelectedClass = selectedClass;
        SelectedClassId = selectedClass != null ? selectedClass.ClassId : string.Empty;
        Seed = seed;
        IsActive = true;
    }

    public void EndRun()
    {
        IsActive = false;
    }

    private void ResetRunState()
    {
        completedRooms.Clear();
        completedNodes.Clear();

        CurrentExploreRoomId = string.Empty;
        PendingBattleRoomId = string.Empty;
        PendingBattleNodeId = string.Empty;
        BattlesWon = 0;
        ExploreMap = null;

        HasExploreReturnPosition = false;
        ExploreReturnPosition = Vector3.zero;
    }

    public bool IsNodeCompleted(string nodeId)
    {
        return completedNodes.Contains(nodeId);
    }

    public void CompleteNode(string nodeId)
    {
        completedNodes.Add(nodeId);
    }
    public void StartPendingBattle(string roomId, string nodeId)
    {
        PendingBattleRoomId = roomId;
        PendingBattleNodeId = nodeId;
    }

    public void CompletePendingBattle()
    {
        if (string.IsNullOrEmpty(PendingBattleRoomId))
        {
            return;
        }

        CompleteRoom(PendingBattleRoomId);
        CompleteNode(PendingBattleNodeId);
        RecordBattleVictory();

        PendingBattleRoomId = string.Empty;
        PendingBattleNodeId = string.Empty;
    }
    public void RecordBattleVictory()
    {
        BattlesWon++;
    }

    public bool IsRoomCompleted(string roomId)
    {
        return completedRooms.Contains(roomId);
    }

    public void CompleteRoom(string roomId)
    {
        completedRooms.Add(roomId);
    }

    public void SetExploreMap(ExploreMapData exploreMap)
    {
        ExploreMap = exploreMap;
        CurrentExploreRoomId = exploreMap != null ? exploreMap.StartRoomId : string.Empty;
    }

    public void SetCurrentExploreRoom(string roomId)
    {
        CurrentExploreRoomId = roomId;
    }

    public void SetExploreReturnPosition(Vector3 position)
    {
        ExploreReturnPosition = position;
        HasExploreReturnPosition = true;
    }

    public void ClearExploreReturnPosition()
    {
        HasExploreReturnPosition = false;
    }



}