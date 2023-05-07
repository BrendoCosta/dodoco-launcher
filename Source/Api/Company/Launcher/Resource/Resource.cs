namespace Dodoco.Api.Company.Launcher.Resource {

    // Resource myDeserializedClass = JsonConvert.DeserializeObject<Resource>(myJsonString);

    public struct Resource {

        public int retcode { get; set; }
        public string message { get; set; }
        public Data data { get; set; }

    }

}