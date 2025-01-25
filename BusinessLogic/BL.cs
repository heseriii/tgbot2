using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;
using DAL;

namespace BusinessLogic
{
    public class BL
    {
        IRepository<MemeSQL> repository = new EntityMemeRepository(new DataContext());
        public void AddMeme(string tags, string description, string imageId)
        {
            MemeSQL meme = new MemeSQL();
            meme.ImageId = imageId;
            meme.Tags = tags;
            meme.Description = description;
            if (!String.IsNullOrEmpty(description) && !String.IsNullOrEmpty(Convert.ToString(imageId)))
            {
                repository.Create(meme);
            }
        }
        public List<Meme> GetMemesByTags(string tags)
        {
            return ListMeme(repository.ReadByTags(tags));
        }
        public List<Meme> GetMemesByDescription(string desc)
        {
            return ListMeme(repository.ReadByDescription(desc));
        }
        public List<Meme> GetAllMemes()
        {
            return ListMeme(repository.ReadAll());
        }
        public Meme MemeSQLtoMeme(MemeSQL memesql)
        {
            Meme meme = new Meme();
            meme.Id = memesql.Id;
            meme.ImageId=memesql.ImageId;
            meme.Description=memesql.Description;
            meme.Tags=memesql.Tags;
            return meme;
        }
        public List<Meme> ListMeme(List<MemeSQL> list)
        {
            List<Meme> listmeme = new List<Meme>();
            foreach (var meme in list)
            {
                listmeme.Add(MemeSQLtoMeme(meme));
            }
            return listmeme;
        }
   
    }   
}
