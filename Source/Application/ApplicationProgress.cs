namespace Dodoco.Application {

    public class ApplicationProgress<T>: IProgress<T> {

        public event EventHandler<T> ProgressChanged = delegate {};

        public void Report(T value) => this.ProgressChanged.Invoke(this, value);

    }

}