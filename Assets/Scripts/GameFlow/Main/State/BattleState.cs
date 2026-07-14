using UnityEngine;

public class BattleState : IState
{
    private readonly GameFlowContext context;

    public BattleState(GameFlowContext context)
    {
        this.context = context;
    }
    public void Enter()
    {
        context.PlayerControlMode.SetBattle();
        context.SceneFlow.LoadBattle();
        Debug.Log("Enter Battle State");
    }

    public void Tick()
    {
    }

    public void Exit()
    {
        Debug.Log("Exit Battle State");
    }
}
