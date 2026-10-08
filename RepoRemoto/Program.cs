using RepoRemoto.Dto;
using RepoRemoto.Entity;
using RepoRemoto.Errors;
using RepoRemoto.Infraestructure;
using RepoRemoto.Notifications;
using RepoRemoto.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationDependencies(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

using (var scope = app.Services.CreateScope())
{
    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
    
    notificationService.Observable.Subscribe(
        notif =>
        {
            var timestamp = notif.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            Console.ForegroundColor = notif.Tipo switch
            {
                TipoNotificacion.Create => ConsoleColor.Green,
                TipoNotificacion.Update => ConsoleColor.Yellow,
                TipoNotificacion.Delete => ConsoleColor.Red,
                _ => ConsoleColor.Magenta
            };

            Console.WriteLine($"[Rx.NET] [{timestamp}] [{notif.Tipo}] -> {notif.Mensaje}");
            Console.ResetColor();
        },
        ex => Console.WriteLine($"[Rx.NET Error] -> {ex.Message}"),
        () => Console.WriteLine("[Rx.NET] Flujo de notificaciones finalizado.")
    );
}

var usersApi = app.MapGroup("/api/users");

usersApi.MapGet("/", async (IUserService userService) =>
{
    var users = await userService.GetAllAsync();
    return Results.Ok(users);
});

usersApi.MapGet("/export", async (IUserService userService) =>
{
    var filePath = await userService.ExportarJsonAsync();
    return Results.Ok(new { Message = "Exportación realizada con éxito", Path = filePath });
});

usersApi.MapGet("/{id:int}", async (int id, IUserService userService) =>
{
    var result = await userService.GetByIdAsync(id);

    if (result.IsSuccess)
    {
        return Results.Ok(result.Value);
    }

    return result.Error switch
    {
        DomainError.NotFound => Results.NotFound(result.Error),
        _ => Results.BadRequest(result.Error)
    };
});

usersApi.MapPost("/", async (CreateUserRequest request, IUserService userService) =>
{
    var result = await userService.CreateAsync(request);

    if (result.IsSuccess)
    {
        return Results.Created($"/api/users/{result.Value.Id}", result.Value);
    }

    return result.Error switch
    {
        DomainError.ValidationError ve => Results.BadRequest(new { ve.Field, ve.Message }),
        _ => Results.BadRequest(result.Error)
    };
});

usersApi.MapPut("/{id:int}", async (int id, CreateUserRequest request, IUserService userService) =>
{
    var result = await userService.UpdateAsync(id, request);

    if (result.IsSuccess)
    {
        return Results.Ok(result.Value);
    }

    return result.Error switch
    {
        DomainError.NotFound => Results.NotFound(result.Error),
        DomainError.ValidationError ve => Results.BadRequest(new { ve.Field, ve.Message }),
        _ => Results.BadRequest(result.Error)
    };
});

usersApi.MapDelete("/{id:int}", async (int id, IUserService userService) =>
{
    var result = await userService.DeleteAsync(id);

    if (result.IsSuccess)
    {
        return Results.NoContent();
    }

    return result.Error switch
    {
        DomainError.NotFound => Results.NotFound(result.Error),
        _ => Results.BadRequest(result.Error)
    };
});

app.Run();