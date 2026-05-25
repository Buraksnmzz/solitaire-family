namespace Levels
{
    public class GameModeSelectionModel : IModel
    {
        public GameMode SelectedGameMode { get; set; } = GameMode.Classic;
    }
}