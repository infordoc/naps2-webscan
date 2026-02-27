# 📚 Índice de Documentação - NAPS2 WebScan

Guia completo de toda a documentação do projeto NAPS2 WebScan.

## 🏠 Documentação Principal

### [README.md](./README.md)
**Visão geral do projeto completo**
- Arquitetura geral do sistema
- Quick start
- Componentes (LocalService + WebServer)
- Requisitos e portas utilizadas
- Exemplos de uso
- Troubleshooting geral

---

## 🔧 NAPS2.WebScan.LocalService

### [LocalService/README.md](./NAPS2.WebScan.LocalService/README.md)
**Guia completo do serviço Windows**
- Recursos e arquitetura
- Instalação (3 métodos)
- API REST
- Protocolo eSCL
- Configuração
- Troubleshooting específico
- Features implementadas

### [LocalService/API_DOCUMENTATION.md](./NAPS2.WebScan.LocalService/API_DOCUMENTATION.md)
**Documentação técnica das APIs**
- **API REST** (`/api/scanners`)
  - GET /api/scanners - Listar scanners
  - GET /api/scanners/current - Scanner atual
  - POST /api/scanners/{id}/select - Selecionar scanner
- **Protocolo eSCL** (`/eSCL/*`)
  - ScannerCapabilities - Capacidades
  - ScannerStatus - Status
  - ScanJobs - Criar job
  - NextDocument - Obter imagem
  - ScanImageInfo - Status do job
- Exemplos práticos (C#, PowerShell, JavaScript)
- Códigos de erro e troubleshooting

### [LocalService/INSTALACAO_SERVICO.md](./NAPS2.WebScan.LocalService/INSTALACAO_SERVICO.md)
**Guia detalhado de instalação como Windows Service**
- Pré-requisitos
- Instalação rápida (batch/PowerShell)
- Instalação manual
- Configuração avançada
- Troubleshooting de instalação
- Permissões e segurança
- Verificação pós-instalação

### [LocalService/instalador.bat](./NAPS2.WebScan.LocalService/instalador.bat)
**Script batch para instalação Windows**
```
Opções:
1. Instalar serviço
2. Desinstalar serviço
3. Iniciar serviço
4. Parar serviço
5. Executar em modo console
6. Publicar executável
7. Testar scanners (API)
8. Testar API
```

### [LocalService/instalador.ps1](./NAPS2.WebScan.LocalService/instalador.ps1)
**Script PowerShell avançado para instalação**
- Menu interativo colorido
- Funções: Install, Uninstall, Start, Stop, Status
- Publish, Test, Logs
- Validação de pré-requisitos
- Integração com API REST

---

## 🌐 NAPS2.WebScan.WebServer

### [WebServer/README.md](./NAPS2.WebScan.WebServer/README.md)
**Guia completo da interface web**
- Arquitetura MVC
- Estrutura de arquivos
- Endpoints (MVC + API proxy)
- Configuração
- SDK eSCL TypeScript
  - Uso básico
  - Scan otimizado
  - Métodos disponíveis
- Exemplos práticos
  - Scan simples
  - Scan do feeder
  - Upload para servidor
- Build e deploy
- Troubleshooting específico

### [WebServer/wwwroot/lib/escl-sdk-ts/](./NAPS2.WebScan.WebServer/wwwroot/lib/escl-sdk-ts/)
**SDK TypeScript para protocolo eSCL**
- `escl/scanner.ts` - Classe Scanner com métodos otimizados
- `types/scanner.d.ts` - Type definitions TypeScript
- Métodos principais:
  - `ScanJobs()` - Iniciar scan
  - `GetNextDocument()` - Obter imagem
  - `GetNextDocumentOptimized()` - Versão otimizada
  - `GetJobStatus()` - Status do job
  - `WaitForDocumentReady()` - Aguardar documento

---

## 📖 Fluxo de Leitura Recomendado

### Para Iniciantes
1. [README.md](./README.md) - Entender o projeto
2. [LocalService/README.md](./NAPS2.WebScan.LocalService/README.md) - Instalar serviço
3. [LocalService/INSTALACAO_SERVICO.md](./NAPS2.WebScan.LocalService/INSTALACAO_SERVICO.md) - Detalhes instalação
4. [WebServer/README.md](./NAPS2.WebScan.WebServer/README.md) - Usar interface

### Para Desenvolvedores
1. [README.md](./README.md) - Arquitetura geral
2. [WebServer/README.md](./NAPS2.WebScan.WebServer/README.md) - SDK e exemplos
3. [LocalService/API_DOCUMENTATION.md](./NAPS2.WebScan.LocalService/API_DOCUMENTATION.md) - APIs REST/eSCL
4. Código-fonte:
   - `LocalService/Worker.cs` - Detecção scanners
   - `WebServer/wwwroot/js/site.ts` - Lógica client-side

### Para Administradores de Sistema
1. [LocalService/INSTALACAO_SERVICO.md](./NAPS2.WebScan.LocalService/INSTALACAO_SERVICO.md) - Instalação
2. [LocalService/instalador.bat](./NAPS2.WebScan.LocalService/instalador.bat) ou [instalador.ps1](./NAPS2.WebScan.LocalService/instalador.ps1) - Scripts
3. [LocalService/README.md](./NAPS2.WebScan.LocalService/README.md) - Troubleshooting

### Para Integração em Sistemas Existentes
1. [LocalService/API_DOCUMENTATION.md](./NAPS2.WebScan.LocalService/API_DOCUMENTATION.md) - Entender APIs
2. Exemplos práticos no documento acima
3. [WebServer/README.md](./NAPS2.WebScan.WebServer/README.md) - SDK TypeScript
4. Adaptar `LocalServiceClient.cs` para seu sistema

---

## 🎯 Quick Reference

### Comandos Rápidos

**Instalar LocalService:**
```powershell
cd NAPS2.WebScan.LocalService
instalador.bat
```

**Verificar Scanners:**
```powershell
curl http://localhost:5000/api/scanners
```

**Executar WebServer:**
```powershell
cd NAPS2.WebScan.WebServer
dotnet run
# http://localhost:5154
```

**Ver Status do Serviço:**
```powershell
sc query "NAPS2.WebScan Service"
```

**Ver Logs:**
```powershell
Get-EventLog -LogName Application -Source "NAPS2.WebScan Service" -Newest 10
```

### URLs Importantes

| Serviço | URL | Descrição |
|---------|-----|-----------|
| LocalService API | http://localhost:5000/api/scanners | API REST
| WebServer | http://localhost:5154 | Interface web |
| Scanner 1 eSCL | http://localhost:9880/eSCL/ | Protocolo eSCL |
| Scanner 2 eSCL | http://localhost:9881/eSCL/ | Protocolo eSCL |
| Scanner 3 eSCL | http://localhost:9882/eSCL/ | Protocolo eSCL |

---

## 🔍 Busca Rápida por Tema

### Instalação
- [INSTALACAO_SERVICO.md](./NAPS2.WebScan.LocalService/INSTALACAO_SERVICO.md)
- [instalador.bat](./NAPS2.WebScan.LocalService/instalador.bat)
- [instalador.ps1](./NAPS2.WebScan.LocalService/instalador.ps1)

### API REST
- [API_DOCUMENTATION.md - Seção REST](./NAPS2.WebScan.LocalService/API_DOCUMENTATION.md#api-rest)

### Protocolo eSCL
- [API_DOCUMENTATION.md - Seção eSCL](./NAPS2.WebScan.LocalService/API_DOCUMENTATION.md#protocolo-escl)

### SDK TypeScript
- [WebServer/README.md - Seção SDK](./NAPS2.WebScan.WebServer/README.md#-sdk-escl)
- [escl-sdk-ts/](./NAPS2.WebScan.WebServer/wwwroot/lib/escl-sdk-ts/)

### Exemplos de Código
- [API_DOCUMENTATION.md - Exemplos](./NAPS2.WebScan.LocalService/API_DOCUMENTATION.md#exemplos-práticos)
- [WebServer/README.md - Exemplos](./NAPS2.WebScan.WebServer/README.md#-exemplos-práticos)

### Troubleshooting
- [LocalService/README.md - Troubleshooting](./NAPS2.WebScan.LocalService/README.md#-troubleshooting)
- [WebServer/README.md - Troubleshooting](./NAPS2.WebScan.WebServer/README.md#-troubleshooting)
- [INSTALACAO_SERVICO.md - Troubleshooting](./NAPS2.WebScan.LocalService/INSTALACAO_SERVICO.md#troubleshooting)

### Arquitetura
- [README.md - Arquitetura](./README.md#-arquitetura)
- [LocalService/README.md - Arquitetura](./NAPS2.WebScan.LocalService/README.md#-arquitetura)
- [WebServer/README.md - Arquitetura](./NAPS2.WebScan.WebServer/README.md#-arquitetura)

---

## 📝 Checklist de Implementação

### ✅ LocalService
- [x] Instalação do serviço Windows
- [x] Detecção de scanners TWAIN
- [x] Configuração TWAIN 64-bit + worker 32-bit
- [x] API REST funcionando
- [x] Servidores eSCL ativos
- [x] CORS habilitado
- [x] Logs configurados

### ✅ WebServer
- [x] Aplicação ASP.NET rodando
- [x] Interface de scan funcionando
- [x] SDK eSCL configurado
- [x] Cliente HTTP para LocalService
- [x] Preview de imagens
- [x] Suporte multi-página

### ✅ Documentação
- [x] README principal
- [x] README LocalService
- [x] README WebServer
- [x] API Documentation completa
- [x] Guia de instalação
- [x] Scripts de instalação (batch + PowerShell)
- [x] Este índice de documentação

---

## 🆘 Precisa de Ajuda?

1. **Consulte o README** do componente específico (LocalService ou WebServer)
2. **Verifique API_DOCUMENTATION.md** para dúvidas sobre endpoints
3. **Execute em modo console** para ver logs detalhados:
   ```powershell
   cd NAPS2.WebScan.LocalService
   dotnet run
   ```
4. **Verifique logs do Event Viewer**:
   ```powershell
   Get-EventLog -LogName Application -Source "NAPS2.WebScan Service" -Newest 20
   ```

---

**Documentação completa e pronta para uso!** 🎉
