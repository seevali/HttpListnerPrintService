# HttpListnerPrintService

## Installation Instructions

### Install the Service as a Windows Service

1. Open a command prompt with administrative privileges.
2. Navigate to the directory where the service executable is located.
3. Run the following command to install the service:
   ```
   sc create HttpListnerPrintService binPath= "C:\path\to\your\service.exe"
   ```
4. Start the service with the following command:
   ```
   sc start HttpListnerPrintService
   ```

### Send HTTPS POST Requests to the Service

To send HTTPS POST requests to the service, you can use tools like `curl` or Postman. Below is an example using `curl`:

```
curl -X POST https://localhost:5001/print -H "Content-Type: application/json" -d "{\"pdfBase64\": \"<base64-encoded-pdf-string>\"}"
```

### Configure the Default Printer in Windows Settings

1. Open the "Settings" app in Windows.
2. Go to "Devices" > "Printers & scanners".
3. Select the printer you want to set as the default.
4. Click on "Manage".
5. Click on "Set as default".
