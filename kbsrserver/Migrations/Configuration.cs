namespace kbsrserver.Migrations
{
    using Models;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Microsoft.AspNet.Identity;
    using System.Text;
    using System.Security.Cryptography;
    using Helpers;

    internal sealed class Configuration : DbMigrationsConfiguration<kbsrserver.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(kbsrserver.Models.ApplicationDbContext context)
        {
            const string name1 = "eldar.gamisoniya@gmail.com";
            const string name2 = "terrrevan@gmail.com";
            if (!(context.Users.Any(u => u.UserName == name1)))
            {
                var userStore = new UserStore<ApplicationUser>(context);
                var userManager = new UserManager<ApplicationUser>(userStore);
                var userToInsert = new ApplicationUser { UserName = name1, PhoneNumber = "123",
                    Email = name1};
                userManager.Create(userToInsert, "Password123*");
                
                context.SaveChanges();
            }

            if (!(context.Users.Any(u => u.UserName == name2)))
            {
                var userStore = new UserStore<ApplicationUser>(context);
                var userManager = new UserManager<ApplicationUser>(userStore);
                var userToInsert = new ApplicationUser
                {
                    UserName = name2,
                    PhoneNumber = "123",
                    Email = name2
                };
                var bytes = Encoding.UTF8.GetBytes("Password123*");
                var hasher = new SHA256Managed();
                var hashed = hasher.ComputeHash(bytes).ToHexString();
                userManager.Create(userToInsert, hashed);

                context.SaveChanges();
            }

            context.Notes.AddOrUpdate(
                    p => p.Name,
                    new Note { Id = 1, Name = "Hello", Text = "Hello, World!" },
                    new Note { Id = 2, Name = "Eldar", Text = @"Eldar Gamisoniya
BSU
GROUP 12" }
                );

            context.SaveChanges();

            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
