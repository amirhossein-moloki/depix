import { SyncService } from '../sync/sync.service';
import { WebhookEventPayload, SyncOperationResult } from '../contracts';

export class WebhookHandler {
  constructor(private syncService: SyncService = new SyncService()) {}

  async processWebhookEvent(payload: WebhookEventPayload): Promise<SyncOperationResult> {
    const { eventId, eventType, data } = payload;
    const productId = (data as { id?: string; productId?: string }).id || (data as { productId?: string }).productId;

    if (!productId) {
      return {
        success: false,
        operation: 'noop',
        medusaProductId: '',
        errors: ['Missing productId in webhook payload data'],
        retryable: false,
        timestamp: new Date().toISOString()
      };
    }

    switch (eventType) {
      case 'commerce.product.created':
      case 'commerce.product.updated':
      case 'product.created':
      case 'product.updated':
        return this.syncService.syncProductById(productId, eventId);

      case 'commerce.product.deleted':
      case 'product.deleted':
        return this.syncService.handleProductDeleted(productId, eventId);

      default:
        return {
          success: true,
          operation: 'noop',
          medusaProductId: productId,
          retryable: false,
          timestamp: new Date().toISOString()
        };
    }
  }
}
