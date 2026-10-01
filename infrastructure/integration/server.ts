import express, { Express } from 'express';
import { createIntegrationRouter } from './routes/integration.routes';

export function createIntegrationApp(): Express {
  const app = express();
  app.use(express.json());

  app.get('/health', (_req, res) => {
    res.status(200).json({ status: 'Healthy', service: 'Depix Integration Layer', timestamp: new Date().toISOString() });
  });

  app.use('/api/v1/integration', createIntegrationRouter());

  return app;
}

if (require.main === module) {
  const port = process.env.PORT || 3001;
  const app = createIntegrationApp();
  app.listen(port, () => {
    console.log(`Integration Layer service running on port ${port}`);
  });
}
