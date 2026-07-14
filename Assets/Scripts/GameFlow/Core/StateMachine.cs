using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public string CurrentStateName => currentState?.GetType().Name ?? "None";
    private IState currentState;

    public IState CurrentState => currentState;

    public void ChangeState(IState nextState)
    {
        if (currentState == nextState)
        {
            return;
        }

        currentState?.Exit();
        currentState = nextState;
        currentState?.Enter();
    }

    private void Update()
    {
        currentState?.Tick();
    }
}