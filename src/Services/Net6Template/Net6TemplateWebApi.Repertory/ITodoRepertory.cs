using System;
using System.Threading.Tasks;

namespace Net6.Template.Repertory
{
    public interface ITodoRepertory
    {
        Task<Todo> Get();
    }
}
