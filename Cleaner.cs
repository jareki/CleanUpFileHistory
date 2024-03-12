using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NLog;

namespace CleanUpFileHistory
{
    internal class Cleaner
    {
        private readonly string _rootFolderFullPath;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly string _pattern = @"\((\d{4})_(\d{2})_(\d{2})(\s)(\d{2})_(\d{2})_(\d{2})(\s)(UTC)\)$";


        public Cleaner(
            string rootFolderFullPath)
        {
            this._rootFolderFullPath = rootFolderFullPath;
        }

        public void CleanUpFolder(bool recursive = true)
        {
            _logger.Trace("Start executing");
            this.CleanUpFolderInternal(this._rootFolderFullPath, recursive);
            _logger.Trace("End executing");
        }

        private void CleanUpFolderInternal(string folderPath, bool recursive = true)
        {
            _logger.Trace($"Start exploring folder = {folderPath}");

            var dict = this.CreateFilesDictionary(folderPath);
            this.RemoveNonDuplicateValues(dict);
            this.Sorting(dict);
            this.DeleteOlderDuplicates(dict);

            _logger.Trace($"End exploring folder = {folderPath}");

            if (!recursive)
            {
                return;
            }

            foreach (string subFolderPath in Directory.GetDirectories(folderPath))
            {
                this.CleanUpFolderInternal(subFolderPath, true);
            }
        }

        private void DeleteOlderDuplicates(Dictionary<string, List<string>> dict)
        {
            _logger.Trace("Delete older duplicates:");
            foreach (var group in dict)
            {
                for (int i = 1; i < group.Value.Count; i++)
                {
                    _logger.Trace($"Delete \"{group.Value[i]}\"");
                    File.SetAttributes(group.Value[i], FileAttributes.Normal);
                    File.Delete(group.Value[i]);
                }
            }
        }

        private void Sorting(Dictionary<string, List<string>> dict)
        {
            _logger.Trace("Found groups:");
            foreach (var group in dict)
            {
                // making last created duplicate as zero element in list

                group.Value
                    .Sort(((f1, f2) =>
                        File.GetCreationTimeUtc(f1).CompareTo(File.GetCreationTimeUtc(f2))));
                group.Value.Reverse();

                _logger.Trace($"Group \"{group.Key}\":");
                _logger.Trace(string.Join(Environment.NewLine, group.Value));
            }
        }

        private void RemoveNonDuplicateValues(Dictionary<string, List<string>> dict)
        {
            var dictKeys = dict.Keys.ToArray();
            foreach (string key in dictKeys)
            {
                if (dict[key].Count <= 1)
                {
                    dict.Remove(key);
                }
            }
        }

        private Dictionary<string, List<string>> CreateFilesDictionary(string folderPath)
        {
            var dict = new Dictionary<string, List<string>>();
            foreach (string fullFileName in Directory.EnumerateFiles(folderPath))
            {
                string fileName = Path.GetFileName(fullFileName);
                string extension = Path.GetExtension(fullFileName);
                string fileNameWoutExt = Path.GetFileNameWithoutExtension(fileName);

                string originalfileNameWoutExt = Regex.Replace(fileNameWoutExt, this._pattern, string.Empty);
                if (originalfileNameWoutExt == fileNameWoutExt)
                {
                    continue;
                }

                string originalFileName = $"{originalfileNameWoutExt.Trim()}{extension}";

                if (!dict.ContainsKey(originalFileName))
                {
                    dict[originalFileName] = new List<string>();
                }
                dict[originalFileName].Add(fullFileName);
            }

            return dict;
        }
    }
}
