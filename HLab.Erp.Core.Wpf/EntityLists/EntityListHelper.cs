﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using HLab.Erp.Data;
using HLab.Erp.Data.Observables;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HLab.Erp.Core.EntityLists
{
    public class EntityListHelper<T> : IEntityListHelper<T>
        where T : class, IEntity, new()
    {
        public void Populate(object grid, IColumnsProvider<T> provider)
        {
            if (grid is ItemsControl dataGrid)
            {
                dataGrid.SourceUpdated += delegate (object sender, DataTransferEventArgs args)
                {
                    ICollectionView cv = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
                    //if (cv != null)
                    //{
                    //    cv.GroupDescriptions.Clear();
                    //    cv.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
                    //}
                };

                provider.Populate(grid);
            }
        }

        public object GetListView(IList list)
        {
            var lcv = new ListCollectionView(list);
            lcv.GroupDescriptions?.Add(new PropertyGroupDescription("FileId"));
            return lcv;
        }

        public async Task ExportAsync(IObservableQuery<T> list, IContractResolver resolver)
        {
            var filename = typeof(T).Name + "-export.gz";
            SaveFileDialog saveFileDialog = new() { FileName = filename, DefaultExt = "gz" };
            if (saveFileDialog.ShowDialog() == false) return;

            var text = JsonConvert.SerializeObject(
                list.ToList(),
                Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = resolver});

            await using var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            await using var fileStream = File.Create(saveFileDialog.FileName);
            await using var gzipStream = new GZipStream(fileStream, CompressionMode.Compress);
            try
            {
                await sourceStream.CopyToAsync(gzipStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public async Task<IEnumerable<T>> ImportAsync()
        {
            var filename = typeof(T).Name + "-export.gz";
            OpenFileDialog openFileDialog = new() { FileName = filename, Filter = $"{typeof(T).Name}|*.{typeof(T).Name}.gz" };
            if (openFileDialog.ShowDialog() == false) return new List<T>();

            await using var fileStream = File.OpenRead(openFileDialog.FileName);
            await using var resultStream = new MemoryStream();
            await using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
            try
            {
                await gzipStream.CopyToAsync(resultStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var text = Encoding.UTF8.GetString(resultStream.ToArray()); ;
            return JsonConvert.DeserializeObject<List<T>>(text);
        }


    }
}
