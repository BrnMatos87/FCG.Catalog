# FCG.Catalog

Microsserviço .NET 8 responsável pelo catálogo de jogos, fluxo de compra, biblioteca do usuário e avaliações da FIAP Cloud Games.

## Arquitetura da Fase 3

```text
Cliente -> Kong -> Catalog API
                   |-> SQL Server: jogos, pedidos e biblioteca
                   |-> Redis: cache distribuído do catálogo
                   |-> MongoDB: avaliações de jogos
                   |-> RabbitMQ: OrderPlacedEvent
                   `-> /metrics <- Prometheus <- Grafana

RabbitMQ -> Catalog Worker -> PaymentProcessedEvent -> biblioteca
```

Kong, Prometheus, Grafana, MongoDB, Redis e RabbitMQ são provisionados de forma integrada pelo `FCG.Orchestration`. Os arquivos locais deste repositório servem ao desenvolvimento isolado.

## Responsabilidades

- cadastrar, atualizar, ativar, inativar e consultar jogos;
- iniciar uma compra e publicar `OrderPlacedEvent` no RabbitMQ;
- consumir `PaymentProcessedEvent` no Catalog Worker;
- atualizar a biblioteca depois de pagamento aprovado;
- persistir avaliações no MongoDB;
- usar Redis para reduzir consultas repetidas ao SQL Server;
- expor métricas Prometheus.

Catalog não chama Notifications. A notificação de pagamento é enviada por Payments diretamente à Azure Function via HTTP.

## Endpoints

| Método | Rota | Finalidade |
|---|---|---|
| POST | `/api/games` | criar jogo |
| GET | `/api/games` | listar jogos |
| GET | `/api/games/{id}` | consultar jogo |
| PUT | `/api/games/{id}` | atualizar jogo |
| PATCH | `/api/games/{id}/activate` | ativar jogo |
| PATCH | `/api/games/{id}/inactivate` | inativar jogo |
| POST | `/api/purchases` | iniciar compra |
| GET | `/api/purchases/users/{userId}/library` | consultar biblioteca |
| POST | `/api/v1/games/{gameId}/reviews` | criar avaliação |
| GET | `/api/v1/games/{gameId}/reviews` | listar avaliações |

Na arquitetura integrada, essas rotas entram pelo Kong e são protegidas por JWT.

## Persistência poliglota

### SQL Server

O `FCGCatalogDb` mantém jogos, pedidos e biblioteca. O banco é exclusivo do Catalog.

### MongoDB

O MongoDB atende ao requisito obrigatório de persistência NoSQL armazenando avaliações flexíveis:

```text
Database: FCGCatalogReviewsDb
Collection: reviews
Campos principais: Id, GameId, UserId, Rating, Comment, CreatedAt
```

O projeto usa o driver oficial `MongoDB.Driver`. `Rating` deve estar entre 1 e 5.

Configurações:

- `MongoDb__ConnectionString`
- `MongoDb__DatabaseName`
- `MongoDb__ReviewsCollectionName`

### Redis

O cache distribuído usa `StackExchange.Redis` para consultas do catálogo:

```text
GET -> Redis HIT -> resposta
GET -> Redis MISS -> SQL Server -> grava cache -> resposta
```

Chaves principais:

- `catalog:game:{id}`
- `catalog:games`

Criação, atualização, ativação e inativação invalidam as entradas relacionadas.

Configurações:

- `ConnectionStrings__Redis`
- `Redis__Password`

## Mensageria

RabbitMQ e MassTransit permanecem somente no fluxo de compra/pagamento:

```text
Catalog API
  -> OrderPlacedEvent
  -> RabbitMQ
  -> Payments Worker
  -> PaymentProcessedEvent
  -> RabbitMQ
  -> Catalog Worker
  -> atualização da biblioteca
```

Principais configurações:

- `RabbitMq__Host`
- `RabbitMq__Port`
- `RabbitMq__VirtualHost`
- `RabbitMq__Username`
- `RabbitMq__Password`
- `RabbitMq__PaymentProcessedQueue`

## Observabilidade

A API usa `prometheus-net.AspNetCore` e expõe:

```text
GET /metrics
```

O Prometheus central do Orchestration coleta `catalog-api:8080/metrics`. O dashboard Grafana provisionado apresenta latência p50/p95, contagem de requisições, status HTTP e taxa de erros.

## JWT e API Gateway

Catalog aceita tokens emitidos pela Users API:

```text
Issuer: FCG.Users.Api
Audience: FCG.CloudGames
Algorithm: HS256
```

O segredo deve coincidir com o configurado na Users API e no Kong. O arquivo declarativo oficial de Services, Routes, consumer e plugin JWT está em `FCG.Orchestration/kong/kong.yml`, sem dependência de configuração manual no Konga.

## Execução local

```powershell
dotnet restore --configfile .\NuGet.config
dotnet build
dotnet test
dotnet run --project .\src\FCG.Catalog.Api
dotnet run --project .\src\FCG.Catalog.Worker
```

Com as dependências locais em execução:

- Swagger: `http://localhost:5002/swagger`
- métricas: `http://localhost:5002/metrics`

## Docker

```powershell
docker build -f Dockerfile.Api -t brnmatos/fcg-catalog-api:1.0.4 .
docker build -f Dockerfile.Worker -t brnmatos/fcg-catalog-worker:1.0.2 .
```

Para o ambiente completo da Fase 3, use:

```powershell
Set-Location ..\FCG.Orchestration
Copy-Item .env.example .env
docker compose --env-file .env up -d
```

Esse stack inclui Catalog API/Worker, SQL Server, MongoDB, Redis, RabbitMQ, Kong, Prometheus e Grafana, com volumes persistentes.

## Kubernetes

Os manifests próprios podem apoiar testes isolados. A implantação integrada deve usar o Kustomize do `FCG.Orchestration`:

```powershell
Copy-Item k8s/shared-secret.example.yaml k8s/shared-secret.yaml
kubectl kustomize .
kubectl apply -k .
```

No ambiente integrado:

- `catalog-api` é `ClusterIP` na porta `8080`;
- apenas o Kong é exposto por `LoadBalancer`;
- API e Worker possuem requests/limits;
- a API possui readiness/liveness probes;
- SQL Server, MongoDB, Redis e RabbitMQ possuem PVCs;
- segredos são lidos de `fcg-shared-secret` não versionado.

## Segurança

- não versione senhas, connection strings ou JWT secret;
- mantenha MongoDB e Redis autenticados;
- exponha a API somente pelo Kong no ambiente integrado;
- preserve a validação JWT também na API;
- mantenha RabbitMQ e bancos como Services internos.

## Relação com os requisitos da Fase 3

- API Gateway: tráfego externo recebido pelo Kong;
- observabilidade: métricas da Catalog coletadas por Prometheus e exibidas no Grafana;
- NoSQL obrigatório: avaliações no MongoDB com driver oficial;
- cache obrigatório: Redis com StackExchange.Redis;
- containers/Kubernetes: imagens separadas para API e Worker, orquestradas centralmente;
- comunicação assíncrona: RabbitMQ preservado no fluxo Catalog/Payments.
