# JWT Authentication and RabbitMQ APIs in C#

This project implements two REST APIs in C# using ASP.NET Core (.NET 9) for learning about JWT authentication and asynchronous communication with RabbitMQ. **API 1** (Producer) publishes messages to a RabbitMQ queue, protected by JWT. **API 2** (Consumer) consumes messages from the queue and interacts with API 1 using JWT.

## Objective
- Demonstrate JWT authentication in a REST API
- Implement asynchronous communication between services using RabbitMQ
- Teach API integration with message brokers and security

## Project Structure

### API 1 (Producer)
- **Endpoint**: `POST /send-message`
- **Function**: Receives a JSON payload, publishes to RabbitMQ queue `user_updates`, and requires JWT authentication
- **Technologies**:
  - ASP.NET Core with `Microsoft.AspNetCore.Authentication.JwtBearer`
  - `RabbitMQ.Client` (version 7.1.2) for publishing messages using async API

### API 2 (Consumer)
- **Endpoint**: `GET /consume`
- **Function**: Generates a JWT, sends a message to API 1, and consumes messages from RabbitMQ queue
- **Technologies**:
  - `System.IdentityModel.Tokens.Jwt` for JWT generation
  - `RabbitMQ.Client` (version 7.1.2) for consuming messages with `AsyncEventingBasicConsumer`
  - `HttpClient` for communication with API 1

## Prerequisites
- **.NET SDK** (version 9 or higher)
- **RabbitMQ** installed and running locally (port 5672, dashboard at `http://localhost:15672`, user/password: `guest/guest`)
- Tools for API testing (e.g., `curl`, Postman)
- NuGet packages:
  - API 1: `RabbitMQ.Client` (7.1.2), `Microsoft.AspNetCore.Authentication.JwtBearer` (9.0.8), `System.IdentityModel.Tokens.Jwt` (8.14.0)
  - API 2: `RabbitMQ.Client` (7.1.2), `System.IdentityModel.Tokens.Jwt` (8.14.0), `Microsoft.AspNetCore.Authentication.JwtBearer` (9.0.8)

## Setup

1. **Install RabbitMQ**:
   ```bash
   sudo apt-get install rabbitmq-server
   sudo systemctl enable rabbitmq-server
   sudo systemctl start rabbitmq-server
   sudo rabbitmq-plugins enable rabbitmq_management
   ```

2. **Create projects**:
   ```bash
   dotnet new webapi -n Api1
   cd Api1
   dotnet add package RabbitMQ.Client --version 7.1.2
   dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.0.8
   dotnet add package System.IdentityModel.Tokens.Jwt --version 8.14.0
   cd ..
   dotnet new webapi -n Api2
   cd Api2
   dotnet add package RabbitMQ.Client --version 7.1.2
   dotnet add package System.IdentityModel.Tokens.Jwt --version 8.14.0
   dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.0.8
   ```

3. **Configure APIs**:
   - Replace `Api1/Program.cs` and `Api2/Program.cs` with the provided code
   - Use secret key: `minha_chave_secreta_super_segura_1234567890`

## Key Changes in RabbitMQ.Client Version 7.x

⚠️ **Important**: Version 7.x of RabbitMQ.Client introduced significant API changes:

### API 1 (Producer):
- `CreateConnection()` → `CreateConnectionAsync()`
- `CreateModel()` → `CreateChannelAsync()`
- `QueueDeclare()` → `QueueDeclareAsync()`
- `BasicPublish()` → `BasicPublishAsync()` with `BasicProperties`
- Connection and channel created within endpoint to avoid scope issues

### API 2 (Consumer):
- `EventingBasicConsumer` → `AsyncEventingBasicConsumer`
- `Received` → `ReceivedAsync` (async event)
- `BasicAck()` → `BasicAckAsync()`
- `BasicConsume()` → `BasicConsumeAsync()`

## How to Run

1. **Start RabbitMQ**:
   ```bash
   sudo systemctl start rabbitmq-server
   ```

2. **Start API 1**:
   ```bash
   cd Api1
   dotnet run --urls=http://localhost:5000
   ```

3. **Start API 2**:
   ```bash
   cd Api2
   dotnet run --urls=http://localhost:5001
   ```

4. **Test API 2**:
   ```bash
   curl http://localhost:5001/consume
   ```
   - **Expected output**:
     ```json
     {"Messages":["{\"UserId\":\"user_123\",\"NewEmail\":\"newemail@example.com\"}"]}
     ```

5. **Test API 1 directly** (optional):
   - `curl -X POST http://localhost:5000/send-message` (without token) → 401 error
   - Use Postman with a valid JWT token

## Concepts Used
- **JWT**: Token generation and validation with expiration and claims
- **RabbitMQ**: Publishing and consuming messages in queues for asynchronous communication with modern API (version 7.x)
- **ASP.NET Core**: Authentication configuration and REST APIs
- **Async Programming**: Using `async/await` for RabbitMQ operations
- **Integration**: Communication between APIs with security and message brokers

## Troubleshooting

### Common Version 7.x Errors:
- **"ConnectionFactory does not contain CreateConnection"**: Use `CreateConnectionAsync()`
- **"EventingBasicConsumer not found"**: Use `AsyncEventingBasicConsumer`
- **"BasicPublishAsync does not accept X arguments"**: Use new signature with `BasicProperties`

### Other Issues:
- **"key size must be greater than 256 bits" error**: Use a key with 32+ characters
- **401 error on /send-message**: Check token in `Authorization: Bearer <token>` header
- **RabbitMQ not connecting**: Confirm it's running (`sudo systemctl status rabbitmq-server`) and use correct credentials (`guest/guest`)

### Alternative for Sync API:
If you prefer synchronous API, downgrade to RabbitMQ.Client 6.8.1:
```xml
<PackageReference Include="RabbitMQ.Client" Version="6.8.1" />
```

## Next Steps
- Add more endpoints to API 1
- Configure OAuth2 with OpenIddict
- Use RabbitMQ exchanges for advanced patterns (e.g., pub/sub)
- Implement retry policies and dead letter queues
- Add structured logging with Serilog
