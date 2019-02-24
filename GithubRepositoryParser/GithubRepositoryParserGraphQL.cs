using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;

namespace GithubGraphQl
{
    public class GithubRepositoryParserGraphQL
    {
        private readonly string _token;
        private readonly string _owner;
        private readonly string _repository;

        public string ApiName { get; set; } = "GraphQLTest";
        public string ApiVersion { get; set; } = "0.1";

        public GithubRepositoryParserGraphQL(string token, string owner, string repository,
            string apiName = null, string apiVersion = null)
        {
            _token = token;
            _owner = owner;
            _repository = repository;

            if (apiName != null)
                ApiName = apiName;
            if (apiVersion != null)
                ApiName = apiName;
        }


        #region Get Folder Tree Helpers

        /// <summary>
        /// Retrieves all files in a given Github branch/path. Optionally
        /// allows recursively retrieving all content in child folders/trees.
        ///
        /// Note: child folders are lazy loaded so recursive action can  
        /// result in a lot of separate HTTP requests made to the server
        /// </summary>
        /// <param name="path">Branch and path to to start parsing on using `branch:path` syntax:
        /// Example: `master:` or `master:subpath\subpath2`</param>
        /// <param name="recursive">If true recurses any subpaths</param>
        /// <returns>List of Items optionally with child items</returns>
        public async Task<List<GithubFolderItem>> GetFolder(string branch = "master", string path = null,
            bool recursive = true)
        {
            var productInformation = new ProductHeaderValue(ApiName, ApiVersion);

            var connection = new Connection(productInformation, _token);
            var query = new Query()
                .Repository(name: _repository, owner: _owner)
                .Object(expression: branch + ":" + path)
                .Cast<Tree>()
                .Entries.Select(x => new GithubFolderItem
                {
                    Name = x.Name,
                    Type = x.Type,
                })
                .Compile();

            var entries = await connection.Run(query);


            foreach (var entry in entries)
            {
                entry.FullPath = path + entry.Name;

                if (!recursive || entry.Type != "tree")
                    continue;

                entry.Items = await GetFolder(branch, entry.FullPath + "/");
            }

            return entries.ToList();
        }
        #endregion

        #region Item Helpers

        /// <summary>
        /// Retrieve the contents of an file/resource from a
        /// from a Github repository. Provide a path to retrieve in
        /// `branch:path\file.md` format.
        /// </summary>
        /// <param name="path">path to a resource: `master:file.txt` or `master:path/file.txt`</param>
        /// <returns>file content or throws</returns>
        public async Task<GithubContentItem> GetItemContent(string path)
        {
            var productInformation = new ProductHeaderValue(ApiName, ApiVersion);
            var connection = new Connection(productInformation, _token);
            var query = new Query()
                .Repository(name: _repository, owner: _owner)
                .Object(expression: path)
                .Cast<Blob>()
                .Select(x => new GithubContentItem
                {
                    FullPath = path,
                    IsBinary = x.IsBinary,
                    ByteSize = x.ByteSize,
                    Text = x.Text
                }).Compile();

            var result = await connection.Run(query);

            result.Name = Path.GetFileName(path);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<GithubContentItem> GetItemContent(GithubFolderItem item)
        {
            return await GetItemContent(item.FullPath);
        }

        #endregion
    }

    /// <summary>
    /// Holds information about an individual folder item
    /// </summary>    
    public class GithubFolderItem
    {
        /// <summary>
        /// The simple name of this file/resource
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type: blob or tree
        /// </summary>
        public string Type { get; set; }        

        /// <summary>
        /// Full Github path to this item
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// Size of a file resource
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// If this is a tree, contains child FolderItems entries
        /// </summary>
        public List<GithubFolderItem> Items { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Items?.Count ?? 0})";
        }
    }

   
    /// <summary>
    /// An individual Github file/resource item
    /// </summary>
    public class GithubContentItem
    {
        /// <summary>
        /// The simple name of the resource blob
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The full path to the resource
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int ByteSize { get; set; }        

        /// <summary>
        /// If the contents is binary data
        /// </summary>
        public bool IsBinary { get; set; }
        
        /// <summary>
        /// Contents of the file/resource
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Sha hash for this file for easy access
        /// https://api.github.com/repos/RickStrahl/MarkdownMonster/git/blobs/68f52879fc3c4ae4299d921ab266bd707415adb3
        /// </summary>
        public string Sha { get; set; }

        public override string ToString()
        {
            return $"{Name}  ({ByteSize.ToString("n0")})";
        }
    }


    public class JsonItem
    {
        public string Path { get; set; }
        public string Type { get; set; }

        public int Size { get; set; }
    }
}
