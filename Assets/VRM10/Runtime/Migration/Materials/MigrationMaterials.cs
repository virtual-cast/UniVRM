﻿using System;
using System.Linq;
using UniGLTF;
using UniJSON;
using UnityEngine;

namespace UniVRM10
{
    public static class MigrationMaterials
    {
        private const string DontUseExtensionShaderName = "VRM_USE_GLTFSHADER";
        private const string MaterialPropertiesKey = "materialProperties";

        public static void Migrate(glTF gltf, JsonNode vrm0)
        {
            var needsDisablingVertexColor = false;
            var vrm0XMaterialList = vrm0[MaterialPropertiesKey].ArrayItems().ToArray();

            // 2. VRM 拡張のうち、古い Unlit 情報からの取得を試みる.
            if (MigrationLegacyUnlitMaterial.Migrate(gltf, vrm0XMaterialList))
            {
                // NOTE: 古い Unlit である場合、頂点カラー情報を破棄する.
                needsDisablingVertexColor = true;
            }

            // 3. VRM 拡張のうち、UnlitTransparentZWrite 情報からの取得を試みる.
            if (MigrationUnlitTransparentZWriteMaterial.Migrate(gltf, vrm0XMaterialList))
            {
                // NOTE: 古い Unlit である場合、頂点カラー情報を破棄する.
                needsDisablingVertexColor = true;
            }

            try
            {
                // 4. VRM 拡張のうち、MToon 情報からの取得を試みる.
                MigrationMToonMaterial.Migrate(gltf, vrm0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            if (needsDisablingVertexColor)
            {
                DisableVertexColor(gltf);
            }
        }

        private static void DisableVertexColor(glTF gltf)
        {
            foreach (var mesh in gltf.meshes)
            {
                foreach (var primitive in mesh.primitives)
                {
                    primitive.attributes.COLOR_0 = -1;
                }
            }
        }
    }
}