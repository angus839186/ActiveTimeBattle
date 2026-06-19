namespace ActiveTimeBattle.Application
{
    public sealed class HubContext
    {
        public bool IsBusy { get; private set; }

        public void SetBusy(bool isBusy)
        {
            IsBusy = isBusy;
        }
    }
}
