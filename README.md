# NAPS2 WebScan

**Sistema completo de scan via web usando NAPS2 SDK e protocolo eSCL (AirPrint).**

Transforma scanners TWAIN tradicionais em scanners acessíveis via HTTP/REST, permitindo scan diretamente do navegador.

## 🎯 O que é?

Este projeto é composto por **duas aplicações**:

1. **LocalService** - Serviço Windows que detecta scanners TWAIN e os expõe via eSCL
2. **WebServer** - Interface web ASP.NET Core MVC para controlar o scan

```
┌──────────────────┐         ┌─────────────────────┐         ┌──────────────┐
│   Navegador      │ HTTP    │   WebServer         │  HTTP   │ LocalService │
│   (JavaScript)   │────────▶│   (ASP.NET MVC)     │────────▶│ (Worker)     │
│                  │         │   localhost:5154    │         │ localhost:5000│
└──────────────────┘         └─────────────────────┘         └──────┬───────┘
                                                                     │
                                                              ┌──────▼────────┐
                                                              │ eSCL Servers  │
                                                              │ :9880, :9881  │
                                                              └──────┬────────┘
                                                                     │
                                                              ┌──────▼────────┐
                                                              │TWAIN Drivers  │
                                                              │  (Scanners)   │
                                                              └───────────────┘
```

## 🚀 Quick Start

### 1. Instalar LocalService

```powershell
cd NAPS2.WebScan.LocalService
instalador.bat
# Escolher: 6 (Publicar) → 1 (Instalar) → 3 (Iniciar)
```

### 2. Verificar Scanners

```powershell
curl http://localhost:5000/api/scanners
```

### 3. Executar WebServer

```powershell
cd NAPS2.WebScan.WebServer
dotnet run
# Abrir: http://localhost:5154
```

## 📦 Componentes

### NAPS2.WebScan.LocalService

**Serviço Windows que gerencia os scanners.**

- ✅ Detecta automaticamente todos os scanners TWAIN
- ✅ Cria servidor eSCL para cada scanner (portas 9880+)
- ✅ API REST em `http://localhost:5000/api/scanners`
- ✅ Suporte TWAIN 64-bit com worker 32-bit
- ✅ Instalável como Windows Service

**Documentação:**
- [LocalService/README.md](./NAPS2.WebScan.LocalService/README.md) - Guia completo
- [LocalService/API_DOCUMENTATION.md](./NAPS2.WebScan.LocalService/API_DOCUMENTATION.md) - API REST e eSCL
- [LocalService/INSTALACAO_SERVICO.md](./NAPS2.WebScan.LocalService/INSTALACAO_SERVICO.md) - Instalação

**Arquivos principais:**
- `Worker.cs` - Detecção de scanners e inicialização eSCL
- `Controllers/ScannersController.cs` - API REST
- `instalador.bat` / `instalador.ps1` - Instaladores

### NAPS2.WebScan.WebServer

**Interface web para controlar o scan.**

- ✅ Interface HTML/TypeScript
- ✅ SDK eSCL em TypeScript
- ✅ Preview de imagens escaneadas
- ✅ Suporte a múltiplas páginas (ADF/feeder)
- ✅ Otimizações de performance (50-75% mais rápido)

**Documentação:**
- `wwwroot/lib/escl-sdk-ts/` - SDK TypeScript para eSCL
- `wwwroot/js/site.ts` - Implementação do scan

**Arquivos principais:**
- `Views/Home/Index.cshtml` - Interface de scan
- `Services/LocalServiceClient.cs` - Cliente HTTP para LocalService
- `wwwroot/lib/escl-sdk-ts/` - SDK eSCL

## 🔧 Requisitos

- **.NET 8.0 SDK** - Para desenvolvimento
- **.NET 8.0 Runtime** - Para produção
- **Windows 10/11** ou **Windows Server 2016+**
- **Drivers TWAIN** dos scanners instalados
- **8GB RAM** recomendado para múltiplos scanners

## 📡 Portas Utilizadas

| Serviço | Porta | Descrição |
|---------|-------|-----------|
| LocalService API | 5000 | API REST de gerenciamento |
| WebServer | 5154 | Interface web |
| Scanner 1 eSCL | 9880 | Protocolo eSCL do scanner 1 |
| Scanner 2 eSCL | 9881 | Protocolo eSCL do scanner 2 |
| Scanner 3 eSCL | 9882 | Protocolo eSCL do scanner 3 |

## 🎨 Arquitetura

### LocalService (Windows Service)

```csharp
Worker (Background Service)
  ↓
ScanningContext (NAPS2)
  ↓
Scanner Detection (TWAIN only)
  ↓
EsclServer Registration (multi-port)
  ↓
TWAIN Worker 32-bit
  ↓
Scanner Drivers
```

### WebServer (ASP.NET MVC)

```
Browser → TypeScript SDK → eSCL HTTP
   ↓           ↓              ↓
Views      site.ts      localhost:9880
   ↓           ↓              ↓
Controller → LocalServiceClient → LocalService API
```

## 📚 Exemplos de Uso

### Listar Scanners

**PowerShell:**
```powershell
(Invoke-WebRequest http://localhost:5000/api/scanners).Content | ConvertFrom-Json
```

**JavaScript:**
```javascript
const response = await fetch('http://localhost:5000/api/scanners');
const scanners = await response.json();
console.log(scanners);
```

### Scan Simples

**TypeScript:**
```typescript
import Scanner from 'escl-sdk-ts';

const scanner = new Scanner({ ip: 'localhost', port: 9880 });

// Iniciar job
const jobUrl = await scanner.ScanJobs({
  Resolution: 300,
  ColorMode: 'RGB24',
  InputSource: 'Feeder'
});

// Obter imagem
const imageBlob = await scanner.GetNextDocument(jobUrl);
const url = URL.createObjectURL(imageBlob);
document.getElementById('preview').src = url;
```

### Scan Otimizado (Multi-página)

```typescript
const images = [];
let pageNum = 1;

while (true) {
  const blob = await scanner.GetNextDocumentOptimized(jobUrl);
  
  if (blob.size === 0) break; // Não há mais páginas
  
  images.push(blob);
  console.log(`Página ${pageNum++} capturada`);
}
```

## 🌟 Features

### LocalService
- [x] Detecção automática de scanners TWAIN
- [x] Multi-scanner simultâneo (portas separadas)
- [x] API REST completa
- [x] Protocolo eSCL (AirPrint)
- [x] Worker TWAIN 32-bit para 64-bit
- [x] Instalador Windows Service
- [x] CORS habilitado
- [x] Logs no Event Viewer

### WebServer
- [x] Interface web responsiva
- [x] SDK eSCL TypeScript
- [x] Preview de imagens
- [x] Suporte ADF (feeder multi-página)
- [x] Otimizações de performance
- [x] Cliente HTTP para LocalService

## 🐛 Troubleshooting

### LocalService não inicia

```powershell
# Verificar status
sc query "NAPS2.WebScan Service"

# Ver logs
Get-EventLog -LogName Application -Source "NAPS2.WebScan Service" -Newest 10

# Executar em modo console para debug
cd NAPS2.WebScan.LocalService
dotnet run
```

### Scanners não aparecem

1. Verifique se os drivers TWAIN estão instalados
2. Teste o scanner no software nativo
3. Execute LocalService em modo console
4. Verifique logs: "Encontrados X dispositivos totais, Y TWAIN"

### Erro CORS

Verifique se LocalService está rodando:
```powershell
curl http://localhost:5000/api/scanners
```

### WebServer não conecta

1. Verifique se LocalService está ativo
2. Confirme porta 5000 não está bloqueada
3. Teste API diretamente: `curl http://localhost:5000/api/scanners`

## 📖 Documentação Completa

- **LocalService:**
  - [README](./NAPS2.WebScan.LocalService/README.md) - Guia completo do serviço
  - [API_DOCUMENTATION](./NAPS2.WebScan.LocalService/API_DOCUMENTATION.md) - Endpoints REST e eSCL
  - [INSTALACAO_SERVICO](./NAPS2.WebScan.LocalService/INSTALACAO_SERVICO.md) - Instalação detalhada

## 🔐 Segurança

⚠️ **Este projeto está configurado para desenvolvimento:**

- CORS: `AllowAnyOrigin` (permitir todas as origens)
- LocalService: Escuta em `0.0.0.0` (todas as interfaces)

**Para produção:**

1. Configure CORS específico:
```csharp
builder.Services.AddCors(options => {
    options.AddPolicy("Production", policy => {
        policy.WithOrigins("https://seu-dominio.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

2. Use HTTPS:
```json
{
  "Urls": "https://localhost:5001"
}
```

3. Configure firewall para portas específicas

## 🤝 Tecnologias

- **NAPS2.Sdk** 1.2.1 - Scanner detection & control
- **NAPS2.Escl.Server** 1.2.1 - eSCL protocol implementation
- **NAPS2.Sdk.Worker.Win32** 1.2.1 - TWAIN 32-bit worker
- **ASP.NET Core** 8.0 - WebServer framework
- **TypeScript** - Browser SDK
- **Axios** - HTTP client

## 📝 Licença

Este projeto usa NAPS2 SDK que é open source (LGPL).

## 🚀 Próximos Passos

1. **Instalar LocalService:**
   ```powershell
   cd NAPS2.WebScan.LocalService
   instalador.bat
   ```

2. **Verificar instalação:**
   ```powershell
   curl http://localhost:5000/api/scanners
   ```

3. **Executar WebServer:**
   ```powershell
   cd NAPS2.WebScan.WebServer
   dotnet run
   ```

4. **Acessar interface:**
   ```
   http://localhost:5154
   ```

**Pronto! Agora você pode fazer scan diretamente do navegador!** 🎉

## 💡 Use Cases

- ✅ Scan remoto via web browser
- ✅ Integração de scanners em aplicações web
- ✅ Scanner-as-a-Service
- ✅ Digitalização de documentos em massa
- ✅ Automatização de workflows com scanners TWAIN
- ✅ Acesso a scanners corporativos via rede

## 📞 Suporte

1. Verifique a documentação específica de cada componente
2. Execute em modo console para debug detalhado
3. Consulte logs no Event Viewer
4. Teste scanners no software nativo primeiro

---

**Original NAPS2.WebScan sample code by cyanfish - Modified and extended**