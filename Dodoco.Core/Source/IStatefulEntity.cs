namespace Dodoco.Core {

    public interface IStatefulEntity<T> {

        T State { get; }
        event EventHandler<T> OnStateUpdate;

    }

}