# Proyecto de Ejemplo: Dependency Injection en Arquitectura por Capas

Este proyecto demuestra cómo implementar **Dependency Injection (DI)** en una arquitectura por capas usando .NET 9 y Controllers.

## 📁 Estructura del Proyecto

```
EjemploLayeredDIP.sln
├── Domain/                    # Capa de Dominio (sin dependencias)
│   ├── Entities/             # Entidades del dominio
│   │   └── Product.cs
│   └── Interfaces/           # Interfaces (abstracciones)
│       └── IProductRepository.cs
│
├── Application/              # Capa de Aplicación (depende de Domain)
│   ├── DTOs/                # Data Transfer Objects
│   │   └── ProductDto.cs
│   ├── Interfaces/          # Interfaces de servicios
│   │   └── IProductService.cs
│   ├── Services/            # Implementación de servicios
│   │   └── ProductService.cs
│   └── DependencyInjection/ # Configuración de DI
│       └── DependencyInjection.cs
│
├── Infrastructure/           # Capa de Infraestructura (depende de Domain y Application)
│   ├── Repositories/        # Implementación de repositorios
│   │   └── ProductRepository.cs
│   └── DependencyInjection/ # Configuración de DI
│       └── DependencyInjection.cs
│
└── API/                     # Capa de Presentación (depende de todas)
    ├── Controllers/         # Controllers de API
    │   └── ProductsController.cs
    └── Program.cs           # Punto de entrada y configuración de DI
```

## 🎯 Conceptos Clave

### 1. Dependency Inversion Principle (DIP)

El **Principio de Inversión de Dependencias** establece:
- Los módulos de alto nivel no deben depender de módulos de bajo nivel. Ambos deben depender de abstracciones.
- Las abstracciones no deben depender de los detalles. Los detalles deben depender de las abstracciones.

**En este proyecto:**
- `Domain` define la interfaz `IProductRepository` (abstracción)
- `Infrastructure` implementa `ProductRepository` (detalle)
- `Application` depende de `IProductRepository`, no de `ProductRepository`
- ✅ La dependencia está invertida: Infrastructure depende de Domain

### 2. Dependency Injection (DI)

**Dependency Injection** es un patrón de diseño que implementa el DIP:
- Las dependencias se "inyectan" en lugar de ser creadas dentro de la clase
- Desacopla las clases de sus dependencias concretas
- Facilita el testing y el mantenimiento

**Tipos de inyección:**
- **Constructor Injection** (usado en este proyecto): Las dependencias se pasan por el constructor
- **Property Injection**: Las dependencias se asignan a propiedades
- **Method Injection**: Las dependencias se pasan como parámetros de método

### 3. Service Lifetimes (Tiempos de Vida)

En .NET, los servicios registrados en el contenedor de DI tienen diferentes tiempos de vida:

#### **Transient** (`AddTransient`)
- Se crea una **nueva instancia** cada vez que se solicita
- Útil para servicios ligeros y sin estado
```csharp
services.AddTransient<IServicio, Servicio>();
```

#### **Scoped** (`AddScoped`) ⭐ Usado en este proyecto
- Se crea **una instancia por solicitud HTTP**
- Se reutiliza dentro de la misma solicitud
- Útil para servicios que necesitan mantener estado durante una solicitud
```csharp
services.AddScoped<IServicio, Servicio>();
```

#### **Singleton** (`AddSingleton`)
- Se crea **una única instancia** para toda la vida de la aplicación
- Se reutiliza en todas las solicitudes
- Útil para servicios costosos de crear o sin estado mutable
```csharp
services.AddSingleton<IServicio, Servicio>();
```

## 🔍 Flujo de Dependency Injection en este Proyecto

### 1. **Configuración (API/Program.cs)**

```csharp
// Registrar servicios de Application
builder.Services.AddApplicationLayer();

// Registrar servicios de Infrastructure
builder.Services.AddInfrastructureLayer();
```

### 2. **Registro en Application Layer**

```csharp
// Application/DependencyInjection/DependencyInjection.cs
public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
{
    services.AddScoped<IProductService, ProductService>();
    return services;
}
```

### 3. **Registro en Infrastructure Layer**

```csharp
// Infrastructure/DependencyInjection/DependencyInjection.cs
public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services)
{
    services.AddScoped<IProductRepository, ProductRepository>();
    return services;
}
```

### 4. **Inyección en Controllers**

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        // ⬆️ IProductService se inyecta automáticamente por constructor
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

### 5. **Inyección en Cadena**

```csharp
// ProductService recibe IProductRepository por constructor
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        // ⬆️ El contenedor de DI inyecta ProductRepository
        _productRepository = productRepository;
    }
}
```

## 🚀 Cómo Ejecutar el Proyecto

### 1. Restaurar dependencias
```bash
dotnet restore
```

### 2. Compilar el proyecto
```bash
dotnet build
```

### 3. Ejecutar la API
```bash
cd API
dotnet run
```

### 4. Probar los endpoints

Abre tu navegador en: `https://localhost:7298/swagger`

O usa los siguientes comandos curl:

```bash
# Obtener todos los productos
curl https://localhost:7298/api/products

# Obtener un producto por ID
curl https://localhost:7298/api/products/1

# Crear un producto
curl -X POST https://localhost:7298/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Teclado","price":49.99,"description":"Teclado mecánico","stock":30}'

# Actualizar un producto
curl -X PUT https://localhost:7298/api/products/1 \
  -H "Content-Type: application/json" \
  -d '{"name":"Laptop Actualizada","price":1099.99,"description":"Nueva descripción","stock":15}'

# Eliminar un producto
curl -X DELETE https://localhost:7298/api/products/1
```

## 📚 Lecciones Aprendidas

### ✅ Ventajas de esta Arquitectura

1. **Bajo Acoplamiento**: Las capas dependen de abstracciones, no de implementaciones concretas
2. **Alta Cohesión**: Cada capa tiene una responsabilidad clara y definida
3. **Testeable**: Fácil crear mocks para testing unitario
4. **Mantenible**: Cambios en una capa no afectan a otras
5. **Flexible**: Fácil cambiar implementaciones sin modificar dependientes

### 🔄 Flujo de Dependencias

```
API → Application → Domain
  ↓        ↓
Infrastructure → Domain
```

**Nota:** Infrastructure depende de Domain, pero Domain NO depende de Infrastructure (DIP)

### 🧪 Testing con DI

Para hacer testing, puedes crear mocks de las interfaces:

```csharp
// Test de ProductService
public class ProductServiceTests
{
    [Fact]
    public async Task GetAllProducts_ReturnsProducts()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        mockRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Product> { /* ... */ });
        
        var service = new ProductService(mockRepo.Object);
        
        // Act
        var result = await service.GetAllProductsAsync();
        
        // Assert
        Assert.NotNull(result);
    }
}
```
## 📖 Recursos Adicionales

- [Dependency Injection en .NET](https://learn.microsoft.com/es-es/dotnet/core/extensions/dependency-injection)
- [Service Lifetimes](https://learn.microsoft.com/es-es/dotnet/core/extensions/dependency-injection#service-lifetimes)
- [Principios SOLID](https://learn.microsoft.com/es-es/dotnet/architecture/modern-web-apps-azure/architectural-principles#dependency-inversion)

---

**Autor**: Proyecto de ejemplo educativo  
**Licencia**: MIT
