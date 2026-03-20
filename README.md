# RandomPay 💸🎲

![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-100%25-239120?style=for-the-badge&logo=c-sharp)
![SQL Server](https://img.shields.io/badge/SQL_Server-Database-CC2927?style=for-the-badge&logo=microsoft-sql-server)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=for-the-badge&logo=bootstrap)

**RandomPay** es una aplicación web diseñada para gestionar gastos de grupo en viajes, cenas y eventos de forma transparente, justa y divertida. Olvídate de las hojas de cálculo y de las discusiones sobre quién pagó qué.

## ✨ Características Principales

* **Gestión de Actividades y Grupos:** Crea grupos mediante un código de invitación único que puedes compartir directamente por WhatsApp.
* **Saldos y Deudas Inteligentes:** El algoritmo calcula automáticamente los balances individuales y te dice exactamente quién debe pagar a quién, minimizando el número de transferencias necesarias.
* **Lista de la Compra Integrada:** Planifica lo que necesitáis comprar con precios estimados y conviértelo en un gasto real con un solo clic una vez comprado.
* **Soporte Multidivisa:** Añade gastos en la moneda local del viaje y visualiza automáticamente su equivalente en tu moneda base.
* **Panel de Estadísticas Visuales:** Gráficos interactivos (Chart.js) para ver la distribución de los gastos y el "Wall of Shame" (Ranking de deudores).
* **La Ruleta RandomPay 🎡:** ¿Nadie quiere invitar la siguiente ronda? Una ruleta animada e interactiva decide al azar a quién le toca pagar.
* **Diseño "Zero-Scroll":** Interfaz de usuario moderna, limpia y compacta construida con tarjetas y scrolls internos para una experiencia premium en cualquier dispositivo.

## 🛠️ Stack Tecnológico

**Backend:**
* C# / ASP.NET Core MVC
* Entity Framework Core (Code-First / Database-First)
* LINQ

**Base de Datos:**
* Microsoft SQL Server

**Frontend:**
* Razor Pages (HTML5)
* CSS3 / Bootstrap (UI moderna y responsive)
* JavaScript (Lógica de la ruleta, conversión de divisas, animaciones)
* [SweetAlert2](https://sweetalert2.github.io/) (Alertas y modales amigables)
* [Chart.js](https://www.chartjs.org/) (Visualización de datos)
* FontAwesome (Iconografía)

## 🔒 Seguridad
* Autenticación y autorización basada en variables de Sesión.
* Protección contra inyección SQL garantizada por Entity Framework Core.
* Contraseñas encriptadas en la base de datos.
* Gestión estricta de la integridad referencial (Foreign Keys) para evitar registros huérfanos.

## 🚀 Instalación y Uso Local

Sigue estos pasos para ejecutar el proyecto en tu máquina local:

1. **Clona el repositorio:**
   ```bash
   git clone [https://github.com/TU_USUARIO/RandomPay.git](https://github.com/TU_USUARIO/RandomPay.git)

2-Configura la Base de Datos:
Abre el archivo appsettings.json y configura tu cadena de conexión a SQL Server en ConnectionStrings:

JSON
"ConnectionStrings": {
  "DefaultConnection": "Server=TU_SERVIDOR;Database=RandomPayDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}

Aplica las migraciones (si usas Code-First):
Abre la Consola del Administrador de Paquetes en Visual Studio y ejecuta:

PowerShell
Update-Database
Ejecuta la aplicación:
Pulsa F5 en Visual Studio o ejecuta el siguiente comando en la terminal:
Bash
dotnet run
