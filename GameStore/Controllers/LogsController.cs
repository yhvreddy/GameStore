using GameStore.Dtos;
using GameStore.Interfaces;
using GameStore.Mapping;
using GameStore.Models;
using GameStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Controllers;

[ApiController]
[Route("logs")]
[Tags("Logs")]
[AllowAnonymous]
public class LogsController(ILogRepository logs, ILogService logService) : ControllerBase
{
    [HttpGet(Name = "GetLogs")]
    [ProducesResponseType<ApiResponse<IReadOnlyCollection<LogDto>>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<LogDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var logDtos = (await logs.GetAllAsync(cancellationToken))
            .Select(log => log.ToDto())
            .ToList();

        return Ok(ApiResponse<IReadOnlyCollection<LogDto>>.Ok(logDtos, "Logs retrieved successfully."));
    }

    [HttpPost(Name = "CreateLog")]
    [ProducesResponseType<ApiResponse<LogDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<LogDto>>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<LogDto>>> Create(CreateLogDto createLog, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(createLog.Level) ||
            string.IsNullOrWhiteSpace(createLog.Message))
        {
            return BadRequest(ApiResponse<LogDto>.Fail("Level and message are required."));
        }

        AppLog log = await logService.LogAsync(
            createLog.Level.Trim(),
            createLog.Message.Trim(),
            string.IsNullOrWhiteSpace(createLog.Source) ? null : createLog.Source.Trim(),
            createLog.UserId,
            string.IsNullOrWhiteSpace(createLog.Exception) ? null : createLog.Exception.Trim(),
            cancellationToken);

        return Created($"/logs/{log.Id}", ApiResponse<LogDto>.Ok(log.ToDto(), "Log created successfully."));
    }
}
