using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetCoreElasticsearch.Entities;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Mvc;
using Nest;

namespace DotnetCoreElasticsearch.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        // GET api/products
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Products");
        }

        // GET api/products/test
        [HttpGet("{searchText}")]
        public IActionResult Get(string searchText)
        {
            searchText = searchText.ToUpperInvariant();

            var host = "localhost";
            var indexName = "products_suggest";
            var pool = new SingleNodeConnectionPool(new Uri($"http://{host}:9200"));
            var defaultIndex = indexName;
            var connectionSettings = new ConnectionSettings(new Uri($"http://{host}:9200"));
            connectionSettings.DefaultIndex(defaultIndex);
            connectionSettings.PrettyJson();
            connectionSettings.DisableDirectStreaming();

            var client = new ElasticClient(connectionSettings);


            var response = client.Search<Product>(s => s
                .Query(q => q
                .QueryString(qs => qs
                     .Query("nameSuggest.input:" + searchText)
                 ))
                 .Size(500)
                 );

            var res = response.Documents;

            return Ok(res);

        }

        [HttpGet("search/{searchText}")]
        public IActionResult Search(string searchText)
        {


            searchText = searchText.ToUpperInvariant();

            var host = "localhost";
            var indexName = "products";
            var pool = new SingleNodeConnectionPool(new Uri($"http://{host}:9200"));
            var defaultIndex = indexName;
            var connectionSettings = new ConnectionSettings(new Uri($"http://{host}:9200"));
            connectionSettings.DefaultIndex(defaultIndex);
            connectionSettings.PrettyJson();
            connectionSettings.DisableDirectStreaming();

            var client = new ElasticClient(connectionSettings);


            var query = new MatchPhrasePrefixQuery
            {
                Field = "name",
                Analyzer = "standard",
                Boost = 1.1,
                Name = "named_query",
                Query = searchText,
                Slop = 2,

            };

            var request = new SearchRequest(indexName)
            {
                Size = 500,
                Query = query
            };

            var response = client.Search<Product>(request);
            var res = response.Documents;

            return Ok(res);
        }

    }



}
