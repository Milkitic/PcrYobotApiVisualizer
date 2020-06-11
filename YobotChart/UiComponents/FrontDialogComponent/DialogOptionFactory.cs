namespace YobotChart.UiComponents.FrontDialogComponent
{
    internal class DialogOptionFactory
    {
        public static FrontDialogOverlay.ShowContentOptions DiffSelectOptions => new FrontDialogOverlay.ShowContentOptions
        {
            Title = null,
            ShowDialogButtons = false,
            Width = 300,
            Height = 400
        };
    }
}
