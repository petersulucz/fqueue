namespace fqueue.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection.Metadata.Ecma335.Blobs;

    using FQueue.Logging;

    using Newtonsoft.Json.Linq;
    using static FQueue.Logging.Logger;

    public class ConfigurationManager
    {
        private const string ConfigurationFileName = "appconfig.json";

        public static ConfigurationManager Instance => new ConfigurationManager();

        private ConfigurationManager()
        {
            this.ReadConfiguration();
        }

        private void ReadConfiguration()
        {
            string text = null;
            JObject jConfig = null;

            try
            {
                text = File.ReadAllText(ConfigurationFileName);
            }
            catch (Exception e)
            {
                Trace.Critical($"Could not find config file. {e.Message}");
                Environment.FailFast("Could not find configuration.");
            }

            try
            {
                jConfig = JObject.Parse(text);
            }
            catch (Exception e)
            {
                Trace.Critical($"Configuration is invalid json. {e.Message}");
                Environment.FailFast("Configuration is invalid json.");
            }

            this.Queues = ((JArray)jConfig["queues"]).Select(q => new QueueConfigItem() { Name = q["name"].ToObject<string>() });
        }

        public IEnumerable<QueueConfigItem> Queues { get; private set; }
    }
}
