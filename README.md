# APIs com Autenticação JWT e RabbitMQ em C#

Este projeto implementa duas APIs REST em C# usando ASP.NET Core (.NET 9) para aprendizado sobre autenticação JWT e comunicação assíncrona com RabbitMQ. A **API 1** (Producer) publica mensagens em uma fila RabbitMQ, protegida por JWT. A **API 2** (Consumer) consome mensagens da fila e interage com a API 1 usando JWT.

## Objetivo
- Demonstrar autenticação JWT em uma API REST.
- Implementar comunicação assíncrona entre serviços usando RabbitMQ.
- Ensinar integração de APIs com message brokers e segurança.

## Estrutura do Projeto

### API 1 (Producer)
- **Endpoint**: `POST /send-message`
- **Função**: Recebe um payload JSON, publica na fila RabbitMQ `user_updates` e exige autenticação JWT.
- **Tecnologias**:
  - ASP.NET Core com `Microsoft.AspNetCore.Authentication.JwtBearer`.
  - `RabbitMQ.Client` (versão 7.1.2) para publicar mensagens usando API assíncrona.

### API 2 (Consumer)
- **Endpoint**: `GET /consume`
- **Função**: Gera um JWT, envia uma mensagem para a API 1 e consome mensagens da fila RabbitMQ.
- **Tecnologias**:
  - `System.IdentityModel.Tokens.Jwt` para gerar JWT.
  - `RabbitMQ.Client` (versão 7.1.2) para consumir mensagens com `AsyncEventingBasicConsumer`.
  - `HttpClient` para comunicação com a API 1.

## Pré-requisitos
- **.NET SDK** (versão 9 ou superior).
- **RabbitMQ** instalado e rodando localmente (porta 5672, painel em `http://localhost:15672`, usuário/senha: `guest/guest`).
- Ferramentas para testar APIs (ex.: `curl`, Postman).
- Pacotes NuGet:
  - API 1: `RabbitMQ.Client` (7.1.2), `Microsoft.AspNetCore.Authentication.JwtBearer` (9.0.8), `System.IdentityModel.Tokens.Jwt` (8.14.0)
  - API 2: `RabbitMQ.Client` (7.1.2), `System.IdentityModel.Tokens.Jwt` (8.14.0), `Microsoft.AspNetCore.Authentication.JwtBearer` (9.0.8)

## Configuração

1. **Instalar RabbitMQ**:
   ```bash
   sudo apt-get install rabbitmq-server
   sudo systemctl enable rabbitmq-server
   sudo systemctl start rabbitmq-server
   sudo rabbitmq-plugins enable rabbitmq_management
   ```

2. **Criar os projetos**:
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

3. **Configurar APIs**:
   - Substitua `Api1/Program.cs` e `Api2/Program.cs` com os códigos fornecidos.
   - Use a chave secreta: `minha_chave_secreta_super_segura_1234567890`.

## Principais Mudanças na Versão 7.x do RabbitMQ.Client

⚠️ **Importante**: A versão 7.x do RabbitMQ.Client introduziu mudanças significativas na API:

### API 1 (Producer):
- `CreateConnection()` → `CreateConnectionAsync()`
- `CreateModel()` → `CreateChannelAsync()`
- `QueueDeclare()` → `QueueDeclareAsync()`
- `BasicPublish()` → `BasicPublishAsync()` com `BasicProperties`
- Conexão e canal criados dentro do endpoint para evitar problemas de escopo

### API 2 (Consumer):
- `EventingBasicConsumer` → `AsyncEventingBasicConsumer`
- `Received` → `ReceivedAsync` (evento assíncrono)
- `BasicAck()` → `BasicAckAsync()`
- `BasicConsume()` → `BasicConsumeAsync()`

## Como Executar

1. **Iniciar RabbitMQ**:
   ```bash
   sudo systemctl start rabbitmq-server
   ```

2. **Iniciar API 1**:
   ```bash
   cd Api1
   dotnet run --urls=http://localhost:5000
   ```

3. **Iniciar API 2**:
   ```bash
   cd Api2
   dotnet run --urls=http://localhost:5001
   ```

4. **Testar API 2**:
   ```bash
   curl http://localhost:5001/consume
   ```
   - **Saída esperada**:
     ```json
     {"Messages":["{\"UserId\":\"user_123\",\"NewEmail\":\"newemail@example.com\"}"]}
     ```

5. **Testar API 1 diretamente** (opcional):
   - `curl -X POST http://localhost:5000/send-message` (sem token) → Erro 401.
   - Use Postman com um token JWT válido.

## Conceitos Usados
- **JWT**: Geração e validação de tokens com expiração e claims.
- **RabbitMQ**: Publicação e consumo de mensagens em filas para comunicação assíncrona com API moderna (versão 7.x).
- **ASP.NET Core**: Configuração de autenticação e APIs REST.
- **Programação Assíncrona**: Uso de `async/await` para operações RabbitMQ.
- **Integração**: Comunicação entre APIs com segurança e message brokers.

## Solução de Problemas

### Erros Comuns da Versão 7.x:
- **"ConnectionFactory não contém CreateConnection"**: Use `CreateConnectionAsync()`.
- **"EventingBasicConsumer não encontrado"**: Use `AsyncEventingBasicConsumer`.
- **"BasicPublishAsync não aceita X argumentos"**: Use a nova assinatura com `BasicProperties`.

### Outros Problemas:
- **Erro "key size must be greater than 256 bits"**: Use uma chave com 32+ caracteres.
- **Erro 401 em /send-message**: Verifique o token no header `Authorization: Bearer <token>`.
- **RabbitMQ não conecta**: Confirme que está rodando (`sudo systemctl status rabbitmq-server`) e use credenciais corretas (`guest/guest`).

### Alternativa para API Síncrona:
Se preferir usar a API síncrona, faça downgrade para RabbitMQ.Client 6.8.1:
```xml
<PackageReference Include="RabbitMQ.Client" Version="6.8.1" />
```

## Próximos Passos
- Adicionar mais endpoints à API 1.
- Configurar OAuth2 com OpenIddict.
- Usar exchanges no RabbitMQ para padrões avançados (ex.: pub/sub).
- Implementar retry policies e dead letter queues.
- Adicionar logging estruturado com Serilog.
