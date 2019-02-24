# Github Repository Content Samples

Personal sample project for retrieving Github repository listings and item content using the GitHub API and the new new GraphQL interface.

Built this to be able to retrieve content from GIthub without delays that are inherent in the **raw** (non-API) URL interface available.

Contains two classes:

* **RepositoryParser**  
Uses the classic API to retrieve a tree of items that describe each item in the repository. This API can be called in one pass and is pretty quick. Also includes method to retrieve content of an individual that provides real time updates that bypass caching.

* **RepositoryParserGraphQL**  
Provides the same functionality but uses GraphQL. Note the directory listing is considerably slower as there's no way I coudl find with GraphQL to retrieve the hierarchical list in one shot. Instead this sample recursively loads each tree with seperate requests.


This project is just a sample for my own reference experimenting with the GraphQL API only to find that the classic API is much more efficient and considerably easier to use. 