namespace Dodoco.Network.HTTP.SSE {

    public record Event {

        public string? eventName;
        public string? data;
        public string? id;
        public string? retry;

        public override string ToString() {

            string toSent = "";

            if (this.eventName != null) {

                toSent += $"event: {this.eventName}\n";

            }

            if (this.data != null) {

                toSent += $"data: {this.data}\n";

            }

            if (this.id != null) {

                toSent += $"id: {this.id}\n";

            }

            if (this.retry != null) {

                toSent += $"retry: {this.retry}\n";

            }

            toSent += $"\n";

            return toSent;

        }

    }

}