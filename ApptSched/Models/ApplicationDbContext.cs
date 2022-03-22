using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApptSched.Models.ViewModels;

namespace ApptSched.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> //class inherited for user login functionality //using the UserModel for identity verification
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options){}

        public DbSet<Appointment>Appointments { get; set; }


    }
}
