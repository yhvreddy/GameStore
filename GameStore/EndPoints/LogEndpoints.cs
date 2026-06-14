using GameStore.Dtos;
using GameStore.Models;
using GameStore.Services;

namespace GameStore.EndPoints;

public static class LogEndpoints
{
    public static void MapLogEndpoints(this WebApplication app)
    {
        var logsGroup = app.MapGroup("/logs")
            .WithTags("Logs")
            .AllowAnonymous();

        logsGroup.MapPost("/", async (CreateLogDto createLog, ILogService logService) =>
        {
            if (string.IsNullOrWhiteSpace(createLog.Level) ||
                string.IsNullOrWhiteSpace(createLog.Message))
            {
                return Results.BadRequest("Level and message are required.");
            }

            AppLog log = await logService.LogAsync(
                createLog.Level.Trim(),
                createLog.Message.Trim(),
                string.IsNullOrWhiteSpace(createLog.Source) ? null : createLog.Source.Trim(),
                createLog.UserId,
                string.IsNullOrWhiteSpace(createLog.Exception) ? null : createLog.Exception.Trim());

            LogDto logDto = new(
                log.Id,
                log.Level,
                log.Message,
                log.Source,
                log.UserId,
                log.Exception,
                log.CreatedAt);

            return Results.Created($"/logs/{log.Id}", logDto);
        })
        .WithName("CreateLog")
        .Produces<LogDto>(StatusCodes.Status201Created)
        .Produces<string>(StatusCodes.Status400BadRequest);
    }
}
