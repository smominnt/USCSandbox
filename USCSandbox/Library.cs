using AssetsTools.NET.Extra;
using USCSandbox.Common;
using USCSandbox.Processor;

namespace USCSandbox
{
    public static class Library
    {
        /// <summary>
        /// Takes raw shader bytes and returns the decompiled string.
        /// </summary>
        public static string DecompileBuffer(byte[] data, AssetRipper.Primitives.UnityVersion version, int platformId)
        {
            Initialize();

            try
            {
                using MemoryStream ms = new MemoryStream(data);

                AssetsFileInstance inst = _staticManager.LoadAssetsFile(ms, "in_memory_shader.assets", false);
                _staticManager.LoadClassDatabaseFromPackage(version.ToString());

                var shaderInf = inst.file.GetAssetsOfType(AssetClassID.Shader).FirstOrDefault();
                if (shaderInf == null) return "// [USC] No Shader object found in buffer.";

                var shaderBf = _staticManager.GetBaseField(inst, shaderInf);
                if (shaderBf == null) return "// [USC] Failed to read Shader base field.";

                GPUPlatform platform = (GPUPlatform)platformId;

                var shaderTextWriter = new ShaderTextWriter(shaderBf, platform, version);
                return shaderTextWriter.LoadAndWrite(platform);
            }
            catch (Exception ex)
            {
                return $"// [USC] Decompilation Error: {ex.Message}\n{ex.StackTrace}";
            }
        }


        private static readonly AssetsManager _staticManager = new AssetsManager();

        private static bool _isInitialized = false;

        private static void Initialize()
        {
            if (!_isInitialized)
            {
                _staticManager.LoadClassPackage("classdata.tpk");
                _isInitialized = true;
            }
        }

    }
}
