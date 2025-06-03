#  API Máquinas

**API RESTful** desarrollada en **C# con .NET 8** y **SQL Server** para la gestión de un catálogo de máquinas industriales. Permite realizar operaciones CRUD, autenticación de usuarios, búsqueda por filtros y validación de datos. Ideal para integrarse con aplicaciones web o móviles que requieran manejo de inventarios o productos.

---

##  Características principales

-  Crear, obtener, actualizar y eliminar máquinas.
-  Búsqueda por **marca** con filtros.
-  Autenticación JWT (usuarios y administradores).
-  Registro e inicio de sesión de usuarios.
-  Control de acceso según rol (usuario/admin).
-  Validaciones básicas en los datos de entrada.
-  Documentación automática con **Swagger**.
-  API versionada.

---

##  Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- SQL Server (local o remoto)
- Visual Studio / Visual Studio Code u otro IDE compatible

---

##  Roles y seguridad

La API implementa autenticación con **JWT** y permite dos tipos de usuarios:

| Rol         | Acceso permitido                                             |
|-------------|--------------------------------------------------------------|
| Usuario     | `GET /api/maquina`, `GET /api/maquina/{id}`, `GET /marca/{marca}` |
| Administrador | Todos los métodos incluyendo `POST`, `PUT`, `DELETE` |

> El token JWT debe incluirse en el encabezado `Authorization` como `Bearer {token}`.

---

##  Endpoints principales

###  Máquinas

| Método | Ruta                               | Descripción                         |
|--------|------------------------------------|-------------------------------------|
| GET    | `/api/maquina`                    | Listar todas las máquinas           |
| GET    | `/api/maquina/{id}`               | Obtener una máquina por ID          |
| GET    | `/api/maquina/marca/{marca}`      | Buscar máquinas por marca           |
| POST   | `/api/maquina`                    | Crear nueva máquina (solo admin)    |
| PUT    | `/api/maquina/{id}`               | Actualizar máquina (solo admin)     |
| DELETE | `/api/maquina/{id}`               | Eliminar máquina (solo admin)       |

###  Autenticación

| Método | Ruta              | Descripción                   |
|--------|-------------------|-------------------------------|
| POST   | `/api/auth/login` | Iniciar sesión                |
| POST   | `/api/auth/register` | Registrar nuevo usuario     |

---
