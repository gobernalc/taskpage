import { AbstractControl, ValidationErrors } from '@angular/forms';

/**
 * Validates that a control value is empty or a valid JSON document.
 * @param control Control holding the optional JSON string.
 * @returns An invalidJson error when the content is not valid JSON.
 */
export function jsonDocumentValidator(control: AbstractControl): ValidationErrors | null {
  const value = (control.value as string) ?? '';
  if (value.trim() === '') {
    return null;
  }
  try {
    JSON.parse(value);
    return null;
  } catch {
    return { invalidJson: true };
  }
}
