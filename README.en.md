# ⛽ GasStationIS
[![Russian](https://img.shields.io/badge/lang-Russian-blue)](README.md)

Gas station management information system with a local SQLite database. It is intended to automate fuel, sales, supplies, store operations, customer, and employee management.

---

## ✨ Features

- Summary dashboard with key indicators
- Fuel and tank balance tracking
- Fuel supply tracking
- Fuel sales tracking
- Store module for related goods
- Customer management and loyalty program
- Employee management
- Report generation
- Role-based access control (Administrator / Operator)

---

## 📸 Screenshots

<table>
<tr align="center">
<td><img src="https://github.com/user-attachments/assets/f77e6c46-6b35-4b0c-9c8c-9a3aa79983a2" width="115"></td>
<td><img src="https://github.com/user-attachments/assets/651a2766-6542-4dde-8239-c08bab5d9068" width="250"></td>
<td><img src="https://github.com/user-attachments/assets/fc1de0aa-ddf3-4b3c-8ac5-edd7a5952374" width="250"></td>
</tr>

<tr>
<td><img src="https://github.com/user-attachments/assets/aa61bdcb-c625-4a7a-930c-6c14753d3fe3" width="250"></td>
<td><img src="https://github.com/user-attachments/assets/e7933db9-87df-445c-8572-12bd95107b56" width="250"></td>
<td><img src="https://github.com/user-attachments/assets/f1f801c2-bb33-4134-8da8-cbc49d1f06eb" width="250"></td>
</tr>

<tr>
<td><img src="https://github.com/user-attachments/assets/129c8609-bd29-417b-9083-6fc11bb3198b" width="250"></td>
<td><img src="https://github.com/user-attachments/assets/429b9150-7128-4416-96d9-0bda9997a718" width="250"></td>
<td><img src="https://github.com/user-attachments/assets/70812ee5-caa5-47fa-ab6c-319ea44e97a2" width="250"></td>
</tr>

<tr>
<td><img src="https://github.com/user-attachments/assets/46e27285-0805-49e4-b9c0-c2708d842299" width="250"></td>
<td></td>
<td></td>
</tr>
</table>

---

## 🚀 Launch

### Via Visual Studio

1. Clone the repository
```
git clone https://github.com/MaxDev43/GasStationIS.git
```
2. Open the `.slnx` file in Visual Studio (NuGet packages will be installed automatically)
3. Run the project

### Ready-made release

Download the archive [`GasStationIS-v1.0.zip`](../../releases/latest) from the **Releases** section, extract it, and run the `.exe`.

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
| `lite` | Simplified version with reduced functionality |

---

## 🛠 Stack

- C#
- WPF (.NET 10)
- SQLite
- MVVM

---

If you found the project useful, please give it a star ⭐
