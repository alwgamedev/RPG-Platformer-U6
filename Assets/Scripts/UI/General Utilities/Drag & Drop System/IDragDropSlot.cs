namespace RPGPlatformer.UI
{
    public interface IDragDropSlot<T> : IDragSource<T>, IDropTarget<T> where T : class { }
}