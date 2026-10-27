// The server's own message instead of the generated client's generic one
export function errorText(e: unknown) {
  const err = e as { response?: string; message?: string };
  try {
    const body = JSON.parse(err.response ?? "");
    if (body?.message || body?.detail || body?.title) return body.message ?? body.detail ?? body.title;
  } catch {
    // not JSON
  }
  return err.message ?? String(e);
}

// Lookups owned by other modules (Catalog, Core, platform settings): small same-origin fetches
async function getJson<T>(url: string, body?: unknown): Promise<T> {
  const response = await fetch(url, {
    method: body ? "POST" : "GET",
    headers: { Accept: "application/json", ...(body ? { "Content-Type": "application/json" } : {}) },
    body: body ? JSON.stringify(body) : undefined,
  });
  if (!response.ok) throw new Error(`${response.status} ${url}`);
  return response.json() as Promise<T>;
}

export type Catalog = { id: string; name: string; isVirtual?: boolean };
export type ListEntry = { id: string; name?: string; code?: string; type?: string; imageUrl?: string };

export const searchCatalogs = () =>
  getJson<{ totalCount: number; results: Catalog[] }>("/api/catalog/catalogs/search", { take: 1000, sort: "name" });
export const getCurrencies = () => getJson<{ code: string; name?: string }[]>("/api/currencies");
export const getLanguages = () => getJson<string[]>("/api/platform/settings/values/VirtoCommerce.Core.General.Languages");
export const searchProducts = (catalogId: string, keyword: string | undefined, skip: number, take: number) =>
  getJson<{ totalCount: number; listEntries: ListEntry[] }>("/api/catalog/listentries", {
    catalogId,
    keyword,
    searchInChildren: true,
    searchInVariations: false,
    objectType: "CatalogProduct",
    responseGroup: "WithProducts",
    skip,
    take,
  });

export const percent = (value?: number) => {
  const n = Number(value ?? 0);
  return `${n % 1 === 0 ? n.toFixed(0) : n.toFixed(1)}%`;
};
