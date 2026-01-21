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
- **SQL Server 2022** - Banco de dados relacional
- **RabbitMQ.Client 7.2.0** - Message broker para comunicação assíncrona
- **FluentValidation 12.1.1** - Validação de dados
- **Swagger/ReDoc** - Documentação da API
- **Docker & Docker Compose** - Containerização
- **Kubernetes** - Orquestração de containers

## 📦 Pré-requisitos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started) e [Docker Compose](https://docs.docker.com/compose/install/)
- [SQL Server](https://www.microsoft.com/sql-server) (ou via Docker)
- [RabbitMQ](https://www.rabbitmq.com/) (ou via Docker)

## 🔧 Configuração e Instalação

### 1. Clone o repositório

```bash
git clone https://github.com/capuchetagames/paymentsapi.git
cd paymentsapi
```

### 2. Configuração com Docker Compose (Recomendado)

A forma mais simples de executar a aplicação é usando Docker Compose:

```bash
docker-compose up -d
```

Isso iniciará:
- SQL Server na porta 1434
- PaymentsAPI na porta 5109

A API estará disponível em: `http://localhost:5109`

### 3. Configuração Manual

#### 3.1. Configure o banco de dados

Atualize a connection string em `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=Db.Payments;User Id=sa;Password=SuaSenha;TrustServerCertificate=True;"
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

- **GET** `/api/orders` - Lista todos os pedidos (Admin only)
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

- **Admin**: Acesso total (listar todos pedidos, deletar pedidos)
- **User**: Acesso limitado (apenas seus próprios pedidos)

## 🐳 Deployment

### Docker

Build da imagem:

```bash
docker build -t paymentsapi:latest .
```

Execute o container:

```bash
docker run -d -p 5109:8080 \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal,1433;Database=Db.Payments;User Id=sa;Password=senha;TrustServerCertificate=True;" \
  -e ASPNETCORE_ENVIRONMENT=Production \
  paymentsapi:latest
```

### Kubernetes

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
- `sql-deployment.yaml` - Deployment do SQL Server
- `sql-service.yaml` - Service do SQL Server
- `payments-deployment.yaml` - Deployment da API
- `payments-service.yaml` - Service da API
- `payments-configmap.yaml` - ConfigMap
- `payments-secret.yaml` - Secrets


## 🔐 Variáveis de Ambiente

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `ConnectionStrings__DefaultConnection` | String de conexão do SQL Server | - |
| `ASPNETCORE_ENVIRONMENT` | Ambiente de execução | Development |
| `Jwt__Key` | Chave secreta JWT | - |
| `RabbitMq__Host` | Host do RabbitMQ | rabbitmq |
| `RabbitMq__User` | Usuário do RabbitMQ | admin |
| `RabbitMq__Password` | Senha do RabbitMQ | admin |
| `Services__UsersApi__BaseUrl` | URL base da UsersAPI | http://users-api:8080/ |

## 🤝 Integração com Outros Serviços

A PaymentsAPI integra-se com:

1. **UsersAPI** - Para validação de tokens e autenticação distribuída
2. **RabbitMQ** - Para consumo de eventos de pedidos (OrderEventsConsumer)

### Eventos Consumidos

A aplicação consome eventos relacionados a pedidos através do RabbitMQ para processar pagamentos automaticamente.

## 📝 Estrutura do Banco de Dados

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

## 👥 Autores

**Capucheta Games** - [GitHub](https://github.com/capuchetagames)

---


## 🔄 Status do Projeto

✅ Projeto em desenvolvimento ativo
