# nopCommerce + Mobile REST API (проект Bell You)

Форк платформы электронной коммерции **nopCommerce** (ASP.NET Core, .NET 9) с собственным
приватным REST API для мобильного клиентского приложения — плагин **`Nop.Plugin.Api.Mobile`**
(эндпоинты `/api/v1/...`, аутентификация JWT, документация OpenAPI/Swagger).

---

## Требования

- **Docker Desktop** (рекомендуемый способ запуска), либо
- **.NET 9 SDK** — для запуска без Docker.

---

## Быстрый запуск (Docker + PostgreSQL)

Из корня репозитория:

```bash
docker compose -f postgresql-docker-compose.yml up --build
```

Поднимаются два контейнера:

- `nopcommerce` — веб-приложение (порт хоста **80** → порт контейнера 8080);
- `nopcommerce_postgres_server` — база данных PostgreSQL.

После старта приложение доступно на **http://localhost/**. При первом запуске произойдёт
переадресация на мастер установки `/install`.

> Есть также `docker-compose.yml` для варианта с MS SQL Server.

---

## Установка магазина (мастер `/install`)

Откройте http://localhost/ — откроется мастер установки. Параметры для варианта с PostgreSQL:

| Поле | Значение |
|---|---|
| Тип базы данных | PostgreSQL |
| Server / Host | `nopcommerce_database` |
| Database | `nopCommerce` (с галочкой «создать базу, если не существует») |
| User | `postgres` |
| Password | `nopCommerce_db_password` |
| Email администратора | `admin@admin.com` (предзаполнено, можно изменить) |
| Пароль администратора | задаётся при установке (в dev использовался `admin`) |

После установки магазин перенаправит на витрину.

---

## Основные ресурсы платформы

| Ресурс | URL | Доступ |
|---|---|---|
| Витрина (главная) | http://localhost/ | публично |
| Админка | http://localhost/admin | под учётной записью администратора |
| Страница входа | http://localhost/login | email + пароль администратора |
| Swagger UI (документация API) | http://localhost/swagger | публично |
| REST API (базовый префикс) | http://localhost/api/v1 | публично / по токену |

### Как войти в админку

Перейдите на **http://localhost/admin** — произойдёт переадресация на **/login**. Введите
**email и пароль администратора**, заданные при установке (по умолчанию email `admin@admin.com`).
После входа откроется панель управления.

---

## Мобильный REST API (плагин `Nop.Plugin.Api.Mobile`)

1. Войдите в админку → **Configuration → Local plugins**.
2. Найдите плагин **«Mobile REST API»** (system name `Api.Mobile`) и нажмите **Install**
   (при установке генерируется секретный ключ для подписи JWT).
3. Документация и «песочница» — **http://localhost/swagger**.

Аутентификация:

- `POST /api/v1/auth/register` — регистрация покупателя;
- `POST /api/v1/auth/token` — вход, выдача JWT;
- полученный токен передаётся в заголовке `Authorization: Bearer <token>`;
- `POST /api/v1/auth/logout` — отзыв токена.

---

## Учётные данные (dev-значения)

> ⚠️ **Внимание.** Значения ниже — для локальной разработки. **Перед выводом в продакшен
> ОБЯЗАТЕЛЬНО смените все пароли и секреты**, ограничьте доступ к админке и Swagger, не храните
> секреты в репозитории.

| Назначение | Логин | Пароль / значение | Где задаётся |
|---|---|---|---|
| PostgreSQL | `postgres` | `nopCommerce_db_password` | `postgresql-docker-compose.yml` |
| MS SQL Server (вариант) | `sa` | `nopCommerce_db_password` | `docker-compose.yml` |
| Администратор магазина | `admin@admin.com` | `admin` (dev; задаётся при установке) | мастер `/install` |
| Секрет подписи JWT | — | генерируется автоматически при установке плагина | настройки магазина (БД) |

**Рекомендации для продакшена:**

- сменить пароли БД (`postgres` / `sa`) и не использовать значения по умолчанию;
- сменить email/пароль администратора на надёжные;
- перегенерировать секрет JWT (переустановка плагина создаёт новый) и хранить его безопасно;
- не публиковать Swagger и админку в открытом доступе без ограничений;
- не коммитить `src/Presentation/Nop.Web/App_Data/appsettings.json` (содержит строку подключения);
  файл уже в `.gitignore`.

---

## Запуск без Docker (опционально)

1. Установите **.NET 9 SDK**.
2. Откройте `src/NopCommerce.sln` в Visual Studio (или выполните
   `dotnet run --project src/Presentation/Nop.Web`).
3. Пройдите мастер `/install`, указав доступную БД (PostgreSQL / MS SQL Server).

---

## Тесты

Интеграционные тесты плагина (NUnit, SQLite, sample-данные):

```bash
dotnet test src/NopCommerce.sln --filter "FullyQualifiedName~Nop.Plugin.Api.Mobile.Tests"
```

---

## Своя тема оформления

Тема — это отдельная папка в `src/Presentation/Nop.Web/Themes/`, которая переопределяет
стили и представления дефолтной темы; править код платформы не нужно.

Кратко:

1. Скопировать `Themes/DefaultClean` в `Themes/<ВашаТема>`.
2. В `theme.json` задать `SystemName` (совпадает с именем папки) и `FriendlyName`.
3. Положить стили в `Content/css/styles.css`, изображения — в `Content/images/`;
   при необходимости переопределить представления в `Views/` (с тем же относительным путём,
   что и в `Nop.Web/Views/`).
4. Активировать в админке: **Configuration → Settings → General settings → Default store theme**.

Подробное руководство — в официальной документации:
https://docs.nopcommerce.com/en/developer/design/new-theme.html

---

## О платформе nopCommerce

[nopCommerce](https://www.nopcommerce.com/) — бесплатная платформа электронной коммерции с
открытым исходным кодом на ASP.NET Core (.NET 9), поддерживает MS SQL Server, PostgreSQL и MySQL,
кросс-платформенна и работает в Docker.

Полезные ссылки:

- Документация: https://docs.nopcommerce.com
- Демо-магазин: https://demo.nopcommerce.com
- Исходный код: https://github.com/nopSolutions/nopCommerce
