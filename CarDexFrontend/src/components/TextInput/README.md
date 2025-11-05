# TextInput Component

CARDEX's custom TextInput component, showcased using our **FFPU system**.

## F - Files
- **TextInput.tsx** - Input component with TypeScript types
- **TextInput.module.css** - Input stylesheet
- **Demo.tsx** - Demo of all input variants

</br>

## F - Features

### Sizes
- **Regular**: 36px height, 12px horizontal padding, 8px vertical padding
- **Large**: 48px height, 16px horizontal padding, 12px vertical padding

### Input Types
- **Text**: Standard text input (for username, email, etc.)
- **Password**: Masked password input
- **Search**: Search input for marketplace/filtering

### States
- **Default**: Standard input state
- **Hover**: Brightened background on hover
- **Focus**: Border highlight with glow effect
- **Error**: Red border with error message below
- **Disabled**: Reduced opacity, not interactive

### Features
- **Icons**: Optional icon on the left side
- **Labels**: Optional label above input
- **Required**: Asterisk indicator for required fields
- **Error Messages**: Validation error display
- **Max Length**: Character limit support
- **Auto-complete**: Support for browser auto-complete

</br>

## P - Properties

```typescript
type InputProps = {
  value?: string;                        // Input value
  onChange?: (e: ChangeEvent) => void;   // Change handler
  size?: "regular" | "large";            // Default: "regular"
  type?: "text" | "password" | "search"; // Default: "text"
  placeholder?: string;                  // Placeholder text
  icon?: React.ReactNode;                // Icon element
  error?: string;                        // Error message
  disabled?: boolean;                    // Disabled state
  label?: string;                        // Label text
  required?: boolean;                    // Required field indicator
  maxLength?: number;                    // Max character limit
  autoComplete?: string;                 // Browser autocomplete
  className?: string;                    // Additional CSS classes
  id?: string;                           // Input ID
  name?: string;                         // Input name
};
```

</br>

## U - Usage

### Basic Inputs
```tsx
import Input from "./Input";

// Regular text input
<Input placeholder="Enter text..." />

// Large input with label
<Input 
  size="large" 
  label="Username"
  placeholder="Enter your username"
/>

// Required field
<Input 
  label="Email"
  placeholder="user@example.com"
  required
/>
```

### Input Types
```tsx
// Text input (default)
<Input 
  type="text"
  placeholder="Enter username"
/>

// Password input
<Input 
  type="password"
  placeholder="Enter password"
/>

// Search input
<Input 
  type="search"
  placeholder="Search cards..."
/>
```

### With Icons
```tsx
// Icon + Input
<Input 
  icon={<UserIcon />}
  placeholder="Username"
  label="Username"
/>

// Using emoji as icon
<Input 
  icon={<span>🔍</span>}
  type="search"
  placeholder="Search..."
/>
```

### Size Variants
```tsx
// Regular size (36px height)
<Input 
  size="regular"
  placeholder="Regular input"
/>

// Large size (48px height)
<Input 
  size="large"
  placeholder="Large input"
/>
```

### Error State
```tsx
// With error message
<Input 
  label="Email"
  placeholder="user@example.com"
  error="Please enter a valid email address"
/>

// Error with validation
<Input 
  label="Password"
  type="password"
  error="Password must be at least 8 characters"
  required
/>
```

### Disabled State
```tsx
<Input 
  placeholder="Cannot edit"
  disabled
  value="Read only"
/>
```

### Controlled Inputs
```tsx
const [username, setUsername] = useState("");

<Input 
  value={username}
  onChange={(e) => setUsername(e.target.value)}
  placeholder="Enter username"
/>
```