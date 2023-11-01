using UnityEngine;
using System;
using Random = System.Random;
using System.Collections.Generic;

public class Map_UI : MonoBehaviour
{
    [SerializeField] Data_Manager dataManager;
    [SerializeField] Transform map;
    [SerializeField] Camera mapCam;
    [SerializeField] Sprite[] spriteSheet;
    [SerializeField] float arbitraryX;
    [SerializeField] float arbitraryY;
    [SerializeField] bool regen;

    const int chunkSize = 64;
    const int chunkArrayLength = 4096;
    const int chunksLoaded = 2;

    void Start()
    {
        for (int y = -chunksLoaded; y < chunksLoaded + 1; y++)
        {
            for (int x = -chunksLoaded; x < chunksLoaded + 1; x++)
            {
                DisplayChunk(GenerateChunk(x, y, dataManager.GetSeed()), x, y);
            }
        }

    }

    private void Update()
    {
        if(regen)
        {
            regen = false;
            foreach (Transform child in map.transform)
            {
                Destroy(child.gameObject);
            }
            for (int y = -chunksLoaded; y < chunksLoaded + 1; y++)
            {
                for (int x = -chunksLoaded; x < chunksLoaded + 1; x++)
                {
                    DisplayChunk(GenerateChunk(x, y, dataManager.GetSeed()), x, y);
                }
            }
        }
    }

    private void LateUpdate()
    {
        mapCam.transform.eulerAngles = new Vector3(90, 0, 0);
        mapCam.transform.position = new Vector3(0, 1, 0);
        map.transform.position = Vector3.zero;
    }

    void DisplayChunk(uint[] chunk, int xChunk, int yChunk)
    {
        Color c = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                uint currentSprite = chunk[x + (chunkSize * y)];
                if (currentSprite > 0)
                {
                    GameObject t = new GameObject();
                    t.layer = 5;
                    t.transform.localEulerAngles = new Vector3(90, 0, 0);
                    t.transform.parent = map;
                    t.transform.localPosition = new Vector3(x + (xChunk * chunkSize), 0, y + (yChunk * chunkSize));
                    SpriteRenderer spr = t.AddComponent<SpriteRenderer>();
                    spr.sprite = spriteSheet[currentSprite - 1];
                    spr.color = c;
                }
            }
        }
    }

    //Chunks are 64x64
    uint[] GenerateChunk(int x, int y, int seed)
    {
        uint[] chunk = new uint[4096];
        uint thisChunkVornoi = FindVornoiOfChunk(x, y, seed);

        //Highway Generation
        for (int i = 0; i < 4; i++)
        {
            int xn = 0;
            int yn = 0;

            switch (i)
            {
                case 0:
                    xn = 1;
                    yn = 0;
                    break;
                case 1:
                    xn = 0;
                    yn = -1;
                    break;
                case 2:
                    xn = -1;
                    yn = 0;
                    break;
                case 3:
                    xn = 0;
                    yn = 1;
                    break;
                default:
                    break;
            }

            if (!(xn == 0 && yn == 0))
            {
                Vector2 vornoiPoint = GetXYFromIndex(FindVornoiOfChunk(x + xn, y + yn, seed)) + new Vector2(xn * chunkSize, yn * chunkSize);
                Vector2 path = GetXYFromIndex(thisChunkVornoi);

                bool reverseApply = false;
                if (vornoiPoint.x < path.x)
                {
                    Vector2 temp = path;
                    path = vornoiPoint;
                    vornoiPoint = temp;
                    reverseApply = true;
                }

                while (true)
                {
                    if (reverseApply)
                    {
                        if (path.x == vornoiPoint.x && path.y == vornoiPoint.y)
                        {
                            break;
                        }
                        else if (path.x < chunkSize && path.y < chunkSize && path.x >= 0 && path.y >= 0)
                        {
                            chunk[GetIndexFromXY(path)] = 1;
                        }
                    }
                    else
                    {
                        if (path.x >= chunkSize || path.y >= chunkSize || path.x < 0 || path.y < 0)
                        {
                            break;
                        }
                        else
                        {
                            chunk[GetIndexFromXY(path)] = 1;
                        }
                    }

                    if (vornoiPoint.x == path.x)
                    {
                        if (vornoiPoint.y > path.y)
                        {
                            path += new Vector2(0, -1);
                        }
                        else if (vornoiPoint.y < path.y)
                        {
                            path += new Vector2(0, 1);
                        }
                    }
                    else
                    {
                        float angle = CalculateSlope(path, vornoiPoint);
                        if (vornoiPoint.x > path.x)
                        {
                            if (angle == 1)
                            {
                                path += new Vector2(1, 1);
                            }
                            else if (angle < 1 && angle > -1)
                            {
                                path += new Vector2(1, 0);
                            }
                            else if (angle > 1)
                            {
                                path += new Vector2(0, 1);
                            }
                            else if (angle < -1)
                            {
                                path += new Vector2(0, -1);
                            }
                            else if (angle == -1)
                            {
                                path += new Vector2(1, -1);
                            }
                        }
                    }
                }

                chunk[GetIndexFromXY(GetXYFromIndex(thisChunkVornoi))] = 2;

            }
        }

        //Flood Fill Everything Needing Roads
        Vector2 pt = Vector2.zero;
        Vector2 vnXY = GetXYFromIndex(thisChunkVornoi);
        for (int i = 0; i < 4; i++)
        {
            switch (i)
            {
                case 0:
                    pt = (vnXY +
                          GetXYFromIndex(FindVornoiOfChunk(x - 1, y, seed)) +
                          GetXYFromIndex(FindVornoiOfChunk(x - 1, y + 1, seed)) +
                          GetXYFromIndex(FindVornoiOfChunk(x, y + 1, seed))) / 4;
                    break;
                case 1:
                    pt = (vnXY +
                          GetXYFromIndex(FindVornoiOfChunk(x, y + 1, seed)) +
                          GetXYFromIndex(FindVornoiOfChunk(x + 1, y + 1, seed)) +
                          GetXYFromIndex(FindVornoiOfChunk(x + 1, y, seed))) / 4;
                    break;
                case 2:
                    pt = (vnXY +
                          GetXYFromIndex(FindVornoiOfChunk(x + 1, y, seed)) +
                          GetXYFromIndex(FindVornoiOfChunk(x + 1, y - 1, seed)) +
                          GetXYFromIndex(FindVornoiOfChunk(x, y - 1, seed))) / 4;
                    break;
                case 3:
                    pt = (vnXY +
                          GetXYFromIndex(FindVornoiOfChunk(x, y - 1, seed)) +
                          GetXYFromIndex(FindVornoiOfChunk(x - 1, y - 1, seed)) +
                          GetXYFromIndex(FindVornoiOfChunk(x - 1, y, seed))) / 4;
                    break;
                default:
                    break;
            }
            pt = new Vector2(Mathf.Clamp(Mathf.Floor(pt.x), 0, chunkSize - 1), Mathf.Clamp(Mathf.Floor(pt.y), 0, chunkSize - 1));
            Debug.Log(pt);
            Stack<Vector2> pixels = new Stack<Vector2>();
            pixels.Push(pt);

            while (pixels.Count > 0)
            {
                Vector2 a = pixels.Pop();
                if (a.x < chunkSize && a.x >= 0 && a.y < chunkSize && a.y >= 0)
                {
                    if (chunk[GetIndexFromXY(a)] == 0)
                    {
                        chunk[GetIndexFromXY(a)] = 3;
                        pixels.Push(new Vector2(a.x - 1, a.y));
                        pixels.Push(new Vector2(a.x + 1, a.y));
                        pixels.Push(new Vector2(a.x, a.y - 1));
                        pixels.Push(new Vector2(a.x, a.y + 1));
                    }
                }
            }
        }

        //Reduce flood to actual roads
        for (uint i = 0; i < chunk.Length; i++)
        {
            if (chunk[i] == 3 && !EvaluateIfStreet(GetXYFromIndex(i) + new Vector2(x * chunkSize, y * chunkSize), seed))
            {
                chunk[i] = 0;
            }
        }
        return chunk;
    }

    bool EvaluateIfStreet(Vector2 xy, int seed)
    {
        float x = Mathf.PerlinNoise1D((xy.x * 12731.00721323f) + (seed % 100000));
        float y = Mathf.PerlinNoise1D((xy.y * 14935.0032131f) + (seed % 100000));

        if (x > 0.4f && x < 0.45f)
        {
            return true;
        }
        if (y > 0.4f && y < 0.45f)
        {
            return true;
        }
        return false;
    }

    float CalculateSlope(Vector2 point1, Vector2 point2)
    {
        float rise = point2.y - point1.y;
        float run = point2.x - point1.x;

        if (run != 0)
        {
            float slope = rise / run;
            return slope;
        }
        else
        {
            return float.NaN;
        }
    }

    Vector2 GetXYFromIndex(uint index)
    {
        return new Vector2(index % chunkSize, Mathf.Floor(index / chunkSize));
    }

    uint GetIndexFromXY(Vector2 xy)
    {
        return (uint)(Mathf.Floor(xy.x) + (chunkSize * Mathf.Floor(xy.y)));
    }

    uint FindVornoiOfChunk(int x, int y, int seed)
    {
        Random rnd = new Random(seed ^ Animator.StringToHash(x.ToString() + y.ToString()));

        return (uint)rnd.Next() % chunkArrayLength;
    }

}

