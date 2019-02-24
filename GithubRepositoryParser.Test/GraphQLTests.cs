using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GithubRepositoryParser.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;


namespace GithubGraphQl
{
    [TestClass]
    public class GithubGraphQlTests
    {
        private readonly string githubToken = null; // set in user secrets;

        public GithubGraphQlTests()
        {
            var configuration = ApplicationConfiguration.GetApplicationConfiguration();
            githubToken = configuration["GithubToken"];
        }

        [TestMethod]
        public async Task ApiGetRepositoryEntriesTest()
        {
            var productInformation = new ProductHeaderValue("GraphQLTest", "0.1");

            var connection = new Connection(productInformation, githubToken);
            var query = new Query()
                .Repository(name: "MarkdownMonster", owner: "RickStrahl")
                .Object(expression: "master:")
                .Cast<Tree>()
                .Entries.Select(x => new
                {
                    x.Name,
                    x.Type
                })
                .Compile();

            var json = query.ToString();
            Console.WriteLine(json);

            //var result = await connection.Run(json);
            //Console.WriteLine(result);

            var result = await connection.Run(query);

            Assert.IsNotNull(result);

            foreach (var file in result)
            {
                Console.WriteLine($"{file.Name} - {file.Type}");
            }
        }


        [TestMethod]
        public async Task GitHubRepositoryTreeTest()
        {            
            var tree = new GithubRepositoryParserGraphQL(githubToken, "RickStrahl", "Westwind.Scripting");

            var entries = await tree.GetFolder("master","");

            Console.WriteLine(entries.Count);
            Assert.IsNotNull(entries);

        }



        [TestMethod]
        public async Task GitHubRepositoryTreeGetContentTest()
        {

            var tree = new GithubRepositoryParserGraphQL(githubToken, "RickStrahl", "MarkdownMonster");

            var content = await tree.GetItemContent("master:README.md");

            Console.WriteLine(content);
            Assert.IsNotNull(content);
            Assert.IsTrue(content.Text.Contains("Markdown Monster"));
        }




        [TestMethod]
        public async Task ApiGetRepositoryItemTextTest()
        {
            var productInformation = new ProductHeaderValue("GraphQLTest", "0.1");

            var connection = new Connection(productInformation, githubToken);
            var query = new Query()
                .Repository(name: "MarkdownMonster", owner: "RickStrahl")
                .Object(expression: "master:README.md")
                .Cast<Blob>()
                .Select(x => new
                {
                    x.IsBinary,
                    x.ByteSize,
                    x.Text
                }).Compile();

            var json = query.ToString();
            Console.WriteLine(json);

            //var result = await connection.Run(json);
            //Console.WriteLine(result);

            var result = await connection.Run(query);

            Assert.IsNotNull(result);

            Console.WriteLine(result.ByteSize);
            Console.WriteLine(result.Text);
        }


        [TestMethod]
        public async Task RawJsonTest()
        {
            var productInformation = new ProductHeaderValue("GraphQLTest", "0.1");
            var connection = new Connection(productInformation, githubToken);


            string rawJson = @"
query {
  repository(name: ""MarkdownMonster"", owner: ""RickStrahl"") {
    object(expression: ""master:README.md"") {
      ... on Blob {
        text
      }
    }
 }
}";

            string vars = null;
            var query = new
            {
                query = rawJson,
                variables = vars
            };

            var json = JsonConvert.SerializeObject(query);

            //Console.WriteLine(json);

            var result = await connection.Run(json);

            Console.WriteLine(result);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("\"text\":"));
        }


        [TestMethod]
        public async Task RawJsonWithTestQueryTest()
        {
            var productInformation = new ProductHeaderValue("GraphQLTest", "0.1");
            var connection = new Connection(productInformation, githubToken);


            string rawJson = @"
query {
  repository(name: ""MarkdownMonster"", owner: ""RickStrahl"") {
    object(expression: ""master:README.md"") {
      ... on Blob {
        text
      }
    }
 }
}";
            var query = new TextQuery(rawJson);
            var result = await connection.Run(query.ToString());

            Console.WriteLine(result);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("\"text\":"));
        }
    }


    public class TextQuery
    {
        private readonly string _queryText;
        private readonly Dictionary<string, object> _variables;

        public TextQuery(string queryText, Dictionary<string, object> variables = null)
        {
            _queryText = queryText;
            _variables = variables;
        }

        public override string ToString()
        {
            var query = new
            {
                query = _queryText,
                _variables = _variables
            };

            var json = JsonConvert.SerializeObject(query);
            return json;
        }

    }
}
