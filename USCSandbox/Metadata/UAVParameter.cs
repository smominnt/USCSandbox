using AssetsTools.NET;

namespace USCSandbox.Metadata;
public class UAVParameter
{
    public string Name;
    public int Index;
    public int OriginalIndex;

    public UAVParameter(AssetsFileReader r, string name)
    {
        Name = name;
        Index = r.ReadInt32();
        OriginalIndex = r.ReadInt32();
    }

	public UAVParameter(AssetTypeValueField field)
	{
		Name = field["name"].AsString;
		Index = field["index"].AsInt;
		OriginalIndex = field["originalIndex"].AsInt;
	}
}