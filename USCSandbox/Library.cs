using AssetsTools.NET;
using AssetsTools.NET.Extra;
using USCSandbox.Common;
using USCSandbox.Processor;
using UnityVersion = AssetRipper.Primitives.UnityVersion;

namespace USCSandbox
{
    public class Library : IDisposable
    {
        private readonly AssetsManager manager;
        private readonly GPUPlatform platform;
        private readonly byte[] data;
        private readonly UnityVersion passedVersion;

        public Library(byte[] data, int platform, string version = "")
        {
            this.manager = new AssetsManager();
            string assemblyLocation = Path.GetDirectoryName(typeof(Library).Assembly.Location) ?? AppDomain.CurrentDomain.BaseDirectory;
            string tpkPath = Path.Combine(assemblyLocation, "classdata.tpk");
            manager.LoadClassPackage(tpkPath);

            this.passedVersion = UnityVersion.Parse(version);
            manager.LoadClassDatabaseFromPackage(version);

            this.platform = (GPUPlatform)platform;
            this.data = data;
        }


        /// <summary>
        /// Decompiles from raw serialized asset field bytes
        /// </summary>
        public string DecompileFromAssetBytes(int classId)
        {
            using var ms = new MemoryStream(this.data);
            using var reader = new AssetsFileReader(ms);

            var cldb = manager.ClassDatabase
                ?? throw new InvalidOperationException("Class database not loaded. Was a version string passed to the constructor?");

            var cldbType = cldb.FindAssetClassByID(classId)
                ?? throw new InvalidOperationException($"Class ID {classId} not found in class database.");

            var templateField = new AssetTypeTemplateField();
            templateField.FromClassDatabase(cldb, cldbType, false);

            var baseField = templateField.MakeValue(reader);

            return DecompileFromField(baseField);
        }


        private string DecompileFromField(AssetTypeValueField shaderBf)
        {
            var shaderTextWriter = new ShaderTextWriter(shaderBf, platform, passedVersion);
            return shaderTextWriter.LoadAndWrite(platform);
        }


        public void Dispose()
        {
            manager.UnloadAll(true);
            GC.SuppressFinalize(this);
        }
    }
}
