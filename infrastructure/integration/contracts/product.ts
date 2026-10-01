export interface ProductVariant {
  id: string;
  sku: string;
  title: string;
  price: number;
  currency: string;
  inventoryQuantity: number;
  options?: Record<string, string>;
}

export interface CommerceProduct {
  id: string;
  title: string;
  handle: string;
  status: 'draft' | 'proposed' | 'published' | 'rejected';
  variants: ProductVariant[];
  basePrice: number;
  currency: string;
  totalInventory: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface CmsMediaRef {
  id: string;
  url: string;
  altText?: string;
  mimeType?: string;
}

export interface CmsSeoMetadata {
  metaTitle?: string;
  metaDescription?: string;
  ogImage?: string;
  keywords?: string[];
}

export interface CmsProductContent {
  id?: string;
  medusaProductId: string;
  description?: string;
  richContentJson?: Record<string, unknown> | string;
  marketingBadge?: string;
  specifications?: Record<string, string>;
  gallery: CmsMediaRef[];
  seo?: CmsSeoMetadata;
  status: 'active' | 'draft' | 'inactive' | 'orphaned';
  updatedAt?: string;
}

export interface MergedProductResponse {
  productId: string;
  commerce: CommerceProduct;
  cmsContent: CmsProductContent | null;
  synchronizedAt: string;
}
