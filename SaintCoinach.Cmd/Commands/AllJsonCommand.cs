﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tharga.Toolkit.Console;
using Tharga.Toolkit.Console.Command;
using Tharga.Toolkit.Console.Command.Base;

using SaintCoinach;
using SaintCoinach.Ex;
using SaintCoinach.Xiv;

#pragma warning disable CS1998

namespace SaintCoinach.Cmd.Commands {
    public class AllJsonCommand : ActionCommandBase {
        private ARealmReversed _Realm;

        public AllJsonCommand(ARealmReversed realm)
            : base("alljson", "Export all data (default), or only specific data files, as json; including all languages.") {
            _Realm = realm;
        }

        public override async Task<bool> InvokeAsync(string paramList) {
            const string CsvFileFormat = "json-all/{0}{1}.json";

            IEnumerable<string> filesToExport;

            if (string.IsNullOrWhiteSpace(paramList))
                filesToExport = _Realm.GameData.AvailableSheets;
            else
                filesToExport = paramList.Split(' ').Select(_ => _Realm.GameData.FixName(_));

            var successCount = 0;
            var failCount = 0;
            foreach (var name in filesToExport) {
                var sheet = _Realm.GameData.GetSheet(name);
                foreach(var lang in sheet.Header.AvailableLanguages) {
                    var code = lang.GetCode();
                    if (code.Length > 0)
                        code = "." + code;
                    var target = new FileInfo(Path.Combine(_Realm.GameVersion, string.Format(CsvFileFormat, name, code)));
                    try {

                        if (!target.Directory.Exists)
                            target.Directory.Create();

                        ExdHelper.SaveAsJson(sheet, lang, target.FullName, false);

                        ++successCount;
                    } catch (Exception e) {
                        OutputError("Export of {0} failed: {1}", name, e.Message);
                        try { if (target.Exists) { target.Delete(); } } catch { }
                        ++failCount;
                    }
                }
                
            }
            OutputInformation("{0} files exported, {1} failed", successCount, failCount);

            return true;
        }
    }
}
