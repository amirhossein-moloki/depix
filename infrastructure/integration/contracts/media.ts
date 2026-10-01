export interface MediaAssetContract {
  id: string;
  payloadMediaId: string;
  filename: string;
  url: string;
  mimeType: string;
  filesize?: number;
  width?: number;
  height?: number;
  altText?: string;
  createdAt?: string;
}
