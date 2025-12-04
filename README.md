# 🐾 VetIng – Sistema Integral de Gestión Veterinaria

### 📚 Proyecto Académico – Ingeniería en Sistemas  
Una plataforma completa diseñada para optimizar la gestión clínica, administrativa y comercial de una veterinaria moderna.  
Construida con foco en **escalabilidad**, **experiencia de usuario** y **toma de decisiones basada en datos**.

---

## 🚀 Live Demo  
El proyecto se encuentra desplegado y completamente funcional.

👉 **Ver Aplicación en Vivo**  
**

---

## 📖 Descripción General

VetIng centraliza la operación diaria de una clínica veterinaria mediante una arquitectura sólida en **ASP.NET Core MVC**.

La plataforma administra de forma eficiente los tres roles fundamentales:

- **Administrador**
- **Veterinario**
- **Cliente**

Incluye módulos de gestión de mascotas, historias clínicas, turnos inteligentes, pagos online y reportes de negocio.

---

## 🏛️ Arquitectura Técnica

La solución adopta una estructura en capas siguiendo el patrón **MVC**, con servicios desacoplados e integraciones externas.

### 📌 Capas del Sistema

| Capa | Descripción |
|------|-------------|
| **Presentación (Views)** | Construida con Razor Pages + Bootstrap para una UI limpia y responsiva. |
| **Controladores (MVC)** | Orquestan solicitudes sin lógica de negocio. |
| **Servicios** | Contienen reglas de negocio, validaciones e integraciones externas. |
| **Datos / Repositorios** | Acceso mediante Entity Framework Core + SQL Server. |

---

## 🔌 Integraciones Externas

- **API Perros Peligrosos** → Validación de normativas y chips.  
- **Mercado Pago** → Procesamiento de pagos online desde el sistema.  
- **SMTP Service** → Recuperación de contraseña, avisos y notificaciones.  

---

## ✨ Módulos Principales

### 👤 Gestión de Usuarios (Identity, Roles y Permisos)
- ASP.NET Core Identity completamente implementado.  
- Recuperación de contraseña por correo.  
- Sistema **RBAC** (Role-Based Access Control).  
- Permisos asignados por rol y por usuario.  

---

### 📅 Sistema de Turnos Inteligente
✔ Clientes reservan turnos directamente desde la web.  
✔ Veterinarios gestionan su agenda y registran atenciones.  
✔ Validaciones avanzadas:

- Evita solapamientos de turnos.  
- Considera disponibilidad horaria individual.  
- Controla bloqueos, ausencias y horarios especiales.  

**Estados admitidos:**  
`Pendiente`, `Cancelado`, `Finalizado`, `Ausente`.

---

## 📊 Business Intelligence – Reportes Estratégicos

Dashboard avanzado para análisis del negocio:

- 💰 **Rendimiento Financiero:** ingresos por período, ticket promedio.  
- ⚙️ **Productividad:** tasa de asistencia, turnos atendidos vs. cancelados.  
- 🐶 **Tendencias:**  
  - Razas frecuentes  
  - Servicios más solicitados  
  - Visitas por cliente  

---

## 🛡️ Auditoría y Trazabilidad (AuditLog)

Basada en la entidad `AuditoriaEvento`, registra:

- Quién realizó la acción  
- Qué acción realizó  
- Cuándo  
- Desde qué rol y sobre qué entidad  

Garantiza integridad, transparencia y cumplimiento normativo.

---

## 🧩 Patrones de Diseño Utilizados

| Patrón | Uso en VetIng |
|--------|----------------|
| **Singleton** | Cacheo de configuraciones horarias globales. |
| **Repository** | Abstracción del acceso a datos (EF Core). |
| **Observer** | Envío automático de mail al registrarse un cliente. |
| **Decorator** | Cálculo flexible de costos (fines de semana, extras, descuentos). |
| **Composite** | Gestión agrupada y jerárquica de permisos. |
| **Memento** | Recuperación de versiones previas de atenciones clínicas. |

---

## 🧪 Calidad y Testing

- **xUnit** → Pruebas unitarias de servicios.  
- **Integration Tests** → Flujo completo (Identity, DB, lógica).  
- **Tests de API externa** → Validación de la API de Perros Potencialmente Peligrosos.  

---

## 🧰 Stack Tecnológico

| Categoría | Tecnología |
|-----------|------------|
| **Core** | .NET 8 (C#) |
| **Framework Web** | ASP.NET Core MVC + Razor |
| **Base de Datos** | SQL Server |
| **ORM** | Entity Framework Core |
| **Testing** | xUnit, Moq, WebApplicationFactory |
| **Frontend** | HTML5, CSS3, JavaScript |
| **Pagos** | Mercado Pago SDK |
| **Herramientas** | Git, Visual Studio |

---

## 👨‍💻 Autores
- **Ulises Ezequiel Sosa**  
- **Leonel Gallaretto**

---
