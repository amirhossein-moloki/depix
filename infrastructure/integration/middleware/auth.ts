import { Request, Response, NextFunction } from 'express';

export function internalAuthMiddleware(req: Request, res: Response, next: NextFunction): void {
  const expectedApiKey = process.env.INTEGRATION_INTERNAL_API_KEY || 'default_integration_secret_key';
  const clientApiKey = req.header('X-Internal-API-Key') || req.header('x-internal-api-key');

  if (!clientApiKey || clientApiKey !== expectedApiKey) {
    res.status(401).json({
      success: false,
      message: 'Unauthorized: Missing or invalid internal API key',
      data: null,
      errors: [
        {
          code: 'UNAUTHORIZED',
          field: 'X-Internal-API-Key',
          message: 'Valid X-Internal-API-Key header required for internal integration access'
        }
      ],
      traceId: req.header('x-trace-id') || 'untraced'
    });
    return;
  }

  next();
}
