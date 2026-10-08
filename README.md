# ⚡ QuickDrop – AI-Powered Rapid Grocery Delivery Platform
### **Modern On-Demand Delivery & Recipe Commerce (.NET MAUI + SQLite + Google Gemini AI + MVVM + Cross-Platform UI)**

---

## 🚀 About QuickDrop

**QuickDrop** is a high-performance, cross-platform (.NET MAUI for Windows, Android, and iOS) on-demand grocery and market delivery platform. 

It provides an end-to-end multi-role ecosystem spanning **Customers**, **Super Admins**, **Vendors**, and **Riders/Couriers**, backed by local **SQLite** offline-first persistence, automated cart and order lifecycle workflows, and intelligent **Google Gemini AI** recipe-to-cart automation.

---

## 📌 Roles & Architecture

1. **👤 Customer Experience:**
   - Browse fresh products by categories, search, and flash deals.
   - Smart cart with real-time discounts, coupon redemption, and address management.
   - Order tracking and post-delivery review system (rating rider service & vendor food quality).
   - Interactive **Wheel of Fortune** rewards system.

2. **⚡ Super Admin & 🏪 Vendor Console (`AdminPanelPage`):**
   - Live KPI metrics (Gross Revenue, Active Orders, Inventory Counts, Accounts).
   - Real-time order dispatch lifecycle transitions (`Preparing` ➔ `On The Way` ➔ `Delivered`).
   - Catalog management: instant stock adjustment steppers and new product publishing.
   - Customer review monitoring with dynamic prioritization (Sort by Rider Rating or Food Quality).

3. **🛵 Rider Delivery Console (`RiderPage`):**
   - Shift metrics (Active Deliveries, Completed Drops, Calculated Shift Earnings).
   - Dispatch filters (`All`, `Ready / On The Way`, `Delivered`).
   - One-tap status updates (`Pick Up`, `Mark Delivered`) and direct customer call action.

---

## 🔑 Demo Login Accounts

| Role | Email | Password | Landing Screen |
|---|---|---|---|
| **⚡ Super Admin** | `admin@quickdrop.com` | `admin123` | **Admin & Vendor Console** |
| **🏪 Vendor** | `vendor@quickdrop.com` | `vendor123` | **Admin & Vendor Console** |
| **🛵 Rider** | `rider@quickdrop.com` | `rider123` | **Rider Delivery Console** |
| **👤 Customer** | `demo@quickdrop.com` | `123456` | **Store Front (Home)** |

---

## ⚙️ Built With

- **.NET MAUI 9**
- **C# 12**
- **SQLite-net-pcl**
- **Google Gemini 3.6 Flash** (Natural language recipe generation & automated cart matching)
- **MVVM Architecture & Reactive UI**
