namespace Dodoco.Application.Control {

    public interface IController<T> {

        T? GetViewData();

    }

}