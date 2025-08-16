using Microsoft.AspNetCore.Identity;

namespace EDAI.Shared.Models.Entities;

public class EDAIUser : IdentityUser
{
    public Organisation Organisation { get; set; }
}