# ⛽ GasStationIS-lite
[![Russian](https://img.shields.io/badge/lang-Russian-blue)](README.md)

Gas station management information system with a local SQLite database. It is intended to automate fuel, sales, supplies, customer, and employee management.

---

## 🔍 Lite Version Differences

A simplified version without the auxiliary goods store module. Differences from the full version:

- **The store is completely absent** — there is no product catalog, cash register, or store sales history.
- **Customer** — only full name, phone number, and car number are stored (without email, points, and loyalty level).
- **Fuel sale** — the “pump number” and “note” fields have been removed.
- **Supply** — the “supplier phone number” and “note” fields have been removed.
- **Dashboard and reports** — show only fuel revenue without store breakdown.

---

## ✨ Features

- Summary dashboard with key indicators
- Fuel and tank balance tracking
- Fuel supply tracking
- Fuel sales tracking
- Customer management
- Employee management
- Report generation
- Role-based access control (Administrator / Operator)

---

## 📸 Screenshots

<table>
<tr align="center">
<td><img src="https://github.com/user-attachments/assets/b58621ea-e922-40f8-b7f2-13e42b564555" width="115"></td>
<td><img src="https://github.com/user-attachments/assets/33504a60-27a0-479a-9f32-3061d7f380f4" width="250"></td>
<td><img src="https://github.com/user-attachments/assets/14e3d1af-5a26-4323-9a61-fd807038c23d" width="250"></td>
</tr>

<tr>
<td><img src="https://github.com/user-attachments/assets/9bcdaa1f-c392-4962-ae35-38d2296ee9f5" width="250"></td>
<td><img src="https://github.com/user-attachments/assets/3fe3591b-e8ae-414a-8663-e85c9229d39a" width="250"></td>
<td><img src="https://github.com/user-attachments/assets/475547c1-86b8-49dc-afed-5799e1d0dbc1" width="250"></td>
</tr>

<tr>
<td><img src="https://github.com/user-attachments/assets/fa6b04ca-cf8e-49ce-9a07-ad7984a5dd9e" width="250"></td>
<td><img src="https://github.com/user-attachments/assets/fd1fa22b-6263-4782-8803-68605894e7ba" width="250"></td>
<td><img src="https://github.com/user-attachments/assets/9c33faec-c505-49c8-96a6-23869a47287f" width="250"></td>
</tr>
</table>

---

## 🚀 Launch

### Via Visual Studio

1. Clone the repository:
```
git clone -b lite https://github.com/MaxDev43/GasStationIS.git
```
2. Open the `.slnx` file in Visual Studio (NuGet packages will be installed automatically)
3. Run the project

### Ready-made release

Download the archive [`GasStationIS-lite-v1.0.zip`](../../releases/latest) from the **Releases** section, extract it, and run `.exe`.

---

## 🔐 Authorization

By default, the login and password are filled in automatically — convenient for testing and demonstration.

To disable autofill, open `Views/LoginWindow.xaml.cs` and set:

```csharp
AutoFillCredentials = false;
```

---

## 🌿 Branches

| Branch | Description |
|-------|----------|
| `main` | Full version |
| `lite` | Simplified version with reduced functionality (current branch) |

---

## 🛠 Stack

- C#
- WPF (.NET 10)
- SQLite
- MVVM

---

If you found the project useful, please give it a star ⭐