# Advanced E-Commerce Microservices Ecosystem

A comprehensive e-commerce platform built with microservices architecture, featuring multi-language support (.NET 8, Spring Boot, Python), event-driven design, AI integration, and enterprise-grade security.

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Client Applications                               │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                    API Gateway (Ocelot)                               │
│                    Port: 5002                                         │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
        ┌────────────────────────────┼────────────────────────────┐
        ▼                          ▼                          ▼
┌───────────────┐          ┌───────────────┐          ┌───────────────┐
│   Product    │          │    Order     │          │  Inventory   │
│  Service     │          │  Service     │          │   Service    │
│  (.NET 8)    │          │(Spring Boot)│          │  (.NET 8)    │
│   Port:5000  │          │   Port:8080  │          │   Port:5001  │
└───────────────┘          └───────────────┘          └───────────────┘
        │                          │                          │
        ▼                          ▼                          ▼
┌───────────────┐          ┌───────────────┐          ┌───────────────┐
│  SQL Server   │          │   PostgreSQL  │          │  PostgreSQL   │
│  + Redis     │          │   + Redis     │          │   + pgvector  │
│  (Cache)    │          │    (Cart)    │          │   (AI Store)  │
└───────────────┘          └───────────────┘          └───────────────┘
                                    │
                                    ▼
                    ┌───────────────────────────────┐
                    │    Apache Kafka               │
                    │    (Event Streaming)         │
                    └───────────────────────────────┘
                                    │
                                    ▼
                    ┌───────────────────────────────┐
                    │       AI Service             │
                    │    (FastAPI + RAG)          │
                    │      Port: 8000              │
                    └───────────────────────────────┘
```

## 🛠️ Tech Stack

| Component | Technology | Description |
|-----------|-----------|------------|
| **Backend** | .NET 8, Spring Boot 3.2, FastAPI | Multi-language microservices |
| **Database** | SQL Server, PostgreSQL, Redis | Data persistence and caching |
| **Messaging** | Apache Kafka | Event-driven architecture |
| **Security** | KeyCloak, OAuth 2.0, JWT | Centralized authentication |
| **Gateway** | Ocelot | API Gateway with routing, rate limiting |
| **AI/ML** | LangChain, LlamaIndex, pgvector | RAG system |
| **DevOps** | Docker, Docker Compose | Containerization |

## 📦 Project Structure

```
Advanced E-Commerce Microservices/
├── docker-compose.yml              # Main orchestration file
├── infrastructure/
│   ├── docker-compose.yml          # Infrastructure services
│   └── keycloak-config.json         # KeyCloak configuration
├── dockerfiles/                     # Dockerfiles for each service
│   ├── Dockerfile.product-service
│   ├── Dockerfile.order-service
│   ├── Dockerfile.inventory-service
│   └── Dockerfile.ai-service
├── gateway/
│   └── Ocelot.json                 # API Gateway configuration
└── src/
    ├── ProductService/              # .NET 8 Product Service
    │   ├── Controllers/
    │   ├── Data/
    │   ├── Models/
    │   ├── Services/
    │   ├── Middleware/
    │   ├── config.yaml
    │   └── ProductService.csproj
    ├── OrderService/                # Spring Boot Order Service
    │   ├── src/main/java/
    │   │   ├── com/ecommerce/order/
    │   │   │   ├── controller/
    │   │   │   ├── dto/
    │   │   │   ├── event/
    │   │   │   ├── model/
    │   │   │   ├── repository/
    │   │   │   └── service/
    │   │   └── resources/
    │   │       └── application.yml
    │   └── pom.xml
    ├── InventoryService/             # .NET 8 Inventory Service
    │   ├── Controllers/
    │   ├── Data/
    │   ├── Events/
    │   ├── Models/
    │   ├── Services/
    │   └── InventoryService.csproj
    └── AIService/                   # FastAPI AI Service
        ├── main.py
        └── requirements.txt
```

## 🚀 Getting Started

### Prerequisites

- Docker Desktop
- .NET 8 SDK
- Java 17
- Python 3.11

### Quick Start

1. **Start Infrastructure Services**

```bash
docker-compose --profile infrastructure up -d
```

Wait for all services to be healthy:
- SQL Server (port 1433)
- PostgreSQL (port 5432)
- Redis (port 6379)
- Kafka (port 9092)
- KeyCloak (port 8080)

2. **Start Microservices**

```bash
docker-compose --profile services up -d
```

3. **Access Services**

| Service | URL |
|--------|-----|
| API Gateway | http://localhost:5002 |
| Product Service | http://localhost:5000 |
| Order Service | http://localhost:8080 |
| Inventory Service | http://localhost:5001 |
| AI Service | http://localhost:8000 |
| KeyCloak | http://localhost:8080 |
| Kafka UI | http://localhost:8081 |

## 📚 API Documentation

### Product Service (Port 5000)

```bash
# Get all products
GET /api/products

# Get product by ID
GET /api/products/{id}

# Create product
POST /api/products
{
  "name": "Product Name",
  "description": "Description",
  "category": "Category",
  "price": 99.99,
  "stockQuantity": 100,
  "brand": "Brand",
  "sku": "SKU001"
}
```

### Order Service (Port 8080)

```bash
# Create order
POST /api/orders
{
  "customerId": "customer-id",
  "shippingAddress": "Address",
  "paymentMethod": "credit_card",
  "items": [...]
}

# Get cart
GET /api/orders/cart/{customerId}

# Add to cart
POST /api/orders/cart/{customerId}
```

### Inventory Service (Port 5001)

```bash
# Get inventory
GET /api/inventory

# Deduct stock
POST /api/inventory/deduct
{
  "productId": "uuid",
  "quantity": 1
}

# Restock
POST /api/inventory/restock
{
  "productId": "uuid",
  "quantity": 10
}
```

### AI Service (Port 8000)

```bash
# Query AI
POST /api/query
{
  "question": "What laptops do you have?",
  "customerId": "optional-customer-id"
}
```

## 🔐 Security

### KeyCloak Configuration

- Realm: `ecommerce-realm`
- Admin Console: http://localhost:8080
- Credentials: `admin` / `admin123`

### Client Roles

- `product-service` - Product read/write
- `order-service` - Order read/write
- `inventory-service` - Inventory read/write
- `api-gateway` - Gateway access

## 🔄 Event Flow

1. **Order Placement Flow**

```
Client → API Gateway → Order Service
                          ↓
                    Create Order
                          ↓
                    Kafka: OrderPlaced Event
                          ↓
                    Inventory Service (Consumer)
                          ↓
                    Deduct Stock
```

## 🧪 Testing

### Docker Compose Profiles

- `infrastructure` - Start only databases and messaging
- `services` - Start all microservices
- `tools` - Start management tools (Kafka UI)

```bash
# Start only infrastructure
docker-compose --profile infrastructure up -d

# Start everything
docker-compose --profile infrastructure --profile services up -d
```

## 📝 Development

### Product Service

```bash
cd src/ProductService
dotnet restore
dotnet run
```

### Order Service

```bash
cd src/OrderService
mvn clean install
mvn spring-boot:run
```

### Inventory Service

```bash
cd src/InventoryService
dotnet restore
dotnet run
```

### AI Service

```bash
cd src/AIService
pip install -r requirements.txt
uvicorn main:app --reload
```

## 🔧 Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `DATABASE_URL` | PostgreSQL connection | postgresql://... |
| `REDIS_URL` | Redis connection | redis:6379 |
| `KAFKA_BOOTSTRAP_SERVERS` | Kafka servers | kafka:29092 |
| `KEYCLOAK_URL` | KeyCloak URL | http://keycloak:8080 |

## 📄 License

This project is for educational purposes.

## 🙏 Acknowledgments

- Microsoft .NET Team
- Spring Community
- Apache Kafka
- KeyCloak Team
- LangChain/LlamaIndex Teams
