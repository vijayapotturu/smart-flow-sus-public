using Assistants.API.Core;
using Assistants.Hub.API.Core;

namespace Assistant.Hub.Api.Assistants
{
    public static class ApiModelExtensions
    {
        public static string? GetRequestImage(this TaskRequest taskRequest)
        {
            foreach (var file in taskRequest.Files)
            {
                if (file.DataUrl == null)
                    return null;
                
                DataUriParser parser = new DataUriParser(file.DataUrl);
                if (parser.MediaType == "image/jpeg" || parser.MediaType == "image/png")
                {
                    return file.DataUrl;
                }
            }
            return null;
        }

        public static string? GetBlobName(this RequestFile requestFile)
        {
       
            if (requestFile.BlobName == null)
                return null;

            return requestFile.BlobName;
   
        }

        public static string? GetDataUrlForPngJpeg(this RequestFile requestFile)
        {
            if (requestFile.DataUrl == null)
                return null;

            DataUriParser parser = new DataUriParser(requestFile.DataUrl);
            if (parser.MediaType == "image/jpeg" || parser.MediaType == "image/png")
            {
                return requestFile.DataUrl;
            }

            return null;
        }

        public static string? GetRequestDocument(this TaskRequest taskRequest)
        {
            foreach (var file in taskRequest.Files)
            {
                DataUriParser parser = new DataUriParser(file.DataUrl);
                if (parser.MediaType == "application/pdf")
                {
                    return file.DataUrl;
                }
            }
            return null;
        }
    }
}
