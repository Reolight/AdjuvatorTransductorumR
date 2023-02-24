namespace AdjuvatorTransductorumRCor.Model;

public static class DataModelFabric
{
    /// <summary>
    /// Creates new named data model, initializes it's XDocument in writer and performs first-save;  
    /// </summary>
    /// <param name="name">Name of the project you saving</param>
    /// <returns>Created data model and have been already saved</returns>
    public static DataModel CreateNewDataModel(string name)
    {
        DataModel data = new(name);
        data.Redactor.ModelXmlWriter.InitXDocument();
        data.Redactor.ModelXmlWriter.SaveProject();
        return data;
    }
}