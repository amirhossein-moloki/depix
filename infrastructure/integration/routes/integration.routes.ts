import { Router, Request, Response } from 'express';
import { MedusaService } from '../medusa/services/medusa.service';
import { PayloadService } from '../payload/services/payload.service';
import { SyncService } from '../sync/sync.service';
import { WebhookHandler } from '../webhooks/webhook.handler';
import { internalAuthMiddleware } from '../middleware/auth';

export function createIntegrationRouter(
  medusaService = new MedusaService(),
  payloadService = new PayloadService(),
  syncService = new SyncService(medusaService, payloadService),
  webhookHandler = new WebhookHandler(syncService)
): Router {
  const router = Router();

  // Public/Internal Read Endpoint for merged product
  router.get('/products/:productId', async (req: Request, res: Response): Promise<void> => {
    const { productId } = req.params;
    try {
      const commerce = await medusaService.fetchCommerceProduct(productId);
      if (!commerce) {
        res.status(404).json({
          success: false,
          message: `Commerce product ${productId} not found`,
          data: null,
          errors: [{ code: 'NOT_FOUND', field: 'productId', message: 'Product not found in Medusa' }],
          traceId: req.header('x-trace-id') || 'untraced'
        });
        return;
      }

      const cmsContent = await payloadService.fetchCmsContentByMedusaProductId(productId);

      res.status(200).json({
        success: true,
        message: 'Merged product retrieved successfully',
        data: {
          productId,
          commerce,
          cmsContent,
          synchronizedAt: new Date().toISOString()
        },
        errors: [],
        traceId: req.header('x-trace-id') || 'untraced'
      });
    } catch (error) {
      res.status(502).json({
        success: false,
        message: 'Failed to retrieve merged product from upstream services',
        data: null,
        errors: [{ code: 'UPSTREAM_ERROR', message: (error as Error).message }],
        traceId: req.header('x-trace-id') || 'untraced'
      });
    }
  });

  // Public/Internal Endpoint for CMS content by Medusa Product ID
  router.get('/products/:productId/content', async (req: Request, res: Response): Promise<void> => {
    const { productId } = req.params;
    try {
      const cmsContent = await payloadService.fetchCmsContentByMedusaProductId(productId);
      if (!cmsContent) {
        res.status(404).json({
          success: false,
          message: `CMS Content for product ${productId} not found`,
          data: null,
          errors: [{ code: 'NOT_FOUND', field: 'productId', message: 'Content not found in Payload CMS' }],
          traceId: req.header('x-trace-id') || 'untraced'
        });
        return;
      }

      res.status(200).json({
        success: true,
        message: 'CMS product content retrieved successfully',
        data: cmsContent,
        errors: [],
        traceId: req.header('x-trace-id') || 'untraced'
      });
    } catch (error) {
      res.status(502).json({
        success: false,
        message: 'Failed to retrieve content from Payload CMS',
        data: null,
        errors: [{ code: 'UPSTREAM_ERROR', message: (error as Error).message }],
        traceId: req.header('x-trace-id') || 'untraced'
      });
    }
  });

  // Protected Internal Endpoint: Sync Trigger
  router.post('/products/:productId/sync', internalAuthMiddleware, async (req: Request, res: Response): Promise<void> => {
    const { productId } = req.params;
    const eventId = req.header('X-Event-ID') || req.header('x-event-id');

    try {
      const result = await syncService.syncProductById(productId, eventId || undefined);

      if (!result.success) {
        res.status(result.retryable ? 503 : 400).json({
          success: false,
          message: 'Product synchronization failed',
          data: result,
          errors: (result.errors || []).map((msg) => ({ code: 'SYNC_ERROR', message: msg })),
          traceId: req.header('x-trace-id') || 'untraced'
        });
        return;
      }

      res.status(200).json({
        success: true,
        message: 'Product synchronization executed successfully',
        data: result,
        errors: [],
        traceId: req.header('x-trace-id') || 'untraced'
      });
    } catch (error) {
      res.status(500).json({
        success: false,
        message: 'Internal error during product sync',
        data: null,
        errors: [{ code: 'INTERNAL_ERROR', message: (error as Error).message }],
        traceId: req.header('x-trace-id') || 'untraced'
      });
    }
  });

  // Protected Internal Endpoint: Webhook Receiver
  router.post('/webhooks', internalAuthMiddleware, async (req: Request, res: Response): Promise<void> => {
    try {
      const result = await webhookHandler.processWebhookEvent(req.body);

      res.status(result.success ? 200 : 400).json({
        success: result.success,
        message: result.success ? 'Webhook processed' : 'Webhook processing failed',
        data: result,
        errors: (result.errors || []).map((msg) => ({ code: 'WEBHOOK_ERROR', message: msg })),
        traceId: req.header('x-trace-id') || 'untraced'
      });
    } catch (error) {
      res.status(500).json({
        success: false,
        message: 'Unhandled error in webhook processor',
        data: null,
        errors: [{ code: 'INTERNAL_ERROR', message: (error as Error).message }],
        traceId: req.header('x-trace-id') || 'untraced'
      });
    }
  });

  return router;
}
