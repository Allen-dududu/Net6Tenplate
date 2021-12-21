using System;
using System.Threading.Tasks;

namespace Net6TemplateWebApi.Repertory
{
    public interface ITodoRepertory
    {
        Task<Todo> Get();
    }
}
