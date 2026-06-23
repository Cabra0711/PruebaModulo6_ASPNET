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
---

## Requisitos previos

| Herramienta | Versión mínima | Verificar con |
|---|---|---|
| Docker Desktop | 24+ | `docker --version` |
| Docker Compose | V2 (incluido en Docker Desktop) | `docker compose version` |
| .NET SDK _(solo para desarrollo local)_ | 10.0 | `dotnet --version` |

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

Esto levanta dos contenedores:
- `rentingbooking_db` — MySQL 8.4 en el puerto `3306`
- `rentingbooking_api` — API .NET 10 en el puerto `8080`
  El contenedor de la API espera automáticamente a que MySQL esté saludable antes de arrancar (`depends_on: condition: service_healthy`).

### 3. Aplicar migraciones de base de datos

Una vez que ambos contenedores estén corriendo:

```bash
docker exec -it rentingbooking_api dotnet ef database update
```

### 4. Acceder a la API

| Recurso | URL |
|---|---|
| API base | http://localhost:8080 |
| Swagger UI | http://localhost:8080/swagger |

### Detener el proyecto

```bash
docker compose down
```

Para eliminar también los volúmenes (borra la base de datos):

```bash
docker compose down -v
```
 
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
    "MySqlConnection": "Server=localhost;Port=3306;Database=RentingBooking;User=appuser;Password=apppass123;"
  },
  "JwtSettings": {
    "SecretKey": "super-secret-jwt-key-change-in-production-min32chars!"
  }
}
```

### 3. Correr migraciones y arrancar

```bash
cd RentingBooking
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

El sistema está construido sobre **.NET 10** con un patrón **MVC + Service Layer**:

```
┌─────────────────────────────────────────────┐
│                   Cliente                    │
│          (Swagger / App / Móvil)            │
└─────────────────┬───────────────────────────┘
                  │ HTTP
┌─────────────────▼───────────────────────────┐
│              Controllers                     │
│   Reciben requests, delegan al servicio      │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│             Service Layer                    │
│  Lógica de negocio, validaciones, JWT        │
│  (IAuthService, IBookingService, etc.)       │
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

**Tecnologías complementarias previstas:**
- Microservicio en **Laravel / Node.js** para el despacho de notificaciones por email (desacoplado del core .NET)
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
 
---

## Estructura del proyecto

```
RentingBooking/
├── Controllers/          # Endpoints HTTP
├── Data/
│   └── ApplicationDbContext.cs   # EF Core context + configuración de relaciones
├── Enum/                 # Enumeraciones del dominio
│   ├── BookingStatus.cs
│   ├── KycStatus.cs
│   ├── NotificationEvent.cs
│   ├── NotificationType.cs
│   └── UserRole.cs
├── Models/               # Entidades del dominio
│   ├── BaseEntity.cs
│   ├── Booking.cs
│   ├── KycVerification.cs
│   ├── NotificationLog.cs
│   ├── Property.cs
│   ├── PropertyImage.cs
│   ├── User.cs
│   └── WishListItem.cs
├── Response/
│   └── ServiceResponse.cs        # Wrapper genérico de respuestas API
├── Service/              # Lógica de negocio
│   ├── Interfaces/
│   │   └── IAuthService.cs
│   └── AuthService.cs
├── Validators/           # Reglas de validación con FluentValidation
│   └── UserValidator.cs
├── appsettings.json
├── docker-compose.yml
├── Dockerfile
└── Program.cs
```
 
---

## Roles del sistema

| Rol | Descripción |
|---|---|
| `User` | Arrendatario / huésped. Puede buscar inmuebles, reservar, gestionar favoritos y completar KYC |
| `Owner` | Propietario / anfitrión. Puede publicar inmuebles, ver dashboard de métricas y exportar reportes Excel |
| `Admin` | Administrador del sistema |
 
---

## Endpoints disponibles

> La documentación interactiva completa está en **http://localhost:8080/swagger**

### Auth
| Método | Ruta | Descripción | Auth |
|---|---|---|---|
| POST | `/api/auth/login` | Iniciar sesión, retorna JWT | No |
| POST | `/api/auth/register` | Registro de arrendatario | No |
| POST | `/api/auth/register-owner` | Registro de propietario | No |

> Los demás módulos (propiedades, reservas, KYC, favoritos, reportes) están en desarrollo activo.
