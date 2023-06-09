using Dodoco.Serialization.Json;

using UrlCombineLib;

namespace Dodoco.Network.Api.Github.Repos {

    public class GitHubReposApiFactory: AbstractApi {

        public GitHubReposApiConfig Config { get; private set; }
        public Uri Url { get; private set; }

        public GitHubReposApiFactory(GitHubReposApiConfig config): base(new JsonSerializer()) {

            this.Config = config;
            this.Url = new Uri(UrlCombine.Combine("https://api.github.com/repos", this.Config.Owner, this.Config.Repository));

        }

        public async Task<List<Content.Content>?> FetchContents(string path = "/") {

            return await this.FetchApi<List<Content.Content>>(UrlCombine.Combine(this.Url.ToString(), "contents", path));

        }

        public async Task<List<Release.Release>?> FetchReleases() {

            return await this.FetchApi<List<Release.Release>>(UrlCombine.Combine(this.Url.ToString(), "releases"));

        }

        public async Task<Release.Release?> FetchReleaseById(int id) {

            return await this.FetchApi<Release.Release>(UrlCombine.Combine(this.Url.ToString(), "releases", id.ToString()));

        }

    }

}