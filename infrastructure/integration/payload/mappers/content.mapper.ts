import { CmsProductContent, CmsMediaRef, CmsSeoMetadata } from '../../contracts/product';
import { RawPayloadProductContent } from '../client';

export class PayloadContentMapper {
  static toCmsProductContent(raw: RawPayloadProductContent): CmsProductContent {
    const gallery: CmsMediaRef[] = (raw.gallery || []).map((m) => ({
      id: m.id,
      url: m.url,
      altText: m.altText,
      mimeType: m.mimeType
    }));

    const seo: CmsSeoMetadata | undefined = raw.seo
      ? {
          metaTitle: raw.seo.metaTitle,
          metaDescription: raw.seo.metaDescription,
          ogImage: raw.seo.ogImage,
          keywords: raw.seo.keywords
        }
      : undefined;

    return {
      id: raw.id,
      medusaProductId: raw.medusa_product_id,
      description: raw.description,
      richContentJson: raw.rich_content_json,
      marketingBadge: raw.marketing_badge,
      specifications: raw.specifications,
      gallery,
      seo,
      status: raw.status || 'active',
      updatedAt: raw.updatedAt
    };
  }

  static toRawPayloadContent(content: CmsProductContent): RawPayloadProductContent {
    return {
      id: content.id,
      medusa_product_id: content.medusaProductId,
      description: content.description,
      rich_content_json: content.richContentJson,
      marketing_badge: content.marketingBadge,
      specifications: content.specifications,
      gallery: content.gallery.map((m) => ({
        id: m.id,
        url: m.url,
        altText: m.altText,
        mimeType: m.mimeType
      })),
      seo: content.seo
        ? {
            metaTitle: content.seo.metaTitle,
            metaDescription: content.seo.metaDescription,
            ogImage: content.seo.ogImage,
            keywords: content.seo.keywords
          }
        : undefined,
      status: content.status,
      updatedAt: content.updatedAt
    };
  }
}
