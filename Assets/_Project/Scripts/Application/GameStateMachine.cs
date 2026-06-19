using System;
using ActiveTimeBattle.Domain;

namespace ActiveTimeBattle.Application
{
    public sealed class GameStateMachine
    {
        public GameFlowState CurrentState { get; private set; } = GameFlowState.Boot;

        public event Action<GameFlowState> StateChanged;

        public void ChangeState(GameFlowState nextState)
        {
            if (CurrentState == nextState)
            {
                return;
            }

            CurrentState = nextState;
            StateChanged?.Invoke(nextState);
        }
    }
}
