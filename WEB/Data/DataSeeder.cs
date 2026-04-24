using Microsoft.AspNetCore.Identity;

namespace WEB.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider,IConfiguration configuration)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
         configuration = serviceProvider.GetRequiredService<IConfiguration>();

     
        string[] roles = { AppRoles.Administrador, AppRoles.JefeProceso, AppRoles.JefeArea, AppRoles.UsuarioNormal };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

   
        var adminSection = configuration.GetSection("AdminUser");
        var email = adminSection["Email"];
        var password = adminSection["Password"];

        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        {
            var adminUser = await userManager.FindByEmailAsync(email);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser { Email = email, UserName = email, EmailConfirmed = true, FullName = "Admin" };
                await userManager.CreateAsync(adminUser, password);
                await userManager.AddToRoleAsync(adminUser, AppRoles.Administrador);
            }
        }
        
        var normalUserSection = configuration.GetSection("NormalUser");
        var emailNormalUser = normalUserSection["Email"];
        var passwordNormalUser = normalUserSection["Password"];
        var fullNameNormalUser = normalUserSection["FullName"];
        
        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(passwordNormalUser))
        {
            var normalUser = await userManager.FindByEmailAsync(emailNormalUser);
            if (normalUser == null)
            {
                normalUser = new ApplicationUser { Email = emailNormalUser, UserName = emailNormalUser, EmailConfirmed = true, FullName = fullNameNormalUser };
                await userManager.CreateAsync(normalUser, passwordNormalUser);
                await userManager.AddToRoleAsync(normalUser, AppRoles.UsuarioNormal);
            }
        }
    }
}