#nullable enable

using System;
using BizHawk.Client.Common;
using System.Collections.Generic;

namespace BizHawk.Client.Headless {
class DialogController : IDialogController {
    public void AddOnScreenMessage(string message) {
        Console.WriteLine($"dialog controller OSM: {message}");
    }

    public IReadOnlyList<string>? ShowFileMultiOpenDialog(
        IDialogParent dialogParent,
        string? filterStr,
        ref int filterIndex,
        string initDir,
        bool discardCWDChange = false,
        string? initFileName = null,
        bool maySelectMultiple = false,
        string? windowTitle = null)
    {
        return new List<string>() {
            "test"
        };
    }

    public string? ShowFileSaveDialog(
        IDialogParent dialogParent,
        bool discardCWDChange,
        string? fileExt,
        string? filterStr,
        string initDir,
        string? initFileName,
        bool muteOverwriteWarning)
    {
            return "hello";
    }

    public void ShowMessageBox(
        IDialogParent? owner,
        string text,
        string? caption = null,
        EMsgBoxIcon? icon = null)
    {
        Console.WriteLine($"DialogController: {text}");
    }

    public bool ShowMessageBox2(
        IDialogParent? owner,
        string text,
        string? caption = null,
        EMsgBoxIcon? icon = null,
        bool useOKCancel = false)
    {
        Console.WriteLine($"DialogController: {text}");
        return true;
    }

    public bool? ShowMessageBox3(
        IDialogParent? owner,
        string text,
        string? caption = null,
        EMsgBoxIcon? icon = null)
    {
        Console.WriteLine($"DialogController: {text}");
        return true;
    }

    public void StartSound() {
        Console.WriteLine("Dialog Controller Starting Sound");
    }

    public void StopSound() {
        Console.WriteLine("Dialog Controller Stopping Sound");
    }

}
}