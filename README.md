# NekoViBE E-Commerce Backend

NekoViBE is a professional anime/wibu e-commerce backend built with .NET 8, following **Clean Architecture** principles and **CQRS pattern**. The solution provides a complete e-commerce platform with product management, shopping cart, order processing, payment integration (VNPay, Momo), shipping integration (GHN), and content management.

## 🏗️ Architecture Overview

The project follows **Clean Architecture** with 4 distinct layers:

```
┌─────────────────────────────────────────────────────────┐
│                    NekoViBE.API                          │
│         (Presentation Layer - Controllers)               │
│  • Controllers (CMS/Customer)                            │
│  • Middlewares (Exception, JWT, Validation)              │
│  • Configurations (Swagger, CORS, JWT, Logging)          │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│              NekoViBE.Application                        │
│           (Application Layer - Business Logic)           │
│  • Features (CQRS Pattern)                               │
│  • MediatR Pipeline Behaviors                            │
│  • DTOs, Interfaces, Mappings                            │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│               NekoViBE.Domain                            │
│              (Domain Layer - Entities)                   │
│  • Entities (Product, Order, User...)                    │
│  • Enums, Base Classes                                   │
│  • No Dependencies                                       │
└─────────────────────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│            NekoViBE.Infrastructure                       │
│        (Infrastructure Layer - Data Access)              │
│  • EF Core DbContext, Repositories                       │
│  • Services (Payment, Email, File, Shipping)             │
│  • External APIs (VNPay, Momo, GHN)                      │
└─────────────────────────────────────────────────────────┘
```

### Layer Descriptions

- **NekoViBE.API**: Web API layer (.NET 8)
  - REST API Controllers (CMS & Customer)
  - Middlewares (Global Exception Handling, JWT, Validation)
  - Configurations (Swagger, CORS, JWT, Logging)
  - Automatic database migration & seeding in Development

- **NekoViBE.Application**: Application/Business Logic layer
  - CQRS implementation with MediatR
  - Command/Query handlers
  - FluentValidation validators
  - AutoMapper profiles
  - Pipeline behaviors (Validation, Authorization, Performance, Exception)
  - Business DTOs and interfaces

- **NekoViBE.Domain**: Domain Model layer
  - Domain entities (Product, Order, User, etc.)
  - Enums and value objects
  - Base entity with audit fields
  - Pure domain logic, no dependencies

- **NekoViBE.Infrastructure**: Infrastructure layer
  - EF Core DbContext & Repositories
  - Identity implementation
  - External service integrations (Payment, Shipping, File Storage)
  - Background jobs configuration
  - Database configurations

## 🎯 Design Patterns & Best Practices

### 1. CQRS Pattern (Command Query Responsibility Segregation)

Separates read and write operations for better scalability and maintainability:

```
Features/
├── Product/
│   ├── Commands/
│   │   ├── CreateProduct/
│   │   │   ├── CreateProductCommand.cs
│   │   │   ├── CreateProductCommandHandler.cs
│   │   │   └── CreateProductValidator.cs
│   │   ├── UpdateProduct/
│   │   └── DeleteProduct/
│   └── Queries/
│       ├── GetProductList/
│       │   ├── GetProductListQuery.cs
│       │   └── GetProductListQueryHandler.cs
│       └── GetProduct/
```

**Example:**
```csharp
// Command (Write)
public record CreateProductCommand(CreateProductRequest Request) 
    : IRequest<Result<ProductResponse>>;

// Query (Read)
public record GetProductListQuery(ProductFilter Filter) 
    : IRequest<PaginationResult<ProductItem>>;
```

### 2. Repository Pattern + Unit of Work

Generic repository for all entities with Unit of Work pattern for transaction management:

```csharp
// Usage
var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
product.Name = "Updated Name";
_unitOfWork.Repository<Product>().Update(product);
await _unitOfWork.SaveChangesAsync();
```

### 3. MediatR Pipeline Behaviors

Request pipeline with cross-cutting concerns:

```
Request → Validation → Authorization → Performance 
       → Handler → Exception Handling → Response
```

Behaviors:
- **ValidationBehavior**: Automatic FluentValidation
- **AuthorizationBehavior**: Permission checks
- **PerformanceBehavior**: Log slow requests
- **UnhandledExceptionBehavior**: Global exception handling

### 4. Factory Pattern

Factory Pattern is used to create instances of different services based on runtime configuration or enum type. This enables easy system expansion with new providers/implementations without modifying existing business logic.

#### 4.1. Payment Gateway Factory

Allows flexible switching between different payment gateways:

```csharp
// Interface
public interface IPaymentGatewayFactory
{
    IPaymentGatewayService GetPaymentGatewayService(PaymentGatewayType gatewayType);
}

// Implementation
public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    public IPaymentGatewayService GetPaymentGatewayService(PaymentGatewayType gatewayType)
    {
        return gatewayType switch
        {
            PaymentGatewayType.VnPay => _serviceProvider.GetRequiredService<VnPayService>(),
            PaymentGatewayType.Momo => _serviceProvider.GetRequiredService<MomoService>(),
            // Easy to add: PaymentGatewayType.PayPal => _serviceProvider.GetRequiredService<PayPalService>(),
            _ => throw new ArgumentException("Invalid payment gateway type")
        };
    }
}

// Usage in Order Processing
var paymentService = _paymentGatewayFactory.GetPaymentGatewayService(order.PaymentGatewayType);
var paymentUrl = await paymentService.CreatePaymentUrl(order);
```

**Supported Payment Gateways:**
- ✅ **VnPayService** - VNPay (Popular Vietnamese gateway)
- ✅ **MomoService** - Momo E-wallet
- 🔄 **Future:** PayPal, Stripe, ZaloPay

#### 4.2. Shipping Service Factory

Manages multiple shipping providers:

```csharp
// Interface
public interface IShippingServiceFactory
{
    IShippingService GetShippingService(ShippingProviderType providerType);
}

// Implementation
public class ShippingServiceFactory : IShippingServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    public IShippingService GetShippingService(ShippingProviderType providerType)
    {
        return providerType switch
        {
            ShippingProviderType.GHN => _serviceProvider.GetRequiredService<GHNService>(),
            // Ready to add: ShippingProviderType.GHTK => _serviceProvider.GetRequiredService<GHTKService>(),
            // Ready to add: ShippingProviderType.ViettelPost => _serviceProvider.GetRequiredService<ViettelPostService>(),
            _ => throw new ArgumentException($"Invalid shipping provider: {providerType}")
        };
    }
}

// Usage in Order Shipping
var shippingService = _shippingServiceFactory.GetShippingService(ShippingProviderType.GHN);
var shippingFee = await shippingService.CalculateShippingFee(fromAddress, toAddress, weight);
var shippingOrder = await shippingService.CreateShippingOrder(orderDetails);
```

**Supported Shipping Providers:**
- ✅ **GHNService** - Giao Hàng Nhanh (Fast delivery)
- 🔄 **Future:** GHTK, ViettelPost, J&T Express

#### 4.3. File Service Factory

Switches between different storage providers:

```csharp
// Interface
public interface IFileServiceFactory
{
    IFileService CreateFileService(string storageType);
}

// Implementation
public class FileServiceFactory : IFileServiceFactory
{
    public IFileService CreateFileService(string storageType)
    {
        return storageType.ToLower() switch
        {
            "local" => new FileService(_webHostEnvironment, _logger, _configuration),
            "firebase" => new FirebaseService(_configuration, _logger),
            // Can add: "azure" => new AzureBlobService(),
            // Can add: "s3" => new S3Service(),
            _ => throw new NotSupportedException($"Storage type {storageType} is not supported")
        };
    }
}

// Usage (configured via appsettings.json)
var fileService = _fileServiceFactory.CreateFileService(config["FileStorage:Provider"]);
var uploadedUrl = await fileService.UploadFileAsync(file);
```

**Supported Storage Providers:**
- ✅ **FileService (Local)** - Local file system storage
- ✅ **FirebaseService** - Firebase Cloud Storage
- 🔄 **Future:** Azure Blob Storage, AWS S3

#### 4.4. Notification Factory

Sends notifications through multiple channels:

```csharp
// Interface
public interface INotificationFactory
{
    INotificationService GetSender(NotificationChannelEnum channel);
}

// Implementation
public class NotificationFactory : INotificationFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    public INotificationService GetSender(NotificationChannelEnum channel)
    {
        return channel switch
        {
            NotificationChannelEnum.Email => _serviceProvider.GetRequiredService<IEmailService>(),
            NotificationChannelEnum.Firebase => _serviceProvider.GetRequiredService<IFirebaseService>(),
            // Can add: NotificationChannelEnum.SMS => _serviceProvider.GetRequiredService<ISmsService>(),
            // Can add: NotificationChannelEnum.Push => _serviceProvider.GetRequiredService<IPushNotificationService>(),
            _ => throw new NotImplementedException($"Channel {channel} not supported")
        };
    }
}

// Usage in Order Confirmation
var emailService = _notificationFactory.GetSender(NotificationChannelEnum.Email);
await emailService.SendAsync(user.Email, "Order Confirmation", orderDetails);

var pushService = _notificationFactory.GetSender(NotificationChannelEnum.Firebase);
await pushService.SendAsync(user.DeviceToken, "Order Shipped!", shippingInfo);
```

**Supported Notification Channels:**
- ✅ **EmailService** - Email notifications (Gmail SMTP)
- ✅ **FirebaseService** - Push notifications via Firebase Cloud Messaging
- 🔄 **Future:** SMS (Twilio), In-app notifications

**Benefits of Factory Pattern:**
- ✅ **Open/Closed Principle** - Easy to add new providers without modifying existing code
- ✅ **Dependency Injection** - All services registered in DI container
- ✅ **Runtime Selection** - Choose provider based on configuration or business logic
- ✅ **Testability** - Easy to mock factories for unit testing
- ✅ **Scalability** - Add new providers by just implementing the interface

### 5. Dependency Injection

Each layer registers its dependencies:

```csharp
services.AddApplication();              // MediatR, AutoMapper, FluentValidation
services.AddInfrastructure(config);     // DbContext, Repositories, Services
services.AddApiServices(config);        // Filters, Middlewares
```

## 💼 Core Features

### Authentication & Authorization
- ✅ User registration & login (JWT-based)
- ✅ Refresh token mechanism
- ✅ OTP verification
- ✅ Password reset & change password
- ✅ Profile management
- ✅ Role-based authorization (Admin, Staff, Customer)
- ✅ ASP.NET Core Identity integration

### Product Management
**CMS Features:**
- ✅ CRUD operations for products
- ✅ Multiple product images with primary image support
- ✅ Product inventory tracking
- ✅ Category & AnimeSeries association
- ✅ Product tags management
- ✅ Pre-order products support

**Customer Features:**
- ✅ Browse products with pagination
- ✅ Advanced filtering (category, series, price range, tags)
- ✅ Sorting (newest, price, rating)
- ✅ Product details with images
- ✅ Product reviews & ratings

### Shopping Cart & Wishlist
- ✅ Add/update/remove cart items
- ✅ Clear cart
- ✅ Get current user cart
- ✅ Wishlist management
- ✅ Move items between cart and wishlist

### Order Management
**Advanced Order Processing:**
- ✅ Create orders (authenticated & guest users)
- ✅ One-click order support
- ✅ Complex price breakdown (like Shopee):
  - Subtotal original (before discounts)
  - Product discount amount
  - Coupon discount amount (Fixed/Percentage)
  - Shipping fee with FreeShip discount support
  - Tax calculation
  - Final amount calculation

**Order Tracking:**
- ✅ Order history
- ✅ Order status management
- ✅ Shipping history tracking

### Payment Integration
**Multiple Payment Gateways:**
- ✅ **VNPay** - Popular Vietnamese payment gateway
- ✅ **Momo** - E-wallet integration
- ✅ Payment callback handling
- ✅ Payment verification
- ✅ Payment transaction history

**Payment Flow:**
```
Create Order → Choose Payment → Redirect to Gateway → 
Payment Callback → Verify Payment → Update Order Status
```

### Shipping Integration
**GHN (Giao Hàng Nhanh) Integration:**
- ✅ Calculate shipping fees
- ✅ Get available shipping services
- ✅ Create shipping orders
- ✅ Track shipping status
- ✅ Address management (Province, District, Ward)

### Coupon System
**Coupon Types:**
- ✅ **Fixed Amount** - Fixed discount (e.g., -50,000₫)
- ✅ **Percentage** - Percentage discount (e.g., -20%)
- ✅ **FreeShip** - Free shipping coupon

**Coupon Features:**
- ✅ Create/update/delete coupons
- ✅ Collect coupons (user claim)
- ✅ Get available coupons
- ✅ Get user's collected coupons
- ✅ Apply coupon to orders
- ✅ Validation (expiry, usage limit, minimum order value)

### Content Management
**Blog System:**
- ✅ CRUD blog posts
- ✅ Post categories & tags
- ✅ Publish/unpublish posts
- ✅ Get latest posts by category

**Events & Promotions:**
- ✅ Event management
- ✅ Event products
- ✅ Homepage banner management (HomeImage)
- ✅ User-specific banners

### Analytics & Dashboard
- ✅ Revenue analytics
- ✅ Order statistics
- ✅ Product performance metrics
- ✅ User activity tracking

### Additional Features
- ✅ **Badges System** - User achievement badges
- ✅ **Product Reviews** - Customer reviews & ratings
- ✅ **File Upload** - Firebase & local storage support
- ✅ **Email Service** - Transactional emails
- ✅ **User Address** - Multiple shipping addresses per user

## 📦 Domain Model

### Base Entity Pattern

All entities inherit from `BaseEntity` with built-in audit and soft delete:

```csharp
public class BaseEntity : IEntityLike
{
    public Guid Id { get; set; }
    
    // Audit Fields
    public DateTime? CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    
    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public Guid? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Status
    public EntityStatusEnum Status { get; set; } = EntityStatusEnum.Active;
}
```

### Key Entities

**User & Identity:**
- `AppUser`, `AppRole` - ASP.NET Core Identity
- `CustomerProfile`, `StaffProfile` - User profiles
- `UserAddress` - Shipping addresses
- `UserAction` - Activity tracking
- `Badge`, `UserBadge` - Achievement system

**Product Catalog:**
- `Product` - Main product entity
- `Category` - Product categories
- `AnimeSeries` - Anime series association
- `ProductImage` - Product images
- `ProductTag`, `Tag` - Product tagging
- `ProductReview` - Customer reviews
- `ProductInventory` - Stock management

**Shopping & Orders:**
- `ShoppingCart`, `CartItem` - Shopping cart
- `Wishlist`, `WishlistItem` - Wishlist
- `Order`, `OrderItem` - Order management
- `OrderShippingMethod` - Shipping details
- `ShippingHistory` - Shipping tracking

**Payment & Promotions:**
- `Payment` - Payment transactions
- `PaymentMethod` - Payment methods
- `Coupon`, `UserCoupon` - Coupon system
- `Event`, `EventProduct` - Promotional events

**Content:**
- `BlogPost` - Blog articles
- `PostCategory`, `PostTag` - Blog taxonomy
- `HomeImage`, `UserHomeImage` - Homepage banners

## 🚀 API Structure

### CMS Endpoints (Admin/Staff)
```
/api/cms/auth          - Authentication & user management
/api/cms/products      - Product CRUD
/api/cms/categories    - Category management
/api/cms/orders        - Order management
/api/cms/coupons       - Coupon management
/api/cms/users         - User management
/api/cms/blog-posts    - Blog management
/api/cms/events        - Event management
/api/cms/badges        - Badge management
```

### Customer Endpoints
```
/api/customer/auth       - Customer authentication
/api/customer/products   - Browse & search products
/api/customer/cart       - Shopping cart operations
/api/customer/orders     - Place & track orders
/api/customer/wishlist   - Wishlist management
/api/customer/profile    - Profile management
/api/customer/reviews    - Product reviews
/api/customer/coupons    - Collect & use coupons
```

### Payment Callbacks
```
/api/payment/vnpay/callback
/api/payment/momo/callback
```

## 🛡️ Security & Cross-Cutting Concerns

### Middlewares
- **GlobalExceptionHandlingMiddleware** - Catch all exceptions globally
- **JwtMiddleware** - Validate JWT tokens
- **ValidationMiddleware** - Request validation

### Authentication & Authorization
- JWT Bearer token authentication
- Refresh token support
- Role-based authorization with `[AuthorizeRoles]` attribute
- OTP verification for sensitive operations

### Logging
- Serilog integration
- Structured logging
- File logging: `Logs/nekovi-{date}.txt`
- Console logging in Development

### CORS
Configurable CORS from `appsettings.json`:
```json
{
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "https://cms.nekovi.com"]
  }
}
```

### API Documentation
- Swagger UI enabled
- Grouped by tags (CMS, Customer)
- JWT Bearer support in Swagger UI

## 🔧 Infrastructure Services

### Core Services
- **IJwtService** - JWT token generation & validation
- **IIdentityService** - User management with Identity
- **IOtpCacheService** - OTP caching & verification
- **ICurrentUserService** - Get current authenticated user

### File Management
- **IFileService** - Abstract file operations
- **FirebaseService** - Firebase Cloud Storage
- **LocalFileService** - Local file system storage

### Payment Gateways
- **IPaymentGatewayService** - Abstract payment interface
- **VnPayService** - VNPay implementation
- **MomoService** - Momo implementation

### Shipping
- **IShippingService** - Abstract shipping interface
- **GHNService** - Giao Hàng Nhanh integration

### Communication
- **IEmailService** - Email sending service

### Database
- **EF Core 8** with SQL Server
- DbContext pooling enabled
- Automatic migrations in Development
- Soft delete with global query filters
- Audit fields auto-populated (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
- Identity tables renamed (AspNetUsers → Users, etc.)

## 📋 Prerequisites

- .NET 8 SDK
- SQL Server (local or container)
- dotnet-ef CLI

Install dotnet-ef (if needed):
```bash
dotnet tool install --global dotnet-ef
```

## ⚙️ Configuration

Copy the example settings and adjust to your environment:
```bash
cd src/NekoViBE.API
copy appsettings.example.json appsettings.json
```

### Important Configuration Keys

**Database:**
- `ConnectionStrings:DefaultConnection` - Main database connection
- `ConnectionStrings:OuterConnection` - External services database

**JWT:**
- `Jwt:Key` - Secret key for JWT signing
- `Jwt:Issuer` - Token issuer
- `Jwt:Audience` - Token audience
- `Jwt:AccessTokenExpirationMinutes` - Access token lifetime
- `Jwt:RefreshTokenExpirationDays` - Refresh token lifetime

**Admin User (Auto-seeded):**
- `AdminUser:Email` - Admin email
- `AdminUser:DefaultPassword` - Admin password (⚠️ Change in production!)

**Payment Gateways:**
- `VNPay:*` - VNPay configuration
- `Momo:*` - Momo configuration

**Shipping:**
- `GHN:*` - Giao Hàng Nhanh configuration

**File Storage:**
- `FileStorage:Provider` - "Firebase" or "Local"
- `Firebase:*` - Firebase configuration

**Data Seeding:**
- `DataSeeding:EnableSeeding` - Enable/disable auto-seeding

**CORS:**
- `Cors:AllowedOrigins` - Allowed origins array

**Security Note:** Always change `AdminUser.DefaultPassword` before deploying to any non-local environment!

## 🗄️ EF Core Migrations

### Create a Migration

```bash
dotnet ef migrations add MigrationName --project src/NekoViBE.Infrastructure --startup-project src/NekoViBE.API
```

### Apply Migrations

Apply migrations manually:
```bash
dotnet ef database update --project src/NekoViBE.Infrastructure --startup-project src/NekoViBE.API
```

**Note:** In Development environment, the API automatically applies pending migrations at startup.

### List Migrations

```bash
dotnet ef migrations list --project src/NekoViBE.Infrastructure --startup-project src/NekoViBE.API
```

### Remove Last Migration (not applied)

```bash
dotnet ef migrations remove --project src/NekoViBE.Infrastructure --startup-project src/NekoViBE.API
```

## 🚀 Run the Project

### Run API

```bash
cd src/NekoViBE.API
dotnet run
```

### Watch Mode (auto-rebuild on changes)

```bash
cd src/NekoViBE.API
dotnet watch run
```

### Startup Behavior (Development)

On startup in Development environment, the API will:
1. ✅ Retry database connectivity
2. ✅ Apply pending migrations automatically
3. ✅ Apply outer database migrations
4. ✅ Seed initial data:
   - Roles from `RoleEnum` (Admin, Staff, Customer)
   - Admin user from `appsettings.json`
   - Test data (if configured)

### Access the API

- **API Base URL:** `https://localhost:5001` or `http://localhost:5000`
- **Swagger UI:** `https://localhost:5001/swagger`
- **Health Check:** `https://localhost:5001/health`

## 🔍 Implementation Notes

### Identity Integration
- `AppUser` extends `IdentityUser<Guid>`
- `AppRole` extends `IdentityRole<Guid>`
- Identity tables are renamed in `NekoViDbContext` for cleaner schema
- Identity entities implement `IEntityLike` for common filters/sorting

### Database Features
- **DbContext Pooling** enabled for better performance
- **Soft Delete** with global query filters
- **Audit Fields** automatically populated via `SaveChangesAsync` override
- **Optimistic Concurrency** support
- **Query Filters** for soft-deleted entities

### Order Calculation Logic

Orders use a detailed breakdown similar to Shopee:

```csharp
// 1. Original price (before any discounts)
SubtotalOriginal = Σ(UnitPriceOriginal × Quantity)

// 2. Product discount
ProductDiscountAmount = Σ((UnitPriceOriginal - UnitPriceAfterDiscount) × Quantity)

// 3. Subtotal after product discount (BASE for coupon calculation)
SubtotalAfterProductDiscount = SubtotalOriginal - ProductDiscountAmount

// 4. Coupon discount (calculated on SubtotalAfterProductDiscount)
CouponDiscountAmount = Calculate based on coupon type

// 5. Total product amount after all discounts
TotalProductAmount = SubtotalAfterProductDiscount - CouponDiscountAmount

// 6. Shipping fees
ShippingFeeActual = ShippingFeeOriginal - ShippingDiscountAmount

// 7. FINAL AMOUNT
FinalAmount = TotalProductAmount + ShippingFeeActual + TaxAmount
```

## 🧪 Development Tips

### Using Swagger
1. Navigate to `/swagger` after running the API
2. Click "Authorize" button
3. Login via `/api/cms/auth/login` or `/api/customer/auth/login`
4. Copy the `accessToken` from response
5. Enter `Bearer {accessToken}` in the authorization dialog
6. All authenticated endpoints will now work

### Database Seeding
To re-seed the database:
1. Delete the database
2. Restart the application in Development mode
3. Migrations and seeding will run automatically

### Debugging
- Set breakpoints in handlers for debugging business logic
- Check `Logs/nekovi-{date}.txt` for detailed logs
- Use Swagger to test API endpoints
- Check `obj/` folder for generated migration files

## 📚 Useful Commands

### Database Management
```bash
# List all migrations
dotnet ef migrations list --project src/NekoViBE.Infrastructure --startup-project src/NekoViBE.API

# Create migration
dotnet ef migrations add MigrationName --project src/NekoViBE.Infrastructure --startup-project src/NekoViBE.API

# Update database
dotnet ef database update --project src/NekoViBE.Infrastructure --startup-project src/NekoViBE.API

# Remove last migration (not applied)
dotnet ef migrations remove --project src/NekoViBE.Infrastructure --startup-project src/NekoViBE.API

# Drop database
dotnet ef database drop --project src/NekoViBE.Infrastructure --startup-project src/NekoViBE.API
```

### Build & Run
```bash
# Build solution
dotnet build

# Run API
dotnet run --project src/NekoViBE.API

# Watch mode (auto-reload)
dotnet watch run --project src/NekoViBE.API

# Build for release
dotnet build -c Release

# Publish
dotnet publish -c Release -o ./publish
```

### Testing
```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true
```

## 🐛 Troubleshooting

### SQL Server Connection Issues
- **Problem:** Cannot connect to SQL Server
- **Solutions:**
  - Verify `ConnectionStrings:DefaultConnection` in `appsettings.json`
  - Check SQL Server is running: `docker ps` (if using Docker)
  - With Docker, ensure port 1433 is published
  - Add `TrustServerCertificate=True` to connection string for self-signed certificates
  - Check user permissions on the database

### Migration Errors
- **Problem:** Migration fails or hangs
- **Solutions:**
  - Ensure no other application is using the database
  - Try dropping and recreating the database
  - Check migration files in `src/NekoViBE.Infrastructure/Migrations`
  - Verify `NekoViDbContext` configurations

### JWT Authentication Issues
- **Problem:** 401 Unauthorized errors
- **Solutions:**
  - Verify `Jwt:Key` is set in `appsettings.json`
  - Check token expiration time
  - Ensure `Bearer {token}` format in Authorization header
  - Try refreshing the token using `/auth/refresh-token`

### File Upload Issues
- **Problem:** File upload fails
- **Solutions:**
  - Check `FileStorage:Provider` setting
  - Verify Firebase credentials (if using Firebase)
  - Ensure `uploads/` folder exists and has write permissions (if using Local)
  - Check file size limits in configuration

### Payment Callback Issues
- **Problem:** Payment callback not working
- **Solutions:**
  - Verify callback URLs are publicly accessible (use ngrok for local testing)
  - Check payment gateway credentials in `appsettings.json`
  - Review payment logs in `Logs/` folder
  - Ensure HTTPS is used for production

## 📖 Additional Resources

### Documentation Files
- `docs/DATABASE_ROLE_VALIDATION_GUIDE.md` - Database role validation
- `docs/SIMPLIFIED_CURRENT_USER_SERVICE.md` - Current user service usage
- `docs/USER_ADDRESS_IMPLEMENTATION.md` - User address feature guide
- `src/NekoViBE.API/README.Configuration.md` - Detailed configuration guide

### API Testing Files
- `src/NekoViBE.API/NekoViBE.API.http` - General API requests
- `src/NekoViBE.API/PaymentMethodAPI.http` - Payment testing
- `src/NekoViBE.API/WishlistAPI.http` - Wishlist testing

## 🎯 Best Practices Implemented

✅ **Clean Architecture** - Clear separation of concerns across layers
✅ **CQRS Pattern** - Separate read/write operations
✅ **Repository Pattern** - Abstracted data access
✅ **Unit of Work** - Transaction management
✅ **Dependency Injection** - Loose coupling
✅ **MediatR Pipeline** - Cross-cutting concerns (Validation, Logging, Performance)
✅ **FluentValidation** - Declarative validation rules
✅ **AutoMapper** - Object-to-object mapping
✅ **Soft Delete** - Data preservation
✅ **Audit Trail** - Track who/when changes
✅ **Global Exception Handling** - Consistent error responses
✅ **Structured Logging** - Serilog with file/console outputs
✅ **JWT Authentication** - Stateless authentication
✅ **Role-Based Authorization** - Fine-grained access control
✅ **Swagger Documentation** - Interactive API docs
✅ **API Versioning Ready** - Scalable API design

## 🔐 Security Considerations

- ✅ Passwords hashed using ASP.NET Core Identity (PBKDF2)
- ✅ JWT tokens with configurable expiration
- ✅ Refresh token rotation
- ✅ Role-based authorization
- ✅ CORS configuration
- ✅ HTTPS enforced in production
- ✅ SQL injection protection via EF Core parameterization
- ✅ XSS prevention via output encoding
- ✅ CSRF protection for state-changing operations
- ⚠️ Remember to change default admin password
- ⚠️ Keep JWT secret key secure
- ⚠️ Use environment variables for sensitive data in production

## 📈 Performance Optimizations

- ✅ DbContext pooling for reduced overhead
- ✅ Async/await throughout the codebase
- ✅ Pagination for large datasets
- ✅ Eager/lazy loading optimization
- ✅ Query result caching where appropriate
- ✅ Connection pooling
- ✅ Indexed database columns
- ✅ Compiled queries for frequently-used queries

## 🚧 Roadmap & Future Enhancements

- [ ] GraphQL API support
- [ ] Real-time notifications (SignalR)
- [ ] Advanced search with Elasticsearch
- [ ] Redis caching layer
- [ ] Message queue integration (RabbitMQ/Azure Service Bus)
- [ ] API rate limiting
- [ ] Comprehensive unit/integration tests
- [ ] Docker containerization
- [ ] Kubernetes deployment configs
- [ ] CI/CD pipeline (GitHub Actions/Azure DevOps)
- [ ] Monitoring & APM (Application Insights)
- [ ] Multi-language support (i18n)
- [ ] Mobile app API optimizations

## 📞 Support & Contribution

For issues, questions, or contributions, please contact the development team or create an issue in the repository.

---

**Built with ❤️ using .NET 8, EF Core, and Clean Architecture principles**
