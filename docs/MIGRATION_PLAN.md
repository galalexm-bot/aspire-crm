# План переноса функциональности ELMA CRM → AspireCRM

## Общая информация

- **Источник**: `EleWise.ELMA.CRM.Web` (ELMA BPM platform, ASP.NET MVC, Telerik UI)
- **Цель**: `AspireCRM` (.NET 10, Blazor Server, Minimal APIs, EF Core + SQLite)
- **Архитектура цели**: Domain / DataLayer / ApiService / Web (чистая многослойная)
- **Статус**: Часть доменной модели и базовый CRUD уже перенесены

---

## Этап 0. Уже реализовано ✓

### Доменная модель (Domain/)
- [x] Common: BaseEntity, Address, Email, Phone, Comment, Category, CategoryType, ApplicationUser, Tenant
- [x] Contractors: Contractor, ContractorLegal, ContractorIndividual, Contact, ContractorRegion, ContractorIndustry, ContractorType, LegalForm, ClientType, ClientDocumentType, ContactPriority
- [x] Leads: Lead, LeadContact, LeadSource, LeadStatus, LeadType
- [x] Sales: Sale, SaleFunnel, SaleProduct, SaleStage, SaleStatus, SaleType, SalePriority, Currency
- [x] Payments: Inpayment, InpaymentStatus
- [x] Products: Product
- [x] Relationships: Relationship, RelationshipCall, RelationshipMail, RelationshipMeeting, RelationshipUser, RelationshipCallType, RelationshipPriority, RelationshipUserStatus

### Слой данных (DataLayer/)
- [x] AspireCRMDbContext (+ настройка TPH, many-to-many, конфигурация всех сущностей)
- [x] Миграции EF Core
- [x] SeedData (все справочники на русском языке)
- [x] Generic репозиторий с tenant-фильтрацией
- [x] TenantService

### API (ApiService/)
- [x] Auth (регистрация, логин, JWT)
- [x] Leads CRUD
- [x] Contractors CRUD
- [x] Contacts CRUD (с фильтром по контрагенту)
- [x] Sales CRUD
- [x] Inpayments CRUD (с фильтром по сделке)
- [x] Products CRUD
- [x] Categories CRUD
- [x] Relationships CRUD (с фильтрами)
- [x] Lookups (11 справочных эндпоинтов)

### Web (Blazor Server)
- [x] Аутентификация (Login.razor, JwtAuthStateProvider, AuthTokenStore)
- [x] Навигация (NavMenu.razor)
- [x] Список лидов (Leads.razor)
- [x] Создание лида (LeadNew.razor)
- [x] Редактирование лида (LeadDetail.razor)
- [x] Список контрагентов (Contractors.razor)
- [x] Список контактов (Contacts.razor)
- [x] Список сделок (Sales.razor)

---

## Этап 1. Расширение Lead-функционала

### 1.1. Статусные переходы Lead
- **ELMA**: `LeadController.Begin()` (→InHand), `Fail()` (→Unqualified), `ConversationNotStart()`, `Activate()` (массовая активация)
- **Задача**: Эндпоинты API + Blazor-страницы для смены статуса
- **Требуется**:
  - `LeadEndpoints.cs`: POST `/api/leads/{id}/begin`, `/fail`, `/conversation-not-start`
  - `Leads.razor`: Кнопки на детальной странице
  - DTO: `LeadStatusChangeRequest` (с комментарием)

### 1.2. Дубликаты Lead
- **ELMA**: `LeadController.Dublicate()`, `LeadAttachDublicate()`, `ContractorAttachDublicate()`, `NotDublicate()`, `CheckCompleteCalcDublicate()`
- **Задача**: Закончить механизм дубликатов (уже есть поля `DublicateLead`, `DublicateContractor` и т.д. в модели Lead)
- **Требуется**:
  - API: POST `/api/leads/{id}/mark-duplicate`, `/unmark-duplicate`
  - Чек дубликатов при создании/редактировании лида
  - UI: модальное окно выбора дубликата

### 1.3. Конвертация Lead (самая сложная часть)
- **ELMA**: `LeadController.Convert()` — пошаговый мастер:
  1. Шаг 1: Выбор типа конвертации (Контрагент (ЮЛ/ИП) / Сделка / Взаимоотношение (Звонок/Письмо/Встреча))
  2. Шаг 2: `PrepareContractorData` — маппинг полей Lead → Contractor, выбор формы
  3. Шаг 3: `PrepareContactsData` — маппинг контактов лида → контакты контрагента
  4. Шаг 4: `PrepareSaleRelData` — создание сделки или взаимоотношения
  5. Финал: `ConvertComplete` — сохранение всего сразу в транзакции
- **Задача**: Создать процедуру конвертации лида в контрагента/сделку/отношение
- **Требуется**:
  - `/api/leads/{id}/convert/preview` (GET — возвращает возможные типы конвертации)
  - `/api/leads/{id}/convert` (POST — выполняет конвертацию)
  - `LeadConversionRequest` / `LeadConversionResult` DTO
  - Blazor-страница мастера конвертации (пошаговая)

### 1.4. Назначение ответственного
- **ELMA**: `SelectAndAssign()`, `AssignTo()`, `ChangeType()` — массовые операции
- **Задача**: API + UI для массового назначения
- **Требуется**:
  - POST `/api/leads/batch/assign`
  - POST `/api/leads/batch/change-type`
  - UI: выбор пользователя, выбор типа

---

## Этап 2. Расширение Contractor-функционала

### 2.1. Детальная страница контрагента
- **ELMA**: Сложная страница с вкладками: контакты, сделки, взаимоотношения, файлы, история
- **Задача**: Создать `ContractorDetail.razor`
- **Требуется**:
  - Детальная страница с отображением всех полей
  - Заполненный адрес (юридический/фактический)
  - Emails, Phones (добавление/удаление inline)

### 2.2. Bank Accounts и Payment Cards
- **ELMA**: `BankAccountController.cs` (78 строк), `PaymentCardController.cs` (79 строк)
- **Задача**: Добавить сущности банковских счетов и платежных карт
- **Требуется**:
  - Domain: `BankAccount`, `PaymentCard` (связь с Contractor)
  - API: CRUD эндпоинты
  - UI: управление на странице контрагента

### 2.3. Юридические / Физические лица
- **ELMA**: Раздельное редактирование для `ContractorLegal` и `ContractorIndividual`
- **Задача**: Создать отдельные формы/страницы для ЮЛ и ИП
- **Требуется**:
  - Проверка/исправление TPH (уже настроено через `ContractorType` discriminator)
  - `ContractorDetail.razor` — отображение специфических полей в зависимости от типа
  - API: включение дискриминатора при создании

### 2.4. Адреса, телефоны, email
- **ELMA**: AddressController, PhoneController — inline-редактирование
- **Задача**: Сделать полноценное управление контактами
- **Требуется**:
  - API: эндпоинты для Address, Phone, Email как вложенных ресурсов
  - UI: inline-редактирование на странице контрагента

---

## Этап 3. Расширение Sale-функционала

### 3.1. Детальная страница сделки
- **Задача**: Создать `SaleDetail.razor` и `SaleNew.razor`
- **Требуется**:
  - Форма создания/редактирования сделки
  - Выбор контрагента, воронки, этапа, типа, валюты
  - Поля: приоритет, маркетинговый эффект, объем продаж

### 3.2. Товары в сделке
- **ELMA**: `SaleProductEditModel`, `SaleProductsModel`, `AddSaleProductPopupViewModel`
- **Задача**: CRUD товарных позиций в сделке
- **Требуется**:
  - API: POST `/api/sales/{id}/products`, PUT/DELETE
  - UI: таблица товаров с количеством, ценой, скидкой

### 3.3. Смена этапа и статуса
- **ELMA**: `SaleChangeStageModel`, `SaleChangeStatusModel`
- **Задача**: API + UI для смены этапа воронки и статуса сделки
- **Требуется**:
  - POST `/api/sales/{id}/change-stage`, `/change-status`
  - UI: кнопки/выпадающие списки

### 3.4. Воронки, этапы, типы сделок
- **Задача**: CRUD для справочников (уже есть модель, нет UI)
- **Требуется**:
  - Blazor-страницы: `SaleFunnels.razor`, `SaleStages.razor`, `SaleTypes.razor`
  - Drag-and-drop сортировка этапов

### 3.5. Портлет сделок
- **ELMA**: `SalePortletModel.cs`, `SalePortlet` view
- **Задача**: Dashboard-виджет со сводкой по сделкам
- **Требуется**:
  - Компонент на главной странице: сводка по статусам, суммам, просроченным

---

## Этап 4. Inpayment (Платежи)

### 4.1. Страница оплат по сделке
- **Задача**: Создать полноценную страницу платежей
- **Требуется**:
  - `Inpayments.razor` — список всех платежей
  - `InpaymentDetail.razor`, `InpaymentNew.razor`

### 4.2. Статусы платежей
- **ELMA**: `InpaymentChangeStatusModel`, `InpaymentChangeDateModel`
- **Задача**: Смена статуса (В плане → Получен → Отменён)
- **Требуется**:
  - POST `/api/inpayments/{id}/change-status`
  - UI: кнопки с подтверждением, комментарий к смене статуса

### 4.3. Портлет платежей
- **ELMA**: `InpaymentPortletModel.cs`
- **Задача**: Dashboard-виджет — сводка по платежам
- **Требуется**:
  - Компонент на главной: план/факт, просроченные

---

## Этап 5. Relationships (Взаимоотношения)

### 5.1. CRUD отношений (Звонки, Встречи, Письма)
- **ELMA**: RelationshipController, RelationshipCallController, RelationshipMailController, RelationshipMeetingController
- **Задача**: Создать детальные страницы для каждого типа
- **Требуется**:
  - `RelationshipCallDetail.razor`, `RelationshipMailDetail.razor`, `RelationshipMeetingDetail.razor`
  - `Relationships.razor` — общий список с фильтрами
  - API: расширить эндпоинты для всех подтипов

### 5.2. Участники отношений
- **ELMA**: `RelationshipUser` (участник/к уведомлению)
- **Задача**: Управление участниками
- **Требуется**:
  - UI: добавление пользователей как участников/наблюдателей

### 5.3. Завершение и приватность
- **ELMA**: `RelationshipCompleteModel`, `RelationshipIsPrivate.cshtml`
- **Задача**: Отметка о выполнении, приватность отношений
- **Требуется**:
  - POST `/api/relationships/{id}/complete`
  - Переключатель приватности

### 5.4. Календарь
- **ELMA**: CalendarPortletCall/Mail/MeetingColumn — отображение в календаре
- **Задача**: Календарный вид отношений
- **Требуется**:
  - Календарный компонент с фильтрацией по типу

---

## Этап 6. Products (Товары)

### 6.1. Иерархический каталог
- **Задача**: Blazor-страница для управления товарами/группами
- **Требуется**:
  - `Products.razor` — дерево групп + список товаров
  - API: дерево продуктов (parent-children)
  - UI: создание групп и товаров, drag-and-drop

---

## Этап 7. Categories (Категории)

### 7.1. CRUD категорий
- **Задача**: Blazor-страница управления категориями
- **Требуется**:
  - `Categories.razor` — список, создание, редактирование
  - Типы категорий: Normal / Exclusive

### 7.2. Category Rules (Автоматические правила)
- **ELMA**: `CategoryRuleController.cs` — EQL-правила для автокатегоризации
- **Задача**: Система правил автораспределения категорий
- **Требуется**:
  - Domain: `CategoryRule` (условия, действия)
  - API: CRUD правил
  - Механизм применения правил при создании/сохранении лидов и контрагентов

---

## Этап 8. Marketing Activities (Маркетинговые активности)

### 8.1. Маркетинговый модуль
- **ELMA**: `MarketingActivityController.cs` (1001 строка!) — самый большой контроллер
- **Задача**: Перенос маркетинговых активностей
- **Требуется**:
  - Domain: `MarketingActivity`, `MarketingPayment`, `MarketingElement`
  - API: CRUD, бюджет, статистика
  - Статистика: эффект от маркетинга (связь с лидами/сделками)

---

## Этап 9. Sales Plans (Планы продаж)

### 9.1. Планирование
- **ELMA**: `SalesPlanController.cs` (173 строки)
- **Задача**: Календарные планы продаж
- **Требуется**:
  - Domain: `SalesPlan`
  - API: CRUD
  - UI: календарный вид с суммами по месяцам

---

## Этап 10. Security (Безопасность)

### 10.1. Категорийный доступ
- **ELMA**: `SecurityController.cs`, `CRMPermissionModel.cs`, `CrmCustomPermissionsModel.cs`
- **Задача**: Система прав доступа на основе категорий
- **Требуется**:
  - API: управление правами пользователей на категории
  - UI: страница настройки прав
  - Middleware: проверка прав при операциях с сущностями

---

## Этап 11. Attachments (Файлы)

### 11.1. Файловый менеджер
- **ELMA**: `AttachmentController.cs`, `CrmAttachment.cs`
- **Задача**: Прикрепление файлов к сущностям
- **Требуется**:
  - Domain: `CrmAttachment` (сущность, тип файла, ссылка на владельца)
  - API: загрузка/скачивание/удаление
  - UI: список файлов на детальных страницах

---

## Этап 12. Full-Text Search

### 12.1. Поиск
- **ELMA**: `FullTextSearchController.cs`
- **Задача**: Полнотекстовый поиск по всем сущностям
- **Требуется**:
  - API: `/api/search?q=...`
  - Индексация (SQLite FTS5 или внешний поиск)
  - UI: строка поиска в шапке

---

## Этап 13. Tasks (Задачи CRM)

### 13.1. Задачи, связанные с CRM
- **ELMA**: `TaskController.cs`, `TaskEditForm.cs`, `TaskViewForm.cs`
- **Задача**: Задачи, привязанные к лидам/контрагентам/сделкам
- **Требуется**:
  - Domain: `CrmTask`
  - API: CRUD
  - UI: список задач на детальных страницах

---

## Этап 14. Portlets (Виджеты дашборда)

### 14.1. Главная страница
- **Задача**: Дашборд с портлетами
- **Требуется**:
  - `Home.razor` — превратить в рабочий дашборд
  - Портлеты: сводка по лидам, сделкам, платежам, предстоящие отношения

---

## Этап 15. History / Audit (История)

### 15.1. Комментарии и история
- **ELMA**: CommentHistoryPartProvider для всех сущностей
- **Задача**: Система комментариев и истории изменений
- **Требуется**:
  - Отображение комментариев на детальных страницах
  - Лента событий (кто и когда менял статусы)

---

## Этап 16. Администрирование

### 16.1. Настройки CRM
- **ELMA**: `CrmSettingsController.cs`, `CrmSettingsModuleController.cs`
- **Задача**: Страница настроек модуля
- **Требуется**:
  - `Admin.razor` — страница настроек (валюта по умолчанию, страна и т.д.)

---

## Приоритеты реализации

| Приоритет | Этап | Обоснование |
|-----------|------|-------------|
| **P0** | 1 (Lead) | Ядро системы, уже частично реализовано |
| **P0** | 2 (Contractor) | Ядро системы, уже частично реализовано |
| **P0** | 3 (Sale) | Ядро системы, уже частично реализовано |
| **P1** | 4 (Inpayment) | Платежи — ключевая функция |
| **P1** | 5 (Relationships) | Коммуникации — важная функция |
| **P1** | 6 (Products) | Каталог — нужен для сделок |
| **P1** | 7 (Categories) | Классификация — нужна для прав |
| **P2** | 10 (Security) | Безопасность |
| **P2** | 11 (Attachments) | Файлы |
| **P2** | 14 (Portlets) | Дашборд |
| **P2** | 15 (History) | История изменений |
| **P3** | 8 (Marketing) | Сложный модуль |
| **P3** | 9 (Sales Plans) | Планирование |
| **P3** | 12 (Full-Text Search) | Поиск |
| **P3** | 13 (Tasks) | Задачи |
| **P3** | 16 (Admin) | Настройки |

---

## Технические заметки

1. **Типографические ошибки**: В ELMA-проекте есть унаследованные опечатки (`Dublicate`, `InfromTo`). Рекомендуется при переносе новой функциональности писать правильно (`Duplicate`, `InformTo`), старые поля оставить для совместимости с пометкой `[Obsolete]`.

2. **TPH (Table-Per-Hierarchy)**: Уже настроен для Contractor и Relationship. Новые подтипы должны использовать ту же стратегию.

3. **Мультиарендность**: Все новые сущности должны наследовать `BaseEntity` (содержит `TenantId`).

4. **Удаление**: Только soft-delete через `IsDeleted`, физическое удаление не используется.

5. **DTO**: Для API рекомендуется создать отдельные Record-типы, а не использовать доменные сущности напрямую.

---

*План создан: 26.06.2026*