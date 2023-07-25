using BizHawk.Client.Common;

namespace BizHawk.Client.Headless {
class DialogParent : IDialogParent {
    public IDialogController DialogController { get; } = new DialogController();
}
}