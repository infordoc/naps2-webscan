import { getScanSettingConfig, getScannerBrightness } from './getCapabilities'
import { getScanSetting } from './serializeScanSetting'

import request from '../utils/request'
/**
 * 
 * @param { Object } opts 
 * @param { string } opts.ip 
 * @param { number} opts.port
 * @param { string} opts.rs
 */

interface IScannerConstructor {
	ip: string;
	version?: number;
	rs?: string;
	port?: number;
}
class Scanner {
	private ip: string;
	private version: number = 2.1;
	private rs: string = 'eSCL';
	private prototype = 'http://'
	private port: number = 8080

	constructor(opts: IScannerConstructor) {
		this.ip = opts.ip;
		this.port = opts.port || 8080;
		this.version = opts.version || 2.0;
		this.rs = opts.rs || 'eSCL'
	}

	private execute<T>(path: string, opts?: any): Promise<T> {
		let params = { url: '' }
		if (opts) {
			params = { ...opts }
		}

		params.url = `${this.prototype}${this.ip}:${this.port}/${this.rs}/${path}`
		return request(params)
	}

	async ScannerCapabilities(): Promise<ICapabilitiesCallback> {
		let data = {
			method: 'GET'
		}
		try {
			let res: ICapabilities = await this.execute('ScannerCapabilities', data)
			return {
				capabilities: res,
				scansetting: getScanSettingConfig(res),
				BrightnessSupport: getScannerBrightness(res)
			}
		} catch (error) {
			throw error
		}

	}


	async ScanJobs(params: IScanSettingParams): Promise<string> {
		let data = {
			method: 'POST',
			data: getScanSetting({ ...params, Version: this.version })
		}
		let res: any = await this.execute('ScanJobs', data)
		return res.headers.location
	}

	async ScannerStatus(): Promise<Record<string, any>> {
		try {
			let res: Record<string, any> = await this.execute('ScannerStatus')
			return res['scan:ScannerStatus']
		} catch (error) {
			throw error
		}
	}

	NextDocument(jobId: string): Promise<any> {
		return this.execute(`ScanJobs/${jobId}/NextDocument`, { responseType: 'arraybuffer' }).then(res => {
			return res
		}, err => {
			if (err.response && err.response.status === 503) {
				return new Promise(resolve => setTimeout(resolve, 2000))
					.then(_ => this.NextDocument(jobId));
			}
			return Promise.reject(err);
		})
	}

	ScanImageInfo(jobId: string): Promise<any> {
		return this.execute(`ScanJobs/${jobId}/ScanImageInfo`).then(res => {
			return res
		})
	}

	// Verifica o status de um job específico
	GetJobStatus(jobId: string): Promise<any> {
		return this.execute(`ScanJobs/${jobId}`).then(res => {
			return res
		})
	}

	// Aguarda até que o documento esteja pronto de forma otimizada
	async WaitForDocumentReady(jobId: string, maxAttempts: number = 30, delayMs: number = 500): Promise<boolean> {
		for (let i = 0; i < maxAttempts; i++) {
			try {
				const imageInfo = await this.ScanImageInfo(jobId);
				// Se conseguiu obter info da imagem, está pronto
				if (imageInfo) {
					return true;
				}
			} catch (err: any) {
				// 404 = ainda não pronto, 409 = scanning em progresso
				if (err.response?.status === 404 || err.response?.status === 409) {
					await new Promise(resolve => setTimeout(resolve, delayMs));
					continue;
				}
				throw err;
			}
		}
		return false;
	}

	// Método otimizado para obter documento com polling inteligente
	async GetNextDocumentOptimized(jobId: string): Promise<any> {
		// Aguarda o documento ficar pronto
		const ready = await this.WaitForDocumentReady(jobId);
		
		if (!ready) {
			throw new Error('Timeout aguardando documento');
		}

		// Agora pega o documento
		return this.NextDocument(jobId);
	}
}


export default Scanner;
