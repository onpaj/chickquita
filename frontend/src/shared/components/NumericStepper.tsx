import { Box, IconButton, TextField, Typography } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import RemoveIcon from '@mui/icons-material/Remove';

interface NumericStepperProps {
  label: string;
  value: number;
  onChange: (value: number) => void;
  min?: number;
  max?: number;
  step?: number;
  disabled?: boolean;
  error?: boolean;
  helperText?: string;
  'aria-label'?: string;
}

/**
 * NumericStepper Component
 *
 * Mobile-friendly numeric input with +/- buttons for easy touch interaction.
 * Follows Material Design guidelines with 48px minimum touch targets.
 *
 * @example
 * <NumericStepper
 *   label="Hens"
 *   value={hensCount}
 *   onChange={setHensCount}
 *   min={0}
 * />
 */
export function NumericStepper({
  label,
  value,
  onChange,
  min = 0,
  max = Number.MAX_SAFE_INTEGER,
  step = 1,
  disabled = false,
  error = false,
  helperText,
  'aria-label': ariaLabel,
}: NumericStepperProps) {
  const handleDecrement = () => {
    const newValue = Math.max(min, value - step);
    if (newValue !== value) {
      onChange(newValue);
    }
  };

  const handleIncrement = () => {
    const newValue = Math.min(max, value + step);
    if (newValue !== value) {
      onChange(newValue);
    }
  };

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const inputValue = event.target.value;

    // Allow empty input for better UX (user can clear and type)
    if (inputValue === '') {
      onChange(min);
      return;
    }

    const numericValue = parseInt(inputValue, 10);

    if (!isNaN(numericValue)) {
      const clampedValue = Math.max(min, Math.min(max, numericValue));
      onChange(clampedValue);
    }
  };

  const isDecrementDisabled = disabled || value <= min;
  const isIncrementDisabled = disabled || value >= max;

  return (
    <Box>
      <Typography
        variant="body2"
        color={error ? 'error' : 'text.secondary'}
        sx={{ mb: 1 }}
      >
        {label}
      </Typography>

      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          gap: 1,
        }}
      >
        <IconButton
          onClick={handleDecrement}
          disabled={isDecrementDisabled}
          aria-label={ariaLabel ? `${ariaLabel} decrease` : `Decrease ${label}`}
          color={error ? 'error' : 'primary'}
          sx={{
            border: 1,
            borderColor: error ? 'error.main' : 'divider',
            '&:hover': {
              borderColor: error ? 'error.dark' : 'primary.main',
            },
          }}
        >
          <RemoveIcon />
        </IconButton>

        <TextField
          type="number"
          value={value}
          onChange={handleInputChange}
          disabled={disabled}
          error={error}
          inputProps={{
            min,
            max,
            step,
            'aria-label': ariaLabel || label,
            style: { textAlign: 'center' },
          }}
          sx={{
            width: 80,
            '& input[type=number]': {
              MozAppearance: 'textfield',
            },
            '& input[type=number]::-webkit-outer-spin-button, & input[type=number]::-webkit-inner-spin-button': {
              WebkitAppearance: 'none',
              margin: 0,
            },
          }}
        />

        <IconButton
          onClick={handleIncrement}
          disabled={isIncrementDisabled}
          aria-label={ariaLabel ? `${ariaLabel} increase` : `Increase ${label}`}
          color={error ? 'error' : 'primary'}
          sx={{
            border: 1,
            borderColor: error ? 'error.main' : 'divider',
            '&:hover': {
              borderColor: error ? 'error.dark' : 'primary.main',
            },
          }}
        >
          <AddIcon />
        </IconButton>
      </Box>

      {helperText && (
        <Typography
          variant="caption"
          color={error ? 'error' : 'text.secondary'}
          sx={{ mt: 0.5, display: 'block' }}
        >
          {helperText}
        </Typography>
      )}
    </Box>
  );
}
