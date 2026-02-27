# 🚀 Compilação e Distribuição - NAPS2 WebScan

Guia rápido para compilar e distribuir o NAPS2 WebScan LocalService.

## 📦 Fluxo Completo

```
1. Compilar       →  2. Testar local  →  3. Zipar  →  4. Distribuir
   compilar.bat      dist\instalador.bat   ZIP      Cliente instala
```

## 🔨 1. Compilar o Projeto

### Windows (Batch)

```cmd
cd NAPS2.WebScan.LocalService
compilar.bat
```

### PowerShell

```powershell
cd NAPS2.WebScan.LocalService
.\compilar.ps1
```

**Resultado:**
- Pasta `dist` criada com todos os arquivos
- Executável compilado
- Instaladores incluídos (.bat e .ps1)
- Documentação completa

## ✅ 2. Testar Localmente

```powershell
# Ir para pasta compilada
cd dist

# Executar instalador como Administrador
instalador.bat

# Menu:
# 1. Instalar como serviço
# 3. Iniciar serviço
# 5. Verificar scanners

# Testar API
curl http://localhost:5000/api/scanners
```

## 📦 3. Zipar para Distribuição

### PowerShell

```powershell
# Na pasta NAPS2.WebScan.LocalService
Compress-Archive -Path .\dist\* -DestinationPath NAPS2-WebScan-v2.0.0.zip
```

### Windows Explorer

1. Entrar na pasta `dist`
2. Selecionar todos os arquivos (Ctrl+A)
3. Botão direito → Enviar para → Pasta compactada

## 🎯 4. Distribuir para Cliente

### O que enviar

- **Arquivo:** `NAPS2-WebScan-v2.0.0.zip`
- **Tamanho:** ~5-10 MB
- **Conteúdo:**
  - `NAPS2.WebScan.LocalService.exe` ✅
  - `instalador.bat` ✅
  - `instalador.ps1` ✅
  - `README_INSTALACAO.txt` ✅ (guia rápido)
  - Documentação completa ✅
  - Todas as DLLs necessárias ✅

### Instruções para o cliente

1. **Extrair ZIP** para uma pasta (ex: `C:\NAPS2-WebScan\`)
2. **Executar como Administrador:** `instalador.bat` ou `instalador.ps1`
3. **Selecionar opção 1** - Instalar como serviço
4. **Selecionar opção 3** - Iniciar serviço
5. **Testar:** Abrir navegador em `http://localhost:5000/api/scanners`

## 📋 Arquivos na Pasta `dist`

Após compilação, a pasta contém:

```
dist/
├── NAPS2.WebScan.LocalService.exe    ← Executável principal
├── instalador.bat                    ← Instalador Windows
├── instalador.ps1                    ← Instalador PowerShell  
├── README.md                         ← Documentação completa
├── README_INSTALACAO.txt             ← Guia rápido (TXT simples)
├── BUILD.md                          ← Guia de compilação
├── API_DOCUMENTATION.md              ← Docs da API
├── INSTALACAO_SERVICO.md             ← Guia de instalação
├── appsettings.json                  ← Configuração
├── *.dll                            ← Bibliotecas necessárias
└── worker32/                        ← Worker TWAIN 32-bit
```

## 🔧 Requisitos

### Para Desenvolvimento (Compilar)

- Windows 10/11
- **.NET 8.0 SDK** (não Runtime)
- Visual Studio 2022 ou VS Code (opcional)

### Para Produção (Cliente)

- Windows 10/11 ou Windows Server 2016+
- **.NET 8.0 Runtime** (não SDK)
- Drivers TWAIN dos scanners instalados
- Permissões de Administrador (para instalar serviço)

## 🎛️ Opções Avançadas de Compilação

### Self-Contained (Inclui .NET Runtime)

```powershell
.\compilar.ps1 -SelfContained
```

**Vantagens:**
- Cliente não precisa instalar .NET Runtime
- Funciona em qualquer Windows

**Desvantagens:**
- ~60MB maior
- Cada atualização inclui .NET completo

### Single File (Arquivo Único)

```powershell
.\compilar.ps1 -SingleFile
```

**Vantagens:**
- Um único executável
- Mais fácil de distribuir

**Desvantagens:**
- Extrai arquivos temporários ao executar
- Pode ser mais lento no primeiro start

### Pasta de Output Customizada

```powershell
.\compilar.ps1 -OutputDir "C:\MeuOutput"
```

## 🧪 Testando a Compilação

Checklist antes de distribuir:

- [ ] Compilação sem erros
- [ ] Executável criado em `dist\NAPS2.WebScan.LocalService.exe`
- [ ] Instaladores presentes (`instalador.bat` e `instalador.ps1`)
- [ ] Documentação incluída
- [ ] Testado instalador localmente
- [ ] Serviço inicia sem erros
- [ ] API responde: `http://localhost:5000/api/scanners`
- [ ] Scanners são detectados
- [ ] Worker TWAIN 32-bit funciona
- [ ] ZIP criado com tamanho correto (~5-10 MB)

## 📝 Comandos Rápidos

```powershell
# Compilar
cd NAPS2.WebScan.LocalService
.\compilar.ps1

# Testar
cd dist
.\instalador.ps1 install
.\instalador.ps1 start
curl http://localhost:5000/api/scanners

# Zipar
cd ..
Compress-Archive -Path .\dist\* -DestinationPath NAPS2-WebScan-v2.0.0.zip

# Limpar (se necessário)
cd dist
.\instalador.ps1 stop
.\instalador.ps1 uninstall
```

## 🐛 Troubleshooting

### Erro: .NET SDK não encontrado

```powershell
# Verificar instalação
dotnet --version

# Se não encontrado, instalar:
# https://dotnet.microsoft.com/download/dotnet/8.0
```

### Erro: Arquivo em uso

```powershell
# Parar serviço antes de recompilar
cd dist
.\instalador.ps1 stop

# Ou matar processo
taskkill /F /IM NAPS2.WebScan.LocalService.exe
```

### Compilação lenta

A primeira compilação pode demorar 1-2 minutos enquanto restaura pacotes NuGet. Compilações subsequentes são mais rápidas (~10-30 segundos).

## 📞 Próximos Passos

1. ✅ Compilar: `.\compilar.bat`
2. ✅ Testar: `cd dist` → `.\instalador.bat`
3. ✅ Zipar: `Compress-Archive -Path .\dist\* ...`
4. ✅ Distribuir: Enviar ZIP para cliente

**Pronto para distribuir!** 🎉

---

Para mais detalhes, consulte:
- [BUILD.md](NAPS2.WebScan.LocalService/BUILD.md) - Guia completo de compilação
- [INSTALACAO_SERVICO.md](NAPS2.WebScan.LocalService/INSTALACAO_SERVICO.md) - Guia de instalação
- [README.md](README.md) - Documentação geral do projeto
