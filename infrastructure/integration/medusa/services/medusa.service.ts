import { MedusaClient, RawMedusaProduct } from '../client';
import { MedusaProductMapper } from '../mappers/product.mapper';
import { CommerceProduct } from '../../contracts/product';

export class MedusaService {
  constructor(private client: MedusaClient = new MedusaClient()) {}

  async fetchCommerceProduct(productId: string): Promise<CommerceProduct | null> {
    const raw: RawMedusaProduct | null = await this.client.getProduct(productId);
    if (!raw) return null;
    return MedusaProductMapper.toCommerceProduct(raw);
  }
}
