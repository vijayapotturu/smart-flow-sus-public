using System.Reflection;

namespace Assistant.Hub.Api.Services.Data
{
    public class ReferenceDataStub
    {
        public static string GetByName(string fileName)
        {
            var resourceName = $"Assistant.Hub.Api.Services.Data.{fileName}";
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new ArgumentException($"The resource {resourceName} was not found.");

                using (StreamReader reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }
    }
}
