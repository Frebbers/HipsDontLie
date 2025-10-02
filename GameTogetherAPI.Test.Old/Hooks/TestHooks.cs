using HipsDontLie.Test.Old.Factories;
using HipsDontLie.Test.Old.Fixtures;
using HipsDontLie.Test.Old.Util;

namespace GameTogetherAPI.Test.Old.Hooks
{
    //Responsible for setting up the test environment
    //Losely based on previous project WorkFlowAPI
    [Binding]
    internal static class TestHooks
    {
        public static APITestContext Context;

        [BeforeFeature]
        public static void GlobalSetup()
        {
            Context = new APITestContext();
            Context.Factory = new APIFactory<Program>();
            Context.Client = Context.Factory.CreateClient();
            //Check if we are running in a docker container
            if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
            {
                Console.WriteLine("Running in container. Connecting to docker address");
                Context.Client.BaseAddress = new Uri(APIConstants.DockerAddress);
            }
            else
            {
                Console.WriteLine("Not running in container. Connecting to " + APIConstants.BaseAddress);
                Context.Client.BaseAddress = new Uri(APIConstants.BaseAddress);
            }
        }
        [AfterFeature]
        public static void GlobalTearDown()
        {
            Context.Client.Dispose();
            Context.Factory.Dispose();
        }
        
    }
}
