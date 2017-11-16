using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracticeBotResults.Models
{
    public class PracticeBotDbContext : DbContext
    {
        public PracticeBotDbContext(DbContextOptions<PracticeBotDbContext> options) : base(options)
        {

        }
    }
}
