namespace Dodoco.Core {

    public class Cache<T> {

        public TimeSpan MaxAge { get; private set; } = TimeSpan.Zero;
        public DateTime LastUpdate { get; private set; } = DateTime.Now;
        public T Resource { get; private set; }

        public Cache(T resource) {

            this.Resource = resource;
            this.LastUpdate = DateTime.Now;

        }

        public bool IsValid() {

            return (DateTime.Now - LastUpdate) < MaxAge;

        }

        public void Update(T newResource, TimeSpan maxAge) {

            this.Resource = newResource;
            this.LastUpdate = DateTime.Now;
            this.MaxAge = maxAge;

        }

    }

}