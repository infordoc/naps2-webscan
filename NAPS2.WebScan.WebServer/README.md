# NAPS2 WebScan WebServer

**Interface web ASP.NET Core MVC para controlar scanners via protocolo eSCL.**

Este componente fornece uma interface web amigável para scanear documentos usando scanners TWAIN/eSCL gerenciados pelo LocalService.

## 🎯 O que é?

WebServer é a **interface visual** do sistema NAPS2 WebScan:

- Interface HTML/Bootstrap para seleção de scanners
- SDK TypeScript para comunicação eSCL
- Preview de imagens escaneadas
- Suporte a múltiplas páginas (feeders ADF)
- Cliente HTTP para comunicação com LocalService

## 🚀 Quick Start

### Desenvolvimento

```powershell
# Instalar dependências JavaScript
npm install

# Compilar TypeScript (opcional - hot reload funciona)
npm run build

# Executar servidor
dotnet run

# Abrir navegador
start http://localhost:5154
```

### Produção

```powershell
# Publicar
dotnet publish -c Release -o ./publish

# Executar
cd publish
NAPS2.WebScan.WebServer.exe
```

## 📦 Estrutura

```
NAPS2.WebScan.WebServer/
├── Controllers/
│   ├── HomeController.cs          # Controller MVC principal
│   └── ScannersController.cs      # API REST (proxy para LocalService)
├── Services/
│   └── LocalServiceClient.cs      # Cliente HTTP para LocalService
├── Views/
│   ├── Home/
│   │   └── Index.cshtml           # Interface de scan
│   └── Shared/
│       └── _Layout.cshtml         # Layout Bootstrap
├── wwwroot/
│   ├── js/
│   │   └── site.ts                # Lógica principal de scan
│   └── lib/
│       └── escl-sdk-ts/           # SDK TypeScript para eSCL
│           ├── escl/
│           │   └── scanner.ts     # Classe Scanner com métodos otimizados
│           └── types/
│               └── scanner.d.ts   # Type definitions
├── Program.cs                     # Configuração ASP.NET Core
├── package.json                   # Dependências Node
├── tsconfig.json                  # Config TypeScript
└── vite.config.js                 # Bundler Vite
```

## 🎨 Arquitetura

```
┌───────────────────────────────────────┐
│          Navegador                    │
│  ┌─────────────────────────────────┐  │
│  │  Index.cshtml (View)            │  │
│  │  site.ts (Logic)                │  │
│  └─────────────────────────────────┘  │
│              │                         │
│              ▼                         │
│  ┌─────────────────────────────────┐  │
│  │  escl-sdk-ts (Scanner SDK)      │  │
│  │  - ScanJobs()                   │  │
│  │  - GetNextDocumentOptimized()   │  │
│  │  - GetJobStatus()               │  │
│  └─────────────────────────────────┘  │
└──────────────┬────────────────────────┘
               │ HTTP
               ▼
┌──────────────────────────────────────┐
│    WebServer (ASP.NET MVC)           │
│  ┌────────────────────────────────┐  │
│  │  HomeController                │  │
│  └────────────────────────────────┘  │
│  ┌────────────────────────────────┐  │
│  │  LocalServiceClient            │  │
│  │  GET /api/scanners             │  │
│  │  POST /api/scanners/{id}/select│  │
│  └────────────────────────────────┘  │
└──────────────┬───────────────────────┘
               │ HTTP
               ▼
         LocalService:5000
               ▼
         eSCL Servers:9880+
```

## 📡 Endpoints

### MVC Routes

| Rota | Descrição |
|------|-----------|
| `GET /` | Página inicial com interface de scan |
| `GET /Home/Privacy` | Página de privacidade (exemplo) |

### API Routes (Proxy para LocalService)

| Rota | Método | Descrição |
|------|--------|-----------|
| `/api/scanners` | GET | Lista todos os scanners disponíveis |
| `/api/scanners/current` | GET | Scanner atualmente selecionado |
| `/api/scanners/{id}/select` | POST | Seleciona um scanner |

## 🔧 Configuração

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "LocalServiceUrl": "http://localhost:5000"
}
```

### LocalServiceClient

Injeta `HttpClient` configurado para comunicar com LocalService:

```csharp
builder.Services.AddHttpClient<LocalServiceClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5000");
});
```

### CORS

CORS está configurado para permitir requisições do LocalService:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

## 🎨 Interface de Usuário

### Views/Home/Index.cshtml

Interface Bootstrap com:
- Dropdown para seleção de scanner
- Botão "Scan" para iniciar captura
- Área de preview de imagens
- Feedback visual de progresso

### wwwroot/js/site.ts

Lógica TypeScript que:
1. Carrega lista de scanners do LocalService
2. Permite seleção de scanner
3. Inicia job de scan via eSCL
4. Captura imagens otimizadamente
5. Exibe preview no navegador

## 🚀 SDK eSCL

### Uso Básico

```typescript
import Scanner from 'escl-sdk-ts';

// Criar instância do scanner
const scanner = new Scanner({ 
  ip: 'localhost', 
  port: 9880 
});

// Iniciar job de scan
const jobUrl = await scanner.ScanJobs({
  Resolution: 300,
  ColorMode: 'RGB24',
  InputSource: 'Feeder',
  Height: 3508,  // A4 em pixels @ 300 DPI
  Width: 2480
});

// Obter primeira imagem
const imageBlob = await scanner.GetNextDocument(jobUrl);

// Exibir no navegador
const url = URL.createObjectURL(imageBlob);
document.getElementById('preview').src = url;
```

### Scan Otimizado (Multi-página)

```typescript
// Método otimizado com polling inteligente
const images = [];
let pageNum = 1;

while (true) {
  const blob = await scanner.GetNextDocumentOptimized(jobUrl);
  
  // Blob vazio = não há mais páginas
  if (blob.size === 0) {
    console.log('Scan finalizado!');
    break;
  }
  
  images.push(blob);
  console.log(`Página ${pageNum++} capturada (${blob.size} bytes)`);
}
```

### Verificar Status do Job

```typescript
// Verificar se há documentos prontos
const status = await scanner.GetJobStatus(jobUrl);

if (status.documentReady) {
  const blob = await scanner.GetNextDocument(jobUrl);
  // processar imagem
}
```

### Aguardar Documento Pronto

```typescript
// Aguarda até documento estar pronto (max 30s)
const ready = await scanner.WaitForDocumentReady(jobUrl, 30000);

if (ready) {
  const blob = await scanner.GetNextDocument(jobUrl);
}
```

## 📊 Métodos Disponíveis

### Scanner Class

| Método | Descrição | Retorno |
|--------|-----------|---------|
| `ScanJobs(params)` | Inicia job de scan | `Promise<string>` (Job URL) |
| `GetNextDocument(jobUrl)` | Obtém próxima imagem | `Promise<Blob>` |
| `GetNextDocumentOptimized(jobUrl)` | Versão otimizada 50-75% mais rápida | `Promise<Blob>` |
| `GetJobStatus(jobUrl)` | Verifica status do job | `Promise<JobStatus>` |
| `WaitForDocumentReady(jobUrl, timeout)` | Aguarda documento | `Promise<boolean>` |
| `GetCapabilities()` | Capabilidades do scanner | `Promise<Capabilities>` |
| `GetStatus()` | Status atual | `Promise<ScannerStatus>` |

### IScanSettingParams

```typescript
interface IScanSettingParams {
  Resolution?: number;           // 75, 150, 300, 600 DPI
  ColorMode?: 'BlackAndWhite1' | 'Grayscale8' | 'RGB24';
  InputSource?: 'Platen' | 'Feeder';
  Height?: number;               // Pixels
  Width?: number;                // Pixels
  XOffset?: number;
  YOffset?: number;
  Intent?: 'Document' | 'Photo' | 'Preview';
  Duplex?: boolean;
}
```

## 🎯 Exemplos Práticos

### Scan Simples (Uma Página)

```typescript
async function scanOnePage() {
  const scanner = new Scanner({ ip: 'localhost', port: 9880 });
  
  const jobUrl = await scanner.ScanJobs({
    Resolution: 300,
    ColorMode: 'RGB24',
    InputSource: 'Platen'
  });
  
  const imageBlob = await scanner.GetNextDocument(jobUrl);
  const url = URL.createObjectURL(imageBlob);
  
  const img = document.createElement('img');
  img.src = url;
  document.body.appendChild(img);
}
```

### Scan do Feeder (Múltiplas Páginas)

```typescript
async function scanFeeder() {
  const scanner = new Scanner({ ip: 'localhost', port: 9880 });
  
  const jobUrl = await scanner.ScanJobs({
    Resolution: 300,
    ColorMode: 'Grayscale8',
    InputSource: 'Feeder'
  });
  
  const images = [];
  
  while (true) {
    try {
      const blob = await scanner.GetNextDocumentOptimized(jobUrl);
      
      if (blob.size === 0) break;
      
      images.push(blob);
      console.log(`Página ${images.length} capturada`);
      
    } catch (error) {
      console.log('Feeder vazio ou scan finalizado');
      break;
    }
  }
  
  return images;
}
```

### Upload para Servidor

```typescript
async function scanAndUpload() {
  const scanner = new Scanner({ ip: 'localhost', port: 9880 });
  
  const jobUrl = await scanner.ScanJobs({
    Resolution: 300,
    ColorMode: 'RGB24'
  });
  
  const images = [];
  
  while (true) {
    const blob = await scanner.GetNextDocumentOptimized(jobUrl);
    if (blob.size === 0) break;
    images.push(blob);
  }
  
  // Upload via FormData
  const formData = new FormData();
  images.forEach((blob, index) => {
    formData.append('files', blob, `scan_${index}.jpg`);
  });
  
  await fetch('/api/documents/upload', {
    method: 'POST',
    body: formData
  });
}
```

## 🔧 Build e Deploy

### Desenvolvimento

```powershell
# Watch mode para TypeScript
npm run dev

# Executar servidor ASP.NET
dotnet watch run
```

### Produção

```powershell
# Build TypeScript
npm run build

# Publicar aplicação
dotnet publish -c Release -r win-x64 --self-contained false -o ./publish

# Executar
./publish/NAPS2.WebScan.WebServer.exe
```

### Docker (Opcional)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY ./publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "NAPS2.WebScan.WebServer.dll"]
```

## 🐛 Troubleshooting

### Scanners não aparecem no dropdown

1. Verifique se LocalService está rodando:
```powershell
curl http://localhost:5000/api/scanners
```

2. Verifique logs do navegador (F12):
```javascript
console.log('Scanners:', await fetch('http://localhost:5000/api/scanners').then(r => r.json()));
```

### Erro CORS

LocalService deve ter CORS habilitado:
```csharp
// NAPS2.WebScan.LocalService/Program.cs
app.UseCors();
```

### Imagens não aparecem

1. Verifique se porta eSCL está acessível:
```powershell
curl http://localhost:9880/eSCL/ScannerStatus
```

2. Verifique se blob tem conteúdo:
```javascript
console.log('Blob size:', blob.size);
```

### TypeScript não compila

```powershell
# Limpar cache
rm -r node_modules
npm install

# Verificar versão Node
node --version  # Mínimo: 18.x

# Recompilar
npm run build
```

## 📚 Referências

- [eSCL Specification](https://mopria.org/mopria-escl-specification) - Protocolo eSCL oficial
- [NAPS2 SDK](https://www.naps2.com/sdk) - Documentação NAPS2
- [ASP.NET Core MVC](https://learn.microsoft.com/aspnet/core/mvc/) - Framework web
- [TypeScript](https://www.typescriptlang.org/) - Linguagem
- [Vite](https://vitejs.dev/) - Build tool

## 🔐 Segurança

⚠️ **Desenvolvimento:**
- Todas as requisições permitidas (CORS aberto)
- HTTP sem criptografia
- Sem autenticação

**Para produção:**
1. Configure HTTPS
2. Adicione autenticação (JWT/Cookie)
3. Restrinja CORS
4. Valide inputs do usuário

## 💡 Próximos Passos

1. **Personalizar Interface:**
   - Editar [Views/Home/Index.cshtml](./Views/Home/Index.cshtml)
   - Modificar [wwwroot/js/site.ts](./wwwroot/js/site.ts)

2. **Adicionar Features:**
   - Upload automático para servidor
   - Processamento de imagens (crop, rotate)
   - OCR (reconhecimento de texto)
   - Salvar como PDF

3. **Integrar com seu sistema:**
   - Usar LocalServiceClient em seus controllers
   - Adicionar autenticação
   - Implementar banco de dados

## 📞 Suporte

Veja documentação do [LocalService](../NAPS2.WebScan.LocalService/README.md) para configuração completa do sistema.
