# NekoViBE Configuration Guide

## 🚀 Quick Setup for Development Team

### 1. Configuration Setup
```bash
# Copy the example configuration file
cp appsettings.example.json appsettings.json
```

### 2. Required Configuration Updates

#### 🔐 **Security Settings**
- **Jwt.Key**: Generate a secure random key (minimum 64 characters)
  ```bash
  # Generate using PowerShell
  [System.Web.Security.Membership]::GeneratePassword(64, 10)
  
  # Or use online generator: https://www.allkeysgenerator.com/Random/Security-Encryption-Key-Generator.aspx
  ```

#### 🗄️ **Database Connections**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=NekoViDb;User Id=sa;Password=YourStrong@Password;TrustServerCertificate=True;",
  "OuterDbConnection": "Server=localhost,1433;Database=NekoViOuterDb;User Id=sa;Password=YourStrong@Password;TrustServerCertificate=True;"
}
```

#### 📧 **Email Configuration (SMTP)**
```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SenderEmail": "your-email@gmail.com",
  "SenderName": "No Reply - NekoVi",
  "SenderPassword": "your-app-password",  // Use App Password for Gmail
  "EnableSsl": true
}
```

**Gmail Setup:**
1. Enable 2-Factor Authentication
2. Generate App Password: [Google Account Settings](https://myaccount.google.com/apppasswords)
3. Use the App Password (not your regular password)

#### 🔐 **Google OAuth Configuration**
```json
"GoogleSettings": {
  "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com",
  "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET",
  "RedirectUri": "https://your-public-base-url.example/api/oauth/callback"
}
```

**Setup Steps:**
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create/Select project → APIs & Services → Credentials
3. Create OAuth 2.0 Client ID
4. Add authorized redirect URIs

#### 🌐 **Base URL & CORS**
```json
"BaseUrl": "https://your-public-base-url.example",
"Cors": {
  "AllowedOrigins": [
    "http://localhost:3000",  // React dev server
    "http://localhost:5173",  // Vite dev server  
    "http://localhost:4200"   // Angular dev server
  ]
}
```

#### 💳 **VNPay Payment (Optional)**
```json
"VNPay": {
  "TmnCode": "YOUR_TMN_CODE",
  "HashSecret": "YOUR_HASH_SECRET",
  "ReturnPath": "/vnpay/return",
  "PaymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"
}
```

**Get VNPay Credentials:**
- Register at [VNPay Merchant Portal](https://sandbox.vnpayment.vn/merchantv2/)
- Use sandbox for testing

### 3. Development Tools Configuration

#### 📊 **Hangfire Dashboard**
- Access: `https://localhost:7777/hangfire`
- Default: No authentication required for development
- Jobs: Background tasks and scheduled processes

#### 🔢 **OTP Settings**
```json
"OTPSettings": {
  "Length": 6,           // OTP code length
  "ExpirationMinutes": 5, // OTP validity period
  "MaxAttempts": 3       // Maximum verification attempts
}
```

### 4. Environment-Specific Notes

#### 🔧 **Development**
- Use `appsettings.Development.json` for dev overrides
- Database: Local SQL Server or Docker container
- Email: Test with your personal Gmail

#### 🚀 **Production**
- Never commit real credentials to Git
- Use environment variables or Azure Key Vault
- Enable SSL/TLS for all external connections

### 5. Security Checklist ✅

- [ ] Changed default JWT key
- [ ] Updated database passwords  
- [ ] Configured proper CORS origins
- [ ] Set strong admin password
- [ ] Enabled SSL for email SMTP
- [ ] Added `.env` and `appsettings.json` to `.gitignore`

### 6. Troubleshooting 🔧

#### Database Connection Issues
```bash
# Check SQL Server is running
docker ps  # If using Docker
# Or check Windows services

# Test connection
sqlcmd -S localhost,1433 -U sa -P YourPassword
```

#### Email Not Sending
- Verify Gmail App Password (not regular password)
- Check SMTP port (587 for TLS, 465 for SSL)
- Ensure "Less secure app access" is disabled (use App Password)

#### OAuth Callback Issues
- Verify `BaseUrl` matches your public URL
- Check Google Console redirect URIs
- Ensure HTTPS in production

---

## 📝 File Structure
```
src/NekoViBE.API/
├── appsettings.example.json  ← Template (commit this)
├── appsettings.json          ← Your config (DON'T commit)
├── appsettings.Development.json
└── README.Configuration.md   ← This guide
```

## 🤝 Team Workflow
1. Always update `appsettings.example.json` when adding new configuration
2. Never commit sensitive data to Git
3. Document new settings in this README
4. Share safe configuration examples with team

---

**Happy Coding! 🚀**
