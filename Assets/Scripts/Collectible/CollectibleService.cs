namespace Collectible
{
    public class CollectibleService : ICollectibelService
    {
        private readonly ISavedDataService _savedDataService;
        private readonly IEventDispatcherService _eventDispatcher;
        private readonly CollectibleModel _model;

        public CollectibleService()
        {
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            _eventDispatcher = ServiceLocator.GetService<IEventDispatcherService>();
            _model = _savedDataService.LoadData<CollectibleModel>();
        }
        
        public int TotalCoin { get; }
    }
}
