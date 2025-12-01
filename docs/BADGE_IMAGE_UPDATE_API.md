# Badge Image Update API Documentation

## Overview
This API endpoint allows updating only the badge image/icon without modifying other badge properties.

---

## Endpoint Details

### **PATCH** `/api/cms/badges/{id}/image`

Updates the icon/image of an existing badge.

---

## Authentication & Authorization
- **Required**: Yes
- **Roles**: Admin, Staff
- **Token**: Bearer JWT token in Authorization header

---

## Request

### Path Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | The unique identifier of the badge |

### Body (multipart/form-data)
| Field | Type | Required | Description | Validation |
|-------|------|----------|-------------|------------|
| `IconPath` | IFormFile | Yes | The badge icon/image file | - Max size: 5MB<br>- Allowed formats: .jpg, .jpeg, .png, .gif, .webp |

---

## Response

### Success Response (200 OK)
```json
{
  "isSuccess": true,
  "message": "Badge image updated successfully",
  "data": null,
  "errors": null,
  "errorCode": null
}
```

### Error Responses

#### 400 Bad Request - Invalid File
```json
{
  "isSuccess": false,
  "message": "Invalid request parameters",
  "data": null,
  "errors": [
    "Icon image file size must not exceed 5MB",
    "Icon image must be a valid image file (jpg, jpeg, png, gif, webp)"
  ],
  "errorCode": "InvalidInput"
}
```

#### 401 Unauthorized
```json
{
  "isSuccess": false,
  "message": "User is not valid",
  "errorCode": "Unauthorized"
}
```

#### 403 Forbidden
```json
{
  "isSuccess": false,
  "message": "Access denied",
  "errorCode": "Forbidden"
}
```

#### 404 Not Found
```json
{
  "isSuccess": false,
  "message": "Badge not found",
  "errorCode": "NotFound"
}
```

#### 500 Internal Server Error
```json
{
  "isSuccess": false,
  "message": "An error occurred while updating the badge image",
  "errorCode": "InternalError"
}
```

---

## Implementation Details

### Backend Flow
1. **Authentication Check**: Validates user authentication and role permissions
2. **Badge Lookup**: Finds badge by ID
3. **Old Image Deletion**: Deletes existing badge icon if present
4. **New Image Upload**: Uploads new image to `/uploads/badge/` directory
5. **Database Update**: Updates badge IconPath and metadata
6. **Action Logging**: Records user action in audit log
7. **Transaction Commit**: Commits all changes atomically

### File Storage
- **Directory**: `wwwroot/uploads/badge/`
- **Naming**: Auto-generated unique filename
- **Access**: Public URL via `/uploads/badge/{filename}`

### Validation Rules
- File size: Maximum 5MB
- File types: JPG, JPEG, PNG, GIF, WEBP
- File must not be empty
- Badge ID must be valid GUID
- Badge must exist in database

---

## Usage Examples

### Example 1: cURL
```bash
curl -X PATCH "https://api.nekovi.com/api/cms/badges/3fa85f64-5717-4562-b3fc-2c963f66afa6/image" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: multipart/form-data" \
  -F "IconPath=@/path/to/badge-icon.png"
```

### Example 2: JavaScript (Fetch API)
```javascript
const formData = new FormData();
formData.append('IconPath', fileInput.files[0]);

fetch('https://api.nekovi.com/api/cms/badges/3fa85f64-5717-4562-b3fc-2c963f66afa6/image', {
  method: 'PATCH',
  headers: {
    'Authorization': `Bearer ${token}`
  },
  body: formData
})
.then(response => response.json())
.then(data => console.log(data));
```

### Example 3: C# HttpClient
```csharp
using var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", token);

using var content = new MultipartFormDataContent();
using var fileStream = File.OpenRead("badge-icon.png");
content.Add(new StreamContent(fileStream), "IconPath", "badge-icon.png");

var response = await client.PatchAsync(
    "https://api.nekovi.com/api/cms/badges/3fa85f64-5717-4562-b3fc-2c963f66afa6/image",
    content
);
```

### Example 4: Postman
1. Method: **PATCH**
2. URL: `https://api.nekovi.com/api/cms/badges/{badgeId}/image`
3. Headers:
   - `Authorization: Bearer YOUR_JWT_TOKEN`
4. Body → form-data:
   - Key: `IconPath` (Type: File)
   - Value: Select your image file

---

## Comparison with Other Update Methods

| Feature | PATCH /image | PUT /{id} |
|---------|-------------|-----------|
| Updates badge image | ✅ | ✅ |
| Updates badge name | ❌ | ✅ |
| Updates description | ❌ | ✅ |
| Updates discount | ❌ | ✅ |
| Updates conditions | ❌ | ✅ |
| Updates time limits | ❌ | ✅ |
| **Use case** | Quick image-only update | Full badge update |

---

## Best Practices

### When to Use
- **Use this endpoint** when you only need to change the badge icon
- **Use PUT /badges/{id}** when updating multiple badge properties
- This endpoint is more efficient for icon-only updates as it:
  - Requires fewer form fields
  - Has simpler validation
  - Faster processing time

### Image Optimization Tips
1. **Recommended size**: 256x256px or 512x512px
2. **Format**: PNG with transparency for best quality
3. **File size**: Keep under 500KB for faster loading
4. **Naming**: Use descriptive names (e.g., `gold-member-badge.png`)

### Error Handling
```javascript
try {
  const response = await updateBadgeImage(badgeId, imageFile);
  if (response.isSuccess) {
    showSuccessMessage('Badge image updated!');
  } else {
    showErrorMessage(response.message);
  }
} catch (error) {
  showErrorMessage('Network error. Please try again.');
}
```

---

## Technical Notes

### Database Changes
- Updates `Badges.IconPath` column
- Updates `Badges.UpdatedAt` timestamp
- Updates `Badges.UpdatedBy` with current user ID
- Creates entry in `UserActions` audit log

### File System
- Old image file is **deleted** from file system
- New image is saved to `wwwroot/uploads/badge/`
- Filename is auto-generated to prevent conflicts

### Transaction Handling
- All operations wrapped in database transaction
- Automatic rollback on any failure
- Ensures data consistency

---

## Related APIs
- `POST /api/cms/badges` - Create new badge
- `PUT /api/cms/badges/{id}` - Update full badge details
- `GET /api/cms/badges/{id}` - Get badge by ID
- `DELETE /api/cms/badges/{id}` - Delete badge

---

## Changelog
- **v1.0** (2025-11-30): Initial implementation
  - PATCH endpoint for badge image update
  - File validation (size, type)
  - Transaction support
  - Audit logging

---

## Support
For issues or questions, contact the development team or check the API documentation at `/swagger`.
