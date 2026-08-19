using Microsoft.AspNetCore.Hosting;

namespace TripVista.Services.Ml
{
    public static class MlModelPaths
    {
        public static string ModelsDirectory(IWebHostEnvironment env) => Path.Combine(env.ContentRootPath, "MLModels");

        public static string PackagePriceModelPath(IWebHostEnvironment env) =>
            Path.Combine(ModelsDirectory(env), "PackagePriceModel.zip");

        public static string TripTierModelPath(IWebHostEnvironment env) =>
            Path.Combine(ModelsDirectory(env), "TripTierModel.zip");
    }
}
