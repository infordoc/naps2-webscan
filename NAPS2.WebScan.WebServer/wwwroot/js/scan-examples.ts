/**
 * Exemplos avançados de uso da API de Scan
 * Para otimização de velocidade e múltiplas páginas
 */

import Scanner from '../lib/escl-sdk-ts/escl/scanner'

// =================================================================
// EXEMPLO 1: Scan Simples Otimizado
// =================================================================
export async function scanSinglePageOptimized(scannerPort: number = 9880): Promise<Blob> {
    const scanner = new Scanner({ ip: '127.0.0.1', port: scannerPort });

    // Criar job
    const jobUrl = await scanner.ScanJobs({ Resolution: 300 });
    const jobId = jobUrl.split('/').pop()!;

    console.time('scan-duration');
    
    // Método otimizado - polling de 500ms
    const doc = await scanner.GetNextDocumentOptimized(jobId);
    
    console.timeEnd('scan-duration');

    return new Blob([doc.data], { type: "image/jpeg" });
}

// =================================================================
// EXEMPLO 2: Scan com Feedback de Status em Tempo Real
// =================================================================
export async function scanWithStatusFeedback(
    scannerPort: number,
    onStatusUpdate: (status: string) => void
): Promise<Blob> {
    const scanner = new Scanner({ ip: '127.0.0.1', port: scannerPort });

    onStatusUpdate('Criando job de scan...');
    const jobUrl = await scanner.ScanJobs({ Resolution: 300 });
    const jobId = jobUrl.split('/').pop()!;

    onStatusUpdate('Aguardando documento...');
    
    // Polling manual com feedback
    let ready = false;
    let attempts = 0;
    const maxAttempts = 30;

    while (!ready && attempts < maxAttempts) {
        try {
            const imageInfo = await scanner.ScanImageInfo(jobId);
            if (imageInfo) {
                onStatusUpdate('Documento pronto!');
                ready = true;
                break;
            }
        } catch (err: any) {
            if (err.response?.status === 404) {
                onStatusUpdate(`Aguardando scan... (${attempts + 1}/${maxAttempts})`);
            } else if (err.response?.status === 409) {
                onStatusUpdate('Scanner processando...');
            } else {
                throw err;
            }
            
            await new Promise(resolve => setTimeout(resolve, 500));
            attempts++;
        }
    }

    if (!ready) {
        throw new Error('Timeout aguardando documento');
    }

    onStatusUpdate('Baixando imagem...');
    const doc = await scanner.NextDocument(jobId);
    
    onStatusUpdate('Concluído!');
    return new Blob([doc.data], { type: "image/jpeg" });
}

// =================================================================
// EXEMPLO 3: Scan de Múltiplas Páginas (Feeder)
// =================================================================
export async function scanMultiplePages(
    scannerPort: number,
    maxPages: number = 50,
    onPageScanned?: (pageNum: number, blob: Blob) => void
): Promise<Blob[]> {
    const scanner = new Scanner({ ip: '127.0.0.1', port: scannerPort });

    // Criar job
    const jobUrl = await scanner.ScanJobs({ Resolution: 300 });
    const jobId = jobUrl.split('/').pop()!;

    const pages: Blob[] = [];
    let pageNum = 1;

    while (pageNum <= maxPages) {
        try {
            console.log(`Aguardando página ${pageNum}...`);
            
            // Aguardar página estar pronta
            const ready = await scanner.WaitForDocumentReady(jobId, 30, 500);
            
            if (!ready) {
                console.log('Timeout aguardando página');
                break;
            }

            // Obter documento
            const doc = await scanner.NextDocument(jobId);
            const blob = new Blob([doc.data], { type: "image/jpeg" });
            
            pages.push(blob);
            console.log(`Página ${pageNum} escaneada`);
            
            if (onPageScanned) {
                onPageScanned(pageNum, blob);
            }
            
            pageNum++;
            
        } catch (err: any) {
            // 404 = sem mais documentos no feeder
            if (err.response?.status === 404) {
                console.log('Fim do feeder');
                break;
            }
            throw err;
        }
    }

    return pages;
}

// =================================================================
// EXEMPLO 4: Verificar Status do Scanner
// =================================================================
export async function getScannerStatus(scannerPort: number): Promise<any> {
    const scanner = new Scanner({ ip: '127.0.0.1', port: scannerPort });
    return await scanner.ScannerStatus();
}

// =================================================================
// EXEMPLO 5: Benchmark de Métodos
// =================================================================
export async function benchmarkScanMethods(scannerPort: number): Promise<void> {
    const scanner = new Scanner({ ip: '127.0.0.1', port: scannerPort });

    console.log('=== BENCHMARK DE VELOCIDADE ===\n');

    // Teste 1: Método Otimizado
    console.log('Testando método otimizado...');
    const jobUrl1 = await scanner.ScanJobs({ Resolution: 300 });
    const jobId1 = jobUrl1.split('/').pop()!;
    
    console.time('Método Otimizado');
    await scanner.GetNextDocumentOptimized(jobId1);
    console.timeEnd('Método Otimizado');

    // Aguardar um pouco entre scans
    await new Promise(resolve => setTimeout(resolve, 3000));

    // Teste 2: Método Padrão (NextDocument direto)
    console.log('\nTestando método padrão...');
    const jobUrl2 = await scanner.ScanJobs({ Resolution: 300 });
    const jobId2 = jobUrl2.split('/').pop()!;
    
    console.time('Método Padrão');
    await scanner.NextDocument(jobId2);
    console.timeEnd('Método Padrão');

    console.log('\n=== FIM DO BENCHMARK ===');
}

// =================================================================
// EXEMPLO 6: Scan com Retry Inteligente
// =================================================================
export async function scanWithRetry(
    scannerPort: number,
    maxRetries: number = 3
): Promise<Blob> {
    const scanner = new Scanner({ ip: '127.0.0.1', port: scannerPort });

    for (let attempt = 1; attempt <= maxRetries; attempt++) {
        try {
            console.log(`Tentativa ${attempt} de ${maxRetries}`);
            
            const jobUrl = await scanner.ScanJobs({ Resolution: 300 });
            const jobId = jobUrl.split('/').pop()!;
            
            const doc = await scanner.GetNextDocumentOptimized(jobId);
            
            return new Blob([doc.data], { type: "image/jpeg" });
            
        } catch (err: any) {
            console.error(`Falha na tentativa ${attempt}:`, err.message);
            
            if (attempt === maxRetries) {
                throw new Error(`Falhou após ${maxRetries} tentativas`);
            }
            
            // Aguardar antes de retry
            await new Promise(resolve => setTimeout(resolve, 2000));
        }
    }

    throw new Error('Unreachable');
}
