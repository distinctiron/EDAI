using AutoMapper;
using EDAI.Server.Data;
using EDAI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using EDAI.Shared.Models.DTO;
using EDAI.Shared.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganisationController(EdaiContext context, IMapper mapper, IOpenAiService openAiService) : ControllerBase
{
    [Authorize]
    [HttpGet(Name = "GetOrganisations")]
    public async Task<IEnumerable<OrganisationDto>> GetOrganisations()
    {
        var entities = await context.Organisations.ToListAsync();

        var organisations = mapper.Map<IEnumerable<OrganisationDto>>(entities);

        return organisations;
    }
}