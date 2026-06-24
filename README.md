# ⛽ GasStationIS-lite
[![English](https://img.shields.io/badge/lang-English-blue)](README.en.md)

Информационная система управления сетью АЗС с локальной базой данных SQLite. Предназначена для автоматизации учёта топлива, продаж, поставок, клиентов и сотрудников.

---

## 🔍 Отличия Lite-версии

Упрощённый вариант без модуля магазина сопутствующих товаров. Отличия от полной версии:

- **Магазин полностью отсутствует** — нет каталога товаров, кассы и истории продаж магазина.
- **Клиент** — хранятся только ФИО, телефон и номер авто (без email, баллов и уровня лояльности).
- **Продажа топлива** — убраны поля «номер колонки» и «примечание».
- **Поставка** — убраны поля «телефон поставщика» и «примечание».
- **Дашборд и отчёты** — показывают только топливную выручку без разбивки по магазину.

---

## ✨ Возможности

- Сводная панель с ключевыми показателями (дашборд)
- Учёт топлива и остатков в резервуарах
- Учёт поставок топлива
- Учёт продаж топлива
- Учёт клиентов
- Управление сотрудниками
- Формирование отчётов
- Разграничение прав доступа (Администратор / Оператор)

---

## 📸 Скриншоты

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

## 🚀 Запуск

### Через Visual Studio

1. Клонируйте репозиторий:
```
git clone -b lite https://github.com/MaxDev43/GasStationIS.git
```
2. Откройте `.slnx` файл в Visual Studio (пакеты NuGet установятся автоматически)
3. Запустите проект

### Готовый релиз

Скачайте архив [`GasStationIS-lite-v1.0.zip`](../../releases/latest) из раздела **Releases**, распакуйте и запустите `.exe`.

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
| `lite` | Упрощённая версия с урезанным функционалом (текущая ветка) |

---

## 🛠 Стек

- C#
- WPF (.NET 10)
- SQLite
- MVVM

---

Если проект оказался полезным, просьба поставить star ⭐
