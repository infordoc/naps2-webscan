// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

import Scanner from '../lib/escl-sdk-ts/escl/scanner'

document.getElementById('scan-button').addEventListener('click', async e => {
    console.log('Starting scan attempt');

    const ip = '127.0.0.1';
    const port = 9880;
    const scanner = new Scanner({ip, port});

    try {
        // Query for scanner capabilities
        const capabilities = await scanner.ScannerCapabilities();
        console.log('scanner capabilities', capabilities)

        // Start a scan job
        const jobUrl = await scanner.ScanJobs({
            // Set scanning options
            Resolution: 300
        });
        console.log('job created', jobUrl);

        // Get the job ID
        const jobId = jobUrl.split('/').pop();

        console.log('Waiting for document...');

        // MÉTODO OTIMIZADO - Mais rápido (polling de 500ms no ScanImageInfo)
        // Use este para melhor performance
        const doc = await scanner.GetNextDocumentOptimized(jobId);
        
        // MÉTODO PADRÃO - Simples mas mais lento (retry de 2s no 503)
        // const doc = await scanner.NextDocument(jobId);
        
        console.log('scanned document', doc);

        // Turn the document JPEG into a blob and load it into the <img> element
        var blob = new Blob([doc.data], {type: "image/jpeg"});
        var imageUrl = window.URL.createObjectURL(blob);
        var img = document.querySelector("#preview") as HTMLImageElement;
        img.src = imageUrl;

    } catch (err) {
        console.log('scanner err', err.response)
    }
});