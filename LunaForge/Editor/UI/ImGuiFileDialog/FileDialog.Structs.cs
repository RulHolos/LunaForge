﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.GUI.ImGuiFileDialog;

/// <summary>
/// A file or folder picker.
/// </summary>
public partial class FileDialog
{
    private struct FileStruct
    {
        public FileStructType Type;
        public string FilePath;
        public string FileName;
        public string Ext;
        public long FileSize;
        public string FormattedFileSize;
        public string FileModifiedDate;
    }

    private readonly struct SideBarItem
    {
        public SideBarItem(string text, string location, char icon)
        {
            this.Text = text;
            this.Location = location;
            this.Icon = icon;
        }

        public string Text { get; init; }

        public string Location { get; init; }

        public char Icon { get; init; }

        public bool CheckExistence()
            => !string.IsNullOrEmpty(this.Location) && Directory.Exists(this.Location);
    }

    private struct FilterStruct
    {
        public string Filter;
        public HashSet<string> CollectionFilters;

        public void Clear()
        {
            this.Filter = string.Empty;
            this.CollectionFilters.Clear();
        }

        public bool Empty()
        {
            return string.IsNullOrEmpty(this.Filter) && (this.CollectionFilters == null || (this.CollectionFilters.Count == 0));
        }

        public bool FilterExists(string filter)
        {
            return this.Filter.Equals(filter, StringComparison.InvariantCultureIgnoreCase) || (this.CollectionFilters != null && this.CollectionFilters.Any(colFilter => colFilter.Equals(filter, StringComparison.InvariantCultureIgnoreCase)));
        }
    }

    private struct IconColorItem
    {
        public char Icon;
        public Vector4 Color;
    }
}
