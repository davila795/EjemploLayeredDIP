# 🎨 Diagramas Visuales - Dependency Injection

## 📊 1. Arquitectura del Sistema

```
┌────────────────────────────────────────────────────────────────┐
│                         CLIENTE                                 │
│                    (Navegador / Postman)                        │
└─────────────────────────┬──────────────────────────────────────┘
                          │ HTTP Request
                          ↓
┌────────────────────────────────────────────────────────────────┐
│                      API LAYER                                  │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │ ProductsController                                        │ │
│  │ • Recibe IProductService por constructor                 │ │
│  │ • Define acciones HTTP (Get, Post, Put, Delete)          │ │
│  └──────────────────────────────────────────────────────────┘ │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │ Program.cs                                                │ │
│  │ • Configura Dependency Injection                          │ │
│  │ • Registra Controllers                                    │ │
│  │ • Middleware Pipeline                                     │ │
│  └──────────────────────────────────────────────────────────┘ │
└─────────────────────────┬──────────────────────────────────────┘
                          │ Llama a
                          ↓
┌────────────────────────────────────────────────────────────────┐
│                   APPLICATION LAYER                             │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │ ProductService : IProductService                          │ │
│  │ • Lógica de Negocio                                       │ │
│  │ • Transformación de Datos (Entity ↔ DTO)                 │ │
│  │ • Orquestación de Repositorios                            │ │
│  └──────────────────────────────────────────────────────────┘ │
└─────────────────────────┬──────────────────────────────────────┘
                          │ Usa
                          ↓
┌────────────────────────────────────────────────────────────────┐
│                     DOMAIN LAYER                                │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │ IProductRepository (INTERFAZ)                             │ │
│  │ • Define el CONTRATO                                      │ │
│  │ • No tiene implementación                                 │ │
│  │ • Independiente de cualquier framework                    │ │
│  └──────────────────────────────────────────────────────────┘ │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │ Product (ENTIDAD)                                         │ │
│  │ • Modelo del Dominio                                      │ │
│  │ • Sin dependencias externas                               │ │
│  └──────────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────────┘
                          ↑
                          │ Implementa
                          │
┌────────────────────────────────────────────────────────────────┐
│                 INFRASTRUCTURE LAYER                            │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │ ProductRepository : IProductRepository                    │ │
│  │ • Implementación CONCRETA                                 │ │
│  │ • Acceso a Base de Datos                                  │ │
│  │ • Lógica de Persistencia                                  │ │
│  └──────────────────────────────────────────────────────────┘ │
└─────────────────────────┬──────────────────────────────────────┘
                          │
                          ↓
                  ┌───────────────┐
                  │   DATABASE    │
                  └───────────────┘
```

## 🔄 2. Flujo de Dependency Injection

```
INICIO: Aplicación arranca
│
├─ Program.cs ejecuta
│  │
│  ├─ PASO 1: Crear WebApplicationBuilder
│  │  var builder = WebApplication.CreateBuilder(args);
│  │
│  ├─ PASO 2: Configurar Servicios
│  │  │
│  │  ├─ builder.Services.AddApplicationLayer();
│  │  │  │
│  │  │  └─ Registra: IProductService → ProductService (Scoped)
│  │  │     ┌─────────────────────────────────────────┐
│  │  │     │ Contenedor DI:                         │
│  │  │     │  [IProductService] → [ProductService]  │
│  │  │     └─────────────────────────────────────────┘
│  │  │
│  │  └─ builder.Services.AddInfrastructureLayer();
│  │     │
│  │     └─ Registra: IProductRepository → ProductRepository (Scoped)
│  │        ┌─────────────────────────────────────────────────┐
│  │        │ Contenedor DI:                                 │
│  │        │  [IProductService] → [ProductService]          │
│  │        │  [IProductRepository] → [ProductRepository]    │
│  │        └─────────────────────────────────────────────────┘
│  │
│  ├─ PASO 3: Construir WebApplication
│  │  var app = builder.Build();
│  │  ┌─────────────────────────────────────────────┐
│  │  │ Contenedor DI está LISTO                   │
│  │  │ Conoce todas las dependencias              │
│  │  └─────────────────────────────────────────────┘
│  │
│  └─ PASO 4: Mapear Controllers
│     app.MapControllers();
│     ┌─────────────────────────────────────────────┐
│     │ ASP.NET Core escanea y registra todos los │
│     │ controllers con atributo [ApiController]   │
│     └─────────────────────────────────────────────┘
│
└─ FIN: Aplicación esperando requests

═══════════════════════════════════════════════════════════════

DURANTE UNA REQUEST: Cliente llama GET /api/products
│
├─ Request llega al controller
│  ProductsController.GetAll()
│  ↑
│  │ Constructor: ProductsController(IProductService service)
│  │
│  ┌──────────────────────────────────┴─────────────────────────┐
│  │ ASP.NET Core ve que necesita IProductService              │
│  └──────────────────────────────────┬─────────────────────────┘
│                                      │
│  ┌──────────────────────────────────↓─────────────────────────┐
│  │ Contenedor DI busca en registros:                         │
│  │   IProductService → ProductService                         │
│  └──────────────────────────────────┬─────────────────────────┘
│                                      │
│  ┌──────────────────────────────────↓─────────────────────────┐
│  │ Contenedor DI intenta crear ProductService                │
│  │ Constructor: ProductService(IProductRepository repo)      │
│  └──────────────────────────────────┬─────────────────────────┘
│                                      │
│  ┌──────────────────────────────────↓─────────────────────────┐
│  │ Contenedor DI ve que necesita IProductRepository          │
│  └──────────────────────────────────┬─────────────────────────┘
│                                      │
│  ┌──────────────────────────────────↓─────────────────────────┐
│  │ Contenedor DI busca en registros:                         │
│  │   IProductRepository → ProductRepository                   │
│  └──────────────────────────────────┬─────────────────────────┘
│                                      │
│  ┌──────────────────────────────────↓─────────────────────────┐
│  │ Contenedor DI crea ProductRepository                      │
│  │   new ProductRepository()                                  │
│  └──────────────────────────────────┬─────────────────────────┘
│                                      │
│  ┌──────────────────────────────────↓─────────────────────────┐
│  │ Contenedor DI crea ProductService                         │
│  │   new ProductService(productRepositoryInstance)            │
│  └──────────────────────────────────┬─────────────────────────┘
│                                      │
│  ┌──────────────────────────────────↓─────────────────────────┐
│  │ Contenedor DI crea ProductsController                     │
│  │   new ProductsController(productServiceInstance)           │
│  └──────────────────────────────────┬─────────────────────────┘
│                                      │
├─ Controller ejecuta la acción con el servicio inyectado
│  public async Task<IActionResult> GetAll()
│  {
│      var products = await _productService.GetAllProductsAsync();
│                             │
│                             ├─ Llama a ProductService.GetAllProductsAsync()
│                             │       │
│                             │       └─ Llama a _repository.GetAllAsync()
│                             │              │
│                             │              └─ ProductRepository obtiene datos
│                             │                     │
│                             │                     └─ Retorna List<Product>
│                             │
│                             └─ Convierte a List<ProductDto>
│      return Ok(products);
│  }
│
└─ Response retornada al cliente
```

## 🎯 3. Inyección en Cadena

```
╔════════════════════════════════════════════════════════════════╗
║                  INYECCIÓN EN CADENA                           ║
╚════════════════════════════════════════════════════════════════╝

ProductsController necesita → IProductService
                         │
                         ↓
                    ┌─────────────────────────────┐
                    │   Contenedor DI resuelve:   │
                    │   IProductService           │
                    │         ↓                   │
                    │   ProductService            │
                    └─────────────────────────────┘
                         │
                         │ ProductService constructor necesita
                         ↓
                    IProductRepository
                         │
                         ↓
                    ┌─────────────────────────────┐
                    │   Contenedor DI resuelve:   │
                    │   IProductRepository        │
                    │         ↓                   │
                    │   ProductRepository         │
                    └─────────────────────────────┘
                         │
                         ↓
            ┌──────────────────────────────────┐
            │ Todas las dependencias resueltas │
            │                                  │
            │ ProductRepository creado         │
            │        ↓                         │
            │ ProductService creado con        │
            │   ProductRepository inyectado    │
            │        ↓                         │
            │ ProductsController recibe        │
            │   ProductService                 │
            └──────────────────────────────────┘
```

## 🏗️ 4. Estructura de Dependencias

```
╔════════════════════════════════════════════════════════════════╗
║              DIRECCIÓN DE LAS DEPENDENCIAS                     ║
╚════════════════════════════════════════════════════════════════╝

┌──────────────┐
│     API      │  Depende de →  Application + Infrastructure
└──────┬───────┘
       │
       ↓
┌──────────────┐
│ Application  │  Depende de →  Domain
└──────┬───────┘
       │
       ↓
┌──────────────┐
│   Domain     │  ← No depende de NADIE (Centro del sistema)
└──────────────┘
       ↑
       │
┌──────┴───────┐
│Infrastructure│  Depende de →  Domain (Implementa interfaces)
└──────────────┘

NOTA: Infrastructure implementa interfaces de Domain,
      pero Domain NO conoce Infrastructure.
      ¡Esto es la INVERSIÓN de dependencias!
```

## 🔀 5. Comparación: Con vs Sin DI

```
╔════════════════════════════════════════════════════════════════╗
║                    SIN DEPENDENCY INJECTION                    ║
╚════════════════════════════════════════════════════════════════╝

public class ProductService
{
    private ProductRepository _repository;
    
    public ProductService()
    {
        _repository = new ProductRepository(); ← ❌ Acoplamiento fuerte
    }                                            ❌ Difícil de testear
}                                                ❌ No se puede cambiar impl.

Problemas:
• ProductService está acoplado a ProductRepository
• No puedes cambiar la implementación sin modificar ProductService
• Imposible inyectar mocks para testing
• Responsabilidad de crear dependencias en el servicio


╔════════════════════════════════════════════════════════════════╗
║                    CON DEPENDENCY INJECTION                    ║
╚════════════════════════════════════════════════════════════════╝

public class ProductService
{
    private readonly IProductRepository _repository;
    
    public ProductService(IProductRepository repository) ← ✅ Desacoplado
    {                                                       ✅ Testeable
        _repository = repository;                           ✅ Flexible
    }
}

Ventajas:
• ProductService depende de ABSTRACCIÓN (IProductRepository)
• Puedes cambiar implementación sin tocar ProductService
• Fácil inyectar mocks para testing
• Responsabilidad de crear dependencias en el contenedor DI
```

## ⏱️ 6. Service Lifetimes Visualizados

```
╔════════════════════════════════════════════════════════════════╗
║                      TRANSIENT                                 ║
╚════════════════════════════════════════════════════════════════╝

Request 1:
  Endpoint A → Logger A (nueva instancia)
  Service B  → Logger B (nueva instancia)
  Service C  → Logger C (nueva instancia)

Request 2:
  Endpoint D → Logger D (nueva instancia)
  Service E  → Logger E (nueva instancia)

Cada solicitud = Nueva instancia
Cada inyección = Nueva instancia


╔════════════════════════════════════════════════════════════════╗
║                        SCOPED                                  ║
╚════════════════════════════════════════════════════════════════╝

Request 1:
  Endpoint A → Repository A (nueva instancia)
  Service B  → Repository A (misma instancia)
  Service C  → Repository A (misma instancia)

Request 2:
  Endpoint D → Repository B (nueva instancia)
  Service E  → Repository B (misma instancia)

Cada request = Nueva instancia
Dentro del request = Misma instancia


╔════════════════════════════════════════════════════════════════╗
║                      SINGLETON                                 ║
╚════════════════════════════════════════════════════════════════╝

Request 1:
  Endpoint A → Config A (nueva instancia)
  Service B  → Config A (misma instancia)

Request 2:
  Endpoint C → Config A (misma instancia)
  Service D  → Config A (misma instancia)

Request N:
  Endpoint X → Config A (misma instancia)

Toda la aplicación = Una única instancia
```

## 🎓 7. Ejemplo Real: Request Completo

```
╔════════════════════════════════════════════════════════════════╗
║              GET /api/products - FLUJO COMPLETO                ║
╚════════════════════════════════════════════════════════════════╝

1. Cliente
   └─> HTTP GET /api/products
        │
        ↓
2. ASP.NET Core
   └─> Enruta a: ProductsController.GetAll()
        │
        ↓
3. Contenedor DI
   └─> Crea ProductsController
        │
        └─> Constructor necesita: IProductService
             │
             ├─> Busca registro: IProductService → ProductService
             │
             ├─> Crea Scope (nueva instancia para esta request)
             │
             └─> Crea ProductService
                  │
                  └─> Constructor necesita: IProductRepository
                       │
                       ├─> Busca registro: IProductRepository → ProductRepository
                       │
                       └─> Crea ProductRepository (mismo Scope)
        │
        ↓
4. Controller ejecuta acción
   └─> _productService.GetAllProductsAsync()
        │
        ↓
5. ProductService
   └─> _repository.GetAllAsync()
        │
        ↓
6. ProductRepository
   └─> Obtiene datos de _products (lista en memoria)
        │
        ↓
7. Retorna List<Product>
   └─> Vuelve a ProductService
        │
        ↓
8. ProductService
   └─> Convierte Product → ProductDto
        │
        ↓
9. Retorna List<ProductDto>
   └─> Vuelve al Controller
        │
        ↓
10. Controller
    └─> Ok(products)
         │
         ↓
11. ASP.NET Core
    └─> Serializa a JSON
         │
         ↓
12. HTTP Response
    └─> Status 200 OK
        Body: [{"id":1,"name":"Laptop",...},...]
         │
         ↓
13. Contenedor DI
    └─> Dispone instancias Scoped (fin de request)
         │
         └─> ProductService → Disposed
         └─> ProductRepository → Disposed
```

## 🧪 8. Testing con Mocks

```
╔════════════════════════════════════════════════════════════════╗
║                  TESTING CON DEPENDENCY INJECTION              ║
╚════════════════════════════════════════════════════════════════╝

// Test sin DI (imposible sin modificar ProductService)
❌ No se puede hacer

// Test con DI (fácil con mocks)
✅

[Fact]
public async Task GetAllProducts_ReturnsProducts()
{
    // Arrange
    var mockRepo = new Mock<IProductRepository>();
                        ↑
                        └─ Mock de la interfaz
    
    mockRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Product> { /* test data */ });
             ↑
             └─ Configurar comportamiento del mock
    
    var service = new ProductService(mockRepo.Object);
                                          ↑
                                          └─ Inyectar mock
    
    // Act
    var result = await service.GetAllProductsAsync();
    
    // Assert
    Assert.NotEmpty(result);
    mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
             ↑
             └─ Verificar que se llamó al método
}

Diagrama del flujo:

Test
 └─> Crea Mock de IProductRepository
      └─> Configura comportamiento esperado
           └─> Inyecta mock en ProductService
                └─> Ejecuta método del servicio
                     └─> Servicio usa el mock (no la impl. real)
                          └─> Verifica comportamiento
```

---

## 📚 Leyenda de Símbolos

- `→` : Dependencia / Flujo
- `↓` : Siguiente paso
- `↑` : Referencia / Implementa
- `┌─┐` : Contenedor / Bloque
- `✅` : Correcto / Ventaja
- `❌` : Incorrecto / Problema
- `═` : Sección importante
- `┬` : División / Bifurcación

---

💡 **Tip**: Imprime estos diagramas y síguelos mientras exploras el código!
