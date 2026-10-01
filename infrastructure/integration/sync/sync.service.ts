import { MedusaService } from '../medusa/services/medusa.service';
import { PayloadService } from '../payload/services/payload.service';
import { SyncOperationResult } from '../contracts';

export class SyncService {
  private processedEvents: Set<string> = new Set();

  constructor(
    private medusaService: MedusaService = new MedusaService(),
    private payloadService: PayloadService = new PayloadService()
  ) {}

  async syncProductById(productId: string, eventId?: string): Promise<SyncOperationResult> {
    const timestamp = new Date().toISOString();

    if (eventId && this.processedEvents.has(eventId)) {
      return {
        success: true,
        operation: 'noop',
        medusaProductId: productId,
        retryable: false,
        timestamp
      };
    }

    try {
      const commerceProduct = await this.medusaService.fetchCommerceProduct(productId);
      if (!commerceProduct) {
        return {
          success: false,
          operation: 'noop',
          medusaProductId: productId,
          errors: [`Product ${productId} not found in Medusa`],
          retryable: false,
          timestamp
        };
      }

      const existingContent = await this.payloadService.fetchCmsContentByMedusaProductId(productId);

      if (existingContent) {
        const updatedContent = await this.executeWithRetry(() =>
          this.payloadService.syncOrUpdateCmsContent(productId, {
            ...existingContent,
            description: existingContent.description || `Commerce product ${commerceProduct.title}`,
            status: commerceProduct.status === 'published' ? 'active' : 'draft'
          })
        );

        if (eventId) this.processedEvents.add(eventId);

        return {
          success: true,
          operation: 'update',
          medusaProductId: productId,
          cmsContentId: updatedContent.id,
          retryable: false,
          timestamp
        };
      } else {
        const newContent = await this.executeWithRetry(() =>
          this.payloadService.syncOrUpdateCmsContent(productId, {
            medusaProductId: productId,
            description: `CMS Content stub for Medusa product ${commerceProduct.title}`,
            gallery: [],
            status: 'active'
          })
        );

        if (eventId) this.processedEvents.add(eventId);

        return {
          success: true,
          operation: 'create',
          medusaProductId: productId,
          cmsContentId: newContent.id,
          retryable: false,
          timestamp
        };
      }
    } catch (error) {
      const errorMessage = (error as Error).message || 'Unknown synchronization error';
      console.error(`[Integration Sync Error] Product: ${productId}, Error: ${errorMessage}`);

      return {
        success: false,
        operation: 'noop',
        medusaProductId: productId,
        errors: [errorMessage],
        retryable: true,
        timestamp
      };
    }
  }

  async handleProductDeleted(productId: string, eventId?: string): Promise<SyncOperationResult> {
    const timestamp = new Date().toISOString();

    if (eventId && this.processedEvents.has(eventId)) {
      return {
        success: true,
        operation: 'noop',
        medusaProductId: productId,
        retryable: false,
        timestamp
      };
    }

    try {
      const existingContent = await this.payloadService.fetchCmsContentByMedusaProductId(productId);
      if (!existingContent) {
        return {
          success: true,
          operation: 'noop',
          medusaProductId: productId,
          retryable: false,
          timestamp
        };
      }

      const deactivated = await this.executeWithRetry(() =>
        this.payloadService.syncOrUpdateCmsContent(productId, {
          ...existingContent,
          status: 'orphaned'
        })
      );

      if (eventId) this.processedEvents.add(eventId);

      return {
        success: true,
        operation: 'deactivate',
        medusaProductId: productId,
        cmsContentId: deactivated.id,
        retryable: false,
        timestamp
      };
    } catch (error) {
      const errorMessage = (error as Error).message || 'Error executing deletion handling';
      return {
        success: false,
        operation: 'noop',
        medusaProductId: productId,
        errors: [errorMessage],
        retryable: true,
        timestamp
      };
    }
  }

  private async executeWithRetry<T>(fn: () => Promise<T>, retries = 3, delays = [100, 300, 500]): Promise<T> {
    let lastError: Error | unknown;
    for (let i = 0; i < retries; i++) {
      try {
        return await fn();
      } catch (err) {
        lastError = err;
        if (i < retries - 1) {
          await new Promise((resolve) => setTimeout(resolve, delays[i] || 500));
        }
      }
    }
    throw lastError;
  }
}
