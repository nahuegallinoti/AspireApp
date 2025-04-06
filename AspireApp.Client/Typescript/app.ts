export function showAlert(message: string): void {
    alert(`Mensaje desde TypeScript: ${message}`);
}

(window as any).showAlert = showAlert;
