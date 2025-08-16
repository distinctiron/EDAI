using EDAI.Server.Data;
using EDAI.Shared.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace EDAI.Server.Tools;

public static class DataSeeder
{
    public static async Task SeedUserAsync(UserManager<EDAIUser> userManager, RoleManager<IdentityRole> roleManager, EdaiContext context)
    {
        var email = "metteferrum@gmail.com";
        var password = "Goose1609!";
        var organisation = context.Organisations.OrderBy(o => o.OrganisationId).First();

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new EDAIUser()
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Organisation = organisation
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create seed user {user.ToString()}");
            }
        }

        var role = "Admin";
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }

    }
}