<div align="center">

# 🐾 VetIng – Sistema Integral de Gestión Veterinaria
### 🗄️ Versión SQL Server (Local)

---
> <sub>Versión Cloud (PostgreSQL + Render + Neon): https://github.com/SosaUlises/SistemaVetIng</sub>
---

### 🎓 Proyecto Académico – Ingeniería en Sistemas Informáticos

**VetIng** es una plataforma integral diseñada para optimizar la **gestión clínica, administrativa y comercial** de una veterinaria moderna.

El sistema fue desarrollado con foco en:

✨ Escalabilidad &nbsp;&nbsp; 🔐 Seguridad &nbsp;&nbsp; 🧠 Experiencia de usuario &nbsp;&nbsp;
🛡️ Trazabilidad y control &nbsp;&nbsp; 📊 Toma de decisiones basada en datos

</div>

---

## 🛠️ Guía de Instalación y Ejecución Local

Esta versión del proyecto utiliza **SQL Server en entorno local**, ideal para revisión de código, pruebas técnicas y evaluación funcional.

### 🔧 Requisitos Previos
* 🧩 Visual Studio 2022 o superior
* ⚙️ .NET 8 SDK
* 🗄️ SQL Server (LocalDB o Express)

---

## 🗄️ Configuración de Base de Datos

El sistema se compone de **dos contextos de base de datos independientes**:

1. 🐕 **API Externa** – Perros Peligrosos
2. 🏥 **Sistema Principal** – VetIng

> 📌 **Nota:** Las migraciones deben ejecutarse **en este orden** desde la **Package Manager Console**.

### 1️⃣ API Perros Peligrosos
Seleccionar el proyecto `PerrosPeligrosos.Api` como **Default Project** y ejecutar:

```powershell
Add-Migration InicialApi
Update-Database
```

### 2️⃣ Sistema Principal – VetIng
Seleccionar el proyecto web principal (`VetIng`) como **Default Project** y ejecutar:

```powershell
Add-Migration InicialVetIng
Update-Database
```

*Esto generará todas las tablas necesarias en SQL Server local.*

---

## 📡 API Externa – Registro de Perros Peligrosos

API RESTful simulada que representa un sistema gubernamental para el control de razas peligrosas.
VetIng se comunica con esta API para registrar y validar mascotas, incluyendo chip cuando corresponde.

### 🔐 Seguridad
La API está protegida mediante API Key.

| Header | Value |
| :--- | :--- |
| `PERROPELIGROSO-API-KEY` | `AccesoVetIng` |

### 🔗 Endpoints Disponibles

| Método | Endpoint |
| :--- | :--- |
| **POST** | `/api/PerrosPeligrosos/registrar` |
| **GET** | `/api/PerrosPeligrosos` |
| **GET** | `/api/PerrosPeligrosos/{id}` |
| **GET** | `/api/PerrosPeligrosos/buscar?termino=` |

---

## 🧭 Guía Funcional por Rol

Esta sección resume rápidamente qué puede hacer cada perfil dentro del sistema.

<details>
<summary><strong>🏥 Rol Veterinaria / Administrador (Owner)</strong> <em>(Click para desplegar)</em></summary>
<br>

Rol con control total del sistema y visión estratégica del negocio.

#### 📊 Reportes y métricas del negocio
La sección de reportes brinda una **visión integral del rendimiento de la veterinaria**, orientada a la toma de decisiones estratégicas.
Cuenta con visualizaciones gráficas dinámicas (gráficos de **barras** y **torta**) e indicadores clave de gestión (KPIs), entre los que se destacan:

* Clientes más frecuentes y nivel de recurrencia.
* Ingresos discriminados por período: **diario, semanal, mensual y anual**.
* Estado de los turnos: **atendidos, pendientes, cancelados y ausentes**.
* Productividad y carga de trabajo por veterinario.
* Servicios más solicitados y tendencias de demanda.

#### 🔐 Gestión de seguridad y permisos
* Asignar y quitar permisos por usuario.
* Asignar y quitar permisos por rol.
* Asignar o revocar el rol Veterinaria a Veterinarios.

#### ⏰ Configuración operativa
* Configuración de días y horarios de atención.
* Definición de duración de consultas.
* Gestión de excepciones horarias y bloqueos.

#### 📅 Turnos
* Visualización de agenda diaria completa.
* Consulta de historial de turnos.
* Supervisión de estados (Pendiente, Cancelado, Finalizado, Ausente).

#### 🐾 Gestión general
* Alta, baja y modificación de:
    * Veterinarios
    * Clientes
    * Mascotas
    * Estudios complementarios
    * Vacunas

#### 🛡️ Auditoría del sistema
* **Registro de:**
    * Login y logout
    * Login fallido
    * Creación, modificación y eliminación de mascotas
* **Visualización de:**
    * Usuario que realizó la acción
    * Rol
    * Fecha y hora exacta
    * Detalle de la accion
</details>

<details>
<summary><strong>🩺 Rol Veterinario (Operativo – Atención clínica)</strong> <em>(Click para desplegar)</em></summary>
<br>

Rol enfocado en la atención médica y gestión diaria.

* Visualizar turnos del día e historial de turnos.
* Realizar atenciones veterinarias:
    * Asociadas a un turno
    * Sin turno previo
* Acceder al historial clínico completo de la mascota.
* Modificar atenciones veterinarias previas.
* Marcar turnos como cancelados o no asistidos.
* Gestionar clientes y mascotas.
* Registrar cobros presenciales:
    * Efectivo
    * Tarjeta
</details>

<details>
<summary><strong>👤 Rol Cliente (Usuario final)</strong> <em>(Click para desplegar)</em></summary>
<br>

Rol orientado a la autogestión y experiencia del cliente.

* Reservar turnos para sus mascotas.
* Cancelar turnos.
* Solicitar primera cita para mascotas no registradas.
* Visualizar mascotas y su historial clínico.
* Consultar historial de turnos.
* Realizar pagos online mediante Mercado Pago.
* Visualizar historial de pagos.
</details>

---

### 🔐 Autenticación y Seguridad

* Sistema de login seguro con **ASP.NET Core Identity**.
* Recuperación de contraseña vía correo electrónico.
* Control de acceso basado en roles y permisos (**RBAC**).

---

## 📖 Descripción General del Sistema

VetIng centraliza la operación diaria de una clínica veterinaria mediante una arquitectura sólida basada en **ASP.NET Core MVC**.

Incluye módulos de:
* Gestión de mascotas
* Historias clínicas
* Turnos inteligentes
* Pagos online
* Reportes de negocio
* Auditoría y trazabilidad

---

## 🏛️ Arquitectura Técnica

Arquitectura en capas siguiendo el patrón **MVC**.

* **Presentación:** Razor Views
* **Controladores:** Orquestación sin lógica de negocio
* **Servicios:** Reglas de negocio e integraciones externas
* **Repositorios:** Acceso a datos con Entity Framework Core
* **Integraciones:** APIs externas, pagos y notificaciones

---

## 🔌 Integraciones Externas

* API Perros Peligrosos
* Mercado Pago
* Servicio SMTP para recuperación de contraseña y notificaciones

---

## 🛡️ Auditoría y Trazabilidad

Sistema basado en la entidad `AuditoriaEvento`, que registra:
* Quién realizó la acción
* Qué acción se realizó
* Cuándo ocurrió
* Desde qué rol
* Sobre qué entidad

---

## 🧩 Patrones de Diseño Utilizados

`Singleton` • `Repository` • `Service Layer` • `Observer` • `Decorator` • `Composite` • `Memento`

---

## 🧪 Testing

* Pruebas unitarias con **xUnit**.
* Pruebas de integración de flujos completos.

---

## 🧰 Stack Tecnológico

* .NET 8 (C#)
* ASP.NET Core MVC
* ASP.NET Core Web API
* SQL Server
* Entity Framework Core
* ASP.NET Core Identity
* Mercado Pago SDK
* HTML, CSS y JavaScript
* Git y Visual Studio

---

<div align="center">

## 👨‍💻 Autores

**Ulises Ezequiel Sosa**  
—  
**Leonel Gallaretto**


</div>
