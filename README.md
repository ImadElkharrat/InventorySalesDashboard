# Inventory & Sales Dashboard

A comprehensive web-based system for managing inventory, tracking sales, and visualizing business performance. Built with ASP.NET Core 8.0 MVC.

## 🚀 Features

- **Dashboard:** Real-time overview of key metrics like total revenue, profit, recent orders, and critical stock alerts.
- **Inventory Management:**
  - Manage Products, Categories, and Suppliers.
  - Track stock levels and receive low-stock alerts.
  - Support for product images.
- **Order Management:** Create and manage customer orders with automatic calculations.
- **Reporting:**
  - **Sales Reports:** View revenue and profit over specific date ranges.
  - **Inventory Reports:** Analyze stock value and identify low-stock items.
  - **Product Performance:** Track best-selling products by revenue and profit.
  - **Export:** Support for exporting data (likely Excel/PDF based on dependencies).
- **Authentication:** Secure user registration and login system.
- **Real-time Updates:** Uses SignalR for live dashboard updates.
- **API Support:** JSON API endpoints for Products and Orders integration.

## 🛠️ Technology Stack

- **Framework:** ASP.NET Core 8.0 (MVC & Razor Pages)
- **Database:** Entity Framework Core (SQL Server / SQLite supported)
- **Frontend:**
  - Razor Views
  - Chart.js (Data Visualization)
  - FontAwesome (Icons)
- **Libraries & Tools:**
  - `ClosedXML` (Excel Export)
  - `QuestPDF` (PDF Generation)
  - `SignalR` (Real-time Web Functionality)
  - `ASP.NET Core Identity` (Security)

## 📋 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or LocalDB / SQLite)

## ⚙️ Installation & Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/InventorySalesDashboard.git
   cd InventorySalesDashboard
   ```

2. **Configure the Database**
   Update the connection string in `appsettings.json` if necessary. The default uses LocalDB:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=InventorySalesDb;Trusted_Connection=true;MultipleActiveResultSets=true"
   }
   ```

3. **Apply Migrations**
   Create the database and apply the schema:
   ```bash
   dotnet ef database update
   ```

4. **Run the Application**
   ```bash
   dotnet run
   ```
   The application will likely start at `https://localhost:7152` or `http://localhost:5246` (check the console output).

## 📧 Configuration

### Email Settings
To enable email features (like alerts), configure the SMTP settings in `appsettings.json`:
```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "Port": 587,
  "SenderName": "Inventory System",
  "SenderEmail": "your-email@example.com",
  "Password": "your-app-password"
}
```

## 🖥️ Usage

1. **Register/Login:** Create an account to access the system.
2. **Setup Data:** Start by adding **Categories** and **Suppliers**, then add **Products**.
3. **Manage Orders:** Create new orders as sales occur.
4. **Monitor Dashboard:** Watch the dashboard for real-time updates on sales and inventory status.
5. **View Reports:** Navigate to the Reports section to analyze performance.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is open-source and available under the [MIT License](LICENSE).
