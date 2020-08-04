using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace WhoIsSpy.Data
{
    class ListLocation
    {
        private static ListLocation instance;
        private List<String> locations = new List<string>();
        public static ListLocation Instance
        {
            get { return instance ?? (instance = new ListLocation()); }
        }

        private FileOpenPicker picker;
        public void OpenFileFromPicker()
        {
            setupPicker();
            openFile();
        }

        public string GetRandomLocation()
        {
            Random random = new Random();
            int index = random.Next(0, locations.Count);
            try
            {
                return locations[index];
            }
            catch (Exception e)
            {
                return "none";
            }
        }
        private void setupPicker()
        {
            picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".txt");
        }

        private async void openFile()
        {
            StorageFile file = await picker.PickSingleFileAsync();
            ReadFile(file);
        }

        private async void ReadFile(StorageFile file)
        {
            string allTextFromFile = await FileIO.ReadTextAsync(file);
            locations.Clear();
            locations = allTextFromFile.Split('\n').ToList();
        }


    }
}
