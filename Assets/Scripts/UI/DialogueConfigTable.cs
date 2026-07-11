using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MonsterMaster.UI
{
    public sealed class DialogueConfigTable
    {
        public sealed class Entry
        {
            public string Id;
            public string Speaker;
            public string Text;
            public readonly List<Option> Options = new List<Option>();
        }

        public struct Option
        {
            public string Label;
            public string Action;
        }

        private readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>();

        public static DialogueConfigTable LoadFromResources(string resourcePath)
        {
            TextAsset asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null) throw new InvalidOperationException("Dialogue config not found: Resources/" + resourcePath + ".csv");
            return Parse(asset.text);
        }

        public bool TryGet(string id, out Entry entry) => entries.TryGetValue(id, out entry);

        private static DialogueConfigTable Parse(string csv)
        {
            DialogueConfigTable table = new DialogueConfigTable();
            List<List<string>> rows = ParseCsv(csv);
            for (int r = 1; r < rows.Count; r++)
            {
                List<string> row = rows[r];
                if (row.Count < 3 || string.IsNullOrWhiteSpace(row[0])) continue;
                Entry entry = new Entry { Id = row[0].Trim(), Speaker = row[1], Text = row[2] };
                for (int i = 3; i + 1 < row.Count; i += 2)
                {
                    if (string.IsNullOrWhiteSpace(row[i])) continue;
                    entry.Options.Add(new Option { Label = row[i], Action = row[i + 1] });
                }
                table.entries[entry.Id] = entry;
            }
            return table;
        }

        private static List<List<string>> ParseCsv(string text)
        {
            List<List<string>> rows = new List<List<string>>();
            List<string> row = new List<string>();
            StringBuilder cell = new StringBuilder();
            bool quoted = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '"')
                {
                    if (quoted && i + 1 < text.Length && text[i + 1] == '"') { cell.Append('"'); i++; }
                    else quoted = !quoted;
                }
                else if (c == ',' && !quoted) { row.Add(cell.ToString()); cell.Length = 0; }
                else if ((c == '\n' || c == '\r') && !quoted)
                {
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                    row.Add(cell.ToString()); cell.Length = 0;
                    if (row.Count > 1 || row[0].Length > 0) rows.Add(row);
                    row = new List<string>();
                }
                else cell.Append(c);
            }
            row.Add(cell.ToString());
            if (row.Count > 1 || row[0].Length > 0) rows.Add(row);
            return rows;
        }
    }
}
