# GenerateFromUrl Performance Optimization

## Problem

`GenerateFromUrl` is slow on first render (cache miss). Takes 30-120s for a typical category of 5-20 articles.

## Bottlenecks (ranked)

1. **Image downloads**: Every `AddImage*` method creates a fresh `new WebClient()` and downloads images one-at-a-time during PDF rendering. 10-50 images × 0.5-2s each = 5-100s.
2. **New HttpClient per API call**: `GetArticleByUrl`, `GetCategoryArticles`, `GetArticleDetail` create new `HttpClient` per invocation — socket overhead under load.
3. **No image cache**: Every cache-miss PDF generation re-downloads all images from scratch.

## Approach: Image Cache + Async Pipeline

### 1. Shared image cache resolver

Static helper in `BusinessPdfController`:

```
ImageCacheDir: ~/App_Data/PdfCache/images/
Cache key: SHA256(url) → "{hash}{ext}"

ResolveImageAsync(url):
  hash = SHA256(url)
  cachePath = CacheDir / "{hash}{ext}"
  if File.Exists(cachePath) → return cachePath
  download via _sharedHttpClient → save → return cachePath
```

### 2. Replace all `new WebClient()` with cache resolver

7 sites consolidated into single method:

| Location | Method |
|----------|--------|
| `AddImageToParaSection` | Main image handler |
| `AddImageToParagraph` | Table cell images |
| `ProcessTableCellContent` | Table cell images (alt path) |
| Other WebClient usages | Various image paths |
| Figure/img in `ProcessInlineElements` | Inline images |

Each becomes: `await ResolveImageAsync(url)` → `paragraph.AddImage(localPath)`.

### 3. Fix API HttpClients

Replace `new HttpClient()` in `GetArticleByUrl`, `GetCategoryArticles` with existing `_sharedHttpClient`.

### 4. Pre-fetch images before PDF render

In `GenerateFromUrl`, after assembling HTML and before calling `GenerateBusinessPdf`:

- Extract all image URLs from HTML (Regex or HtmlAgilityPack)
- Call `ResolveImageAsync` in parallel (batch of 5-10)
- When PDF rendering starts, all images are locally cached — zero wait

### 5. Warm cache for known images

`ponytail:` Future improvement: LRU eviction for image cache. Not needed yet — disk is cheap.

### Code changes

- Single file: `Controllers/BusinessPdfController.cs`
- ~80 lines added, ~30 removed
- Mark `AddImage*` methods with `ponytail:` comments for upgrade path

## Success Criteria

- First render (cache miss): -60-80% (from 60-120s to 15-25s)
- Subsequent renders of same content: instant (already cached by existing PDF cache)
- Force refresh with unchanged images: improved (API re-fetched, images from cache)
- No functional change in PDF output
