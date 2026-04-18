using AssetsTools.NET;
using AssetsTools.NET.Extra;
using USCSandbox.Common;
using USCSandbox.Metadata;
using USCSandbox.Processor;
using UnityVersion = AssetRipper.Primitives.UnityVersion;

namespace USCSandbox
{
    public class Library
    {
        private readonly AssetsManager manager;

        private readonly GPUPlatform platform;

        UnityVersion? passedVersion = null;

        private readonly string[] inputFiles;

        private readonly string outDir;

        private readonly Dictionary<string, string> results;

        public Library(int platform, string[] inputFiles, string outDir, string version = "")
        {
            Console.WriteLine("\n[USCSandbox 5b15dac] Library Functions");
            this.manager = new AssetsManager();
            manager.LoadClassPackage("classdata.tpk");
            if (!string.IsNullOrEmpty(version)) 
            {
                this.passedVersion = UnityVersion.Parse(version);
            }
            this.platform = (GPUPlatform)platform;
            this.inputFiles = inputFiles;
            this.outDir = outDir;
            results = [];
        }


        public void DecompileShaders()
        {
            foreach (var file in inputFiles)
            {
                try
                {
                    LoadFile(file);

                }
                catch (NotSupportedException)
                {
                    Console.WriteLine($"[!] Skipped Unsupported File: {Path.GetFileName(file)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[!] Error {ex.Message}: {Path.GetFileName(file)}");
                }
            }

            foreach (var result in results)
            {
                Directory.CreateDirectory(Path.Combine(outDir, Path.GetDirectoryName(result.Key)!));
                File.WriteAllText($"{Path.Combine(outDir, result.Key)}.shader", result.Value);
            }
        }


        private void LoadFile(string? filePath = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }
            manager.UnloadAll(unloadClassData: false);


            if (Path.GetExtension(filePath) == ".assets")
            {
                LoadAssetsFile(filePath);
            }
            else
            {
                LoadBundleFile(filePath);
            }
        }


        private void LoadBundleFile(string filePath)
        {
            UnityVersion? ver = passedVersion;
            var bundleFile = manager.LoadBundleFile(filePath, true);

            if (passedVersion is null)
            {
                var verStr = bundleFile.file.Header.EngineVersion;
                if (verStr != "0.0.0")
                {
                    var fixedVerStr = new AssetsTools.NET.Extra.UnityVersion(verStr).ToString();
                    ver = UnityVersion.Parse(fixedVerStr);
                }
            }

            if (ver is null)
            {
                throw new NullReferenceException(nameof(ver));
            }

            manager.LoadClassDatabaseFromPackage(ver.ToString());
            var dirInfs = bundleFile.file.BlockAndDirInfo.DirectoryInfos;
            Console.WriteLine($"[Bundle]: {bundleFile.name}");

            foreach (var dirInf in dirInfs)
            {
                var afileInst = manager.LoadAssetsFileFromBundle(bundleFile, dirInf.Name);
                Console.WriteLine($"[Asset File From Bundle]: {dirInf.Name}");
                ProcessShaders(afileInst, ver.Value);
            }

        }


        private void LoadAssetsFile(string filePath)
        {
            UnityVersion? ver = passedVersion;
            var aFileInst = manager.LoadAssetsFile(filePath);

            if (passedVersion is null)
            {
                var verStr = aFileInst.file.Metadata.UnityVersion;
                if (verStr != "0.0.0")
                {
                    var fixedVerStr = new AssetsTools.NET.Extra.UnityVersion(verStr).ToString();
                    ver = UnityVersion.Parse(fixedVerStr);
                }
            }

            if (ver is null)
            {
                throw new NullReferenceException(nameof(ver));
            }

            manager.LoadClassDatabaseFromPackage(ver.ToString());
            Console.WriteLine($"[Assets File]: {aFileInst.name}");
            ProcessShaders(aFileInst, ver.Value);
        }


        private void ProcessShaders(AssetsFileInstance afileInst, UnityVersion ver)
        {
            manager.LoadClassDatabaseFromPackage(afileInst.file.Metadata.UnityVersion);
            var shadersToLoad = new List<AssetFileInfo>();
            shadersToLoad.AddRange(afileInst.file.GetAssetsOfType(AssetClassID.Shader));

            foreach (var shaderInf in shadersToLoad)
            {
                if (TryProcessShaderInfo(afileInst, shaderInf, ver, out var shaderDecompiled))
                {
                    // if doesn't exist
                    if (!results.TryGetValue(shaderDecompiled.Key, out var existing))
                    {
                        results[shaderDecompiled.Key] = shaderDecompiled.Value;
                    }
                    // if existing shader is more "complete"
                    else if (string.IsNullOrEmpty(existing) || existing.Length < shaderDecompiled.Value.Length)
                    {
                        results[shaderDecompiled.Key] = shaderDecompiled.Value;
                    }
                    // else keep what is already present
                }
            }
        }


        private bool TryProcessShaderInfo(AssetsFileInstance afileInst, AssetFileInfo shaderInf, UnityVersion ver, out KeyValuePair<string, string> result)
        {
            result = new KeyValuePair<string, string>();
            try
            {
                Console.WriteLine($"   [Decompiling]: {shaderInf.PathId}");
                var shaderBf = manager.GetBaseField(afileInst, shaderInf);
                if (shaderBf == null)
                {
                    Console.WriteLine("      [!]: Shader asset not found or couldn't be read.");
                    return false;
                }

                var shaderName = shaderBf["m_ParsedForm"]["m_Name"].AsString;
                var serShader = new SerializedShader(shaderBf, GPUPlatform.d3d11, ver);
                var shaderTextWriter = new ShaderTextWriter(shaderBf, GPUPlatform.d3d11, ver);
                var output = shaderTextWriter.LoadAndWrite(platform);
                result = new KeyValuePair<string, string>(shaderName, output);

                Console.WriteLine($"      [OK]: {shaderName}");
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine("      [!]: Error processing shader.", ex);
            }

            return false;
        }

    }
}
