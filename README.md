# FCG.Catalog

Microsserviço responsável pelo gerenciamento do catálogo de jogos da plataforma **FIAP Cloud Games (FCG)**.

A aplicação foi desenvolvida em **.NET 8**, seguindo princípios de **Clean Architecture**, **DDD**, **CQRS**, **SOLID** e arquitetura orientada a eventos.

Na Fase 3, o `FCG.Catalog` evolui com:

- métricas no formato Prometheus;
- cache distribuído com Redis;
- persistência poliglota com MongoDB para avaliações de jogos;
- integração local com Kong/Konga para validação do API Gateway;
- manutenção do SQL Server como banco principal do catálogo;
- manutenção da mensageria com RabbitMQ.

---

## Responsabilidades

O `FCG.Catalog` é responsável por:

- cadastrar jogos;
- atualizar jogos;
- consultar o catálogo;
- disponibilizar jogos;
- iniciar o fluxo de compra;
- publicar o evento `OrderPlacedEvent`;
- consumir o evento `PaymentProcessedEvent`;
- atualizar a biblioteca do usuário após pagamento aprovado;
- disponibilizar avaliações de jogos;
- persistir avaliações no MongoDB;
- utilizar Redis como cache distribuído para consultas do catálogo;
- expor métricas da aplicação no formato Prometheus.

---

## Arquitetura

```text
FCG.Catalog
│
├── src
│   ├── FCG.Catalog.Api
│   ├── FCG.Catalog.Application
│   ├── FCG.Catalog.Domain
│   ├── FCG.Catalog.Infrastructure
│   └── FCG.Catalog.Worker
│
├── tests
│
├── k8s
│   ├── namespace.yaml
│   ├── configmap.yaml
│   ├── secret.yaml
│   ├── api-deployment.yaml
│   ├── api-service.yaml
│   ├── worker-deployment.yaml
│   ├── sqlserver.yaml
│   └── rabbitmq.yaml
│
├── Dockerfile.Api
├── Dockerfile.Worker
├── docker-compose.yml
├── docker-compose.full.yml
├── NuGet.config
└── README.md
```

---

## Camadas

### FCG.Catalog.Api

Responsável por:

- controllers;
- Swagger/OpenAPI;
- autenticação e autorização;
- middlewares;
- endpoints HTTP;
- endpoint `/metrics`;
- endpoints de avaliações de jogos;
- registro das dependências específicas da API.

### FCG.Catalog.Application

Responsável por:

- Commands;
- Queries;
- Handlers;
- DTOs;
- Responses;
- contratos;
- casos de uso;
- abstração de cache por meio de `IGameCache`;
- abstração da persistência de avaliações.

### FCG.Catalog.Domain

Responsável por:

- entidades;
- regras de negócio;
- agregados;
- enums;
- contratos de domínio.

### FCG.Catalog.Infrastructure

Responsável por:

- Entity Framework Core;
- SQL Server;
- repositórios;
- migrations;
- RabbitMQ/MassTransit;
- implementação do cache Redis;
- acesso ao MongoDB;
- persistência das avaliações;
- publicação de eventos de integração.

### FCG.Catalog.Worker

Responsável por:

- consumo de eventos;
- processamento assíncrono;
- consumo de `PaymentProcessedEvent`;
- atualização da biblioteca após pagamento;
- integração entre os microsserviços.

---

## Evolução — Fase 3

A Fase 3 adiciona componentes para alta performance, observabilidade, persistência poliglota e API Gateway.

```text
                            Cliente
                               │
                               ▼
                         Kong API Gateway
                               │
                               ▼
                         FCG.Catalog.Api
                               │
          ┌────────────────────┼────────────────────┐
          │                    │                    │
          ▼                    ▼                    ▼
     SQL Server              Redis               MongoDB
       Games                Cache                Reviews
          │
          ▼
      RabbitMQ
          │
          ▼
   Catalog Worker

FCG.Catalog.Api
      │
      └── /metrics
             │
             ▼
         Prometheus
             │
             ▼
          Grafana
```

No ambiente local deste repositório, Kong e Konga podem ser utilizados para testes isolados.

Na arquitetura final, a configuração centralizada do Gateway, Prometheus e Grafana pertence ao repositório `FCG.Orchestration`.

---

## Tecnologias

- .NET 8
- ASP.NET Core
- Worker Service
- Entity Framework Core
- SQL Server 2022
- RabbitMQ
- MassTransit
- Redis 7.2
- StackExchange.Redis
- MongoDB 7
- MongoDB.Driver
- Prometheus
- prometheus-net.AspNetCore
- Kong API Gateway
- Konga
- PostgreSQL para Kong/Konga
- JWT
- Docker
- Docker Compose
- Kubernetes
- Swagger / OpenAPI
- xUnit
- NuGet

---

## Dependência compartilhada

```xml
<PackageReference Include="FCG.BuildingBlocks" Version="1.0.1" />
```

---

## Banco de dados principal — SQL Server

Banco utilizado:

```text
FCGCatalogDb
```

O SQL Server persiste os dados estruturados do catálogo, incluindo jogos e dados relacionados ao fluxo de compra.

As migrations são executadas com Entity Framework Core.

---

## Persistência poliglota — MongoDB

Na Fase 3, o MongoDB é utilizado para armazenar avaliações de jogos.

Banco:

```text
FCGCatalogReviewsDb
```

Collection:

```text
reviews
```

Separação de persistência:

```text
SQL Server
└── Games / Catalog

MongoDB
└── Reviews
```

### Estrutura da avaliação

```text
Id
GameId
UserId
Rating
Comment
CreatedAt
```

O `Rating` deve ficar entre `1` e `5`.

### Endpoints

Criar avaliação:

```text
POST /api/v1/games/{gameId}/reviews
```

Consultar avaliações:

```text
GET /api/v1/games/{gameId}/reviews
```

Exemplo:

```json
{
  "userId": "3b7db91b-a55a-4568-a712-88c7eb9b8620",
  "rating": 5,
  "comment": "Muito bom esse jogo"
}
```

### GUID no MongoDB

Os identificadores `GameId` e `UserId` são persistidos utilizando UUID Standard.

```csharp
BsonSerializer.RegisterSerializer(
    new GuidSerializer(GuidRepresentation.Standard));
```

### Validar diretamente no MongoDB

```powershell
docker exec -it fcg-catalog-mongodb mongosh `
  --username mongouser `
  --password mongopassword `
  --authenticationDatabase admin
```

Depois:

```javascript
use FCGCatalogReviewsDb
show collections
db.reviews.find().pretty()
```

---

## Cache distribuído — Redis

O Redis é utilizado para reduzir consultas repetidas ao SQL Server.

A implementação utiliza:

```text
StackExchange.Redis
IGameCache
RedisGameCache
```

Fluxo:

```text
GET Game
   │
   ▼
Redis
   │
   ├── HIT  → retorna cache
   │
   └── MISS → consulta SQL
                │
                ▼
             salva Redis
                │
                ▼
              resposta
```

Operações principais:

```text
StringGetAsync
StringSetAsync
KeyDeleteAsync
```

Chaves utilizadas:

```text
catalog:game:{id}
catalog:games
```

### Invalidação

```text
Create
→ remove catalog:games

Update
→ remove catalog:game:{id}
→ remove catalog:games

Activate / Inactivate
→ remove catalog:game:{id}
→ remove catalog:games
```

### Validar diretamente no Redis

```powershell
docker exec -it fcg-catalog-redis redis-cli -a password123
```

Depois:

```text
KEYS catalog:*
```

---

## Observabilidade — Prometheus

A API expõe métricas em:

```text
/metrics
```

Em Docker:

```text
http://localhost:5002/metrics
```

A aplicação utiliza:

```text
prometheus-net.AspNetCore
```

e configura:

```csharp
app.UseHttpMetrics();
app.MapMetrics();
```

Prometheus e Grafana serão centralizados no `FCG.Orchestration`.

---

## Autenticação e JWT

A Catalog utiliza tokens emitidos pela `FCG.Users.Api`.

```text
Issuer: FCG.Users.Api
Audience: FCG.CloudGames
ExpirationMinutes: 180
Algorithm: HS256
```

Fluxo esperado:

```text
Cliente
   │
   │ Bearer JWT
   ▼
Kong
   │
   │ valida token
   ▼
FCG.Catalog.Api
   │
   │ mantém sua própria autenticação/autorização
   ▼
Endpoint
```

---

## API Gateway — Kong

Kong e Konga estão disponíveis localmente para validação isolada.

Na arquitetura final haverá um único Kong:

```text
                   Kong
                  /    \
                 v      v
          FCG.Users   FCG.Catalog
```

Para registrar a Catalog localmente:

```text
Service:
catalog-service

Upstream:
http://catalog-api:8080
```

A configuração definitiva de Routes, Consumers, JWT Credentials e Plugins será centralizada no `FCG.Orchestration`.

### URLs locais

```text
Kong Proxy:
http://localhost:8000

Kong Admin API:
http://localhost:8001

Konga:
http://localhost:1337
```

---

## Mensageria

Broker:

```text
RabbitMQ
```

Biblioteca:

```text
MassTransit
```

Evento publicado:

```text
OrderPlacedEvent
```

Evento consumido:

```text
PaymentProcessedEvent
```

---

## Fluxo de compra

```text
Cliente
   │
   ▼
Catalog API
   │
   │ publica
   ▼
OrderPlacedEvent
   │
   ▼
RabbitMQ
   │
   ▼
Payments
   │
   │ publica
   ▼
PaymentProcessedEvent
   │
   ├───────────────┐
   ▼               ▼
Catalog Worker   Notifications
   │
   ▼
Atualiza biblioteca
```

---

## Fluxo de avaliação

```text
Cliente
   │
   ▼
Catalog API
   │
   ▼
GameReviewsController
   │
   ▼
IGameReviewRepository
   │
   ▼
MongoGameReviewRepository
   │
   ▼
MongoDB
   │
   ▼
FCGCatalogReviewsDb
   │
   ▼
reviews
```

---

## Variáveis de ambiente

### SQL Server

```text
ConnectionStrings__DefaultConnection
```

### Redis

```text
ConnectionStrings__Redis
Redis__Password
```

### MongoDB

```text
MongoDb__ConnectionString
MongoDb__DatabaseName
MongoDb__ReviewsCollectionName
```

### JWT

```text
Jwt__SecretKey
Jwt__Issuer
Jwt__Audience
Jwt__ExpirationMinutes
```

### RabbitMQ

```text
RabbitMq__Host
RabbitMq__Port
RabbitMq__VirtualHost
RabbitMq__Username
RabbitMq__Password
```

---

## appsettings.Local.json

Exemplo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1436;Database=FCGCatalogDb;User Id=sa;Password=<PASSWORD>;TrustServerCertificate=True;Encrypt=False",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "SecretKey": "<JWT_SECRET>"
  },
  "RabbitMq": {
    "Username": "rabbitmquser",
    "Password": "<RABBITMQ_PASSWORD>"
  },
  "Redis": {
    "Password": "<REDIS_PASSWORD>"
  },
  "MongoDb": {
    "ConnectionString": "mongodb://mongouser:<MONGO_PASSWORD>@localhost:27017/?authSource=admin",
    "DatabaseName": "FCGCatalogReviewsDb",
    "ReviewsCollectionName": "reviews"
  }
}
```

---

## Execução local

### Pré-requisitos

- .NET SDK 8
- Docker Desktop
- Git

### Restaurar

```powershell
dotnet restore --configfile .\NuGet.config
```

### Compilar

```powershell
dotnet build
```

### Testes

```powershell
dotnet test
```

### Executar a API

```powershell
dotnet run --project .\src\FCG.Catalog.Api
```

---

## Docker

### API

```powershell
docker build -f Dockerfile.Api -t brnmatos/fcg-catalog-api:1.0.0 .
docker run -p 5002:8080 brnmatos/fcg-catalog-api:1.0.0
```

### Worker

```powershell
docker build -f Dockerfile.Worker -t brnmatos/fcg-catalog-worker:1.0.0 .
docker run brnmatos/fcg-catalog-worker:1.0.0
```

---

## Docker Hub

```powershell
docker push brnmatos/fcg-catalog-api:1.0.0
docker push brnmatos/fcg-catalog-worker:1.0.0
```

---

## Docker Compose

### Infraestrutura

```powershell
docker compose up -d
```

O `docker-compose.yml` pode subir:

```text
SQL Server
RabbitMQ
Redis
MongoDB
Kong
Konga
PostgreSQL do Kong
PostgreSQL do Konga
```

### Ambiente completo

```powershell
docker compose -f docker-compose.full.yml up -d --build
```

O `docker-compose.full.yml` sobe:

```text
FCG.Catalog.Api
FCG.Catalog.Worker
SQL Server
RabbitMQ
Redis
MongoDB
Kong
Konga
PostgreSQL do Kong
PostgreSQL do Konga
```

### Verificar

```powershell
docker compose -f docker-compose.full.yml ps -a
```

Estado esperado:

```text
fcg-catalog-api         Up
fcg-catalog-worker      Up
fcg-catalog-sqlserver   Up (healthy)
fcg-catalog-redis       Up (healthy)
fcg-catalog-mongodb     Up (healthy)
fcg-rabbitmq            Up
fcg-kong                Up (healthy)
fcg-kong-database       Up (healthy)
fcg-konga               Up
fcg-konga-database      Up (healthy)
```

É esperado que:

```text
fcg-kong-migrations
fcg-konga-prepare
```

terminem com:

```text
Exited (0)
```

### URLs locais

| Componente | URL |
|---|---|
| Catalog Swagger | `http://localhost:5002/swagger/index.html` |
| Catalog Metrics | `http://localhost:5002/metrics` |
| RabbitMQ Management | `http://localhost:15672` |
| MongoDB | `localhost:27017` |
| Redis | `localhost:6379` |
| Kong Proxy | `http://localhost:8000` |
| Kong Admin API | `http://localhost:8001` |
| Konga | `http://localhost:1337` |

### Encerrar

```powershell
docker compose -f docker-compose.full.yml down
```

Evite `down -v` se quiser preservar bancos e configurações.

---

## Kubernetes

Os manifests específicos da Catalog permanecem em:

```text
k8s
```

Arquivos:

```text
namespace.yaml
configmap.yaml
secret.yaml
api-deployment.yaml
api-service.yaml
worker-deployment.yaml
sqlserver.yaml
rabbitmq.yaml
```

### Aplicar

```powershell
kubectl apply -f .\k8s\namespace.yaml
kubectl apply -f .\k8s\
```

### Logs

```powershell
kubectl logs -f deployment/fcg-catalog-api -n fcg
kubectl logs -f deployment/fcg-catalog-worker -n fcg
```

### Service da Catalog

Na arquitetura integrada da Fase 3, o ideal é expor a Catalog internamente por `ClusterIP`, deixando o Kong como ponto de entrada externo.

```text
Cliente
   │
   ▼
Kong
   │
   ▼
catalog-api
   │
   ▼
FCG.Catalog.Api
```

Para diagnóstico local:

```powershell
kubectl port-forward service/catalog-api 5002:8080 -n fcg
```

### SQL Server

```powershell
kubectl port-forward service/catalog-sqlserver 1437:1433 -n fcg
```

### RabbitMQ

```powershell
kubectl port-forward service/rabbitmq 15672:15672 -n fcg
```

### Componentes compartilhados

Na arquitetura final, os manifests/configurações compartilhados devem ser consolidados no `FCG.Orchestration`, incluindo:

```text
Kong
Prometheus
Grafana
Redis
MongoDB
RabbitMQ
```

---

## Comunicação

Dentro de Docker/Kubernetes, use nomes de serviços:

```text
catalog-api:8080
catalog-sqlserver:1433
rabbitmq:5672
redis:6379
mongodb:27017
kong:8001
```

Em execução local/Visual Studio:

```text
SQL Server → localhost:1436
RabbitMQ   → localhost:5672
Redis      → localhost:6379
MongoDB    → localhost:27017
```

---

## Segurança

- `ConfigMap` para configurações não sensíveis;
- `Secret` para credenciais;
- JWT com expiração de 180 minutos;
- SQL Server não deve ser exposto publicamente em produção;
- Redis deve exigir autenticação;
- MongoDB deve exigir autenticação;
- credenciais não devem ser mantidas no código;
- tokens e secrets não devem ser versionados;
- Kong deve validar JWT nas rotas protegidas;
- a Catalog mantém sua própria validação JWT;
- em produção, utilizar um gerenciador de segredos.

---

## CI/CD

```text
Restore
   ↓
Build
   ↓
Tests
   ↓
Docker Build
   ↓
Docker Push
   ↓
Deploy Kubernetes
```

---

## Troubleshooting

### Containers

```powershell
docker compose -f docker-compose.full.yml ps -a
```

### Logs da API

```powershell
docker compose -f docker-compose.full.yml logs catalog-api --tail 150
```

### Logs do SQL Server

```powershell
docker logs fcg-catalog-sqlserver --tail 100
```

### Logs do MongoDB

```powershell
docker logs fcg-catalog-mongodb --tail 100
```

### Logs do Redis

```powershell
docker logs fcg-catalog-redis --tail 100
```

### Kubernetes

```powershell
kubectl get pods -n fcg
kubectl describe pod <pod> -n fcg
kubectl logs -f deployment/fcg-catalog-api -n fcg
kubectl logs -f deployment/fcg-catalog-worker -n fcg
```

---

## Autor

**Bruno Matos**

Pós-graduação em Arquitetura de Software — FIAP

Projeto desenvolvido para o Tech Challenge da FIAP, utilizando arquitetura de microsserviços, mensageria, Docker, Kubernetes, Redis, MongoDB, API Gateway, observabilidade e boas práticas de desenvolvimento em .NET.
