using Nest;
using System;

namespace DotnetCoreElasticsearch.Entities
{
    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public CompletionField NameSuggest { get; set; }
    }
}
