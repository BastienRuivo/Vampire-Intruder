using Unity.VisualScripting;

namespace Interfaces
{
    public interface IEventObserver<TContext>
    {
        public void OnEvent(TContext context);
    }

    public interface IEventObserver : IEventObserver<Null>
    {
        public void OnEvent();
    }
}