public abstract class ViewElement : UiElement
{
    public void Open() => OnOpen();
    public void Close() => OnClose();
    protected abstract void OnOpen();
    protected abstract void OnClose();

}