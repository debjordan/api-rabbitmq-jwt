using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("minha_chave_secreta_super_segura_1234567890"))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/send-message", async ([FromBody] dynamic payload) =>
{
    var factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
    using var connection = await factory.CreateConnectionAsync();
    using var channel = await connection.CreateChannelAsync();

    await channel.QueueDeclareAsync(queue: "user_updates", durable: true, exclusive: false, autoDelete: false, arguments: null);

    var message = JsonSerializer.Serialize(payload);
    var body = Encoding.UTF8.GetBytes(message);

    var properties = new BasicProperties();
    await channel.BasicPublishAsync(exchange: "", routingKey: "user_updates", mandatory: false, basicProperties: properties, body: body);

    return Results.Ok(new { Message = "Mensagem enviada para a fila!" });
}).RequireAuthorization();

app.Run();
