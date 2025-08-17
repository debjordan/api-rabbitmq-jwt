using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configurar HttpClient para chamar API 1
builder.Services.AddHttpClient();

var app = builder.Build();

app.MapGet("/consume", async (IHttpClientFactory clientFactory) =>
{
    // Gerar JWT
    var claims = new[]
    {
        new Claim("user_id", "123")
    };
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("minha_chave_secreta_super_segura_1234567890"));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(15),
        signingCredentials: creds
    );
    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

    // Enviar mensagem para API 1
    var client = clientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenString);
    var payload = new { UserId = "user_123", NewEmail = "newemail@example.com" };
    var response = await client.PostAsJsonAsync("http://localhost:5000/send-message", payload);

    if (!response.IsSuccessStatusCode)
    {
        return Results.Problem($"Falha ao enviar mensagem para API 1: {await response.Content.ReadAsStringAsync()}", statusCode: (int)response.StatusCode);
    }

    // Configurar conexão com RabbitMQ para consumo
    var factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
    using var connection = await factory.CreateConnectionAsync();
    using var channel = await connection.CreateChannelAsync();

    // Declarar a fila
    await channel.QueueDeclareAsync(queue: "user_updates", durable: true, exclusive: false, autoDelete: false, arguments: null);

    // Consumir mensagens da fila
    var receivedMessages = new List<string>();
    var consumer = new AsyncEventingBasicConsumer(channel);

    consumer.ReceivedAsync += async (model, ea) =>
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        receivedMessages.Add(message);
        await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
    };

    await channel.BasicConsumeAsync(queue: "user_updates", autoAck: false, consumer: consumer);

    // Aguardar mensagem (para demonstração, espera 2 segundos)
    await Task.Delay(2000);

    return Results.Ok(new { Messages = receivedMessages });
});

app.Run();
