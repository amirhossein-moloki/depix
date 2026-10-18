# گزارش کامل معماری سیستم کنترل دسترسی (RBAC) و لیست تمام APIها به همراه سطوح دسترسی

این گزارش شامل نحوه کارکرد فنی سیستم RBAC در پروژه، نقش‌ها، کدهای دسترسی (Permission Policy) و تفکیک کامل تک‌تک APIهای موجود در تمامی ماژول‌های پروژه به همراه سطح دسترسی مورد نیاز آن‌ها می‌باشد.

---

## بخش ۱: نحوه کارکرد سیستم RBAC در پروژه

سیستم کنترل دسترسی بر اساس نقش و دسترسی (Role-Based and Permission-Based Access Control) در این پروژه به‌صورت داینامیک و مبتنی بر استاندارد ASP.NET Core Policy-Based Authorization پیاده‌سازی شده است.

### ۱. موجودیت‌های پایگاه داده (Data Entities)
- **`User` (کاربر):** موجودیت اصلی کاربر که دارای وضعیت (`PendingApproval`, `Active`, `Inactive`, `Rejected`, `LockedOut`) است.
- **`Role` (نقش):** عنوان نقش‌ها در سیستم (مانند `Admin`, `SalesManager`, `SalesRep` و غیره).
- **`Permission` (دسترسی):** کدهای دسترسی با الگوی استاندارد `{Module}.{Entity}.{Action}` (مانند `CRM.Lead.Create`).
- **`UserRole` (ارتباط کاربر و نقش):** رابطه چند-به-چند بین کاربر و نقش‌ها.
- **`RolePermission` (ارتباط نقش و دسترسی):** رابطه چند-به-چند بین نقش و دسترسی‌ها.

### ۲. صدور JWT Token و Claims
در هنگام احراز هویت (`POST /api/auth/login`)، سرویس `AuthUseCaseService`:
1. نقش‌های کاربر را از پایگاه داده استخراج می‌کند.
2. تمام کدهای دسترسی (Permissions) مرتبط با این نقش‌ها را دریافت می‌کند.
3. در سرویس `JwtTokenService` توکن JWT تولید کرده و:
   - نقش‌ها را تحت Claim نوع `role` و `ClaimTypes.Role` ثبت می‌کند.
   - کدهای دسترسی را تحت Claim اختصاصی `"permissions"` در توکن جاگذاری می‌کند.

### ۳. مکانیزم بررسی دسترسی داینامیک (Dynamic Authorization Handler & Policy Provider)
- **`PermissionRequirement` و `PermissionAuthorizationHandler`:**
  کلاس `PermissionAuthorizationHandler` که از `AuthorizationHandler<PermissionRequirement>` ارث‌بری می‌کند، Claimهای `"permissions"` توکن کاربر جاری را خوانده و بررسی می‌کند که آیا کد دسترسی مورد نیاز در توکن موجود است یا خیر.
- **`PermissionPolicyProvider`:**
  کلاس `PermissionPolicyProvider` که از `DefaultAuthorizationPolicyProvider` ارث‌بری می‌کند، هرگاه در کنترلر متد با ویژگی `[Authorize(Policy = "CRM.Lead.Create")]` فراخوانی شود، به‌صورت خودکار و داینامیک یک Authorization Policy ایجاد کرده و متناسب با اسم policy، الزام `PermissionRequirement` را قرار می‌دهد.

---

## بخش ۲: لیست کامل APIها و کدهای دسترسی (Permissions / Roles)

### ۱. ماژول احراز هویت و مدیریت کاربران (Identity Module)
**مسیر پایه:** `/api/auth` و `/api/users`

| متد HTTP | آدرس Endpoint | نام متد / عملیات | سطح دسترسی / نقش مورد نیاز |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/login` | ورودی کاربر | عمومی (`[AllowAnonymous]`) |
| `POST` | `/api/auth/register` | ثبت نام کاربر | عمومی (`[AllowAnonymous]`) |
| `POST` | `/api/auth/refresh` | تمدید توکن | عمومی (`[AllowAnonymous]`) |
| `POST` | `/api/auth/logout` | خروج از حساب | کاربر احراز هویت شده (`[Authorize]`) |
| `GET` | `/api/auth/me` | دریافت اطلاعات کاربر جاری | کاربر احراز هویت شده (`[Authorize]`) |
| `POST` | `/api/auth/reset-password` | تغییر رمز عبور خود | کاربر احراز هویت شده (`[Authorize]`) |
| `POST` | `/api/users` | ایجاد کاربر جدید | نقش مدیر سیستم (`Roles = "Admin"`) |
| `GET` | `/api/users/pending` | لیست کاربران در انتظار تایید | نقش مدیر سیستم (`Roles = "Admin"`) |
| `POST` | `/api/users/{id}/approve` | تایید حساب کاربر | نقش مدیر سیستم (`Roles = "Admin"`) |
| `POST` | `/api/users/{id}/reject` | رد حساب کاربر | نقش مدیر سیستم (`Roles = "Admin"`) |
| `POST` | `/api/users/{id}/reset-password` | ریست رمز کاربر توسط مدیر | نقش مدیر سیستم (`Roles = "Admin"`) |

---

### ۲. ماژول مدیریت ارتباط با مشتری (CRM Module)
**مسیر پایه:** `/api/crm`

#### بخش سرنخ‌ها (Leads) - `/api/crm/leads`
| متد HTTP | آدرس Endpoint | عملیات | پرمیشن مورد نیاز (Policy) |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/crm/leads` | ایجاد سرنخ | `CRM.Lead.Create` |
| `GET` | `/api/crm/leads` | لیست سرنخ‌ها | `CRM.Lead.Read` |
| `GET` | `/api/crm/leads/{id}` | جزئیات سرنخ | `CRM.Lead.Read` |
| `PUT` | `/api/crm/leads/{id}` | ویرایش سرنخ | `CRM.Lead.Update` |
| `POST` | `/api/crm/leads/{id}/assign` | تخصیص مسئول سرنخ | `CRM.Lead.Assign` |
| `POST` | `/api/crm/leads/{id}/status` | تغییر وضعیت سرنخ | `CRM.Lead.StatusChange` |
| `POST` | `/api/crm/leads/{id}/qualify` | تایید صلاحیت سرنخ | `CRM.Lead.Qualify` |
| `POST` | `/api/crm/leads/{id}/disqualify` | عدم تایید صلاحیت سرنخ | `CRM.Lead.Qualify` |
| `POST` | `/api/crm/leads/{id}/convert` | تبدیل سرنخ به فرآیند فروش | `CRM.Lead.Convert` |
| `DELETE` | `/api/crm/leads/{id}` | آرشیو / حذف سرنخ | `CRM.Lead.Delete` |

#### بخش شرکت‌ها (Companies) - `/api/crm/companies`
| متد HTTP | آدرس Endpoint | عملیات | پرمیشن مورد نیاز (Policy) |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/crm/companies` | ثبت شرکت جدید | `CRM.Company.Create` |
| `GET` | `/api/crm/companies` | دریافت لیست شرکت‌ها | `CRM.Company.Read` |
| `GET` | `/api/crm/companies/{id}` | دریافت جزئیات شرکت | `CRM.Company.Read` |
| `PUT` | `/api/crm/companies/{id}` | ویرایش شرکت | `CRM.Company.Update` |
| `DELETE` | `/api/crm/companies/{id}` | حذف / آرشیو شرکت | `CRM.Company.Delete` |

#### بخش مخاطبین (Contacts) - `/api/crm/contacts`
| متد HTTP | آدرس Endpoint | عملیات | پرمیشن مورد نیاز (Policy) |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/crm/contacts` | ایجاد مخاطب | `CRM.Contact.Create` |
| `GET` | `/api/crm/contacts` | لیست مخاطبین | `CRM.Contact.Read` |
| `GET` | `/api/crm/contacts/{id}` | دریافت مخاطب | `CRM.Contact.Read` |
| `GET` | `/api/crm/contacts/company/{companyId}` | مخاطبین یک شرکت | `CRM.Contact.Read` |
| `PUT` | `/api/crm/contacts/{id}` | بروزرسانی مخاطب | `CRM.Contact.Update` |
| `DELETE` | `/api/crm/contacts/{id}` | حذف مخاطب | `CRM.Contact.Delete` |

#### بخش فعالیت‌ها (Activities) - `/api/crm/activities`
| متد HTTP | آدرس Endpoint | عملیات | پرمیشن مورد نیاز (Policy) |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/crm/activities` | ثبت فعالیت جدید | `CRM.Activity.Create` |
| `GET` | `/api/crm/activities` | لیست فعالیت‌ها | `CRM.Activity.Read` |
| `GET` | `/api/crm/activities/{id}` | جزئیات فعالیت | `CRM.Activity.Read` |
| `GET` | `/api/crm/activities/lead/{leadId}` | فعالیت‌های یک سرنخ | `CRM.Activity.Read` |
| `PUT` | `/api/crm/activities/{id}` | ویرایش فعالیت | `CRM.Activity.Update` |
| `DELETE` | `/api/crm/activities/{id}` | حذف فعالیت | `CRM.Activity.Delete` |

#### بخش یادداشت‌های فروش (Sales Notes) - `/api/crm/sales-notes`
| متد HTTP | آدرس Endpoint | عملیات | پرمیشن مورد نیاز (Policy) |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/crm/sales-notes` | ایجاد یادداشت فروش | `CRM.SalesNote.Create` |
| `GET` | `/api/crm/sales-notes` | دریافت لیست یادداشت‌ها | `CRM.SalesNote.Read` |
| `GET` | `/api/crm/sales-notes/{id}` | مشاهده یادداشت | `CRM.SalesNote.Read` |
| `GET` | `/api/crm/sales-notes/lead/{leadId}` | یادداشت‌های یک سرنخ | `CRM.SalesNote.Read` |
| `PUT` | `/api/crm/sales-notes/{id}` | ویرایش یادداشت | `CRM.SalesNote.Update` |
| `DELETE` | `/api/crm/sales-notes/{id}` | حذف یادداشت | `CRM.SalesNote.Delete` |

#### بخش ویزیت‌های حضوری (Sales Visits) - `/api/crm/sales-visits`
| متد HTTP | آدرس Endpoint | عملیات | پرمیشن مورد نیاز (Policy) |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/crm/sales-visits` | ثبت ویزیت حضوری | کاربر احراز هویت شده (`[Authorize]`) |
| `GET` | `/api/crm/sales-visits` | لیست ویزیت‌های حضوری | کاربر احراز هویت شده (`[Authorize]`) |
| `GET` | `/api/crm/sales-visits/{id}` | دریافت جزئیات ویزیت | کاربر احراز هویت شده (`[Authorize]`) |

---

### ۳. ماژول مدیریت مشتریان (Customer Module)
**مسیر پایه:** `/api/customers`

| متد HTTP | آدرس Endpoint | عملیات | پرمیشن مورد نیاز (Policy) |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/customers` | تعریف مشتری جدید | `Customer.Create` |
| `GET` | `/api/customers` | دریافت لیست مشتریان | `Customer.Read` |
| `GET` | `/api/customers/{id}` | مشاهده جزئیات مشتری | `Customer.Read` |
| `GET` | `/api/customers/company/{companyId}` | مشتری مرتبط با شرکت | `Customer.Read` |
| `GET` | `/api/customers/{id}/contacts` | مخاطبین مشتری | `Customer.Read` |
| `PUT` | `/api/customers/{id}` | بروزرسانی اطلاعات مشتری | `Customer.Update` |
| `POST` | `/api/customers/{id}/status` | تغییر وضعیت مشتری | `Customer.StatusChange` |
| `POST` | `/api/customers/{id}/primary-contact` | تعیین مخاطب اصلی | `Customer.Update` |
| `POST` | `/api/customers/{id}/account-manager` | تخصیص مدیر حساب | `Customer.Assign` |
| `POST` | `/api/customers/{id}/assign` | تخصیص مسئول مشتری | `Customer.Assign` |
| `POST` | `/api/customers/{id}/reactivate` | فعال‌سازی مجدد مشتری | `Customer.Update` |
| `DELETE` | `/api/customers/{id}` | حذف / آرشیو مشتری | `Customer.Delete` |

---

### ۴. ماژول فروش (Sales Module)
**مسیر پایه:** `/api/sales`

#### فرصت‌های فروش (Opportunities) - `/api/sales/opportunities`
| متد HTTP | آدرس Endpoint | عملیات | پرمیشن مورد نیاز (Policy) |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/sales/opportunities` | ثبت فرصت فروش جدید | `Sales.Opportunity.Create` |
| `GET` | `/api/sales/opportunities` | دریافت لیست فرصت‌ها | `Sales.Opportunity.Read` |
| `GET` | `/api/sales/opportunities/search` | جستجوی فرصت‌های فروش | `Sales.Opportunity.Read` |
| `GET` | `/api/sales/opportunities/pipeline` | گزارش قیف فروش | `Sales.Opportunity.Read` |
| `GET` | `/api/sales/opportunities/{id}` | مشاهده جزئیات فرصت | `Sales.Opportunity.Read` |
| `PUT` | `/api/sales/opportunities/{id}` | ویرایش فرصت فروش | `Sales.Opportunity.Update` |
| `POST` | `/api/sales/opportunities/{id}/assign` | تخصیص کارشناس | `Sales.Opportunity.Assign` |
| `POST` | `/api/sales/opportunities/{id}/stage` | تغییر مرحله فروش | `Sales.Opportunity.StageChange` |
| `POST` | `/api/sales/opportunities/{id}/won` | ثبت موفقیت فرصت (Won) | `Sales.Opportunity.Close` |
| `POST` | `/api/sales/opportunities/{id}/lost` | ثبت شکست فرصت (Lost) | `Sales.Opportunity.Close` |
| `DELETE` | `/api/sales/opportunities/{id}` | حذف / آرشیو فرصت | `Sales.Opportunity.Delete` |

#### پیش‌نویس‌ها و پروپوزال‌ها (Proposals) - `/api/sales/proposals`
| متد HTTP | آدرس Endpoint | عملیات | پرمیشن مورد نیاز (Policy) |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/sales/proposals` | صدور پیش‌نویس/پروپوزال | `Sales.Proposal.Create` |
| `GET` | `/api/sales/proposals` | لیست پروپوزال‌ها | `Sales.Proposal.Read` |
| `GET` | `/api/sales/proposals/opportunity/{opportunityId}` | پروپوزال‌های یک فرصت | `Sales.Proposal.Read` |
| `GET` | `/api/sales/proposals/{id}` | جزئیات پروپوزال | `Sales.Proposal.Read` |
| `PUT` | `/api/sales/proposals/{id}` | ویرایش پروپوزال | `Sales.Proposal.Update` |
| `POST` | `/api/sales/proposals/{id}/items` | افزودن قلم کالا/خدمت | `Sales.Proposal.Update` |
| `PUT` | `/api/sales/proposals/{id}/items/{itemId}` | ویرایش قلم کالا | `Sales.Proposal.Update` |
| `DELETE` | `/api/sales/proposals/{id}/items/{itemId}` | حذف قلم کالا | `Sales.Proposal.Update` |
| `POST` | `/api/sales/proposals/{id}/status` | تغییر وضعیت پروپوزال | `Sales.Proposal.StatusChange` |
| `DELETE` | `/api/sales/proposals/{id}` | آرشیو پروپوزال | `Sales.Proposal.Delete` |

---

### ۵. ماژول مدیریت پروژه‌ها (Project Module)
**مسیر پایه:** `/api/projects`

| متد HTTP | آدرس Endpoint | عملیات | پرمیشن مورد نیاز (Policy) |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/projects` | تعریف پروژه جدید | `Project.Create` |
| `GET` | `/api/projects` | لیست پروژه‌ها | `Project.Read` |
| `GET` | `/api/projects/{id}` | جزئیات پروژه | `Project.Read` |
| `PUT` | `/api/projects/{id}` | ویرایش اطلاعات پروژه | `Project.Update` |
| `POST` | `/api/projects/{id}/start` | شروع پروژه | `Project.StatusChange` |
| `POST` | `/api/projects/{id}/status` | تغییر وضعیت پروژه | `Project.StatusChange` |
| `POST` | `/api/projects/{id}/complete` | تکمیل پروژه | `Project.StatusChange` |
| `POST` | `/api/projects/{id}/cancel` | لغو پروژه | `Project.StatusChange` |
| `DELETE` | `/api/projects/{id}` | آرشیو پروژه | `Project.Delete` |
| `POST` | `/api/projects/{id}/repositories` | افزودن مخزن سورس (Repository) | `Project.Repository.Manage` |
| `PUT` | `/api/projects/{id}/repositories/{repositoryId}` | ویرایش مخزن | `Project.Repository.Manage` |
| `DELETE` | `/api/projects/{id}/repositories/{repositoryId}` | حذف مخزن | `Project.Repository.Manage` |
| `POST` | `/api/projects/{id}/deployments` | ثبت استقرار/دپلویمنت | `Project.Deployment.Manage` |
| `PUT` | `/api/projects/{id}/deployments/{deploymentId}` | ویرایش دپلویمنت | `Project.Deployment.Manage` |
| `DELETE` | `/api/projects/{id}/deployments/{deploymentId}` | حذف دپلویمنت | `Project.Deployment.Manage` |
| `POST` | `/api/projects/{id}/requirement` | ثبت نیازمندی‌های پروژه | `Project.Update` |

---

### ۶. ماژول مالی و فاکتورها (Finance Module)
**مسیر پایه:** `/api/finance/invoices`

| متد HTTP | آدرس Endpoint | عملیات | پرمیشن مورد نیاز (Policy) |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/finance/invoices` | صدور فاکتور جدید | `Finance.Invoice.Create` |
| `GET` | `/api/finance/invoices` | دریافت لیست فاکتورها | `Finance.Invoice.Read` |
| `GET` | `/api/finance/invoices/{id}` | مشاهده جزئیات فاکتور | `Finance.Invoice.Read` |
| `GET` | `/api/finance/invoices/customer/{customerId}` | فاکتورهای یک مشتری | `Finance.Invoice.Read` |
| `GET` | `/api/finance/invoices/project/{projectId}` | فاکتورهای یک پروژه | `Finance.Invoice.Read` |
| `PUT` | `/api/finance/invoices/{id}` | ویرایش فاکتور | `Finance.Invoice.Update` |
| `DELETE` | `/api/finance/invoices/{id}` | حذف فاکتور | `Finance.Invoice.Delete` |
| `POST` | `/api/finance/invoices/{id}/items` | افزودن ردیف فاکتور | `Finance.Invoice.Update` |
| `PUT` | `/api/finance/invoices/{id}/items/{itemId}` | ویرایش ردیف فاکتور | `Finance.Invoice.Update` |
| `DELETE` | `/api/finance/invoices/{id}/items/{itemId}` | حذف ردیف فاکتور | `Finance.Invoice.Update` |
| `POST` | `/api/finance/invoices/{id}/issue` | نهایی‌سازی و صدور فاکتور | `Finance.Invoice.Issue` |
| `POST` | `/api/finance/invoices/{id}/cancel` | ابطال فاکتور | `Finance.Invoice.Cancel` |
| `POST` | `/api/finance/invoices/{id}/payments` | ثبت دریافت پرداخت | `Finance.Payment.Create` |
| `GET` | `/api/finance/invoices/{id}/payments` | دریافت لیست پرداخت‌ها | `Finance.Payment.Read` |

---

### ۷. ماژول پشتیبانی و تیکتینگ (Support Module)
**مسیر پایه:** `/api/support`

| متد HTTP | آدرس Endpoint | عملیات | پرمیشن مورد نیاز (Policy) |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/support/tickets` | ثبت تیکت جدید | `Support.Ticket.Create` |
| `GET` | `/api/support/tickets` | مشاهده لیست تیکت‌ها | `Support.Ticket.Read` |
| `GET` | `/api/support/tickets/{id}` | جزئیات تیکت | `Support.Ticket.Read` |
| `PUT` | `/api/support/tickets/{id}` | ویرایش تیکت | `Support.Ticket.Update` |
| `DELETE` | `/api/support/tickets/{id}` | حذف تیکت | `Support.Ticket.Delete` |
| `POST` | `/api/support/tickets/{id}/assign` | ارجاع/تخصیص تیکت | `Support.Ticket.Assign` |
| `POST` | `/api/support/tickets/{id}/priority` | تغییر اولویت تیکت | `Support.Ticket.StatusChange` |
| `POST` | `/api/support/tickets/{id}/status` | تغییر وضعیت تیکت | `Support.Ticket.StatusChange` |
| `POST` | `/api/support/tickets/{id}/resolve` | حل‌وفصل تیکت | `Support.Ticket.Resolve` |
| `POST` | `/api/support/tickets/{id}/close` | بستن تیکت | `Support.Ticket.Close` |
| `POST` | `/api/support/tickets/{id}/cancel` | لغو تیکت | `Support.Ticket.Close` |
| `POST` | `/api/support/tickets/{id}/reopen` | بازگشایی مجدد تیکت | `Support.Ticket.Update` |
| `GET` | `/api/support/customers/{customerId}/tickets` | تیکت‌های یک مشتری | `Support.Ticket.Read` |
| `GET` | `/api/support/projects/{projectId}/tickets` | تیکت‌های یک پروژه | `Support.Ticket.Read` |
| `POST` | `/api/support/tickets/{ticketId}/comments` | ثبت پاسخ/کامنت | `Support.Comment.Create` |
| `GET` | `/api/support/tickets/{ticketId}/comments` | دریافت پاسخ‌های تیکت | `Support.Comment.Read` |

---

### ۸. ماژول زیرساخت و پلتفرم (Platform Module)
**مسیر پایه:** `/api/platform`

#### مدیریت تسک‌ها (Tasks) - `/api/platform/tasks`
| متد HTTP | آدرس Endpoint | عملیات | پرمیشن مورد نیاز (Policy) |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/platform/tasks` | ایجاد تسک جدید | `Platform.Task.Create` |
| `GET` | `/api/platform/tasks` | دریافت لیست تسک‌ها | `Platform.Task.Read` |
| `GET` | `/api/platform/tasks/{id}` | دریافت جزئیات تسک | `Platform.Task.Read` |
| `PUT` | `/api/platform/tasks/{id}/status` | تغییر وضعیت تسک | `Platform.Task.Update` |
| `DELETE` | `/api/platform/tasks/{id}` | حذف تسک | `Platform.Task.Delete` |

#### مدیریت فایل‌ها (Files) - `/api/platform/files`
| متد HTTP | آدرس Endpoint | عملیات | پرمیشن مورد نیاز (Policy) |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/platform/files/upload` | آپلود فایل | `Platform.File.Create` |
| `GET` | `/api/platform/files/{id}` | دریافت متادیتای فایل | `Platform.File.Read` |
| `GET` | `/api/platform/files/{id}/download` | دانلود فایل | `Platform.File.Read` |
| `GET` | `/api/platform/files/entity/{entityType}/{entityId}` | فایل‌های متصل به یک موجودیت | `Platform.File.Read` |
| `DELETE` | `/api/platform/files/{id}` | حذف فایل | `Platform.File.Delete` |

#### لاگ‌های رویدادها (Audit Logs) - `/api/platform/audit-logs`
| متد HTTP | آدرس Endpoint | عملیات | پرمیشن مورد نیاز (Policy) |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/platform/audit-logs` | مشاهده لیست لاگ‌های سیستم | `Platform.Audit.Read` |
| `GET` | `/api/platform/audit-logs/{id}` | مشاهده جزئیات لاگ | `Platform.Audit.Read` |

#### تنظیمات سیستم (Settings) - `/api/platform/settings`
| متد HTTP | آدرس Endpoint | عملیات | پرمیشن مورد نیاز (Policy) |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/platform/settings` | ثبت تنظیم جدید | `Platform.Setting.Write` |
| `GET` | `/api/platform/settings` | مشاهده لیست تنظیمات | `Platform.Setting.Read` |
| `GET` | `/api/platform/settings/{key}` | دریافت تنظیم با کلید | `Platform.Setting.Read` |
| `PUT` | `/api/platform/settings/{key}` | بروزرسانی تنظیم | `Platform.Setting.Write` |
| `DELETE` | `/api/platform/settings/{key}` | حذف تنظیم | `Platform.Setting.Delete` |

---

### ۹. ماژول گزارش‌گیری (Reporting Module)
**مسیر پایه:** `/api/reports`

| متد HTTP | آدرس Endpoint | عملیات | پرمیشن مورد نیاز (Policy) |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/reports/summary` | خلاصه گزارش داشبورد مدیریتی | `Reporting.Read` |
| `GET` | `/api/reports/crm/leads` | گزارش سرنخ‌های CRM | `Reporting.CRM.Read` |
| `GET` | `/api/reports/crm/activities` | گزارش فعالیت‌های CRM | `Reporting.CRM.Read` |
| `GET` | `/api/reports/sales/pipeline` | گزارش قیف فروش | `Reporting.Sales.Read` |
| `GET` | `/api/reports/customers` | گزارش مشتریان | `Reporting.Customer.Read` |
| `GET` | `/api/reports/projects` | گزارش پروژه‌ها | `Reporting.Project.Read` |
| `GET` | `/api/reports/finance` | گزارش مالی و فاکتورها | `Reporting.Finance.Read` |
| `GET` | `/api/reports/support` | گزارش پشتیبانی و تیکت‌ها | `Reporting.Support.Read` |

---

## خلاصه الگوی نام‌گذاری پرمیشن‌ها (Permission Naming Convention)
الگوی ساختار پرمیشن‌ها در تمام بخش‌های نرم‌افزار به شکل زیر است:
`{Module}.{Entity}.{Action}`

نمونه کدهای دسترسی در نقش‌های سازمان:
- **Admin:** دسترسی کامل به `Roles = "Admin"` و تمام پرمیشن‌ها.
- **Sales Manager (مدیر فروش):** دارای پرمیشن‌های `CRM.*` و `Sales.*` و `Reporting.Sales.Read`.
- **Support Agent (کارشناس پشتیبانی):** دارای پرمیشن‌های `Support.*` و `Customer.Read`.
- **Finance Officer (مسئول مالی):** دارای پرمیشن‌های `Finance.*` و `Reporting.Finance.Read`.
