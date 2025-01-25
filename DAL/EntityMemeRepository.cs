using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;
using System.Data.Entity;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class EntityMemeRepository: IRepository<MemeSQL>
    {

        public EntityMemeRepository(DataContext context)
        {
            _context = context;
        }
        private DataContext _context;
        public void Create(MemeSQL meme)
        {
            _context.Set<MemeSQL>().Add(meme);
            _context.SaveChanges();
        }
        public List<MemeSQL> ReadAll()
        {
            return new List<MemeSQL>(_context.Set<MemeSQL>());
        }
        public MemeSQL ReadById(int id)
        {
            return _context.MemeSQLs.Where(o => o.Id == id).FirstOrDefault();
        }
        public List<MemeSQL> ReadByTags(string tagsstr)
        {
            List<string> tags = tagsstr.Split(' ').ToList();
            return _context.MemeSQLs
                .Where(meme => tags.All(tag => meme.Tags.Contains(tag)))
                .ToList();
        }
        public List<MemeSQL> ReadByDescription(string description)
        {
            var searchTerms = description.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int allowedMissingWords = 0;

            return _context.MemeSQLs
                .Where(meme => searchTerms.Count(term => !meme.Description.Contains(term)) <= allowedMissingWords)
                .ToList();
        }
        public void Delete(MemeSQL meme)
        {
            _context.Set<MemeSQL>().Remove(meme);
            _context.SaveChanges();
        }
    }
}
