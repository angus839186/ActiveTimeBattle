namespace ActiveTimeBattle.Domain
{
    public enum GameFlowState
    {
        Boot,
        Hub,
        RunGeneration,
        Exploration,
        Combat,
        Reward,
        GameOver,
        ReturnToHub
    }
}
