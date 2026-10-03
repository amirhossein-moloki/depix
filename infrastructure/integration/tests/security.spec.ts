import assert from 'assert';
import http from 'http';
import express from 'express';
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

function makeRequest(
  port: number,
  path: string,
  method: string,
  headers: Record<string, string> = {},
  body?: any
): Promise<{ statusCode: number; body: any }> {
  return new Promise((resolve, reject) => {
    const payload = body ? JSON.stringify(body) : undefined;
    const reqHeaders: Record<string, string> = {
      ...headers
    };
    if (payload) {
      reqHeaders['Content-Type'] = 'application/json';
      reqHeaders['Content-Length'] = Buffer.byteLength(payload).toString();
    }

    const req = http.request(
      {
        hostname: '127.0.0.1',
        port,
        path,
        method,
        headers: reqHeaders
      },
      (res) => {
        let responseData = '';
        res.on('data', (chunk) => {
          responseData += chunk;
        });
        res.on('end', () => {
          let parsed: any = null;
          try {
            parsed = JSON.parse(responseData);
          } catch {
            parsed = responseData;
          }
          resolve({ statusCode: res.statusCode || 500, body: parsed });
        });
      }
    );

    req.on('error', (err) => reject(err));
    if (payload) {
      req.write(payload);
    }
    req.end();
  });
}

async function runSecurityTests() {
  console.log('--- Starting Integration Security & Auth Tests ---');

  const mockMedusaClient = new MockMedusaClient();
  const mockPayloadClient = new MockPayloadClient();

  mockMedusaClient.products.set('prod_sec_01', {
    id: 'prod_sec_01',
    title: 'Security Test Product',
    handle: 'sec-product',
    status: 'published'
  });

  const medusaService = new MedusaService(mockMedusaClient as any);
  const payloadService = new PayloadService(mockPayloadClient as any);
  const syncService = new SyncService(medusaService, payloadService);
  const webhookHandler = new WebhookHandler(syncService);

  const app = express();
  app.use(express.json());
  app.use('/api/v1/integration', createIntegrationRouter(medusaService, payloadService, syncService, webhookHandler));

  const TEST_PORT = 3099;
  const TEST_API_KEY = 'test_integration_internal_api_key_value';
  process.env.INTEGRATION_INTERNAL_API_KEY = TEST_API_KEY;

  const server = app.listen(TEST_PORT);

  try {
    // 1. Public Endpoint Access (No key required)
    const publicRes = await makeRequest(TEST_PORT, '/api/v1/integration/products/prod_sec_01', 'GET');
    assert.strictEqual(publicRes.statusCode, 200, 'Public endpoint should return 200 OK without API key');
    assert.strictEqual(publicRes.body.success, true);
    console.log('✓ 1. Public Read Endpoint succeeds without authentication');

    // 2. Protected Sync Endpoint - Missing API Key
    const missingKeyRes = await makeRequest(TEST_PORT, '/api/v1/integration/products/prod_sec_01/sync', 'POST');
    assert.strictEqual(missingKeyRes.statusCode, 401, 'Sync endpoint should reject missing API key with 401');
    assert.strictEqual(missingKeyRes.body.success, false);
    assert.strictEqual(missingKeyRes.body.errors[0].code, 'UNAUTHORIZED');
    console.log('✓ 2. Protected Endpoint rejects request missing service key (401 Unauthorized)');

    // 3. Protected Sync Endpoint - Invalid API Key
    const invalidKeyRes = await makeRequest(
      TEST_PORT,
      '/api/v1/integration/products/prod_sec_01/sync',
      'POST',
      { 'X-Internal-API-Key': 'invalid_key_value' }
    );
    assert.strictEqual(invalidKeyRes.statusCode, 401, 'Sync endpoint should reject invalid API key with 401');
    assert.strictEqual(invalidKeyRes.body.success, false);
    console.log('✓ 3. Protected Endpoint rejects request with invalid service key (401 Unauthorized)');

    // 4. Protected Sync Endpoint - Valid API Key
    const validKeyRes = await makeRequest(
      TEST_PORT,
      '/api/v1/integration/products/prod_sec_01/sync',
      'POST',
      { 'X-Internal-API-Key': TEST_API_KEY }
    );
    assert.strictEqual(validKeyRes.statusCode, 200, 'Sync endpoint should allow valid API key with 200 OK');
    assert.strictEqual(validKeyRes.body.success, true);
    console.log('✓ 4. Protected Sync Endpoint succeeds with valid service key');

    // 5. Protected Webhook Receiver - Missing API Key
    const missingWebhookRes = await makeRequest(
      TEST_PORT,
      '/api/v1/integration/webhooks',
      'POST',
      {},
      { eventId: 'evt_sec_01', eventType: 'commerce.product.created', source: 'medusa' }
    );
    assert.strictEqual(missingWebhookRes.statusCode, 401, 'Webhook endpoint should reject missing API key');
    console.log('✓ 5. Protected Webhook Receiver rejects missing service key (401 Unauthorized)');

    // 6. Protected Webhook Receiver - Valid API Key
    const validWebhookRes = await makeRequest(
      TEST_PORT,
      '/api/v1/integration/webhooks',
      'POST',
      { 'X-Internal-API-Key': TEST_API_KEY },
      { eventId: 'evt_sec_02', eventType: 'commerce.product.created', source: 'medusa', data: { id: 'prod_sec_01' } }
    );
    assert.strictEqual(validWebhookRes.statusCode, 200, 'Webhook endpoint should succeed with valid API key');
    assert.strictEqual(validWebhookRes.body.success, true);
    console.log('✓ 6. Protected Webhook Receiver succeeds with valid service key');

    console.log('--- All Integration Security & Auth Tests Passed Successfully ---');
  } finally {
    server.close();
  }
}

runSecurityTests().catch((err) => {
  console.error('Security Test Failure:', err);
  process.exit(1);
});
