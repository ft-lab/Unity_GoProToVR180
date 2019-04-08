/**
 * GoProで撮影した写真をVR180にする.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace GoProToVR180 {
[RequireComponent(typeof(Camera))]
    public class GoProToVR180 : MonoBehaviour
    {
        //--------------------------------------------.
        // 公開パラメータ.
        //--------------------------------------------.
        [SerializeField] float fovH = 122.6f;        // 水平の視野角度.
        [SerializeField] float fovV = 94.4f;        // 垂直の視野角度.

        [SerializeField] Texture2D leftPhotoImage = null;       // 左の静止画.
        [SerializeField] Texture2D rightPhotoImage = null;      // 右の静止画.
        [SerializeField] Color backgroundColor = new Color(0, 0, 0);    // 背景色.

        //--------------------------------------------.
        // Private.
        //--------------------------------------------.
        private float m_angleH = 0.0f;      // 水平の視野角度 (度数).
        private float m_angleV = 0.0f;      // 垂直の視野角度 (度数).

        private GameObject m_backgroundSphere = null;       // 背景球.
        private Material m_backgroundSphereMat = null;      // 背景のマテリアル.
        private float m_radius = 100.0f;                    // 背景球の半径.

        void Start() {
            // 背景球を作成.
            m_CreateBackgroundSphere();
        }

        /**
         * 背景球を作成.
         */
        private void m_CreateBackgroundSphere () {
            if (m_backgroundSphereMat == null) {
                // 以下、ビルドして実行する時にShaderを読み込めるように
                // Shader.FindではなくResources.Load<Shader>を使用している.
                Shader shader = Resources.Load<Shader>("Shaders/fishEyeToVR180");
                m_backgroundSphereMat = new Material(shader);
            }
            if (m_backgroundSphere == null) {
                Mesh mesh = Resources.Load<Mesh>("Objects/backgroundSphere_vr360");
                m_backgroundSphere = new GameObject("panorama360Sphere");

                MeshRenderer meshRenderer = m_backgroundSphere.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = m_backgroundSphere.AddComponent<MeshFilter>();
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                meshRenderer.receiveShadows    = false;
                meshRenderer.material = m_backgroundSphereMat;
                meshFilter.mesh = mesh;

                m_backgroundSphere.transform.localScale = new Vector3(m_radius, m_radius, m_radius);
                m_backgroundSphere.transform.position = this.transform.position;
 
                // Y軸中心の回転角度.
                Quaternion currentCameraRot = this.transform.rotation;
                float cRotY = currentCameraRot.eulerAngles.y;
                m_backgroundSphere.transform.rotation = Quaternion.Euler(0, cRotY + 90, 0);
            }
        }

        void Update () {
            m_updateSphereTexture();
        }

        void OnDestroy () {
            if (m_backgroundSphereMat != null) {
                Destroy(m_backgroundSphereMat);
            }
            if (m_backgroundSphere != null) {
                GameObject.Destroy(m_backgroundSphere);
            }
        }

        /**
         * 背景球の更新.
         */
        void m_updateSphereTexture () {
            m_backgroundSphereMat.SetTexture("_LeftTex", leftPhotoImage);
            m_backgroundSphereMat.SetTexture("_RightTex", rightPhotoImage);
            m_backgroundSphereMat.SetFloat("_AngleH", fovH);
            m_backgroundSphereMat.SetFloat("_AngleV", fovV);
            m_backgroundSphereMat.SetVector("_BackgroundColor", new Vector4(backgroundColor.r, backgroundColor.g, backgroundColor.b, 1.0f));
        }
    }
}
