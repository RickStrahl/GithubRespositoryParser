using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GithubRepositoryParser.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;

namespace GithubGraphQl
{
    [TestClass]
    public class GithubApiTests
    {
        
        private readonly string githubToken = null;  // set in UserSecrets

        public GithubApiTests()
        {
            var configuration = ApplicationConfiguration.GetApplicationConfiguration();
            githubToken = configuration["GithubToken"];
        }

        [TestMethod]
        public async Task GetFullRepoTest()
        {
            var tree = new GithubRepositoryParser("RickStrahl", "MarkdownMonster", githubToken);

            var entries = await tree.GetFolder("master", "");

            PrintItems(entries);

            Console.WriteLine(entries);
            Assert.IsNotNull(entries);
        }

        [TestMethod]
        public async Task GetFolderRepoTest()
        {
            var parser = new GithubRepositoryParser("RickStrahl", "MarkdownMonster",githubToken);

            // Get subfolder
            var entries = await parser.GetFolder("master", "AddIns");

            PrintItems(entries);

            Assert.IsNotNull(entries);
        }



        [TestMethod]
        public async Task GithubRepositoryPrivateRepoTest()
        {
            var tree = new GithubRepositoryParser("RickStrahl", "Westwind.WebConnection", githubToken);

            var entries = await tree.GetFolder("master", "");

            PrintItems(entries);

            Console.WriteLine(entries);
            Assert.IsNotNull(entries);
        }


        [TestMethod]
        public async Task GetItemContentTest()
        {
            var parser = new GithubRepositoryParser("RickStrahl", "MarkdownMonster",githubToken);

            var contentItem = await parser.GetItemContent("AddIns/ScreenCaptureAddin/ScreenCaptureConfigurationForm.xaml");

            Console.WriteLine(contentItem.Text);

            Assert.IsNotNull(contentItem);
        }





        public void PrintItems(List<GithubFolderItem> items, int level = 0)
        {
            string leader = new StringBuilder().Insert(0, "-", level).ToString();

            foreach (var item in items)
            {
                Console.WriteLine(leader + item.FullPath);

                if (item.Items != null)
                    PrintItems(item.Items, level + 1);
            }
        }
    }


}