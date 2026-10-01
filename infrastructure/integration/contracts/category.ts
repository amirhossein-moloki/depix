export interface CategoryContract {
  id: string;
  medusaCategoryId?: string;
  name: string;
  handle: string;
  parentId?: string;
  description?: string;
  cmsContentId?: string;
  updatedAt?: string;
}
