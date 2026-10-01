export * from './product';
export * from './category';
export * from './media';

export interface WebhookEventPayload<T = unknown> {
  eventId: string;
  eventType: string;
  source: 'medusa' | 'payload' | 'integration';
  timestamp: string;
  data: T;
}

export interface SyncOperationResult {
  success: boolean;
  operation: 'create' | 'update' | 'deactivate' | 'noop';
  medusaProductId: string;
  cmsContentId?: string;
  errors?: string[];
  retryable: boolean;
  timestamp: string;
}
