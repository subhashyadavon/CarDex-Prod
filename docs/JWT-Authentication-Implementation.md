# JWT Authentication Implementation

## Overview
JWT authentication is implemented for the CarDex API using password hashing and token-based authentication.

## Setup

### 1. Packages
- `Microsoft.AspNetCore.Authentication.JwtBearer` (v8.0.0)
- `BCrypt.Net-Next` (v4.0.3)

### 2. Configuration
Set the JWT secret key in `.env`:
```bash
JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLong!123
```

Generate a secure key:
```bash
openssl rand -base64 64
```

### 3. Auth Endpoints
- `POST /auth/register` - Register new user (returns JWT token)
- `POST /auth/login` - Login with credentials (returns JWT token)
- `POST /auth/logout` - Logout current user (requires authentication)

## Protecting Endpoints

Add `[Authorize]` to protect endpoints:

```csharp
[HttpPost("purchase")]
[Authorize] // Requires valid JWT token
public async Task<IActionResult> PurchasePack(...)
```

For public endpoints in a protected controller:
```csharp
[HttpGet("{id}")]
[AllowAnonymous] // Public access
public async Task<IActionResult> GetPublicData(...)
```

## Accessing User Information

Get current user from JWT claims:

```csharp
[HttpPut("me")]
[Authorize]
public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateRequest request)
{
    // Extract user ID
    var userIdClaim = User.FindFirst("sub") ?? User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
    {
        return Unauthorized(new ErrorResponse { Message = "Invalid token." });
    }
    
    // Use userId in your logic
    return Ok();
}
```

## Client Usage

**Login and store token:**
```typescript
const response = await fetch('/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ username, password })
});
const { accessToken } = await response.json();
localStorage.setItem('authToken', accessToken);
```

**Send token with requests:**
```typescript
const token = localStorage.getItem('authToken');
fetch('/api/packs/purchase', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});
```

## Testing

**Register:**
```bash
curl -X POST http://localhost:5088/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"password"}'
```

**Login:**
```bash
curl -X POST http://localhost:5088/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"password"}'
```

**Protected endpoint:**
```bash
curl -X GET http://localhost:5088/users/me \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## Security Notes

- Secret key must be at least 32 characters
- Never commit secrets to version control
- Use different keys for development and production
- Tokens expire after 60 minutes (configurable)
- Always use HTTPS in production
