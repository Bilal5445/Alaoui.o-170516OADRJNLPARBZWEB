using OADRJNLPCommon.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Domain.Models
{
    public class ArabiziDbContext : DbContext
    {
        public ArabiziDbContext()
        : base("ConnLocalDBArabizi")
        {
        }

        public DbSet<M_ARABICDARIJAENTRY> M_ARABICDARIJAENTRYs { get; set; }
        public DbSet<M_ARABICDARIJAENTRY_LATINWORD> M_ARABICDARIJAENTRY_LATINWORDs { get; set; }
        public DbSet<M_ARABICDARIJAENTRY_TEXTENTITY> M_ARABICDARIJAENTRY_TEXTENTITYs { get; set; }
        public DbSet<M_ARABIZIENTRY> M_ARABIZIENTRYs { get; set; }
        public DbSet<M_TWINGLYACCOUNT> M_TWINGLYACCOUNTs { get; set; }
        public DbSet<M_XTRCTTHEME> M_XTRCTTHEMEs { get; set; }
        public DbSet<M_XTRCTTHEME_KEYWORD> M_XTRCTTHEME_KEYWORDs { get; set; }

        // authentication entities
        public DbSet<RegisterUser> RegisterUsers { get; set; }
        public DbSet<RegisterApp> RegisterApps { get; set; }
        public DbSet<TokensManager> TokensManager { get; set; }
        public DbSet<ClientKeys> ClientKeys { get; set; }
        public DbSet<RegisterAppCallingLog> RegisterAppCallingLogs { get; set; }
        public DbSet<ScheduleTask> ScheduleTasks { get; set; }
    }

    public class ScrappyWebDbContext : DbContext
    {
        public ScrappyWebDbContext()
            : base("ScrapyWebEntities")
        {
        }

        public DbSet<FB_POST> T_FB_POST { get; set; }
    }
}
