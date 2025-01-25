using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

using Model;

namespace DAL
{
    public class DataContext : DbContext
    {
        public DbSet<MemeSQL> MemeSQLs { get; set; }  // Набор данных для мемов

        public DataContext() : base("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\lisic\\source\\repos\\tgbot2\\DAL\\Database1.mdf;Integrated Security=True") 
        { }
    }
}