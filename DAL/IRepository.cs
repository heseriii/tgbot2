using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;

namespace DAL
{
    public interface IRepository<T> where T : IDomainObject, new()
    {
        void Create(T obj);
        List<T> ReadAll();
        T ReadById(int id);
        List<T> ReadByTags(string tags);
        List<T> ReadByDescription(string description);
        void Delete(T obj);
    }
}
