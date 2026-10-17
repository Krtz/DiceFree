"""Audit stated Sketchfab page licenses for the complete staged inventory."""
import argparse,collections,concurrent.futures,difflib,json,re,urllib.parse
from pathlib import Path
from datetime import datetime,timezone
from research_sketchfab_sources import fetch,normalized

def query_name(file):
    return re.sub(r"\s*\(\d+\)$","",Path(file).stem).replace("-"," ").replace("_"," ")

def compact(item):
    user=item.get("user",{})
    return dict(uid=item["uid"],title=item.get("name"),creator=user.get("displayName"),
        username=user.get("username"),url="https://sketchfab.com/3d-models/"+item["uid"],
        triangles=item.get("faceCount"),vertices=item.get("vertexCount"),
        downloadable=item.get("isDownloadable"),license=item.get("license"))

def select(row,items):
    query=normalized(query_name(row["file"]))
    tris={m["triangles"] for m in row.get("models",[]) if m.get("triangles")}
    verts={m.get("vertices",m.get("positionVertices")) for m in row.get("models",[])}
    matches=[]
    for item in items:
        title=normalized(item.get("title",""));ratio=difflib.SequenceMatcher(None,query,title).ratio()
        item["titleSimilarity"]=round(ratio,4);item["exactNormalizedTitle"]=title==query
        item["triangleMatch"]=item.get("triangles") in tris
        item["vertexMatch"]=item.get("vertices") in verts and item.get("vertices") is not None
        if item["triangleMatch"] and (ratio>=.70 or item["exactNormalizedTitle"]):
            matches.append(item)
    if matches:return matches,"title/filename context plus matching packaged triangle count"
    exact=[i for i in items if i["exactNormalizedTitle"]]
    if len(exact)==1:return exact,"unique exact filename/title match; geometry comparison unavailable or different"
    return [],"archive-to-page mapping unresolved"

def audit(row,prior,source=None):
    result=dict(file=row["file"],sha256=row["sha256"],queries=[],candidates=[],pages=[],matchBasis=None)
    if source is not None:
        try:
            if source["sha256"]!=row["sha256"]:raise ValueError("Verified source map does not match this archive hash")
            url="https://api.sketchfab.com/v3/models/"+source["uid"]
            raw=fetch(url);page=compact(raw)
            page.update(license=raw.get("license") or {},animationCount=raw.get("animationCount"))
            result.update(queries=[dict(url=url,resultCount=1)],pages=[page],
                listedLicense=page["license"],matchBasis=source["matchBasis"],
                status="page license recorded" if page["license"].get("url") else "page has no recorded license")
        except Exception as error:result.update(status="lookup error",error=str(error))
        print(json.dumps(dict(file=row["file"],status=result["status"],license=(result.get("listedLicense") or {}).get("label"))),flush=True)
        return result
    query=query_name(row["file"]);items={c["uid"]:dict(c) for c in prior}
    # Always search fresh. Generic titles receive bounded additional result pages.
    url="https://api.sketchfab.com/v3/search?"+urllib.parse.urlencode({"type":"models","q":query,"count":100})
    pages=15 if len(query.split())<=3 else 2
    try:
        for page in range(pages):
            response=fetch(url);batch=response.get("results",[])
            result["queries"].append(dict(url=url,resultCount=len(batch)))
            for item in batch:items[item["uid"]]=compact(item)
            matches,basis=select(row,list(items.values()))
            if matches:break
            url=response.get("next")
            if not url:break
        matches,basis=select(row,list(items.values()))
        # If no exact result, retry generic titles without 'model' or animation qualifier.
        if not matches and len(query.split())<=4:
            alternate=re.sub(r"\b(model|animated)\b","",query,flags=re.I).strip()
            if alternate!=query and alternate:
                url="https://api.sketchfab.com/v3/search?"+urllib.parse.urlencode({"type":"models","q":alternate,"count":100})
                for page in range(5):
                    response=fetch(url);batch=response.get("results",[])
                    result["queries"].append(dict(url=url,resultCount=len(batch)))
                    for item in batch:items[item["uid"]]=compact(item)
                    matches,basis=select(row,list(items.values()))
                    if matches:break
                    url=response.get("next")
                    if not url:break
        result["matchBasis"]=basis
        relevant=matches or sorted(items.values(),key=lambda i:(i.get("exactNormalizedTitle",False),i.get("titleSimilarity",0)),reverse=True)[:8]
        for item in relevant[:12]:
            detail=fetch("https://api.sketchfab.com/v3/models/"+item["uid"])
            item["license"]=detail.get("license") or {}
            item["animationCount"]=detail.get("animationCount")
            item["publishedAt"]=detail.get("publishedAt")
            item["downloadable"]=detail.get("isDownloadable")
            item["descriptionLinks"]=re.findall(r"https?://[^\s)<>]+",detail.get("description",""))
        if len(matches)>1:
            licensed=[i for i in matches if (i.get("license") or {}).get("url") and i.get("downloadable")]
            animations={m.get("animationStacks",m.get("animations")) for m in row.get("models",[])}
            animation_matches=[i for i in licensed if i.get("animationCount") in animations]
            if len(animation_matches)==1:
                matches=animation_matches;result["matchBasis"]+="; unique matching animation count and downloadable licensed page"
            elif len(licensed)==1:
                matches=licensed;result["matchBasis"]+="; only currently downloadable licensed match"
        result["pages"]=matches
        result["candidates"]=sorted(items.values(),key=lambda i:(i.get("triangleMatch",False),i.get("exactNormalizedTitle",False),i.get("titleSimilarity",0)),reverse=True)[:12]
        licenses={(i.get("license") or {}).get("url") for i in matches}
        licenses.discard(None)
        if len(matches)==1 and licenses:
            result["status"]="page license recorded";result["listedLicense"]=matches[0]["license"]
        elif len(matches)>1 and len(licenses)==1 and all((i.get("license") or {}).get("url") for i in matches):
            result["status"]="matching pages agree on license; uploader mapping ambiguous";result["listedLicense"]=matches[0]["license"]
        else:
            result["status"]="source mapping unresolved" if not matches else "page has no recorded license"
            result["listedLicense"]=None
    except Exception as error:
        result["error"]=str(error);result["status"]="lookup error"
    print(json.dumps(dict(file=row["file"],status=result["status"],license=(result.get("listedLicense") or {}).get("label"),
        pages=len(result["pages"]),searched=sum(q["resultCount"] for q in result["queries"]),error=result.get("error"))),flush=True)
    return result

def main():
    p=argparse.ArgumentParser();p.add_argument("--inventory",required=True);p.add_argument("--output",required=True)
    p.add_argument("--research",nargs="*",default=[])
    p.add_argument("--source-map")
    p.add_argument("--only",nargs="*")
    a=p.parse_args()
    inv=json.loads(Path(a.inventory).read_text(encoding="utf8"))
    prior={}
    for path in a.research:
        for result in json.loads(Path(path).read_text(encoding="utf8"))["results"]:
            prior[result["file"]]=[dict(uid=c["uid"],title=c.get("name"),creator=c.get("creator"),url=c.get("url"),
                triangles=c.get("faceCount"),vertices=c.get("vertexCount"),downloadable=c.get("isDownloadable"),
                license=(c.get("detail") or {}).get("license") or c.get("license")) for c in result.get("candidates",[])]
    source_map={}
    if a.source_map:
        source_map={r["file"]:r for r in json.loads(Path(a.source_map).read_text(encoding="utf8"))["files"]}
    selected=[r for r in inv["files"] if not a.only or r["file"] in a.only]
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
        results=list(pool.map(lambda row:audit(row,prior.get(row["file"],[]),source_map.get(row["file"])),selected))
    output=dict(checkedUtc=datetime.now(timezone.utc).isoformat(),scope="Selected inventory files" if a.only else "All staged ZIP/GLB files",
        method="License fields from official Sketchfab model details; archive-to-page evidence recorded separately",
        pageLicenseMeaning="Stated model-page license, distinct from underlying ownership/provenance review",
        summary=dict(collections.Counter(r["status"] for r in results)),files=results)
    Path(a.output).write_text(json.dumps(output,indent=2,ensure_ascii=False),encoding="utf8")

if __name__=="__main__":main()
