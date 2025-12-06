namespace Services
{
    public interface IUndoService: IService
    {
        bool UndoAvailable { get; set; }
    }
}