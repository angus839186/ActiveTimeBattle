using System.Collections.Generic;

public class RunSession
{
    public string SelectedClassId { get; private set; }
    public int Seed { get; private set; }
    public bool IsActive { get; private set; }
    public int BattlesWon { get; private set; }

    public PlayerClassDefinition SelectedClass { get; private set; }

    public void StartRun(string selectedClassId, int seed)
    {
        SelectedClassId = selectedClassId;
        Seed = seed;
        IsActive = true;
    }

    public void StartRun(PlayerClassDefinition selectedClass, int seed)
    {
        SelectedClass = selectedClass;
        SelectedClassId = selectedClass != null ? selectedClass.ClassId : string.Empty;
        Seed = seed;
        IsActive = true;
    }

    public void EndRun()
    {
        IsActive = false;
    }
    public void RecordBattleVictory()
    {
        BattlesWon++;
    }

    #region Encounter
    private readonly HashSet<string> completedEncounters = new HashSet<string>();

    public bool IsEncounterCompleted(string encounterId)
    {
        return completedEncounters.Contains(encounterId);
    }

    public void CompleteEncounter(string encounterId)
    {
        completedEncounters.Add(encounterId);
    }
    #endregion
}