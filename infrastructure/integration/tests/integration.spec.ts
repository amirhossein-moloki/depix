import assert from 'assert';
import http from 'http';
import { createIntegrationRouter } from '../routes/integration.routes';
import { MedusaService } from '../medusa/services/medusa.service';
import { PayloadService } from '../payload/services/payload.service';
import { SyncService } from '../sync/sync.service';
import { WebhookHandler } from '../webhooks/webhook.handler';
import { RawMedusaProduct } from '../medusa/client';
import { RawPayloadProductContent } from '../payload/client';

class MockMedusaClient {
  public products: Map<string, RawMedusaProduct> = new Map();

  async getProduct(productId: string): Promise<RawMedusaProduct | null> {
    return this.products.get(productId) || null;
  }
}

class MockPayloadClient {
  public contents: Map<string, RawPayloadProductContent> = new Map();

  async getContentByMedusaProductId(medusaProductId: string): Promise<RawPayloadProductContent | null> {
    for (const content of this.contents.values()) {
      if (content.medusa_product_id === medusaProductId) {
        return content;
      }
    }
    return null;
  }

  async upsertContentByMedusaProductId(
    medusaProductId: string,
    contentData: Partial<RawPayloadProductContent>
  ): Promise<RawPayloadProductContent> {
    const existing = await this.getContentByMedusaProductId(medusaProductId);
    const id = existing?.id || `cms_content_${Date.now()}`;
    const record: RawPayloadProductContent = {
      id,
      medusa_product_id: medusaProductId,
      description: contentData.description || existing?.description,
      status: contentData.status || existing?.status || 'active',
      updatedAt: new Date().toISOString()
    };
    this.contents.set(id, record);
    return record;
  }
}

async function runTests() {
  console.log('--- Starting Integration Layer Tests ---');

  const mockMedusaClient = new MockMedusaClient();
  const mockPayloadClient = new MockPayloadClient();

  const medusaService = new MedusaService(mockMedusaClient as any);
  const payloadService = new PayloadService(mockPayloadClient as any);
  const syncService = new SyncService(medusaService, payloadService);
  const webhookHandler = new WebhookHandler(syncService);

  // 1. Product Creation Sync
  mockMedusaClient.products.set('prod_100', {
    id: 'prod_100',
    title: 'T-Shirt',
    handle: 't-shirt',
    status: 'published',
    variants: [{ id: 'var_1', sku: 'TS-001', prices: [{ amount: 2500, currency_code: 'usd' }], inventory_quantity: 10 }]
  });

  const syncResult1 = await syncService.syncProductById('prod_100', 'event_001');
  assert.strictEqual(syncResult1.success, true, 'Test 1 Failed: syncResult1.success should be true');
  assert.strictEqual(syncResult1.operation, 'create', 'Test 1 Failed: operation should be create');
  assert.strictEqual(syncResult1.medusaProductId, 'prod_100', 'Test 1 Failed: medusaProductId mismatch');

  const createdContent = await payloadService.fetchCmsContentByMedusaProductId('prod_100');
  assert.notStrictEqual(createdContent, null, 'Test 1 Failed: createdContent should not be null');
  assert.strictEqual(createdContent?.medusaProductId, 'prod_100');
  assert.strictEqual(createdContent?.status, 'active');
  console.log('✓ 1. Product Creation Sync Passed');

  // 2. Product Update Sync
  mockMedusaClient.products.set('prod_200', {
    id: 'prod_200',
    title: 'Updated Hoodie',
    handle: 'hoodie',
    status: 'published'
  });

  mockPayloadClient.contents.set('cms_200', {
    id: 'cms_200',
    medusa_product_id: 'prod_200',
    description: 'Old Description',
    status: 'active'
  });

  const syncResult2 = await syncService.syncProductById('prod_200', 'event_002');
  assert.strictEqual(syncResult2.success, true);
  assert.strictEqual(syncResult2.operation, 'update');

  const updatedContent = await payloadService.fetchCmsContentByMedusaProductId('prod_200');
  assert.strictEqual(updatedContent?.id, 'cms_200');
  console.log('✓ 2. Product Update Sync Passed');

  // 3. Idempotency Check
  mockMedusaClient.products.set('prod_300', {
    id: 'prod_300',
    title: 'Idempotency Item',
    handle: 'idempotency-item',
    status: 'published'
  });

  const firstRun = await syncService.syncProductById('prod_300', 'event_dup_1');
  assert.strictEqual(firstRun.operation, 'create');

  const secondRun = await syncService.syncProductById('prod_300', 'event_dup_1');
  assert.strictEqual(secondRun.operation, 'noop');
  assert.strictEqual(secondRun.success, true);
  console.log('✓ 3. Idempotency Check Passed');

  // 4. Failure Handling / Degraded Mode
  const syncResult4 = await syncService.syncProductById('prod_nonexistent');
  assert.strictEqual(syncResult4.success, false);
  assert.strictEqual(syncResult4.operation, 'noop');
  assert.strictEqual(syncResult4.retryable, false);
  assert.strictEqual(syncResult4.errors?.[0], 'Product prod_nonexistent not found in Medusa');
  console.log('✓ 4. Failure Handling / Degraded Mode Passed');

  // 5. Webhook Event Processing
  mockMedusaClient.products.set('prod_500', {
    id: 'prod_500',
    title: 'Webhook Product',
    handle: 'webhook-product',
    status: 'published'
  });

  const webhookResult = await webhookHandler.processWebhookEvent({
    eventId: 'evt_wh_01',
    eventType: 'commerce.product.created',
    source: 'medusa',
    timestamp: new Date().toISOString(),
    data: { id: 'prod_500' }
  });
  assert.strictEqual(webhookResult.success, true);
  assert.strictEqual(webhookResult.operation, 'create');
  console.log('✓ 5. Webhook Event Processing Passed');

  console.log('--- All Integration Layer Tests Passed Successfully ---');
}

runTests().catch((err) => {
  console.error('Integration Test Failure:', err);
  process.exit(1);
});
