using DotnetCoreElasticsearch.Entities;
using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotnetCoreElasticsearch.ConsoleApp
{
    class Program
    {

        static void Main(string[] args)
        {
            CreateIndex();
            SuggestIndex();
        }

        public static void CreateIndex()
        {
            var host = "localhost";
            var indexName = "products";
            var pool = new SingleNodeConnectionPool(new Uri($"http://{host}:9200"));
            var defaultIndex = indexName;
            var connectionSettings = new ConnectionSettings(pool)
                    .DefaultIndex(defaultIndex)
                    .PrettyJson()
                    .DisableDirectStreaming();

            var client = new ElasticClient(connectionSettings);


            var text = "";
            int counter = 0;
            System.IO.StreamReader file = new System.IO.StreamReader(@"p.json");
            List<Product> products = new List<Product>();
            while ((text = file.ReadLine()) != null)
            {
                if (text.IndexOf("null") == -1)
                {

                    Product product = JsonConvert.DeserializeObject<Product>(text.Trim());
                    if (product.Name != "")
                    {
                        products.Add(product);
                     

                        counter++;
                        Console.WriteLine(counter);
                    }
                }
            }

            var response = client.IndexMany(products, indexName);

            Console.WriteLine($"Finish: {counter}");
            Console.ReadLine();
        }

        public static void SuggestIndex()
        {
            var indexName = "products_suggest";
            var node = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
            var settings = new ConnectionSettings(node);
            settings.DefaultIndex(indexName);
            settings.PrettyJson();
            settings.DisableDirectStreaming();

            var client = new ElasticClient(settings);


            var text = "";
            int counter = 0;
            System.IO.StreamReader file = new System.IO.StreamReader(@"p.json");
            List<Product> products = new List<Product>();
            while ((text = file.ReadLine()) != null)
            {
                if (text.IndexOf("null") == -1)
                {
                    Product product = JsonConvert.DeserializeObject<Product>(text.Trim());
                    if (product.Name != "")
                    {
                        List<string> array = product.Name.Split().ToList();
                        int count = array.Count;
                        string[] copyArray = new string[count];
                        array.CopyTo(copyArray);

                        for (int i = 0; i < count; i++)
                        {
                            var newString = string.Join(" ", copyArray, 0, i + 1);
                            array.Add(newString);
                        }

                        copyArray = array.Distinct().ToArray();

                        product.NameSuggest = new CompletionField()
                        {
                            Input = copyArray
                        };
                        products.Add(product);

                        counter++;
                        if (products.Count == 50000)
                        {
                            var response2 = client.IndexMany(products);
                            products.Clear();
                        }
                    }

                }

                Console.WriteLine(counter);
            }

            if (products.Count > 0)
            {
                var response2 = client.IndexMany(products);
                products.Clear();
            }

            Console.WriteLine($"Finish");


            Console.ReadLine();
        }
    }
}

