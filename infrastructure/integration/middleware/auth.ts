import { Request, Response, NextFunction } from 'express';

export function internalAuthMiddleware(req: Request, res: Response, next: NextFunction): void {
  const isProduction = process.env.NODE_ENV === 'production';
  const expectedApiKey = process.env.INTEGRATION_INTERNAL_API_KEY;

  if (isProduction && !expectedApiKey) {
    res.status(500).json({
      success: false,
      message: 'Server Error: INTEGRATION_INTERNAL_API_KEY is not configured in production environment',
      data: null,
      errors: [
        {
          code: 'SERVER_CONFIGURATION_ERROR',
          field: 'INTEGRATION_INTERNAL_API_KEY',
          message: 'INTEGRATION_INTERNAL_API_KEY environment variable is required in production'
        }
      ],
      traceId: req.header('x-trace-id') || 'untraced'
    });
    return;
  }

  const effectiveApiKey = expectedApiKey || 'default_integration_secret_key';
  const clientApiKey = req.header('X-Internal-API-Key') || req.header('x-internal-api-key');

  if (!clientApiKey || clientApiKey !== effectiveApiKey) {
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
