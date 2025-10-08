# Guía Visual: Dependency Injection Paso a Paso

## 🔄 Flujo Completo de una Solicitud HTTP

```
1. HTTP Request: GET /api/products
         ↓
2. ProductsController recibe la solicitud
   El contenedor DI inyecta IProductService en el constructor
         ↓
3. Contenedor DI crea ProductService
         ↓
4. ProductService requiere IProductRepository (constructor)
         ↓
5. Contenedor DI inyecta ProductRepository
         ↓
6. ProductService.GetAllProductsAsync() ejecuta
         ↓
7. ProductRepository.GetAllAsync() ejecuta
         ↓
8. Datos retornados a ProductService
         ↓
9. ProductService convierte a DTOs
         ↓
10. DTOs retornados al Controller
         ↓
11. HTTP Response: JSON con productos
```

## 📊 Diagrama de Dependencias

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ ProductsController                                   │   │
│  │ - Recibe IProductService por constructor            │   │
│  │ - Define acciones HTTP (Get, Post, Put, Delete)     │   │
│  └─────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ Program.cs                                           │   │
│  │ - Configura DI                                       │   │
│  │ - Registra Controllers                               │   │
│  └─────────────────────────────────────────────────────┘   │
└───────────────────┬─────────────────────────────────────────┘
                    │ depende de
                    ↓
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                          │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ IProductService (Interfaz)                           │   │
│  └──────────────────────┬──────────────────────────────┘   │
│                         │ implementada por                   │
│  ┌──────────────────────↓──────────────────────────────┐   │
│  │ ProductService                                       │   │
│  │ - Lógica de negocio                                  │   │
│  │ - Usa IProductRepository                            │   │
│  └─────────────────────────────────────────────────────┘   │
└───────────────────┬─────────────────────────────────────────┘
                    │ depende de
                    ↓
┌─────────────────────────────────────────────────────────────┐
│                     Domain Layer                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ IProductRepository (Interfaz) ← DIP: Abstracción    │   │
│  └─────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ Product (Entidad)                                    │   │
│  └─────────────────────────────────────────────────────┘   │
└────────────────────────────────────────┬────────────────────┘
                                         │ implementada por
                    ┌────────────────────┘
                    ↓
┌─────────────────────────────────────────────────────────────┐
│                 Infrastructure Layer                         │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ ProductRepository                                    │   │
│  │ - Implementa IProductRepository                      │   │
│  │ - Acceso a datos (DB, API, etc.)                    │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## 🎯 Ejemplo Detallado: Constructor Injection

### Paso 1: Definir la Interfaz (Domain)
```csharp
// Domain/Interfaces/IProductRepository.cs
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
}
```

### Paso 2: Implementar la Interfaz (Infrastructure)
```csharp
// Infrastructure/Repositories/ProductRepository.cs
public class ProductRepository : IProductRepository
{
    public Task<IEnumerable<Product>> GetAllAsync()
    {
        // Lógica de acceso a datos
        return Task.FromResult<IEnumerable<Product>>(_products);
    }
}
```

### Paso 3: Inyectar en el Servicio (Application)
```csharp
// Application/Services/ProductService.cs
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    
    // ✅ Constructor Injection
    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        // Usar la dependencia inyectada
        var products = await _repository.GetAllAsync();
        return products.Select(p => /* mapear a DTO */);
    }
}
```

### Paso 4: Registrar en el Contenedor DI (Infrastructure)
```csharp
// Infrastructure/DependencyInjection/DependencyInjection.cs
public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services)
{
    services.AddScoped<IProductRepository, ProductRepository>();
    //                 ↑ Interfaz           ↑ Implementación
    return services;
}
```

### Paso 5: Registrar en el Contenedor DI (Application)
```csharp
// Application/DependencyInjection/DependencyInjection.cs
public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
{
    services.AddScoped<IProductService, ProductService>();
    //                 ↑ Interfaz        ↑ Implementación
    return services;
}
```

### Paso 6: Configurar en el Punto de Entrada (API)
```csharp
// API/Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); // ← Registrar controllers

// Registrar todas las capas
builder.Services.AddApplicationLayer();    // ← Registra IProductService
builder.Services.AddInfrastructureLayer(); // ← Registra IProductRepository

var app = builder.Build();

app.MapControllers(); // ← Mapear controllers

app.Run();
```

### Paso 7: Usar en Controllers (API)
```csharp
// API/Controllers/ProductsController.cs
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        // ↑ El contenedor DI inyecta automáticamente ProductService
        // ProductService a su vez recibe ProductRepository inyectado
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }
}
```

## 🔍 ¿Qué Hace el Contenedor de DI?

### Cuando se hace una solicitud HTTP:

1. **ASP.NET Core** recibe la solicitud y enruta al controller apropiado
2. **Contenedor de DI** detecta que `ProductsController` necesita `IProductService`
3. **Contenedor de DI** busca en sus registros:
   - Encuentra: `IProductService` → `ProductService`
4. **Contenedor de DI** crea una instancia de `ProductService`
5. Para crear `ProductService`, el contenedor ve que necesita `IProductRepository`
6. **Contenedor de DI** busca en sus registros:
   - Encuentra: `IProductRepository` → `ProductRepository`
7. **Contenedor de DI** crea una instancia de `ProductRepository`
8. **Contenedor de DI** inyecta `ProductRepository` en `ProductService`
9. **Contenedor de DI** inyecta `ProductService` en `ProductsController`
10. La acción del controller ejecuta y retorna el resultado

### Diagrama del Contenedor:

```
┌─────────────────────────────────────┐
│     Contenedor de DI                │
│  ┌───────────────────────────────┐ │
│  │ Registros:                    │ │
│  │  IProductService              │ │
│  │    → ProductService (Scoped)  │ │
│  │                               │ │
│  │  IProductRepository           │ │
│  │    → ProductRepository (Scoped)│ │
│  └───────────────────────────────┘ │
│                                     │
│  Cuando se solicita:                │
│  1. Verifica registro              │
│  2. Crea instancia                 │
│  3. Resuelve dependencias          │
│  4. Inyecta en constructor         │
│  5. Retorna instancia              │
└─────────────────────────────────────┘
```

## 🎨 Comparación: Con y Sin DI

### ❌ Sin Dependency Injection (Acoplamiento Fuerte)

```csharp
public class ProductService
{
    private readonly ProductRepository _repository;
    
    public ProductService()
    {
        // ❌ Crear dependencia directamente
        _repository = new ProductRepository();
    }
    
    // Problemas:
    // - Difícil de testear (no puedes usar mocks)
    // - Acoplado a ProductRepository concreto
    // - No puedes cambiar implementación fácilmente
}
```

### ✅ Con Dependency Injection (Acoplamiento Débil)

```csharp
public class ProductService
{
    private readonly IProductRepository _repository;
    
    public ProductService(IProductRepository repository)
    {
        // ✅ Recibir dependencia por constructor
        _repository = repository;
    }
    
    // Ventajas:
    // - Fácil de testear (inyectar mocks)
    // - Depende de abstracción, no de implementación
    // - Cambiar implementación sin modificar esta clase
}
```

## 📝 Lifetimes en Acción

### Ejemplo con diferentes lifetimes:

```csharp
// Transient: Nueva instancia cada vez
services.AddTransient<ILogger, ConsoleLogger>();

// Request 1:
// Endpoint llama Logger → Instancia A creada
// Service llama Logger → Instancia B creada
// Repository llama Logger → Instancia C creada

// Scoped: Una instancia por request
services.AddScoped<IProductRepository, ProductRepository>();

// Request 1:
// Endpoint usa Repository → Instancia A creada
// Service usa Repository → Instancia A (reutilizada)
// Otro Service usa Repository → Instancia A (reutilizada)

// Request 2:
// Endpoint usa Repository → Instancia B creada (nueva request)

// Singleton: Una instancia para toda la app
services.AddSingleton<IConfigService, ConfigService>();

// Request 1:
// Cualquier lugar usa ConfigService → Instancia A creada

// Request 2:
// Cualquier lugar usa ConfigService → Instancia A (la misma)

// Request N:
// Cualquier lugar usa ConfigService → Instancia A (siempre la misma)
```

## 🧪 Testing con Dependency Injection

```csharp
public class ProductServiceTests
{
    [Fact]
    public async Task GetAllProducts_ReturnsProducts()
    {
        // Arrange: Crear un mock del repositorio
        var mockRepo = new Mock<IProductRepository>();
        mockRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Product>
                {
                    new Product { Id = 1, Name = "Test" }
                });
        
        // Act: Inyectar el mock en el servicio
        var service = new ProductService(mockRepo.Object);
        var result = await service.GetAllProductsAsync();
        
        // Assert
        Assert.Single(result);
        mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }
}
```

## 💡 Tips y Best Practices

### ✅ DO (Hacer)

1. **Usar interfaces** para todas las dependencias
2. **Inyectar por constructor** (preferido)
3. **Usar métodos de extensión** para configurar DI por capa
4. **Registrar en el orden correcto** (Domain → Application → Infrastructure)
5. **Usar el lifetime apropiado** (Scoped para la mayoría de servicios web)

### ❌ DON'T (No Hacer)

1. **No usar `new` para crear dependencias** dentro de servicios
2. **No inyectar implementaciones concretas**, siempre usar interfaces
3. **No mezclar configuración de DI** en múltiples lugares
4. **No usar Singleton** para servicios con estado mutable
5. **No crear dependencias circulares** (A depende de B, B depende de A)

## 🎓 Ejercicio Práctico

Implementa un nuevo servicio siguiendo estos pasos:

1. **Domain**: Define `IOrderRepository`
2. **Application**: Crea `IOrderService` y `OrderService`
3. **Infrastructure**: Implementa `OrderRepository`
4. **DI**: Registra en ambas capas
5. **API**: Crea endpoints que usen `IOrderService`

---

**¿Preguntas?** Revisa el código y experimenta cambiando implementaciones!
