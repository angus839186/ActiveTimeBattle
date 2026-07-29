using UnityEngine;

public class ExploreSceneController : MonoBehaviour
{
    [SerializeField] private ExploreMapSpawner mapSpawner;
    private void Start()
    {
        GameFlowController gameFlow = GameFlowController.Instance;

        if (gameFlow == null)
        {
            Debug.LogWarning("ExploreSceneController: GameFlowController not found.");
            return;
        }

        RunSession runSession = gameFlow.CurrentRunSession;


        if (runSession.SelectedClass != null)
        {
            Debug.Log($"Explore started. Class: {runSession.SelectedClass.DisplayName}, Seed: {runSession.Seed}");
        }
        else
        {
            Debug.Log($"Explore started. ClassId: {runSession.SelectedClassId}, Seed: {runSession.Seed}");
        }
        Debug.Log($"Explore map rooms: {runSession.ExploreMap.Rooms.Count}");

        if (mapSpawner != null)
        {
            mapSpawner.ApplyStartRoom(runSession.ExploreMap);
        }
        else
        {
            Debug.LogWarning("ExploreSceneController: MapSpawner is not assigned.");
        }
    }
}