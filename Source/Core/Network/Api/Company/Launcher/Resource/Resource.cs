namespace Dodoco.Core.Network.Api.Company.Launcher.Resource {

    public class Resource: CompanyApi {

        public Data data { get; set; }

        // Schema

        public struct Data {

            public Game game { get; set; }
            public Plugin plugin { get; set; }
            public string web_url { get; set; }
            public object force_update { get; set; }
            public Game? pre_download_game { get; set; }
            public List<DeprecatedPackage> deprecated_packages { get; set; }
            public object sdk { get; set; }
            public List<DeprecatedFile> deprecated_files { get; set; }

        }

        public struct DeprecatedFile {

            public string name { get; set; }
            public string md5 { get; set; }
            
        }

        public struct DeprecatedPackage {

            public string name { get; set; }
            public string md5 { get; set; }
            
        }

        public struct Diff {

            public string name { get; set; }
            public string version { get; set; }
            public string path { get; set; }
            public long size { get; set; }
            public string md5 { get; set; }
            public bool is_recommended_update { get; set; }
            public List<VoicePack> voice_packs { get; set; }
            public long package_size { get; set; }
            
        }

        public struct Game {

            public Latest latest { get; set; }
            public List<Diff> diffs { get; set; }
            
        }

        public struct Latest {

            public string name { get; set; }
            public string version { get; set; }
            public string path { get; set; }
            public long size { get; set; }
            public string md5 { get; set; }
            public string entry { get; set; }
            public List<VoicePack> voice_packs { get; set; }
            public Uri decompressed_path { get; set; }
            public List<Segment> segments { get; set; }
            public long package_size { get; set; }
            
        }

        public struct Plugin {

            public List<PluginEntry> plugins { get; set; }
            public string version { get; set; }
            
        }

        public struct PluginEntry {

            public string name { get; set; }
            public string version { get; set; }
            public string path { get; set; }
            public long size { get; set; }
            public string md5 { get; set; }
            public string entry { get; set; }
            
        }

        public struct Segment {

            public string path { get; set; }
            public string md5 { get; set; }
            public long package_size { get; set; }
            
        }

        public struct VoicePack {

            public string language { get; set; }
            public string name { get; set; }
            public string path { get; set; }
            public long size { get; set; }
            public string md5 { get; set; }
            public long package_size { get; set; }
            
        }

    }

}