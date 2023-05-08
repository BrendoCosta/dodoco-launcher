namespace Dodoco.Api.Company.Launcher {

    public class Content: CompanyApi {

        public Data data { get; set; }

        // Schema

        public struct Adv {

            public string background { get; set; }
            public string icon { get; set; }
            public string url { get; set; }
            public string version { get; set; }
            public string bg_checksum { get; set; }

        }

        public struct Banner {

            public string banner_id { get; set; }
            public string name { get; set; }
            public string img { get; set; }
            public string url { get; set; }
            public string order { get; set; }

        }

        public struct Data {

            public Adv adv { get; set; }
            public List<Banner> banner { get; set; }
            public List<Icon> icon { get; set; }
            public List<Post> post { get; set; }
            public List<object> qq { get; set; }
            public More more { get; set; }
            public Links links { get; set; }

        }

        public struct Icon {

            public string icon_id { get; set; }
            public string img { get; set; }
            public string tittle { get; set; }
            public string url { get; set; }
            public string qr_img { get; set; }
            public string qr_desc { get; set; }
            public string img_hover { get; set; }
            public List<OtherLink> other_links { get; set; }
            public string title { get; set; }
            public string icon_link { get; set; }
            public List<Link> links { get; set; }

        }

        public struct Link {

            public string title { get; set; }
            public string url { get; set; }
            public string faq { get; set; }
            public string version { get; set; }

        }

        public struct More {

            public string activity_link { get; set; }
            public string announce_link { get; set; }
            public string info_link { get; set; }
            public string news_link { get; set; }
            public string trends_link { get; set; }
            public string supply_link { get; set; }
            public string tools_link { get; set; }

        }

        public struct Links {

            public string faq { get; set; }
            public int version { get; set; }

        }

        public struct OtherLink {

            public string title { get; set; }
            public string url { get; set; }

        }

        public struct Post {

            public string post_id { get; set; }
            public string type { get; set; }
            public string tittle { get; set; }
            public string url { get; set; }
            public string show_time { get; set; }
            public string order { get; set; }
            public string title { get; set; }

        }

    }

}