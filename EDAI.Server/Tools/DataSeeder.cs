using EDAI.Server.Data;
using EDAI.Shared.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace EDAI.Server.Tools;

public static class DataSeeder
{
    public static async Task SeedUserAsync(UserManager<EDAIUser> userManager, RoleManager<IdentityRole> roleManager, EdaiContext context)
    {
        var organisation1 = new  Organisation{CVR = "43809679", Name = "Distinct", OrganisationId = 1};
        var organisation2 = new Organisation
            { CVR = "11748708", Name = "NEXT Københavns Mediegymnasium", OrganisationId = 2 };

        context.Set<Organisation>().Add(organisation1);
        context.Set<Organisation>().Add(organisation2);
        
        var adminRole = "Admin";
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }
        
        var userRole = "User";
        if (!await roleManager.RoleExistsAsync(userRole))
        {
            await roleManager.CreateAsync(new IdentityRole(userRole));
        }

        var users = new List<(EDAIUser, string, string)>();

        var user1 = (new EDAIUser {UserName = "s.ferrum@distinct.dk", Email = "s.ferrum@distinct.dk", EmailConfirmed = true, Organisation = organisation1}, "J4K05um4!", adminRole);
        users.Add(user1);
        
        var user2 = (new EDAIUser {UserName = "metteferrum@gmail.com", Email = "metteferrum@gmail.com", EmailConfirmed = true, Organisation = organisation1},"Goose2609!", userRole);
        users.Add(user2);
        
        var user3 = (new EDAIUser {UserName = "soda@nextkbh.dk", Email = "soda@nextkbh.dk", EmailConfirmed = true, Organisation = organisation2},"Fynerfin2025!",userRole);
        users.Add(user3);

        foreach (var userToSeed in users)
        {
            var user = await userManager.FindByEmailAsync(userToSeed.Item1.Email);

            if (user is null)
            {
                user = userToSeed.Item1;
                var result = await userManager.CreateAsync(userToSeed.Item1, userToSeed.Item2);
                if (!result.Succeeded)
                {
                    Console.WriteLine(result.ToString());
                    throw new Exception($"Failed to create seed user {userToSeed.Item1.ToString()}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, userToSeed.Item3))
            {
                await userManager.AddToRoleAsync(user, userToSeed.Item3);
            }

        }
        
        var studentClass = new StudentClass() { Class = "2X", Organisation = organisation2 };
        context.Set<StudentClass>().Add(studentClass);

        var students = new List<Student>();
            
        students.Add(new Student()
            { FirstName = "Angelina", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add(new Student()
            { FirstName = "Daniel", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Denis", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Devin", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Emil F", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Emil S", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Francesca", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Freja", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Frida", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Ida", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Isabella", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Lærke", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Mateusz", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Matt", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "My", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Philip", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Sara", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Sarah", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Silje", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Simon", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Solvej", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Sophia", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Sylvester", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Valdemar", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Vishal", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Yelyzaveta", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = "Zen", LastName = "Soda", Class = "2X", GraduationYear = 2027, StudentClass = studentClass});
                
        context.Set<Student>().AddRange(students);


        await context.SaveChangesAsync();

    }
    
    
}