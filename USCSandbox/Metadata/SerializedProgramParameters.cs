using AssetsTools.NET;

namespace USCSandbox.Metadata;
// this class appears in the serialized asset data.
// for the data that appears after the base shader in a shader blob, see ShaderParameters.
public class SerializedProgramParameters
{
    public List<ConstantBufferParameter> Vectors;
    public List<ConstantBufferParameter> Matrices;
    public List<TextureParameter> Textures;
    public List<ConstantBufferBinding> Buffers;
    public List<ConstantBuffer> CBuffers;
    public List<ConstantBufferBinding> CBufferBindings;
    public List<UAVParameter> UAVParams;
    public List<SamplerParameter> Samplers;

    public SerializedProgramParameters(AssetTypeValueField field, Dictionary<int, string> nameTable)
    {
        // this will error out if we hit any unsupported fields (.AsInt ones) since none will be int
        Vectors = SerializedMetadataHelpers.GetArrayFirstValue(field["m_VectorParams.Array"])
            .Select(p => new ConstantBufferParameter(p, nameTable)).ToList();
        Matrices = SerializedMetadataHelpers.GetArrayFirstValue(field["m_MatrixParams.Array"])
            .Select(p => new ConstantBufferParameter(p, nameTable)).ToList();
        Textures = SerializedMetadataHelpers.GetArrayFirstValue(field["m_TextureParams.Array"])
            .Select(p => new TextureParameter(p, nameTable)).ToList();
        Buffers = SerializedMetadataHelpers.GetArrayFirstValue(field["m_BufferParams.Array"])
            .Select(p => new ConstantBufferBinding(p, nameTable)).ToList();
        CBuffers = SerializedMetadataHelpers.GetArrayFirstValue(field["m_ConstantBuffers.Array"])
            .Select(p => new ConstantBuffer(p, nameTable)).ToList();
        CBufferBindings = SerializedMetadataHelpers.GetArrayFirstValue(field["m_ConstantBufferBindings.Array"])
            .Select(p => new ConstantBufferBinding(p, nameTable)).ToList();
        UAVParams = SerializedMetadataHelpers.GetArrayFirstValue(field["m_UAVParams.Array"])
            .Select(p => new UAVParameter(p)).ToList();
        Samplers = SerializedMetadataHelpers.GetArrayFirstValue(field["m_Samplers.Array"])
            .Select(p => new SamplerParameter(p)).ToList();
    }
}
