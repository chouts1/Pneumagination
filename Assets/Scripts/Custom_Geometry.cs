using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Custom_Geometry : MonoBehaviour
{
    [Header("Static Data")]
    [SerializeField] MaterialType materialType;
    [SerializeField] MaterialLocation materialLocation;
    [SerializeField] MaterialColorType materialColorType;

    [Header("Player Decided Data")]
    [SerializeField] Color colorA = Color.white;
    [SerializeField] Color colorB = Color.white;
    [SerializeField] Color colorC = Color.white;
    [SerializeField] Color colorD = Color.white;
    float rain;
    float snow;
    float oldRain = -1;
    float oldSnow = -1;
    Color oldColorA;
    Color oldColorB;
    Color oldColorC;
    Color oldColorD;

    MeshRenderer[] meshRenderers;
    Data_Manager dataManager;

    private void Start()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        dataManager = GameObject.Find("Data Manager").GetComponent<Data_Manager>();
        int hashName;
        if(!int.TryParse(name,out hashName))
        {
            name = Random.Range(int.MinValue, int.MaxValue).ToString();
        }
    }

    public string GetObjectHash()
    {
        return name;
    }

    public Color GetColorA()
    {
        return colorA;
    }
    public void SetColorA(Color newColor)
    {
        colorA = newColor;
    }
    public Color GetColorB()
    {
        return colorB;
    }
    public void SetColorB(Color newColor)
    {
        colorB = newColor;
    }
    public Color GetColorC()
    {
        return colorC;
    }
    public void SetColorC(Color newColor)
    {
        colorC = newColor;
    }
    public Color GetColorD()
    {
        return colorD;
    }
    public void SetColorD(Color newColor)
    {
        colorD = newColor;
    }

    public MaterialColorType GetMaterialColorType()
    {
        return materialColorType;
    }


    private void Update()
    {
        if (oldColorA != colorA)
        {
            oldColorA = colorA;
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].material.SetColor("_ColorA", colorA);
            }
        }
        if (oldColorB != colorB)
        {
            oldColorB = colorB;
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].material.SetColor("_ColorB", colorA);
            }
        }
        if (oldColorC != colorC)
        {
            oldColorC = colorC;
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].material.SetColor("_ColorC", colorA);
            }
        }
        if (oldColorD != colorD)
        {
            oldColorD = colorD;
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].material.SetColor("_ColorD", colorA);
            }
        }

        if (materialLocation == MaterialLocation.exterior)
        {
            rain = dataManager.GetCurrentRainValue();
            snow = dataManager.GetCurrentSnowValue();
            if (oldRain != rain)
            {
                oldRain = rain;
                for (int i = 0; i < meshRenderers.Length; i++)
                {
                    meshRenderers[i].material.SetFloat("_Rain", rain);
                }
            }
            if (oldSnow != snow)
            {
                oldSnow = snow;
                for (int i = 0; i < meshRenderers.Length; i++)
                {
                    meshRenderers[i].material.SetFloat("_Snow", snow);
                }
            }
        }
    }

    public enum MaterialType
    {
        wallpaper,
        bricks,
        slantedRoof,
        flooring,
        road,
        ceiling,
        concrete,
        plaster,
        plastic,
        earth,
        vinyl,
        metal,
    }
    public enum MaterialLocation
    {
        exterior,
        interior,
    }

    public enum MaterialColorType
    {
        primary,
        secondary,
        tertiary,
        light,
        dark,
    }
}
