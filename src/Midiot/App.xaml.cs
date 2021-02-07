namespace Midiot
{
    using System.Windows;
    using SingleInstanceCore;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstance
    {
        public void OnInstanceInvoked(string[] args)
        {
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var isFirstInstance = SingleInstance<App>.InitializeAsFirstInstance("tcbroad_Midiot");
            if (!isFirstInstance)
            {
                //If it's not the first instance, arguments are automatically passed to the first instance
                //OnInstanceInvoked will be raised on the first instance
                //You may shut down the current instance
                Current.Shutdown();
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            //Do not forget to cleanup
            SingleInstance<App>.Cleanup();

            base.OnExit(e);
        }
    }
}
