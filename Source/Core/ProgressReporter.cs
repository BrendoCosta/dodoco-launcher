namespace Dodoco.Core {

    public class ProgressReporter<T>: IProgress<T> {

        public event EventHandler<T> ProgressChanged = delegate {};

        public void Report(T value) => this.ProgressChanged.Invoke(this, value);

    }

}