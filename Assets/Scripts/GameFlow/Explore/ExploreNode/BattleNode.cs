using UnityEngine;

public class BattleNode : ExploreNode
{

    protected override void OnInteract()
    {
        if (GameFlowController.Instance == null)
        {
            Debug.LogWarning("BattleNode: GameFlowController not found.");
            return;
        }

        RunSession runSession = GameFlowController.Instance.CurrentRunSession;

        if (runSession != null)
        {
            runSession.StartPendingBattle(RoomController.RoomId, NodeId);
        }

        ExplorePlayerController player = FindFirstObjectByType<ExplorePlayerController>();

        if (runSession != null && player != null)
        {
            runSession.SetExploreReturnPosition(player.transform.position);
        }

        GameFlowController.Instance.ChangeToBattle();
    }
}