# ClientesAPI
ClientesAPI - Sistema de Gestión de Clientes
Descripción General
API REST desarrollada con ASP.NET Core 8.0 para la gestión de clientes y sus archivos asociados. Permite registrar clientes con fotografías, cargar múltiples archivos mediante ZIP y mantener un sistema de logs para auditoría.

Tecnologías Utilizadas

Framework: ASP.NET Core 8.0 Web API
Lenguaje: C# 12.0
ORM: Entity Framework Core 8.0
Base de Datos: SQL Server

Paquetes NuGet

Microsoft.EntityFrameworkCore (8.0.x)
Microsoft.EntityFrameworkCore.SqlServer (8.0.x)
Microsoft.EntityFrameworkCore.Tools (8.0.x)


Instrucciones de Ejecución Local
Requisitos Previos

.NET 8.0 SDK
SQL Server o SQL Server Express
Visual Studio 2022 (opcional)

Paso 1: Clonar el Repositorio
bashgit clone https://github.com/TU_USUARIO/lenguajesvisuales2-segundoparcial.git
cd lenguajesvisuales2-segundoparcial
Paso 2: Configurar la Cadena de Conexión
Editar appsettings.json:
json{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ClientesDB;Integrated Security=True;TrustServerCertificate=True;"
  }
}
Paso 3: Restaurar Paquetes
bashdotnet restore
Paso 4: Crear la Base de Datos
bashdotnet ef database update
O en Visual Studio Package Manager Console:
powershellUpdate-Database
Paso 5: Ejecutar la Aplicación
bashdotnet run
La API estará disponible en:

HTTP: http://localhost:5131
Swagger: http://localhost:5131/swagger
