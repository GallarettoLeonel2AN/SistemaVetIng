# 🐾 VetIng – Sistema Integral de Gestión Veterinaria

### 📚 Proyecto Académico – Ingeniería en Sistemas Informaticos
Una plataforma completa diseñada para optimizar la gestión clínica, administrativa y comercial de una veterinaria moderna.  
Construida con foco en **escalabilidad**, **experiencia de usuario** y **toma de decisiones basada en datos**.

---

## 🛠️ Guía de Instalación y Ejecución Local

Para levantar el proyecto en tu entorno de desarrollo, sigue estos pasos para configurar la base de datos y las dependencias.

### Requisitos previos
- Visual Studio 2022 o superior.
- .NET 8 SDK.
- SQL Server (LocalDB o Express).

### Paso 1: Configuración de Base de Datos
El sistema consta de dos contextos de base de datos separados (API externa y Sistema principal). Ejecuta las migraciones en el siguiente orden desde la **Consola del Administrador de Paquetes** (Package Manager Console):

**1. Levantar API Perros Peligrosos**  
Selecciona el proyecto `PerrosPeligrosos.Api` en la lista desplegable de "Default Project" y ejecuta:

```powershell
Add-Migration InicialApi
Update-Database
```

**2. Levantar VetIng (Sistema Principal)**  
Selecciona el proyecto **VetIng** (o tu proyecto web principal) y ejecuta:

```powershell
Add-Migration InicialVetIng
Update-Database
```

Esto generará las tablas necesarias en tu instancia local de SQL Server.

---

## 📡 API Externa: Registro de Perros Peligrosos

Este proyecto incluye una API RESTful simulada que actúa como un sistema gubernamental para el control de animales peligrosos. VetIng se comunica con ella para validar y registrar mascotas.

### 🔐 Seguridad y Acceso  
La API está protegida mediante API Key. Para probar los endpoints en Swagger o Postman, debes autorizarte:

**Header:** `PERROPELIGROSO-API-KEY`  
**Value:** `AccesoVetIng`

### 🔗 Endpoints Principales

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| **POST** | `/api/PerrosPeligrosos/registrar` | Upsert: Registra un nuevo perro o actualiza si ya existe (incluyendo su chip). |
| **GET** | `/api/PerrosPeligrosos` | Listar Todos: Devuelve el padrón completo de animales peligrosos. |
| **GET** | `/api/PerrosPeligrosos/{id}` | Detalle: Obtiene información específica de un animal por su ID. |
| **GET** | `/api/PerrosPeligrosos/buscar?termino=` | Búsqueda Inteligente: Filtra por DNI del dueño o Código de Chip. |

---

## 📖 Descripción General VetIng

VetIng centraliza la operación diaria de una clínica veterinaria mediante una arquitectura sólida en ASP.NET Core MVC.  
La plataforma administra de forma eficiente los tres roles fundamentales:

- **Administrador**
- **Veterinario**
- **Cliente**

Incluye módulos de gestión de mascotas, historias clínicas, turnos inteligentes, pagos online y reportes de negocio.

---

## 🏛️ Arquitectura Técnica

La solución adopta una estructura en capas siguiendo el patrón MVC, con servicios desacoplados e integraciones externas.

### 📌 Capas del Sistema

| Capa | Descripción |
|------|-------------|
| **Presentación (Views)** | Construida con Razor Pages para una UI limpia y responsiva. |
| **Controladores (MVC)** | Orquestan solicitudes sin lógica de negocio. |
| **Servicios** | Contienen reglas de negocio, validaciones e integraciones externas. |
| **Datos / Repositorios** | Acceso mediante Entity Framework Core + SQL Server. |

---

## 🔌 Integraciones Externas

- **API Perros Peligrosos** → Validación de normativas y chips (Backend propio).  
- **Mercado Pago** → Procesamiento de pagos online desde el sistema.  
- **SMTP Service** → Recuperación de contraseña, avisos y notificaciones.  

---

## ✨ Módulos Principales

### 👤 Gestión de Usuarios (Identity, Roles y Permisos)
- ASP.NET Core Identity completamente implementado.  
- Recuperación de contraseña por correo.  
- Sistema RBAC (Role-Based Access Control).  
- Permisos asignados por rol y por usuario.  

---

### 📅 Sistema de Turnos Inteligente

✔ Clientes reservan turnos directamente desde la web.  
✔ Veterinarios gestionan su agenda y registran atenciones.  
✔ Validaciones avanzadas:

- Evita solapamientos de turnos.  
- Considera disponibilidad horaria individual.  
- Controla bloqueos, ausencias y horarios especiales.  

**Estados admitidos:** *Pendiente, Cancelado, Finalizado, Ausente.*

---

## 📊 Business Intelligence – Reportes Estratégicos

Dashboard avanzado para análisis del negocio:

- 💰 Rendimiento Financiero: ingresos por período, ticket promedio.  
- ⚙️ Productividad: tasa de asistencia, turnos atendidos vs. cancelados.  
- 🐶 Tendencias:  
  - Razas frecuentes  
  - Servicios más solicitados  
  - Visitas por cliente  

---

## 🛡️ Auditoría y Trazabilidad (AuditLog)

Basada en la entidad **AuditoriaEvento**, registra:

- Quién realizó la acción  
- Qué acción realizó  
- Cuándo  
- Desde qué rol  
- Sobre qué entidad  

Garantiza integridad, transparencia y cumplimiento normativo.

---

## 🧩 Patrones de Diseño Utilizados

| Patrón | Uso en VetIng |
|--------|----------------|
| **Singleton** | Cacheo de configuraciones horarias globales. |
| **Repository** | Abstracción del acceso a datos (EF Core). |
| **Service Layer** | Desacople de lógica de negocio en la API y el sistema principal. |
| **Observer** | Envío automático de mail al registrarse un cliente. |
| **Decorator** | Cálculo flexible de costos (fines de semana, extras, descuentos). |
| **Composite** | Gestión agrupada y jerárquica de permisos. |
| **Memento** | Recuperación de versiones previas de atenciones clínicas. |

---

## 🧪 Calidad y Testing

- **xUnit** → Pruebas unitarias de servicios.  
- **Integration Tests** → Flujo completo (Identity, DB, lógica).  

---

## 🧰 Stack Tecnológico

| Categoría | Tecnología |
|-----------|-----------|
| **Core** | .NET 8 (C#) |
| **Framework Web** | ASP.NET Core MVC + Razor |
| **API** | ASP.NET Core Web API (Swagger) |
| **Base de Datos** | SQL Server |
| **ORM** | Entity Framework Core |
| **Testing** | xUnit, Moq, WebApplicationFactory |
| **Frontend** | HTML5, CSS3, JavaScript |
| **Pagos** | Mercado Pago SDK |
| **Herramientas** | Git, Visual Studio |

---

## 👨‍💻 Autores

**Ulises Ezequiel Sosa - Leonel Gallaretto**

