# ⛽ GasStationIS
[![English](https://img.shields.io/badge/lang-English-blue)](README.en.md)
Информационная система управления сетью АЗС с локальной базой данных SQLite. Предназначена для автоматизации учёта топлива, продаж, поставок, работы магазина, клиентов и сотрудников.

---

## ✨ Возможности

- Сводная панель с ключевыми показателями (дашборд)
- Учёт топлива и остатков в резервуарах
- Учёт поставок топлива
- Учёт продаж топлива
- Модуль магазина сопутствующих товаров
- Управление клиентами и программа лояльности
- Управление сотрудниками
- Формирование отчётов
- Разграничение прав доступа (Администратор / Оператор)

---

## 📸 Скриншоты

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

## 🚀 Запуск

### Через Visual Studio

1. Клонируйте репозиторий
```
git clone https://github.com/MaxDev43/GasStationIS.git
```
3. Откройте `.slnx` файл в Visual Studio (пакеты NuGet установятся автоматически)
4. Запустите проект

### Готовый релиз

Скачайте архив [`GasStationIS-v1.0.zip`](../../releases/latest) из раздела **Releases**, распакуйте и запустите `.exe`.

---

## 🔐 Авторизация

По умолчанию логин и пароль подставляются автоматически - удобно для тестирования и демонстрации.

Чтобы отключить автозаполнение, откройте `Views/LoginWindow.xaml.cs` и установите:

```csharp
AutoFillCredentials = false;
```

---

## 🌿 Ветки

| Ветка | Описание |
|-------|----------|
| `main` | Полная версия |
| `lite` | Упрощённая версия с урезанным функционалом |

---

## 🛠 Стек

- C#
- WPF (.NET 10)
- SQLite
- MVVM

---

Если проект оказался полезным, просьба поставить star ⭐
