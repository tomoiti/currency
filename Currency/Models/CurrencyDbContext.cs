using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Models
{
    public class CurrencyDbContext : DbContext
    {
        public DbSet<Currency> Currencies { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Currency>().Property(e=>e.Code)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("UQ_Currency", 1) { IsUnique = true }));
        }
    }
}
