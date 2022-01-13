using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Net6.Template.Repertory
{
    public class TodoRepertory : ITodoRepertory
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TodoRepertory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Todo> Get()
        {
            var client = _httpClientFactory.CreateClient(typeof(ITodoRepertory).Name);
            await client.GetAsync("u/6992432?s=60&v=4");

           return new Todo() { Id = 1 };
        }
    }
}
