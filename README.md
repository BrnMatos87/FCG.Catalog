# FCG.Catalog

Microsserviço responsável pelo gerenciamento do catálogo de jogos da plataforma FIAP Cloud Games.

## Responsabilidades

- Cadastro de jogos
- Atualização do catálogo
- Consulta de jogos
- Disponibilização de jogos
- Consumo de eventos de pagamento

## Arquitetura

```
FCG.Catalog
├── src
│   ├── FCG.Catalog.Api
│   ├── FCG.Catalog.Application
│   ├── FCG.Catalog.Domain
│   ├── FCG.Catalog.Infrastructure
│   └── FCG.Catalog.Worker
├── tests
├── k8s
├── Dockerfile.Api
├── Dockerfile.Worker
├── docker-compose.yml
├── docker-compose.full.yml
├── NuGet.config
└── README.md
```

## Camadas

### API
- Controllers
- Swagger
- Autenticação
- Endpoints HTTP

### Application
- Commands
- Queries
- Handlers
- DTOs
- Casos de uso

### Domain
- Entidades
- Regras de negócio
- Agregados
- Interfaces

### Infrastructure
- EF Core
- SQL Server
- RabbitMQ
- Repositórios
- Migrations

### Worker
- Consumo de eventos
- Processamento assíncrono
- Integração entre microsserviços

## Tecnologias

- .NET 8
- ASP.NET Core
- Worker Service
- Entity Framework Core
- SQL Server
- RabbitMQ
- MassTransit
- Docker
- Kubernetes
- Swagger
- xUnit

## Dependência compartilhada

```xml
<PackageReference Include="FCG.BuildingBlocks" Version="1.0.1" />
```
## Execução Local

```powershell
dotnet restore --configfile .\NuGet.config
dotnet build
dotnet test
dotnet run --project .\src\FCG.Catalog.Api
```

## Docker

API

```powershell
docker build -f Dockerfile.Api -t brnmatos/fcg-catalog-api:1.0.0 .
docker run -p 5002:8080 brnmatos/fcg-catalog-api:1.0.0
```

Worker

```powershell
docker build -f Dockerfile.Worker -t brnmatos/fcg-catalog-worker:1.0.0 .
docker run brnmatos/fcg-catalog-worker:1.0.0
```

## Docker Hub

```powershell
docker push brnmatos/fcg-catalog-api:1.0.0
docker push brnmatos/fcg-catalog-worker:1.0.0
```

## Docker Compose

Infraestrutura:

```powershell
docker compose up -d
```

Completo:

```powershell
docker compose -f docker-compose.full.yml up -d --build
```

## Kubernetes

Arquivos:

- namespace.yaml
- configmap.yaml
- secret.yaml
- api-deployment.yaml
- api-service.yaml
- worker-deployment.yaml
- sqlserver.yaml
- rabbitmq.yaml

Aplicação:

```powershell
kubectl apply -f .\k8s\namespace.yaml
kubectl apply -f .\k8s\
```

Logs:

```powershell
kubectl logs -f deployment/fcg-catalog-api -n fcg
kubectl logs -f deployment/fcg-catalog-worker -n fcg
```

Swagger:

```powershell
kubectl get svc catalog-api -n fcg
```

Acesso:

http://<EXTERNAL-IP>:5002/swagger

SQL Server:

```powershell
kubectl port-forward service/catalog-sqlserver 1437:1433 -n fcg
```

SSMS:

127.0.0.1,1437

RabbitMQ:

```powershell
kubectl port-forward service/rabbitmq 15672:15672 -n fcg
```

http://127.0.0.1:15672

## Fluxo

```
Cliente
   │
   ▼
Catalog API
   │
   ▼
SQL Server

PaymentProcessedEvent
   │
   ▼
RabbitMQ
   │
   ▼
Catalog Worker
   │
   ▼
Atualização do catálogo
```

## Comunicação

- catalog-api
- catalog-sqlserver
- rabbitmq

## Segurança

- ConfigMap para configurações
- Secret para credenciais
- SQL ClusterIP
- RabbitMQ ClusterIP
- API LoadBalancer

## CI/CD

Restore
→ Build
→ Tests
→ Docker Build
→ Docker Push
→ Kubernetes

## Troubleshooting

- kubectl get pods -n fcg
- kubectl describe pod <pod>
- kubectl logs -f deployment/fcg-catalog-api -n fcg
- kubectl logs -f deployment/fcg-catalog-worker -n fcg

## Autor

Bruno Matos

Pós-graduação FIAP - Tech Challenge
