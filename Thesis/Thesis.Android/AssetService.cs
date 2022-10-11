using Android.Content.Res;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(Thesis.Droid.AssetService))]

namespace Thesis.Droid
{
    public class AssetService : IAssetService
    {
        public string LoadFile(string fileName)
        {
            if (fileName == null)
            {
                return null;
            }

            // Read the contents of our asset
            string content;
            AssetManager assets = Android.App.Application.Context.Assets;
            using (StreamReader sr = new StreamReader(assets.Open(fileName)))
            {
                content = sr.ReadToEnd();
            }
            return content;
        }
    }
}