# Medusa ↔ Payload Integration Layer

## Architecture Overview

The Depix Platform integration layer connects **Medusa Commerce Engine** and **Payload CMS** through a clean, decoupled interface boundary.
The databases of Medusa and Payload are strictly separated and do not communicate directly.

```text
                        Nginx API Gateway
                               │
                      Unified API Surface
                               │
       ┌───────────────────────┼───────────────────────┐
       │                       │                       │
/api/v1/commerce/         /api/v1/cms/          /api/v1/integration/
       │                       │                       │
 ┌─────▼─────┐           ┌─────▼─────┐           ┌─────▼─────┐
 │  Medusa   │           │  Payload  │           │Integration│
 │ Commerce  │           │    CMS    │           │   Layer   │
 └─────┬─────┘           └─────┬─────┘           └─────┬─────┘
       │                       │                       │
       └────────────── Event / Webhooks / REST ─────────┘
```

---

## Data Ownership Matrix

| Domain Field / Entity | Owner | Storage System | Notes |
| :--- | :--- | :--- | :--- |
| **Product ID / Identity** | Medusa | Medusa DB | Primary cross-system anchor |
| **SKU & Variant Identifiers**| Medusa | Medusa DB | Inventory tracking |
| **Prices & Currency** | Medusa | Medusa DB | Real-time pricing & discounts |
| **Inventory & Stock** | Medusa | Medusa DB | Stock allocations |
| **Cart & Checkout** | Medusa | Medusa DB | Shopping cart state |
| **Orders & Transactions** | Medusa | Medusa DB | Order fulfillment & payments |
| **Customer Commerce Data** | Medusa | Medusa DB | Customer purchase history |
| **Product Rich Content** | Payload | Payload DB | Extended markdown/blocks |
| **SEO Content & Metadata** | Payload | Payload DB | Title tags, OG descriptions |
| **CMS Media Assets** | Payload | Payload DB | High-res images, galleries |
| **Articles & Blog Content**| Payload | Payload DB | Brand narrative & content |
| **Pages & Marketing Copy** | Payload | Payload DB | Landing pages & dynamic layouts |
| **Authors & Editors** | Payload | Payload DB | Content creation workflows |
| **Site Settings** | Payload | Payload DB | Navigation menus, banner settings |

---

## Cross-System Identifier Strategy

To prevent coupling and allow independent product catalog changes (e.g., name or slug modifications):

1. **Primary Key**: Medusa `product.id` (e.g. `prod_01H8X...`).
2. **Payload Link**: Payload stores `medusa_product_id` as a indexed scalar string field in CMS product content documents.
3. **Immutability**: Product title, handle, and category changes in Medusa do NOT break CMS content references.

---

## Integration Contracts

All communication between Medusa, Payload, and the Integration Layer conforms to strongly-typed TypeScript contracts located in:
`infrastructure/integration/contracts/`

- **Product Contract**: Merged commerce specifications + CMS content payload.
- **Category Contract**: Navigation tree mapping.
- **Media Contract**: Standardized CDN / media references.
- **CMS Content Contract**: Extended CMS page and section attributes.

---

## Event-Driven Webhooks & Synchronization

### Events
1. `commerce.product.created`: Triggered when a product is created in Medusa -> Creates or links initial CMS content stub in Payload.
2. `commerce.product.updated`: Triggered on price, title, or status change -> Updates CMS metadata cache.
3. `commerce.product.deleted`: Soft-marks CMS content as inactive/orphaned (does NOT hard-delete CMS copy).
4. `cms.content.updated`: Triggered when marketing copy/media is published in Payload.

### Idempotency & Retry Strategy
- Webhooks pass unique event identifiers (`X-Event-ID`).
- The integration layer looks up existing CMS documents by `medusa_product_id` before performing updates or inserts.
- Retry strategy: Exponential backoff with maximum 3 retries (1s, 5s, 15s delays).
- Errors do not block primary Medusa/Payload user requests.

---

## API Routes & Interface Boundary

The integration layer exposes the following protected REST endpoints:

- `GET /api/v1/integration/products/:productId`: Returns merged product commerce details + CMS content.
- `POST /api/v1/integration/products/:productId/sync`: Triggers manual or event-driven catalog sync.
- `GET /api/v1/integration/products/:productId/content`: Fetches associated Payload CMS content by Medusa Product ID.

---

## Security & Service Authentication

- Service-to-service internal calls require `X-Internal-API-Key` header matching `INTEGRATION_INTERNAL_API_KEY`.
- Internal ports are isolated within Docker network `depix-network` and not exposed to public interfaces.
- Environment configurations are defined in `.env.example` without exposed secrets.

---

## Local Development & Docker Verification

1. Start full Docker stack:
   ```bash
   docker-compose up --build
   ```
2. Verify endpoints:
   - Unified Swagger UI: `http://localhost:8080/api/docs`
   - Gateway Health: `http://localhost:8080/api/v1/health`
   - Integration Product API: `http://localhost:8080/api/v1/integration/products/prod_sample`
