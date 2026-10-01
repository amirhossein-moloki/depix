export interface RawPayloadProductContent {
  id?: string;
  medusa_product_id: string;
  description?: string;
  rich_content_json?: Record<string, unknown> | string;
  marketing_badge?: string;
  specifications?: Record<string, string>;
  gallery?: Array<{
    id: string;
    url: string;
    altText?: string;
    mimeType?: string;
  }>;
  seo?: {
    metaTitle?: string;
    metaDescription?: string;
    ogImage?: string;
    keywords?: string[];
  };
  status?: 'active' | 'draft' | 'inactive' | 'orphaned';
  updatedAt?: string;
}

export class PayloadClient {
  private baseUrl: string;
  private apiKey?: string;

  constructor(baseUrl?: string, apiKey?: string) {
    this.baseUrl = baseUrl || process.env.PAYLOAD_INTERNAL_URL || 'http://payload:3000';
    this.apiKey = apiKey || process.env.PAYLOAD_INTERNAL_API_KEY;
  }

  async getContentByMedusaProductId(medusaProductId: string): Promise<RawPayloadProductContent | null> {
    try {
      const response = await fetch(
        `${this.baseUrl}/api/product-contents?where[medusa_product_id][equals]=${encodeURIComponent(medusaProductId)}`,
        {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
            ...(this.apiKey ? { Authorization: `API-Key ${this.apiKey}` } : {})
          }
        }
      );

      if (response.status === 404) return null;
      if (!response.ok) {
        throw new Error(`Payload API error: ${response.status} ${response.statusText}`);
      }

      const body = await response.json() as { docs?: RawPayloadProductContent[] };
      if (body.docs && body.docs.length > 0) {
        return body.docs[0];
      }
      return null;
    } catch (error) {
      if ((error as { code?: string }).code === 'ECONNREFUSED' || (error as Error).message.includes('fetch failed')) {
        throw new Error(`Payload service unavailable at ${this.baseUrl}`);
      }
      throw error;
    }
  }

  async upsertContentByMedusaProductId(
    medusaProductId: string,
    contentData: Partial<RawPayloadProductContent>
  ): Promise<RawPayloadProductContent> {
    try {
      const existing = await this.getContentByMedusaProductId(medusaProductId);
      const payloadBody = {
        ...contentData,
        medusa_product_id: medusaProductId,
        status: contentData.status || 'active'
      };

      if (existing && existing.id) {
        const response = await fetch(`${this.baseUrl}/api/product-contents/${existing.id}`, {
          method: 'PATCH',
          headers: {
            'Content-Type': 'application/json',
            ...(this.apiKey ? { Authorization: `API-Key ${this.apiKey}` } : {})
          },
          body: JSON.stringify(payloadBody)
        });

        if (!response.ok) {
          throw new Error(`Payload API update error: ${response.status} ${response.statusText}`);
        }
        const body = await response.json() as { doc?: RawPayloadProductContent } | RawPayloadProductContent;
        return (body as { doc?: RawPayloadProductContent }).doc || (body as RawPayloadProductContent);
      } else {
        const response = await fetch(`${this.baseUrl}/api/product-contents`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            ...(this.apiKey ? { Authorization: `API-Key ${this.apiKey}` } : {})
          },
          body: JSON.stringify(payloadBody)
        });

        if (!response.ok) {
          throw new Error(`Payload API create error: ${response.status} ${response.statusText}`);
        }
        const body = await response.json() as { doc?: RawPayloadProductContent } | RawPayloadProductContent;
        return (body as { doc?: RawPayloadProductContent }).doc || (body as RawPayloadProductContent);
      }
    } catch (error) {
      if ((error as { code?: string }).code === 'ECONNREFUSED' || (error as Error).message.includes('fetch failed')) {
        throw new Error(`Payload service unavailable at ${this.baseUrl}`);
      }
      throw error;
    }
  }
}
