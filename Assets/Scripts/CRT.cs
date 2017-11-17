using UnityEngine;
using System.Collections;

public enum CRTScanLinesSizes { S32 = 32, S64 = 64, S128 = 128, S256 = 256, S512 = 512, S1024 = 1024 };

[ExecuteInEditMode]
public class CRT : MonoBehaviour {

    #region Variables
    public Shader curShader;
    public float Distortion = 0.1f;
    public float Gamma = 1.0f;
    public float YExtra = 0.5f;
    public float CurvatureSet1 = 0.5f;
    public float CurvatureSet2 = 1.0f;
    public float DotWeight = 1.0f;
    public CRTScanLinesSizes scanSize = CRTScanLinesSizes.S512;
    public Color rgb1 = Color.white;
    public Color rgb2 = Color.white;
    private Material curMaterial;

    #endregion

    #region Properties
    Material material {
        get {
            if (curMaterial == null) {
                curMaterial = new Material(curShader);
                curMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return curMaterial;
        }
    }
    #endregion
    // Use this for initialization
    void Start() {
        if (!SystemInfo.supportsImageEffects) {
            enabled = false;
            return;
        }
    }

    void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture) {
        if (curShader != null) {
            material.SetFloat("_Distortion", Distortion);
            material.SetFloat("_Gamma", Gamma);
            material.SetFloat("_curvatureSet1", CurvatureSet1);
            material.SetFloat("_curvatureSet2", CurvatureSet2);
            material.SetFloat("_YExtra", YExtra);
            material.SetFloat("_rgb1R", rgb1.r);
            material.SetFloat("_rgb1G", rgb1.g);
            material.SetFloat("_rgb1B", rgb1.b);
            material.SetFloat("_rgb2R", rgb2.r);
            material.SetFloat("_rgb2G", rgb2.g);
            material.SetFloat("_rgb2B", rgb2.b);
            material.SetFloat("_dotWeight", DotWeight);
            material.SetVector("_TextureSize", new Vector2((float)scanSize, (float)scanSize));
            Graphics.Blit(sourceTexture, destTexture, material);
        }
        else {
            Graphics.Blit(sourceTexture, destTexture);
        }


    }

    // Update is called once per frame
    void Update() {

    }

    void OnDisable() {
        if (curMaterial) {
            DestroyImmediate(curMaterial);
        }

    }


}