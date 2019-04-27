using System;
using System.Windows;
using System.Management.Automation;
using System.Diagnostics;

namespace TechRepairActivator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (Object sender, ResolveEventArgs args) =>
            {
                String thisExe = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                System.Reflection.AssemblyName embeddedAssembly = new System.Reflection.AssemblyName(args.Name);
                String resourceName = thisExe + "." + embeddedAssembly.Name + ".dll";

                using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return System.Reflection.Assembly.Load(assemblyData);
                }
            };

            InitializeComponent();
            OperatingSystem os = Environment.OSVersion;
            Version vs = os.Version;
            WindowsVersionTextBlock.Text = "Windows OS: " + ((vs.Major == 10 || vs.Minor == 0) ? "10\n" : "7/8\n");
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            var userName = userBox.Text;
            var password = passwordBox.Text;
            if (userName == (null) || userName == ""
                || password == (null) || password == "")
            {
                MessageBoxResult test = MessageBox.Show("Username and password cannot be empty!", "Error");
            }
            else
            {
                try
                {
                    windowsAlgorithm(userName, password);
                    MessageBoxResult response = MessageBox.Show("Account activated! Click OK to restart.", "Success");
                    if (response == MessageBoxResult.OK)
                    {
                        System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
                    }
                }
                catch
                {
                    MessageBox.Show("Unexpected error while creating new admin user", "Error");
                    throw;
                }
            }
        }
        private void windowsAlgorithm(string userName, string password)
        {
            OperatingSystem os = Environment.OSVersion;
            Version vs = os.Version;
            Console.WriteLine("Windows OS: " + ((vs.Major == 10 || vs.Minor == 0) ? "10\n" : "7/8\n"));
            string operatingSystem = (vs.Major == 10 || vs.Minor == 0) ? "10" : "8";
            userName = "\"" + userName + "\"";

            string win10c1 = "New-localUser –Name " + userName + " -Description " + userName + " -NoPassword";
            string win10c2 = "Add-LocalGroupMember –Group \"Administrators\" - Member " + userName;
            string win10c3 = "Disable-localUser –Name \"Administrator\"";

            string win8c1 = "net user /add " + userName + " " + password;
            string win8c2 = "Net localgroup Administrators " + userName + " /add";
            string win8c3 = "Net user Administrator /active:no";
            if (operatingSystem == "10")
            {
                using (PowerShell PowerShellInstance = PowerShell.Create())
                {
                    PowerShellInstance.AddScript(win10c1);
                    PowerShellInstance.Invoke();
                    PowerShellInstance.AddScript(win10c2);
                    PowerShellInstance.Invoke();
                    PowerShellInstance.AddScript(win10c3);
                    PowerShellInstance.Invoke();
                    PowerShellInstance.Stop();
                }
            }
            else if (operatingSystem == "8")
            {
                Process cmd = new Process();
                cmd.StartInfo.FileName = "cmd.exe";
                cmd.StartInfo.RedirectStandardInput = true;
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.UseShellExecute = false;
                cmd.Start();

                cmd.StandardInput.WriteLine(win8c1);
                cmd.StandardInput.WriteLine(win8c2);
                cmd.StandardInput.WriteLine(win8c3);
                cmd.StandardInput.Flush();
                cmd.StandardInput.Close();
                cmd.WaitForExit();
                Console.WriteLine(cmd.StandardOutput.ReadToEnd());
            }
        }
    }
}