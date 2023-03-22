using Microsoft.EntityFrameworkCore;

namespace WebApi.Models
{
    public class CustomerContext: DbContext
    {
        public DbSet<Customer> Customers { get; set; }

        public CustomerContext()
        {
            
        }

        public CustomerContext(DbContextOptions<CustomerContext> options): base(options) 
        {

        }
    }
}
