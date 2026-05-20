# PaymentsAPI 💳

API REST para processamento de pagamentos de compras de jogos desenvolvida em .NET 8.0. Faz parte da arquitetura de microsserviços da Capucheta Games.

## 📋 Sobre o Projeto

A PaymentsAPI é responsável por processar (simular) pagamentos de compras de jogos na plataforma. A aplicação utiliza arquitetura limpa com separação em camadas (Core, Infrastructure, API) e integra-se com outros microsserviços através de eventos RabbitMQ.

### Principais Funcionalidades

- ✅ Processamento de pagamentos de jogos
- ✅ Gerenciamento de pedidos/orders
- ✅ Autenticação distribuída via integração com UsersAPI
- ✅ Sistema de permissões (Admin/User)
- ✅ Cache de dados para melhor performance
- ✅ Consumidor de eventos via RabbitMQ
- ✅ Health checks para monitoramento
- ✅ Documentação OpenAPI/Swagger
- ✅ Logging estruturado no AWS DynamoDB
- ✅ Pipeline CI/CD automatizado com deploy no Amazon EKS

## 🏗️ Arquitetura

O projeto segue os princípios de Clean Architecture dividido em camadas:

```
paymentsapi/
├── Core/                 # Camada de domínio (Entities, DTOs, Interfaces)
│   ├── Entity/           # Entidades do domínio (Payment)
│   ├── Dtos/             # Data Transfer Objects
│   ├── Models/           # Modelos e configurações
│   └── Repository/       # Interfaces de repositório
├── Infrastructure/       # Camada de infraestrutura
│   ├── Repository/       # Implementação dos repositórios
│   └── Migrations/       # Migrações do Entity Framework
├── PaymentsApi/          # Camada de apresentação (API)
│   ├── Controllers/      # Endpoints da API
│   ├── Service/          # Serviços da aplicação
│   ├── Middlewares/      # Middlewares customizados
│   └── Configs/          # Configurações
└── PaymentsApi.Tests/    # Testes automatizados
```

## 🚀 Tecnologias Utilizadas

- **.NET 8.0** - Framework principal
- **Entity Framework Core 8.0.22** - ORM para acesso ao banco de dados
- **PostgreSQL 16** - Banco de dados relacional (via Npgsql.EntityFrameworkCore.PostgreSQL 8.0.11)
- **RabbitMQ.Client 7.2.0** - Message broker para comunicação assíncrona
- **FluentValidation 12.1.1** - Validação de dados
- **AWS DynamoDB (AWSSDK.DynamoDBv2 4.0.18)** - Logging estruturado de eventos críticos
- **Serilog 4.3.1 + NewRelic.LogEnrichers.Serilog** - Logging e integração com New Relic APM
- **New Relic APM** - Monitoramento e rastreamento da aplicação
- **Swagger/ReDoc** - Documentação da API
- **Docker & Docker Compose** - Containerização
- **Kubernetes (Amazon EKS)** - Orquestração de containers em produção
- **GitHub Actions** - Pipeline de CI/CD automatizado

## 📦 Pré-requisitos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started) e [Docker Compose](https://docs.docker.com/compose/install/)
- [PostgreSQL 16](https://www.postgresql.org/) (ou via Docker)
- [RabbitMQ](https://www.rabbitmq.com/) (ou via Docker)
- Conta AWS com acesso ao DynamoDB (para logging em produção)

## 🔧 Configuração e Instalação

### 1. Clone o repositório

```bash
git clone https://github.com/capuchetagames/paymentsapi.git
cd paymentsapi
```

### 2. Configuração com Docker Compose (Recomendado)

O projeto possui dois arquivos Docker Compose separados:

- **`docker-compose.local.yaml`** — sobe o banco de dados PostgreSQL e o DynamoDB local.
- **`docker-compose.api.yaml`** — sobe apenas a PaymentsAPI (depende da rede criada pelo arquivo anterior).

Para subir o ambiente completo localmente:

```bash
# 1. Suba o banco de dados e o DynamoDB local
docker compose -f docker-compose.local.yaml up -d

# 2. Suba a API
docker compose -f docker-compose.api.yaml up -d
```

Isso iniciará:
- PostgreSQL na porta 5433
- DynamoDB Local (porta padrão)
- PaymentsAPI na porta 5109

A API estará disponível em: `http://localhost:5109`

### 3. Configuração Manual

#### 3.1. Configure o banco de dados

Atualize a connection string em `appsettings.json` (ou via variável de ambiente):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=Db.Payments;Username=postgres;Password=SuaSenha;"
  }
}
```

#### 3.2. Execute as migrações

```bash
dotnet ef database update --project Infrastructure --startup-project PaymentsApi
```

#### 3.3. Execute a aplicação

```bash
dotnet run --project PaymentsApi
```

## 📖 Documentação da API

### Endpoints Disponíveis

#### Pedidos (Orders)

- **GET** `/api/orders` - Lista todos os pedidos
- **GET** `/api/orders/{id}` - Busca pedido por ID
- **GET** `/api/orders/my-orders` - Lista pedidos do usuário autenticado
- **DELETE** `/api/orders/{id}` - Deleta um pedido (Admin only)

#### Health Check

- **GET** `/health` - Verifica o status do serviço
- **GET** `/api/orders/health` - Health check específico de orders

### Swagger UI

Quando executado em modo Development, a documentação Swagger está disponível em:

- **Swagger UI**: `http://localhost:5109/swagger`
- **ReDoc**: Configure conforme necessário

### Autenticação

A API utiliza autenticação JWT distribuída através da integração com a UsersAPI. Inclua o token JWT no header:

```
Authorization: Bearer {seu-token-jwt}
```

### Permissões

- **Admin**: Acesso total (deletar pedidos, visualizar qualquer pedido por ID)
- **User**: Acesso limitado (apenas seus próprios pedidos via `/my-orders` ou `/api/orders/{id}` com verificação de propriedade)

## 🐳 Deployment

### Docker

Build da imagem:

```bash
docker build -t paymentsapi:latest .
```

Execute o container:

```bash
docker run -d -p 5109:8080 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=Db.Payments;Username=postgres;Password=senha;" \
  -e ASPNETCORE_ENVIRONMENT=Production \
  paymentsapi:latest
```

### Kubernetes (Amazon EKS)

Scripts de deployment estão disponíveis no diretório `k8s/`:

```bash
# Deploy completo
./k8s/k8s-start-all-deploy.sh

# Deploy apenas do banco de dados
./k8s/k8s-deploy-db.sh

# Deploy apenas da API
./k8s/k8s-deploy-api.sh

# Ambiente de desenvolvimento
./k8s/k8s-start-all-dev.sh
```

Recursos Kubernetes disponíveis:
- `payments-db-deployment.yaml` — Deployment do PostgreSQL
- `payments-db-service.yaml` — Service do PostgreSQL
- `payments-deployment.yaml` — Deployment da API
- `payments-service.yaml` — Service da API
- `payments-configmap.yaml` — ConfigMap
- `payments-secret.yaml` — Secrets
- `payments-hpa.yaml` — HorizontalPodAutoscaler (mín. 2 / máx. 5 réplicas, CPU 80%)

> **Nota:** Os arquivos `sql-deployment.yaml` e `sql-service.yaml` ainda existem para compatibilidade, mas o banco oficial agora é o PostgreSQL (`payments-db-deployment.yaml`).

### CI/CD (GitHub Actions)

O pipeline está definido em `.github/workflows/ci-cd.yml` e é executado automaticamente a cada push na branch `main`. Ele segue as etapas:

1. **Build & Test** — Restaura dependências, compila e executa testes automatizados.
2. **Containerize & Push** — Constrói a imagem Docker e publica no DockerHub com as tags `latest` e `<commit-sha>`.
3. **Security Scan (Trivy)** — Varre a imagem publicada em busca de vulnerabilidades CRITICAL/HIGH.
4. **Deploy to EKS (Rolling Update)** — Injeta variáveis de ambiente no deployment e atualiza a imagem no cluster EKS `cluster-fiapstore`, namespace `fiapstore`, via `kubectl rollout`.

Secrets necessários no repositório GitHub:

| Secret | Descrição |
|--------|-----------|
| `DOCKERHUB_USERNAME` | Usuário do DockerHub |
| `DOCKERHUB_TOKEN` | Token de acesso ao DockerHub |
| `AWS_ACCESS_KEY_ID` | Credencial AWS para acesso ao EKS |
| `AWS_SECRET_ACCESS_KEY` | Credencial AWS para acesso ao EKS |
| `AWS_SESSION_TOKEN` | Token de sessão AWS (se aplicável) |
| `LAMBDA_AWS_ACCESS_KEY_ID` | Credencial AWS para acesso ao DynamoDB |
| `LAMBDA_AWS_SECRET_ACCESS_KEY` | Credencial AWS para acesso ao DynamoDB |
| `DB_CONNECTION_STRING` | String de conexão com PostgreSQL |
| `JWT_KEY` | Chave secreta JWT |


## 🔐 Variáveis de Ambiente

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `ConnectionStrings__DefaultConnection` | String de conexão do PostgreSQL | - |
| `ASPNETCORE_ENVIRONMENT` | Ambiente de execução | Development |
| `Jwt__Key` | Chave secreta JWT | - |
| `RabbitMq__Host` | Host do RabbitMQ | rabbitmq |
| `RabbitMq__User` | Usuário do RabbitMQ | admin |
| `RabbitMq__Password` | Senha do RabbitMQ | admin |
| `Services__UsersApi__BaseUrl` | URL base da UsersAPI | http://users-api:8080/ |
| `DynamoDb__LogTableName` | Nome da tabela DynamoDB para logs | - |
| `DynamoDb__UseLocal` | Usar DynamoDB local (true/false) | false |
| `DynamoDb__LocalUrl` | URL do DynamoDB local | - |
| `DynamoDb__Region` | Região AWS do DynamoDB | - |
| `AWS_ACCESS_KEY_ID` | Credencial AWS | - |
| `AWS_SECRET_ACCESS_KEY` | Credencial secreta AWS | - |
| `AWS_DEFAULT_REGION` | Região padrão AWS | - |
| `NEW_RELIC_LICENSE_KEY` | Chave de licença do New Relic para envio de telemetria | - |
| `NEW_RELIC_APP_NAME` | Nome da aplicação no painel do New Relic | payments-api |

## 📊 Observabilidade

A aplicação utiliza dois mecanismos de observabilidade: **AWS DynamoDB** para logging estruturado de eventos críticos e **New Relic APM** para monitoramento da aplicação.

### Logging com DynamoDB

Os logs são gravados diretamente em uma tabela do AWS DynamoDB configurada via `DynamoDb__LogTableName`. O nível mínimo de log enviado ao DynamoDB é **Warning** (apenas eventos críticos), mantendo o volume de dados reduzido.

Em ambiente local, é possível utilizar o [DynamoDB Local](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html) configurando:

```env
DynamoDb__UseLocal=true
DynamoDb__LocalUrl=http://localhost:8000
```

> O `docker-compose.local.yaml` inclui o serviço `dynamodb-local` pronto para uso em desenvolvimento.

### New Relic APM

O agente New Relic é instalado automaticamente no container Docker (via `Dockerfile`) e configurado para:

- Monitorar a aplicação .NET
- Encaminhar logs em tempo real via in-process log forwarding (`NEW_RELIC_APPLICATION_LOGGING_FORWARDING_ENABLED=true`)

Para habilitar o monitoramento com New Relic, configure as variáveis de ambiente:

```bash
NEW_RELIC_LICENSE_KEY=<sua_chave_de_licenca>
NEW_RELIC_APP_NAME=payments-api
```

Adicione essas variáveis no arquivo `.env` na raiz do projeto antes de executar com Docker Compose.

## 🤝 Integração com Outros Serviços

A PaymentsAPI integra-se com:

1. **UsersAPI** - Para validação de tokens e autenticação distribuída
2. **RabbitMQ** - Para consumo de eventos de pedidos (OrderEventsConsumer)

### Eventos Consumidos

A aplicação consome eventos relacionados a pedidos através do RabbitMQ para processar pagamentos automaticamente.

## 📝 Estrutura do Banco de Dados

### Banco de Dados: PostgreSQL 16

A aplicação utiliza **PostgreSQL** como banco de dados relacional, gerenciado pelo Entity Framework Core com o provider Npgsql.

### Tabela: Payments

| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | int | Identificador único (PK) |
| UserId | int | ID do usuário |
| GameId | int | ID do jogo |
| Price | decimal | Preço do pagamento |
| Status | string | Status do pagamento |
| CreatedAt | datetime | Data de criação |
| UpdatedAt | datetime | Data de atualização |

## 🛠️ Desenvolvimento

### Executar em modo de desenvolvimento

```bash
dotnet watch run --project PaymentsApi
```

### Adicionar nova migração

```bash
dotnet ef migrations add NomeDaMigracao --project Infrastructure --startup-project PaymentsApi
```

### Aplicar migrações

```bash
dotnet ef database update --project Infrastructure --startup-project PaymentsApi
```