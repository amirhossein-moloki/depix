export interface RawMedusaVariant {
  id: string;
  sku?: string;
  title?: string;
  prices?: Array<{ amount: number; currency_code: string }>;
  inventory_quantity?: number;
  options?: Record<string, string>;
}

export interface RawMedusaProduct {
  id: string;
  title: string;
  handle: string;
  status?: string;
  variants?: RawMedusaVariant[];
  created_at?: string;
  updated_at?: string;
}

export class MedusaClient {
  private baseUrl: string;
  private apiKey?: string;

  constructor(baseUrl?: string, apiKey?: string) {
    this.baseUrl = baseUrl || process.env.MEDUSA_INTERNAL_URL || 'http://medusa:9000';
    this.apiKey = apiKey || process.env.MEDUSA_INTERNAL_API_KEY;
  }

  async getProduct(productId: string): Promise<RawMedusaProduct | null> {
    try {
      const response = await fetch(`${this.baseUrl}/admin/products/${productId}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          ...(this.apiKey ? { 'x-medusa-access-token': this.apiKey } : {})
        }
      });

      if (response.status === 404) {
        return null;
      }

      if (!response.ok) {
        throw new Error(`Medusa API error: ${response.status} ${response.statusText}`);
      }

      const body = await response.json() as { product: RawMedusaProduct };
      return body.product;
    } catch (error) {
      if ((error as { code?: string }).code === 'ECONNREFUSED' || (error as Error).message.includes('fetch failed')) {
        throw new Error(`Medusa service unavailable at ${this.baseUrl}`);
      }
      throw error;
    }
  }
}
