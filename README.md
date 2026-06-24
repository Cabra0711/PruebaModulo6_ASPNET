# RentingBooking

Plataforma de gestión de rentas cortas que conecta propietarios e inquilinos. Permite buscar inmuebles, gestionar reservas con validación de disponibilidad estricta, verificar identidad mediante KYC asistido por IA y ofrecer a los propietarios un dashboard de métricas financieras con exportación a Excel.

---

## Índice

- [Requisitos previos](#requisitos-previos)
- [Levantar el proyecto con Docker](#levantar-el-proyecto-con-docker)
- [Levantar en modo desarrollo local](#levantar-en-modo-desarrollo-local)
- [Variables de entorno](#variables-de-entorno)
- [Arquitectura](#arquitectura)
- [Decisiones técnicas clave](#decisiones-técnicas-clave)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Roles del sistema](#roles-del-sistema)
- [Endpoints disponibles](#endpoints-disponibles)
- [Usuarios de prueba](#usuarios-de-prueba)
---

## Requisitos previos

| Herramienta | Versión mínima | Verificar con |
|---|---|---|
| Docker Desktop | 24+ | `docker --version` |
| Docker Compose | V2 (incluido en Docker Desktop) | `docker compose version` |
| .NET SDK _(solo para desarrollo local)_ | 9.0 | `dotnet --version` |

> No se requiere tener MySQL instalado localmente. El compose levanta la base de datos automáticamente.

---

## Levantar el proyecto con Docker

### 1. Clonar el repositorio

```bash
git clone <url-del-repositorio>
cd RentingBooking
```

### 2. Levantar todos los servicios

```bash
docker compose up --build
```

Esto levanta tres contenedores:
- `db` — MySQL 8.4 en el puerto `3306`
- `api` — API .NET en el puerto `8080` (espera a que MySQL esté saludable)
- `n8n` — n8n workflow automation en el puerto `5678` (para notificaciones por email)

### 3. Aplicar migraciones de base de datos

Una vez que ambos contenedores estén corriendo:

```bash
docker exec -it rentingbooking_api dotnet ef database update
```

### 4. Acceder a la aplicación

| Recurso | URL |
|---|---|
| Aplicación | http://localhost:8080 |
| Swagger UI | http://localhost:8080/swagger |
| n8n (notificaciones) | http://localhost:5678 |

### Detener el proyecto

```bash
docker compose down
```

Para eliminar también los volúmenes (borra la base de datos):

```bash
docker compose down -v
```

> **Nota**: El `Dockerfile` actualmente referencia imágenes `dotnet/sdk:10.0` y `dotnet/aspnet:10.0`, pero el proyecto apunta a `net9.0`. Si encuentras errores de compilación en Docker, cambia las imágenes base en el `Dockerfile` a `mcr.microsoft.com/dotnet/sdk:9.0` y `mcr.microsoft.com/dotnet/aspnet:9.0`.

---

## Levantar en modo desarrollo local

### 1. Levantar solo la base de datos

```bash
docker compose up db -d
```

### 2. Configurar la cadena de conexión

En `appsettings.json`, llenar el campo `MySqlConnection`:

```json
{
  "ConnectionStrings": {
    "MySqlConnection": "Server=localhost;Port=3306;Database=RentingBooking;User=root;Password=booking123;"
  },
  "JwtSettings": {
    "SecretKey": "super-secret-jwt-key-change-in-production-min32chars!"
  }
}
```

### 3. Correr migraciones y arrancar

```bash
dotnet ef database update
dotnet run
```

---

## Variables de entorno

| Variable | Descripción | Valor por defecto (Docker) |
|---|---|---|
| `ConnectionStrings__MySqlConnection` | Cadena de conexión a MySQL | `Server=db;Port=3306;...` |
| `JwtSettings__SecretKey` | Clave para firmar tokens JWT (mín. 32 chars) | Ver `docker-compose.yml` |
| `ASPNETCORE_ENVIRONMENT` | Entorno de ejecución | `Development` |

> En producción, reemplazar `SecretKey` por un valor seguro generado externamente. No commitear secretos reales al repositorio.

---

## Arquitectura

El sistema está construido sobre **.NET 9** con un patrón **MVC + Service Layer** y **autenticación dual (Cookie + JWT Bearer)**:

```
┌─────────────────────────────────────────────┐
│               Cliente (Browser)             │
│         (Razor Views + Tailwind CSS)        │
└─────────────────┬───────────────────────────┘
                  │ HTTP / MVC
┌─────────────────▼───────────────────────────┐
│              Controllers                     │
│   Reciben requests, delegan al servicio      │
│   Auth | User | Owner | Property | Booking   │
│   Admin | Wishlist | Home                    │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│             Service Layer                    │
│  Lógica de negocio, validaciones, JWT        │
│  Auth | User | Property | Booking            │
│  Wishlist | Dashboard | Notification (n8n)   │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│          ApplicationDbContext                │
│     Entity Framework Core + Pomelo          │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│              MySQL 8.4                       │
│       (contenedor Docker dedicado)           │
└─────────────────────────────────────────────┘
```

**Autenticación:**
- **Cookie Authentication** (por defecto) para las vistas Razor — login path: `/BookingRenting/Login`
- **JWT Bearer** para consumo desde Swagger o clientes externos
- Claims: `Name` (username) y `Role` para autorización por rol

**Notificaciones:**
- Las notificaciones por email se despachan mediante **n8n** (workflow automation) vía webhook HTTP, desacoplado del núcleo .NET.

---

## Decisiones técnicas clave

### Prevención de double-booking
La disponibilidad se valida en la capa de servicio antes de insertar una reserva. A nivel de base de datos existe un índice compuesto `(PropertyId, CheckInDate, CheckOutDate)` como segunda línea de defensa. La entidad `Property` usa `RowVersion` para **optimistic concurrency**, evitando condiciones de carrera si dos usuarios intentan reservar simultáneamente.

### Fechas con `DateOnly`
Las fechas de check-in y check-out se almacenan como `DateOnly`, no `DateTime`. Los horarios estándar (14:00 entrada / 12:00 salida) son propiedades calculadas en el dominio, nunca persistidas. Esto elimina la posibilidad de guardar fechas con horas incorrectas.

### Precio histórico en la reserva
El campo `PricePerNightAtBooking` en `Booking` guarda el precio vigente al momento de confirmar, independientemente de cambios futuros en la tarifa del inmueble. Garantiza integridad histórica para reportes y conciliación contable.

### Autenticación diferida
La navegación del catálogo es pública (sin autenticación). El sistema solicita login únicamente al intentar reservar, guardar favoritos o hacer el pago, reduciendo la fricción y la tasa de rebote.

### KYC con IA
Los documentos de identidad se procesan en memoria mediante un servicio de IA externo para extraer nombre, número de documento y fecha de nacimiento. Los bytes del documento **no se persisten** en la base de datos, cumpliendo el requerimiento de privacidad. Solo se almacena el resultado de la extracción en `KycVerification`.

### JWT + BCrypt
- Tokens JWT firmados con HMAC-SHA256, expiración de 30 minutos
- Contraseñas hasheadas con BCrypt (BCrypt.Net-Next)
- Claims incluyen `Name` y `Role` para autorización por rol en los endpoints

### Enums como string en BD
Todos los enums (`UserRole`, `BookingStatus`, `KycStatus`, `NotificationType`, `NotificationEvent`) se almacenan como `string` en MySQL. Esto hace las migraciones legibles y evita bugs silenciosos si se reordenan los valores del enum.

### Navegación responsive
- **Desktop**: Navbar superior con enlaces según el rol del usuario
- **Mobile**: Barra de navegación inferior fija (Wishlist-style) con iconos SVG y estado activo resaltado
- Roles: User (Explorar, Wishlist, Bookings, Perfil) | Owner (Propiedades, Dashboard, Publicar, Perfil) | Admin

---

## Estructura del proyecto

```
RentingBooking/
├── Controllers/              # Endpoints HTTP (MVC)
│   ├── AdminController.cs
│   ├── AuthController.cs
│   ├── BookingController.cs
│   ├── HomeController.cs
│   ├── OwnerController.cs
│   ├── PropertyController.cs
│   ├── UserController.cs
│   └── WishlistController.cs
├── Data/
│   └── ApplicationDbContext.cs       # EF Core context + relaciones
├── Enum/                     # Enumeraciones del dominio
│   ├── BookingStatus.cs
│   ├── KycStatus.cs
│   ├── NotificationEvent.cs
│   ├── NotificationType.cs
│   └── UserRole.cs
├── Migrations/               # Migraciones EF Core
├── Models/                   # Entidades del dominio
│   ├── BaseEntity.cs
│   ├── Booking.cs
│   ├── DashboardViewModel.cs
│   ├── ErrorViewModel.cs
│   ├── KycVerification.cs
│   ├── NotificationLog.cs
│   ├── Property.cs
│   ├── PropertyImage.cs
│   ├── User.cs
│   └── WishListItem.cs
├── Response/
│   └── ServiceResponse.cs            # Wrapper genérico ServiceResponse<T>
├── Service/                  # Lógica de negocio
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IBookingService.cs
│   │   ├── IDashboardService.cs
│   │   ├── INotificationService.cs
│   │   ├── IPropertyService.cs
│   │   ├── IUserService.cs
│   │   └── IWishlistService.cs
│   ├── AuthService.cs
│   ├── BookingService.cs
│   ├── DashboardService.cs
│   ├── N8nSettings.cs
│   ├── NotificationRequest.cs
│   ├── NotificationService.cs
│   ├── PropertyService.cs
│   ├── UserService.cs
│   └── WishlistService.cs
├── Validators/               # Reglas de validación (FluentValidation)
│   ├── PropertyValidator.cs
│   └── UserValidator.cs
├── Views/                    # Razor Views
│   ├── Auth/                 # Login, KYC status
│   ├── Booking/              # Create, PropertyBookings, confirm/cancel
│   ├── Home/                 # Index, Privacy
│   ├── Owner/                # Landing, Dashboard, Analytics
│   ├── Property/             # Public properties, create, edit, detail
│   ├── Shared/               # _Layout (nav responsive), Error
│   ├── User/                 # Landing, Profile, Kyc, MyBookings
│   └── Wishlist/             # Index
├── wwwroot/                  # Archivos estáticos (css, js, lib)
├── appsettings.json
├── appsettings.Development.json
├── docker-compose.yml        # MySQL + API + n8n
├── Dockerfile
├── Program.cs                # Punto de entrada + middleware
└── RentingBooking.csproj
```

---

## Roles del sistema

| Rol | Descripción |
|---|---|
| `User` | Arrendatario / huésped. Puede buscar inmuebles, reservar, gestionar favoritos, completar KYC y ver su perfil |
| `Owner` | Propietario / anfitrión. Puede publicar inmuebles, ver dashboard de métricas, exportar reportes Excel y gestionar reservas |
| `Admin` | Administrador del sistema. Acceso al panel de administración |

---

## Endpoints disponibles

> La documentación interactiva completa está en **http://localhost:8080/swagger**

### Home
| Método | Ruta | Descripción | Auth |
|---|---|---|---|
| GET | `/` o `/Home/Index` | Página principal del catálogo público | No |
| GET | `/Home/Privacy` | Política de privacidad | No |

### Auth (`/BookingRenting`)
| Método | Ruta | Descripción | Auth |
|---|---|---|---|
| GET | `/BookingRenting/Login` | Formulario de inicio de sesión | No |
| POST | `/BookingRenting/Login` | Iniciar sesión (Cookie + JWT) | No |
| POST | `/BookingRenting/RegisterCustomer` | Registro de arrendatario | No |
| POST | `/BookingRenting/RegisterOwner` | Registro de propietario | No |
| GET | `/BookingRenting/Logout` | Cerrar sesión | Sí |

### Properties (`/BookingRenting/Property`)
| Método | Ruta | Descripción | Auth |
|---|---|---|---|
| GET | `/BookingRenting/Property/public` | Listado público de inmuebles | No |
| GET | `/BookingRenting/Property/{id:guid}` | Detalle de un inmueble | No |
| GET | `/BookingRenting/Property/dashboard` | Inmuebles del propietario | Owner |
| GET | `/BookingRenting/Property/create` | Formulario de creación | Owner |
| POST | `/BookingRenting/Property/create` | Crear inmueble | Owner |
| GET | `/BookingRenting/Property/edit/{id:guid}` | Formulario de edición | Owner |
| POST | `/BookingRenting/Property/edit/{id:guid}` | Editar inmueble | Owner |
| POST | `/BookingRenting/Property/delete/{id:guid}` | Eliminar inmueble | Owner |

### Bookings (`/BookingRenting/Booking`)
| Método | Ruta | Descripción | Auth |
|---|---|---|---|
| GET | `/BookingRenting/Booking/Create/{propertyId:guid}` | Formulario de reserva | User |
| POST | `/BookingRenting/Booking/Create/{propertyId:guid}` | Crear reserva | User |
| GET | `/BookingRenting/Booking/Property/{propertyId:guid}` | Reservas de un inmueble | Owner |
| POST | `/BookingRenting/Booking/Cancel/{bookingId:guid}` | Cancelar reserva | User |

### User (`/BookingRenting/User`)
| Método | Ruta | Descripción | Auth |
|---|---|---|---|
| GET | `/BookingRenting/User/Landing` | Dashboard del usuario | User |
| GET | `/BookingRenting/User/Bookings` | Mis reservas | User |
| GET | `/BookingRenting/User/Profile` | Perfil del usuario | User |
| GET | `/BookingRenting/User/Kyc` | Verificación KYC | User |

### Owner (`/BookingRenting/Owner`)
| Método | Ruta | Descripción | Auth |
|---|---|---|---|
| GET | `/BookingRenting/Owner/Landing` | Landing del propietario | Owner |
| GET | `/BookingRenting/Owner/Analytics` | Dashboard de analíticas | Owner |
| GET | `/BookingRenting/Owner/Dashboard` | Dashboard de métricas | Owner |
| GET | `/BookingRenting/Owner/ExportBookings` | Exportar reservas a Excel | Owner |

### Wishlist (`/BookingRenting/Wishlist`)
| Método | Ruta | Descripción | Auth |
|---|---|---|---|
| GET | `/BookingRenting/Wishlist` | Lista de favoritos | User |
| POST | `/BookingRenting/Wishlist/Add/{propertyId:guid}` | Agregar a favoritos | User |
| POST | `/BookingRenting/Wishlist/Remove/{propertyId:guid}` | Quitar de favoritos | User |

### Admin (`/Admin`)
| Método | Ruta | Descripción | Auth |
|---|---|---|---|
| GET | `/Admin/Admin` | Panel de administración | No (pendiente autorización) |

---

## Usuarios de prueba

| Usuario | Contraseña | Rol |
|---|---|---|
| `client` | `Customer123!` | User |
| `customer` | `Customer123!` | User |
| `owner` | `Owner123!` | Owner |

---

## Paquetes NuGet

| Paquete | Versión |
|---|---|
| `BCrypt.Net-Next` | 4.2.0 |
| `EPPlus` | 7.5.1 |
| `FluentValidation.AspNetCore` | 11.3.0 |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 9.0.2 |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 9.0.2 |
| `Microsoft.EntityFrameworkCore.Design` | 9.* |
| `Pomelo.EntityFrameworkCore.MySql` | 9.0.0 |
| `Swashbuckle.AspNetCore` | 7.2.0 |
