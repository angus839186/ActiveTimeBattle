using UnityEngine;

public class BattleNode : ExploreEventNode
{
    [SerializeField] private string encounterId = "test_battle_001";

    protected override void OnInteract()
    {
        if (GameFlowController.Instance == null)
        {
            Debug.LogWarning("BattleNode: GameFlowController not found.");
            return;
        }

        CompleteNode();
        GameFlowController.Instance.ChangeToBattle();
    }
}