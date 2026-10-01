import { PayloadClient, RawPayloadProductContent } from '../client';
import { PayloadContentMapper } from '../mappers/content.mapper';
import { CmsProductContent } from '../../contracts/product';

export class PayloadService {
  constructor(private client: PayloadClient = new PayloadClient()) {}

  async fetchCmsContentByMedusaProductId(medusaProductId: string): Promise<CmsProductContent | null> {
    const raw: RawPayloadProductContent | null = await this.client.getContentByMedusaProductId(medusaProductId);
    if (!raw) return null;
    return PayloadContentMapper.toCmsProductContent(raw);
  }

  async syncOrUpdateCmsContent(
    medusaProductId: string,
    partialData: Partial<CmsProductContent>
  ): Promise<CmsProductContent> {
    const rawInput = partialData.medusaProductId
      ? PayloadContentMapper.toRawPayloadContent(partialData as CmsProductContent)
      : {
          medusa_product_id: medusaProductId,
          description: partialData.description,
          marketing_badge: partialData.marketingBadge,
          status: partialData.status || 'active'
        };

    const savedRaw = await this.client.upsertContentByMedusaProductId(medusaProductId, rawInput);
    return PayloadContentMapper.toCmsProductContent(savedRaw);
  }
}
