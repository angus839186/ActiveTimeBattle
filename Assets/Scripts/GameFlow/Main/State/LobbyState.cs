using UnityEngine;

public class LobbyState : IState
{

    public LobbyState(GameFlowContext context)
    {
        this.context = context;
    }
    private readonly GameFlowContext context;
    public void Enter()
    {
        context.SceneFlow.LoadLobby();
        context.InputMode.SetLobby();
        Debug.Log("Enter Lobby State");
    }

    public void Tick()
    {
    }

    public void Exit()
    {
        Debug.Log("Exit Lobby State");
    }
}
