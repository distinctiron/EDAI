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
        
        var studentClass = new StudentClass() { Class = "1C", Organisation = organisation1 };
        context.Set<StudentClass>().Add(studentClass);

        var students = new List<Student>();
            
        students.Add(new Student()
            { FirstName = "Alice", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add(new Student()
            { FirstName = " Alma", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Amalie", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Anne Mona", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Asbjørn", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " August", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Camille", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Carl", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Carla", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Coco", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Ella", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Elsa", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Esther", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Ida H", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Ida C", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Ingrid", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Janus", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Jonathan", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Karl", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Laura", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Luka", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Noah", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Othilia", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Pelle", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Theodor", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Timian", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
        students.Add( new Student()
            { FirstName = " Ursula", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass});
                
        context.Set<Student>().AddRange(students);


        await context.SaveChangesAsync();

    }
    
    
}