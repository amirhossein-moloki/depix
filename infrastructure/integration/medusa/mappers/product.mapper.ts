import { CommerceProduct, ProductVariant } from '../../contracts/product';
import { RawMedusaProduct, RawMedusaVariant } from '../client';

export class MedusaProductMapper {
  static toCommerceProduct(raw: RawMedusaProduct): CommerceProduct {
    const variants: ProductVariant[] = (raw.variants || []).map((v: RawMedusaVariant) => {
      const primaryPrice = v.prices && v.prices.length > 0 ? v.prices[0] : { amount: 0, currency_code: 'usd' };
      return {
        id: v.id,
        sku: v.sku || v.id,
        title: v.title || 'Default Variant',
        price: primaryPrice.amount,
        currency: primaryPrice.currency_code,
        inventoryQuantity: v.inventory_quantity || 0,
        options: v.options
      };
    });

    const firstVariant = variants[0];
    const basePrice = firstVariant ? firstVariant.price : 0;
    const currency = firstVariant ? firstVariant.currency : 'usd';
    const totalInventory = variants.reduce((acc, v) => acc + v.inventoryQuantity, 0);

    let status: 'draft' | 'proposed' | 'published' | 'rejected' = 'published';
    if (raw.status === 'draft') status = 'draft';
    if (raw.status === 'rejected') status = 'rejected';

    return {
      id: raw.id,
      title: raw.title,
      handle: raw.handle,
      status,
      variants,
      basePrice,
      currency,
      totalInventory,
      createdAt: raw.created_at,
      updatedAt: raw.updated_at
    };
  }
}
