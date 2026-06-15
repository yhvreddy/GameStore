using GameStore.Dtos;
using GameStore.Interfaces;
using GameStore.Mapping;
using GameStore.Models;
using GameStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Controllers;

[ApiController]
[Route("roles")]
[Tags("Roles")]
[Authorize]
public class RolesController(IRoleRepository roles, ILogService logService) : ControllerBase
{
    [HttpGet(Name = "GetAllRoles")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<RoleDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var roleDtos = (await roles.GetAllAsync(cancellationToken))
            .Select(role => role.ToDto())
            .ToList();

        return Ok(ApiResponse<IReadOnlyCollection<RoleDto>>.Ok(roleDtos, "Roles retrieved successfully."));
    }

    [HttpGet("{id:int}", Name = "GetRoleById")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        Role? role = await roles.GetByIdAsync(id, cancellationToken);
        return role is null
            ? NotFound(ApiResponse<RoleDto>.Fail("Role was not found."))
            : Ok(ApiResponse<RoleDto>.Ok(role.ToDto(), "Role retrieved successfully."));
    }

    [HttpPost(Name = "CreateRole")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Create(RoleCreateDto roleDto, CancellationToken cancellationToken)
    {
        Role role = new() { Name = roleDto.Name, Slug = roleDto.Slug };
        await roles.AddAsync(role, cancellationToken);
        await logService.LogAsync("Information", $"Role created: {role.Name}.", "Roles.Create", cancellationToken: cancellationToken);

        return CreatedAtRoute(
            "GetRoleById",
            new { id = role.Id },
            ApiResponse<RoleDto>.Ok(role.ToDto(), "Role created successfully."));
    }

    [HttpPut("{id:int}", Name = "UpdateRole")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Update(int id, RoleCreateDto roleDto, CancellationToken cancellationToken)
    {
        Role? role = await roles.GetByIdAsync(id, cancellationToken);
        if (role is null)
        {
            return NotFound(ApiResponse<RoleDto>.Fail("Role was not found."));
        }

        role.Name = roleDto.Name;
        role.Slug = roleDto.Slug;
        await roles.SaveChangesAsync(cancellationToken);
        await logService.LogAsync("Information", $"Role updated: {role.Name}.", "Roles.Update", cancellationToken: cancellationToken);

        return Ok(ApiResponse<RoleDto>.Ok(role.ToDto(), "Role updated successfully."));
    }

    [HttpDelete("{id:int}", Name = "DeleteRole")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        Role? role = await roles.GetByIdAsync(id, cancellationToken);
        if (role is null)
        {
            return NotFound(ApiResponse<object>.Fail("Role was not found."));
        }

        await roles.DeleteAsync(role, cancellationToken);
        await logService.LogAsync("Information", $"Role deleted: {role.Name}.", "Roles.Delete", cancellationToken: cancellationToken);

        return Ok(ApiResponse<object>.Ok(null, "Role deleted successfully."));
    }
}
