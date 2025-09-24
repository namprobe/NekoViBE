# AutoMapper Helper Usage Guide

## MappingHelper Extensions

### 🎯 Purpose
Cung cấp các extension methods để dễ dàng ignore các fields của BaseEntity và Identity khi mapping từ DTO/Request vào Entity.

### 📋 Available Methods

#### 1. `IgnoreBaseEntityFields<TSource, TDestination>()`
- **Mục đích**: Ignore các trường BaseEntity trừ Status
- **Sử dụng cho**: Entity kế thừa từ BaseEntity
- **Trường được ignore**: Id, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted, DeletedBy, DeletedAt
- **Trường KHÔNG ignore**: Status (có thể map nếu cần)

```csharp
CreateMap<RegisterRequest, CustomerProfile>()
    .IgnoreBaseEntityFields();
    // Status có thể được map tự động nếu có trong RegisterRequest
```

#### 2. `IgnoreAllBaseEntityFields<TSource, TDestination>()`
- **Mục đích**: Ignore TẤT CẢ các trường BaseEntity bao gồm Status
- **Sử dụng cho**: Khi không muốn update Status từ request
- **Trường được ignore**: Tất cả fields của BaseEntity

```csharp
CreateMap<UpdateProfileRequest, CustomerProfile>()
    .IgnoreAllBaseEntityFields()
    .ForMember(dest => dest.UserId, opt => opt.Ignore());
    // Status sẽ KHÔNG được update từ request
```

#### 3. `IgnoreIdentityFields<TSource>()`
- **Mục đích**: Ignore các trường Identity của AppUser
- **Sử dụng cho**: Khi update user profile mà không muốn thay đổi authentication fields
- **Trường được ignore**: UserName, Email, Password, SecurityStamp, etc.

```csharp
CreateMap<UpdateProfileRequest, AppUser>()
    .IgnoreIdentityFields()
    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
    .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
```

### 🔄 Mapping Patterns

#### ✅ Request → Entity (One-way mapping)
```csharp
// Cho Entity kế thừa BaseEntity
CreateMap<CreateProductRequest, Product>()
    .IgnoreBaseEntityFields(); // Status có thể được map nếu có trong request

// Khi không muốn update Status
CreateMap<UpdateProductRequest, Product>()
    .IgnoreAllBaseEntityFields();

// Cho AppUser
CreateMap<UpdateProfileRequest, AppUser>()
    .IgnoreIdentityFields()
    .IgnoreAllBaseEntityFields();
```

#### ✅ Entity → Response (Keep existing mappings)
```csharp
// KHÔNG thay đổi các mapping này
CreateMap<AppUser, ProfileResponse>()
    .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => ...));
```

### 🚫 Important Notes

1. **Chỉ áp dụng cho mapping một chiều**: Request/DTO → Entity
2. **KHÔNG thay đổi**: Entity → Response mappings
3. **AutoMapper tự động map**: Các fields có tên giống nhau
4. **Chỉ khai báo explicit**: Khi cần custom mapping hoặc ignore

### 📁 Usage Examples

#### Customer Profile Update
```csharp
CreateMap<UpdateProfileRequest, CustomerProfile>()
    .IgnoreAllBaseEntityFields()
    .ForMember(dest => dest.UserId, opt => opt.Ignore());
    // Gender, DateOfBirth, Bio được map tự động
```

#### User Registration
```csharp
CreateMap<RegisterRequest, AppUser>()
    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => EntityStatusEnum.Active))
    .IgnoreIdentityFields();
```

#### Product Creation
```csharp
CreateMap<CreateProductRequest, Product>()
    .IgnoreBaseEntityFields();
    // Name, Price, Description được map tự động
    // Status có thể map từ request nếu có
```

### 🎨 Benefits

✅ **Consistent**: Đồng nhất cách ignore BaseEntity fields  
✅ **Reusable**: Tái sử dụng cho nhiều mappings  
✅ **Maintainable**: Dễ maintain khi BaseEntity thay đổi  
✅ **Clean**: Code mapping ngắn gọn và rõ ràng  
✅ **Safe**: Tránh override nhầm audit fields  