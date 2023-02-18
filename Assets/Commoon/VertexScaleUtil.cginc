#pragma multi_compile_local _ USE_ATLAS

float4 _textureRect;

// uvが描画中のパーツ内かどうか判定する。
// 内側なら1を返す。そうでなければ0を返す。
// textureRect.xy ... atlas内でのパーツのxy座標
// textureRect.zw ... パーツの幅、高さ
int IsInner(float2 uv, float2 allasSize, float4 textureRect)
{
    float width = allasSize.x;
    float minX = textureRect.x / width; // パーツの左端座標
    float maxX = (textureRect.x + textureRect.z) / width; // パーツの右端座標

    // https://qiita.com/yuichiroharai/items/6e378cd128279ac9a2f0
    // x <= edge なら 1.0
    // step(x, edge);
    int insideOfLeftEdge = step(minX, uv.x); // uv.xがパーツ左端より内側か
    int insideOfRightEdge = step(uv.x, maxX); // uv.xがパーツ右端より内側か

    float height = allasSize.y;
    float minY = textureRect.y / height; // パーツの下端座標
    float maxY = (textureRect.y + textureRect.w) / height; // パーツの上端座標

    int insideOfBottomEdge = step(minY, uv.y); // パーツ下端より内側か
    int insideOfTopEdge = step(uv.y, maxY); // パーツ上端より内側か

    // 上下左右の端より内側か判定
    return insideOfLeftEdge * insideOfRightEdge * insideOfBottomEdge * insideOfTopEdge;
}

float remap(float value, float inputMin, float inputMax, float outputMin, float outputMax)
{
    return (value - inputMin) * ((outputMax - outputMin) / (inputMax - inputMin)) + outputMin;
}

// textureRect.xy ... atlas内でのパーツのxy座標
// textureRect.zw ... パーツの幅、高さ
float2 AtlasUVtoMeshUV(float2 uv, float2 allasSize, float4 textureRect)
{
    float u = uv.x;
    float width = allasSize.x;
    float minX = textureRect.x / width;
    float maxX = (textureRect.x + textureRect.z) / width;
    u = remap(u, minX, maxX, 0, 1);

    float v = uv.y;
    float height = allasSize.y;
    float minY = textureRect.y / height;
    float maxY = (textureRect.y + textureRect.w) / height;
    v = remap(v, minY, maxY, 0, 1);

    float2 localUv = float2(u, v);
    return localUv;
}

// meshのuv座標が、atlas内のパーツではどの部分にあたるのか調べる
// textureRect.xy ... atlas内でのパーツのxy座標
// textureRect.zw ... パーツの幅、高さ
float2 MeshUVtoAtlasUV(float2 localUV, float2 allasSize, float4 textureRect)
{
    float width = textureRect.z;
    // atlas内のpixel座標を求める
    float x = textureRect.x + width * localUV.x;
    float height = textureRect.w;
    float y = textureRect.y + height * localUV.y;

    // 0 〜 1に正規化する
    return float2(x / allasSize.x, y / allasSize.y);
}
