# 🛒 POS System — .NET 8 + WPF + SQLite

A complete, local-only Point of Sale (POS) desktop application built with **.NET 8**, **WPF**, **C#**, and **SQLite**. Runs entirely on a single Windows machine — no server, no internet required.

---

## ✨ Features

| Module | What it does |
|---|---|
| 🔐 **Login & Signup** | Username + password (SHA256 + per-user salt). Public signups are always Cashiers; Admins are created from the Users panel. |
| 🧾 **Sales / Checkout** | Search products by name or barcode, add to cart, apply % discount, choose payment method (Cash / Card / Mobile), complete sale, print receipt. Cash payments validate that the amount received covers the total and show the correct change. |
| 📦 **Inventory** | Full CRUD for products. Stock auto-decreases after every sale (atomic DB transaction). Rows highlight in **yellow** when `Stock <= MinStock`. |
| 💳 **Payments** | Three methods recorded per sale: Cash, Card, Mobile. |
| 📊 **Reports** | Pick any date range. Shows: # of sales, revenue, cost of goods, **profit (Revenue − Cost)**, breakdown by payment method, and **top 5 best-selling products**. |
| 👥 **Users (Admin only)** | Add/delete cashiers and admins. |

### Roles
- **Admin** sees: Checkout, Inventory, Reports, Users.
- **Cashier** sees: Checkout only.

### Default seeded accounts (created on first run)
| Username | Password | Role |
|---|---|---|
| `admin` | `admin123` | Admin |
| `cashier1` | `cashier123` | Cashier |
| `cashier2` | `cashier123` | Cashier |

The database also seeds 10 sample products on first run so reports and inventory aren't empty.

---

## 📋 Requirements (for you and your friend)

- **Windows 10 or Windows 11** (WPF is Windows-only)
- **.NET 8 SDK** — download here: <https://dotnet.microsoft.com/download/dotnet/8.0>
  (Pick **".NET 8.0 SDK"** for Windows x64.)
- *(Optional)* **Visual Studio 2022** (Community edition is free) — only if you prefer an IDE over the command line.

You only need the SDK. You do NOT need to install SQLite separately — the app's NuGet package handles it.

---

## ▶️ How to run the project

### Option A — Easiest (command line)

1. **Unzip** `POSSystem.zip` somewhere, e.g. `C:\Projects\POSSystem`.
2. Open **Command Prompt** or **PowerShell** in that folder.
3. Run:
   ```
   cd POSSystem
   dotnet restore
   dotnet run
   ```
   The first `dotnet restore` downloads SQLite + dependencies (needs internet **once**). After that the app runs offline.
4. The login window opens. Use one of the default accounts above.

### Option B — Visual Studio 2022

1. Unzip the project.
2. Double-click **`POSSystem.sln`**.
3. Press **F5** (or click ▶ Start). Visual Studio handles restore + build + run.

### Option C — Build a portable EXE to share

If you want to give your friend a single ready-to-run EXE (no SDK install on their machine):

```
cd POSSystem
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

The output appears in:
```
POSSystem\bin\Release\net8.0-windows\win-x64\publish\
```

Send the `publish` folder. Your friend just double-clicks `POSSystem.exe`. No .NET install needed on their side.

---

## 🗄️ Where is the database stored?

A file called **`pos.db`** is created next to the executable on first run. All users, products, sales, and sale items live in there. To **reset the system**, just delete `pos.db` and restart — it will recreate itself with default accounts and sample products.

---

## 📁 Project Structure

```
POSSystem/
├── POSSystem.sln
├── README.md
└── POSSystem/
    ├── POSSystem.csproj         ← project + SQLite dependency
    ├── App.xaml / App.xaml.cs   ← startup + global styles
    ├── Models/                  ← User, Product, Sale, SaleItem, CartItem
    ├── Data/
    │   └── DatabaseHelper.cs    ← creates pos.db, schema, seeds defaults
    ├── Helpers/
    │   └── PasswordHasher.cs    ← SHA256 + salt
    ├── Services/                ← Auth, Product, Sale, User, Report, Session
    ├── Views/                   ← Login, Signup, Main, Receipt, ProductDialog
    └── UserControls/            ← Checkout, Inventory, Reports, Users
```

---

## 🧪 Quick test workflow

1. Launch the app, login as `admin` / `admin123`.
2. Go to **Inventory** → confirm 10 sample products are listed.
3. Go to **Checkout** → search "Cola" → click **+ Add** → click **✓ Complete Sale** → receipt window opens.
4. Go to **Inventory** → notice the stock for that product decreased.
5. Go to **Reports** → click **Today** → see the sale you just made, with revenue + profit.
6. Logout → login as `cashier1` / `cashier123` → confirm only Checkout is visible.

---

## 🛠️ Troubleshooting

**"dotnet is not recognized as a command"**
You haven't installed the .NET 8 SDK, or it's not on PATH. Reinstall from the link above and reopen your terminal.

**"Cannot open file pos.db"** or weird DB errors
Delete `pos.db` (next to the .exe or in `POSSystem\bin\Debug\net8.0-windows\`) and restart the app — it will rebuild from scratch.

**Build error mentioning Microsoft.Data.Sqlite**
Run `dotnet restore` once with internet, then build again.

**App opens but no products show in Inventory**
You probably ran a build that created `pos.db` before products were seeded. Delete `pos.db` and re-run.

---

Made with care. Happy selling 🛒
# POINT-OF-SALE-SYSTEM
