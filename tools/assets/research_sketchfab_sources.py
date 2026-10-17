"""Find public source candidates for newly inventoried downloads; never grants clearance."""
import argparse, concurrent.futures, difflib, hashlib, json, re, urllib.parse, urllib.request
from datetime import datetime, timezone
from pathlib import Path

def normalized(text):
    return re.sub(r"[^a-z0-9]+", " ", text.lower()).strip()

def fetch(url):
    request = urllib.request.Request(url, headers={"User-Agent": "DiceFree-AssetCatalog/1.0"})
    with urllib.request.urlopen(request, timeout=20) as response:
        data = response.read(8 * 1024 * 1024 + 1)
    if len(data) > 8 * 1024 * 1024:
        raise ValueError("Public metadata exceeds 8 MiB limit")
    return json.loads(data)

def research(row):
    query = Path(row["file"]).stem.replace("-", " ")
    api = "https://api.sketchfab.com/v3/search?" + urllib.parse.urlencode(
        {"type": "models", "q": query, "count": 100})
    result = dict(file=row["file"], query=query, api=api, candidates=[])
    try:
        models = row.get("models", [])
        triangles = {m["triangles"] for m in models if m.get("triangles")}
        total = sum(m.get("triangles", 0) for m in models)
        if total:
            triangles.add(total)
        for item in fetch(api).get("results", []):
            title = normalized(item.get("name", ""))
            exact = title == normalized(query)
            similarity = difflib.SequenceMatcher(None, normalized(query), title).ratio()
            faces = item.get("faceCount")
            candidate = dict(
                name=item.get("name"), uid=item["uid"],
                url="https://sketchfab.com/3d-models/" + item["uid"],
                creator=item.get("user", {}).get("displayName"),
                username=item.get("user", {}).get("username"),
                license=item.get("license"), faceCount=faces,
                vertexCount=item.get("vertexCount"), exactTitle=exact,
                similarity=round(similarity, 4), triangleMatch=faces in triangles,
                isDownloadable=item.get("isDownloadable"))
            result["candidates"].append(candidate)
        result["candidates"].sort(key=lambda c: (c["exactTitle"] and c["triangleMatch"],
             c["triangleMatch"], c["exactTitle"], c["similarity"]), reverse=True)
        result["candidates"] = result["candidates"][:5]
        for candidate in result["candidates"][:2]:
            try:
                detail = fetch("https://api.sketchfab.com/v3/models/" + candidate["uid"])
                description = detail.get("description", "")
                candidate["detail"] = dict(
                    license=detail.get("license"), animationCount=detail.get("animationCount"),
                    publishedAt=detail.get("publishedAt"),
                    descriptionExcerpt=" ".join(description.split()[:18]),
                    descriptionSha256=hashlib.sha256(description.encode()).hexdigest())
            except Exception as error:
                candidate["detailError"] = str(error)
        matches = [c for c in result["candidates"] if c["triangleMatch"] and
                   (c["exactTitle"] or c["similarity"] >= .85)]
        result["sourceCandidate"] = matches[0] if len(matches) == 1 else None
        result["sourceMatchBasis"] = (
            "Matching title/search context and packaged triangle count; ownership not established"
            if result["sourceCandidate"] else "Exact source unresolved; candidates only")
    except Exception as error:
        result["error"] = str(error)
    print(json.dumps(dict(file=row["file"],
        matched=result.get("sourceCandidate", {}).get("name") if result.get("sourceCandidate") else None,
        candidates=len(result["candidates"]), error=result.get("error"))), flush=True)
    return result

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--inventory", required=True)
    parser.add_argument("--baseline", required=True)
    parser.add_argument("--output", required=True)
    args = parser.parse_args()
    inventory = json.loads(Path(args.inventory).read_text(encoding="utf8"))
    old = json.loads(Path(args.baseline).read_text(encoding="utf8"))
    known = {row["file"] for row in old["files"]}
    rows = [row for row in inventory["files"] if row["file"] not in known]
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as executor:
        results = list(executor.map(research, rows))
    output = dict(retrievedUtc=datetime.now(timezone.utc).isoformat(),
        method="Unauthenticated official public Sketchfab API; title/triangle source candidates, no ownership clearance",
        results=results)
    Path(args.output).write_text(json.dumps(output, indent=2, ensure_ascii=False), encoding="utf8")

if __name__ == "__main__":
    main()
