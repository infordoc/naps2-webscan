# Otimização de Velocidade de Scan

## 📊 Comparação de Métodos

### 1. Método Otimizado (Recomendado) ⚡
```typescript
const doc = await scanner.GetNextDocumentOptimized(jobId);
```

**Como funciona:**
- Faz polling no endpoint `ScanImageInfo` a cada 500ms
- Retorna assim que a imagem está pronta
- Não bloqueia a thread principal

**Vantagens:**
- ✅ Mais rápido (0.5-2 segundos)
- ✅ Não bloqueia conexão HTTP
- ✅ Feedback de progresso possível
- ✅ Menor uso de recursos

**Desvantagens:**
- ❌ Código um pouco mais complexo (já encapsulado)

---

### 2. Método Padrão (Simples)
```typescript
const doc = await scanner.NextDocument(jobId);
```

**Como funciona:**
- Chama `NextDocument` que bloqueia até receber resposta
- Se recebe 503, espera 2 segundos e tenta novamente
- Continua até receber a imagem ou erro

**Vantagens:**
- ✅ Código simples
- ✅ Funciona sempre
- ✅ Sem polling manual

**Desvantagens:**
- ❌ Mais lento (2-4 segundos por retry)
- ❌ Bloqueia conexão HTTP
- ❌ Sem feedback de progresso

---

## 🎯 Endpoints Disponíveis

### Status do Job
```http
GET /eSCL/ScanJobs/{jobId}
```
Retorna o status completo do job (Pending, Processing, Completed)

### Informações da Imagem
```http
GET /eSCL/ScanJobs/{jobId}/ScanImageInfo
```
Retorna informações sobre a imagem quando pronta:
- **200 OK** - Imagem disponível para download
- **404 Not Found** - Imagem ainda não disponível
- **409 Conflict** - Scan em progresso

### Próximo Documento
```http
GET /eSCL/ScanJobs/{jobId}/NextDocument
```
Retorna a imagem escaneada:
- **200 OK** - Retorna a imagem
- **404 Not Found** - Nenhum documento (fim do feeder)
- **503 Service Unavailable** - Processando, tente novamente

---

## 🚀 Exemplos Práticos

### Scan Simples e Rápido
```typescript
import Scanner from './lib/escl-sdk-ts/escl/scanner';

const scanner = new Scanner({ ip: '127.0.0.1', port: 9880 });

// Criar job
const jobUrl = await scanner.ScanJobs({ Resolution: 300 });
const jobId = jobUrl.split('/').pop();

// Obter documento (método otimizado)
const doc = await scanner.GetNextDocumentOptimized(jobId);

// Exibir imagem
const blob = new Blob([doc.data], { type: "image/jpeg" });
const imageUrl = URL.createObjectURL(blob);
document.querySelector('#preview').src = imageUrl;
```

### Scan com Feedback de Status
```typescript
const scanner = new Scanner({ ip: '127.0.0.1', port: 9880 });
const jobUrl = await scanner.ScanJobs({ Resolution: 300 });
const jobId = jobUrl.split('/').pop();

// Polling manual com feedback
let attempts = 0;
while (attempts < 30) {
    try {
        const imageInfo = await scanner.ScanImageInfo(jobId);
        if (imageInfo) {
            console.log('Imagem pronta!');
            break;
        }
    } catch (err) {
        if (err.response?.status === 404) {
            console.log(`Aguardando... (${attempts}/30)`);
            await new Promise(resolve => setTimeout(resolve, 500));
            attempts++;
            continue;
        }
        throw err;
    }
}

// Baixar imagem
const doc = await scanner.NextDocument(jobId);
```

### Múltiplas Páginas (Feeder)
```typescript
const scanner = new Scanner({ ip: '127.0.0.1', port: 9880 });
const jobUrl = await scanner.ScanJobs({ Resolution: 300 });
const jobId = jobUrl.split('/').pop();

const pages = [];
let pageNum = 1;

while (true) {
    try {
        // Aguardar página pronta
        const ready = await scanner.WaitForDocumentReady(jobId);
        if (!ready) break;

        // Obter página
        const doc = await scanner.NextDocument(jobId);
        pages.push(new Blob([doc.data], { type: "image/jpeg" }));
        
        console.log(`Página ${pageNum} escaneada`);
        pageNum++;
        
    } catch (err) {
        if (err.response?.status === 404) {
            console.log('Fim do feeder');
            break;
        }
        throw err;
    }
}

console.log(`Total de ${pages.length} páginas escaneadas`);
```

---

## 📈 Performance Esperada

| Cenário | Método Padrão | Método Otimizado | Ganho |
|---------|---------------|------------------|-------|
| Scan simples (1 página) | 2-4 segundos | 0.5-2 segundos | **50-75%** |
| Scan feeder (10 páginas) | 20-40 segundos | 5-20 segundos | **60-75%** |
| Scan feeder (50 páginas) | 100-200s | 25-100s | **60-75%** |

*Tempos estimados após o scanner iniciar o processo físico de scan*

---

## 🔧 Configuração de Polling

Você pode ajustar os parâmetros de polling para seu caso específico:

```typescript
// Polling mais agressivo (mais rápido, mais requisições)
const doc = await scanner.GetNextDocumentOptimized(
    jobId,
    60,   // maxAttempts: 60 tentativas
    250   // delayMs: 250ms entre tentativas
);

// Polling mais suave (mais lento, menos requisições)
const doc = await scanner.GetNextDocumentOptimized(
    jobId,
    20,   // maxAttempts: 20 tentativas
    1000  // delayMs: 1000ms entre tentativas
);
```

---

## 💡 Dicas de Otimização

1. **Use GetNextDocumentOptimized** para melhor performance
2. **Configure polling baseado no scanner**:
   - Scanners rápidos: 250-500ms
   - Scanners médios: 500-1000ms
   - Scanners lentos: 1000-2000ms

3. **Para feeders**, use `WaitForDocumentReady` entre páginas
4. **Mostre feedback** ao usuário durante o polling
5. **Trate erros apropriadamente**:
   - 404 = fim do feeder (normal)
   - 503 = retry (normal)
   - Outros = erro real

---

## 📚 Mais Exemplos

Veja [scan-examples.ts](./scan-examples.ts) para exemplos completos de:
- Scan com retry inteligente
- Benchmark de métodos
- Feedback em tempo real
- Scan de múltiplas páginas
- Tratamento de erros
