﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitCommitFileCollector.Models
{
    /// <summary>
    /// 選択ファイルのSHAとファイルパス
    /// </summary>
    public class ExtractFileGroup
    {
        public string Sha { get; set; }

        public List<string> FilePaths { get; set; } = new List<string>();

        public bool IsEmpty => FilePaths.Count == 0;

        public ExtractFileGroup(string sha) 
        {
            Sha = sha;
        }

        public void Add(string path)
        {
            FilePaths.Add(path);
        }

        public void Remove(string path)
        {
            FilePaths.Remove(path);
        }
    }
}
