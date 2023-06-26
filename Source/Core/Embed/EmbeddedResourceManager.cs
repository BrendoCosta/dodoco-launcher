using Dodoco.Core.Game;
using Dodoco.Core.Network.Api.Company.Launcher.Resource;
using Dodoco.Core.Serialization.Json;

using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Dodoco.Core.Embed {

    public static class EmbeddedResourceManager {

        private static string GetEmbeddedResourceContent(string resourceName) {

            using (Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) {

                if (stream == null) {

                    throw new CoreException($"Can't get a stream to read the requested resource's content");

                } else {

                    using (StreamReader source = new StreamReader(stream)) {

                        return source.ReadToEnd();

                    }

                }

            }

            throw new CoreException($"Can't get the requested resource's content");

        }

        public static Resource GetLauncherResource(GameServer gameServer, Version gameVersion) {

            foreach (string resourceName in Assembly.GetExecutingAssembly().GetManifestResourceNames()) {

                if (Regex.IsMatch(resourceName, $"(Launcher.Resource.resource_{gameVersion.ToString().Replace(".", "")}_{gameServer.ToString().ToLower()})")) {

                    return new JsonSerializer().Deserialize<Resource>(Encoding.UTF8.GetString(Convert.FromBase64String(EmbeddedResourceManager.GetEmbeddedResourceContent(resourceName))));

                }

            }

            throw new CoreException($"Can't find the requested resource");

        }

    }

}