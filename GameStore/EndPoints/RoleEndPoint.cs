using GameStore.Data;
using GameStore.Dtos;
using GameStore.Models;
using GameStore.Services;
using Microsoft.EntityFrameworkCore;

namespace GameStore.EndPoints;

public static class RoleEndPoint
{
    public static void MapRoleEndpoints(this WebApplication app)
    {
        // Map the role endpoints
        var rolesGroup = app.MapGroup("/roles").WithTags("Roles").RequireAuthorization();

        // Endpoint to get all roles
        // GET /roles
        rolesGroup.MapGet("/", async (GameStoreContext context) => await context.Roles
        .Select(role => new RoleDto(
            role.Id,
            role.Name,
            role.Slug
        ))
        .AsNoTracking()
        .ToListAsync()).WithName("GetAllRoles").AllowAnonymous();

        // Endpoint to get a role by ID
        // GET /roles/{id}
        rolesGroup.MapGet("/{id:int}", async (int id, GameStoreContext context) =>
        {
            var role = await context.Roles
                .Where(r => r.Id == id)
                .Select(r => new RoleDto(
                    r.Id,
                    r.Name,
                    r.Slug
                ))
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return role is not null ? Results.Ok(role) : Results.NotFound();
        }).WithName("GetRoleById").AllowAnonymous();

        // Endpoint to create a new role
        // POST /roles
        rolesGroup.MapPost("/", async (RoleCreateDto roleDto, GameStoreContext context, ILogService logService) =>
        {
            var role = new Role
            {
                Name = roleDto.Name,
                Slug = roleDto.Slug
            };

            context.Roles.Add(role);
            await context.SaveChangesAsync();
            await logService.LogAsync("Information", $"Role created: {role.Name}.", "Roles.Create");

            return Results.Created($"/roles/{role.Id}", new RoleDto(role.Id, role.Name, role.Slug));
        }).WithName("CreateRole");


        // Endpoint to update an existing role
        // PUT /roles/{id}
        rolesGroup.MapPut("/{id:int}", async (int id, RoleCreateDto roleDto, GameStoreContext context, ILogService logService) =>
        {
            var role = await context.Roles.FindAsync(id);
            if (role is null)
            {
                return Results.NotFound();
            }

            role.Name = roleDto.Name;
            role.Slug = roleDto.Slug;
            await context.SaveChangesAsync();
            await logService.LogAsync("Information", $"Role updated: {role.Name}.", "Roles.Update");

            return Results.NoContent();
        }).WithName("UpdateRole");

        // Endpoint to delete a role
        // DELETE /roles/{id}
        rolesGroup.MapDelete("/{id:int}", async (int id, GameStoreContext context, ILogService logService) =>
        {
            var role = await context.Roles.FindAsync(id);
            if (role is null)
            {
                return Results.NotFound();
            }

            await context.Roles.Where(r => r.Id == id).ExecuteDeleteAsync();
            await logService.LogAsync("Information", $"Role deleted: {role.Name}.", "Roles.Delete");

            return Results.NoContent();
        }).WithName("DeleteRole");
    }
}
