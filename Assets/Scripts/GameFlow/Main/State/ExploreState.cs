using UnityEngine;

public class ExploreState : IState
{
    private readonly GameFlowContext context;

    public ExploreState(GameFlowContext context)
    {
        this.context = context;
    }

    public void Enter()
    {
        context.SceneFlow.LoadExplore();
        context.InputMode.SetExplore();
        Debug.Log("Enter Explore State");
    }

    public void Tick()
    {
    }

    public void Exit()
    {
        Debug.Log("Exit Explore State");
    }
}
