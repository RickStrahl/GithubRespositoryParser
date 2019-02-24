using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GithubRepositoryParser.Test
{
    public class ApplicationConfiguration
    {
        public static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()                
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets("edcf8f9e-f209-451e-a606-18ac97d2b429")                
                .Build();
        }

        public static IConfiguration GetApplicationConfiguration()
        {
            var configuration = new ApplicationConfiguration();

            var iConfig = GetIConfigurationRoot();
            return iConfig;
        }
    }
}
