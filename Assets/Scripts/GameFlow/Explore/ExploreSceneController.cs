using UnityEngine;

public class ExploreSceneController : MonoBehaviour
{
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
    }
}