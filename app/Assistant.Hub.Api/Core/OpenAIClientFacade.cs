using Microsoft.SemanticKernel;

namespace Assistants.API.Core
{
    public class OpenAIClientFacade
    {
        // Dictionary to hold kernels based on their version deployment names
        private Dictionary<string, Kernel> kernels = new Dictionary<string, Kernel>();

        // Constructor initializing the facade with two kernels
        public OpenAIClientFacade()
        {
        }

        public void RegisterKernel(string deploymentName, Kernel kernel)
        {
            kernels[deploymentName] = kernel;
        }

        // Retrieves the kernel based on the deployment name
        public Kernel GetKernelByDeploymentName(string deploymentName)
        {
            if (kernels.ContainsKey(deploymentName))
            {
                return kernels[deploymentName];
            }
            throw new ArgumentException("Deployment name not recognized.");
        }
    }
}
