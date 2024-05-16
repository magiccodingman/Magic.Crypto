namespace Crypto.MauiBlazor
{
    public partial class App : Application
    {
        public string CurrentPassword { get; set; }
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }
#if WINDOWS || MACCATALYST || LINUX
        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);
            const int newWidth = 1920;
            const int newHeight = 1080; // You can adjust the height as needed

            window.Width = newWidth;
            window.Height = newHeight;
            return window;
        }
#endif
    }
}
