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
            BattleType battleType = NodeType == ExploreNodeType.EliteBattle
            ? BattleType.Elite
            : BattleType.Normal;

            runSession.StartPendingBattle(
                RoomController.RoomId,
                NodeId,
                battleType);
        }

        ExplorePlayerController player = FindFirstObjectByType<ExplorePlayerController>();

        if (runSession != null && player != null)
        {
            runSession.SetExploreReturnPosition(player.transform.position);
        }

        GameFlowController.Instance.ChangeToBattle();
    }
}