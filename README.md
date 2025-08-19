# 🔗 Binder Tool - Advanced Executable Binding Framework

> **⚠️ DISCLAIMER: This tool is developed for EDUCATIONAL PURPOSES ONLY. Users are responsible for ensuring compliance with applicable laws and ethical guidelines. This tool should only be used for legitimate software development, testing, and learning purposes.**

[![.NET](https://img.shields.io/badge/.NET-6.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/6.0)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows-blue.svg)](https://www.microsoft.com/windows)

## 📖 Overview

The **Binder** is a sophisticated C# console application that demonstrates advanced software development concepts including code generation, compilation automation, and executable binding techniques. This project serves as an excellent learning resource for developers interested in:

- **Dynamic Code Generation**: Runtime C# code creation and compilation
- **Build Automation**: Automated project compilation and deployment
- **Software Packaging**: Single-file executable creation
- **Anti-Analysis Techniques**: Understanding how software protection works
- **System Integration**: Windows API calls and process management

## 🚀 Features

### 🔧 Core Functionality
- **Multi-Executable Binding**: Combine multiple remote executables into a single distributable file
- **Individual Stub Links**: Configure separate download URLs for each executable
- **Flexible Execution Location**: Choose between `%TEMP%` folder or same directory execution
- **Automatic Compilation**: Generates and compiles C# code into final executable

### 🛡️ Advanced Protection Features
- **Code Obfuscation**: Randomize namespace, class, method, and variable names
- **Anti-Debugger Protection**: Detect and exit when debugger is attached
- **Customizable Timeout**: Set download timeout or run indefinitely
- **Silent Error Handling**: Graceful failure handling for robust operation

### 🎨 Customization Options
- **Custom Icon Support**: Download and apply custom `.ico` files via URL
- **Icon Filename Control**: Specify exact filename for downloaded icons
- **Assembly Metadata**: Customizable assembly information and versioning

### 📦 Build & Deployment
- **Single-File Output**: Self-contained executables with no external dependencies
- **Cross-Platform Ready**: Built with .NET 6.0 for modern Windows systems
- **Automatic Cleanup**: Removes temporary files and compilation artifacts
- **Build Scripts**: Automated build processes for Windows (`.bat` and `.ps1`)

## 🛠️ Technical Requirements

- **Operating System**: Windows 10/11 (64-bit)
- **.NET Runtime**: .NET 6.0 or later
- **Development Tools**: Visual Studio 2022 or VS Code (optional)
- **System Architecture**: x64 (64-bit)

## 📥 Installation

### Option 1: Build from Source (Recommended)
```bash
# Clone the repository
git clone https://github.com/MidasRX/Binder.git
cd binder-tool

# Build using provided scripts
# Windows Batch
build.bat

# Or PowerShell
build.ps1

# Or manual build
dotnet restore
dotnet publish --configuration Release --output ./publish --self-contained true --runtime win-x64
```

### Option 2: Download Pre-built
Download the latest release from the [Releases](https://github.com/MidasRX/Binder/releases) page.

## 🎯 Usage Guide

### Basic Usage
1. **Run the Tool**: Execute `Binder.exe`
2. **Configure Executables**: Specify number and details for each executable
3. **Set Options**: Choose protection and customization features
4. **Generate**: Create the final bound executable

### Advanced Configuration
```bash
# Example workflow
Enter the number of executables to bind: 2

Choose execution location:
1. %TEMP% folder (recommended)
2. Same folder as the binder

Do you want to obfuscate the generated code? (y/n): y
Do you want to set a custom timeout? (y/n): n
Do you want to add anti-debugger protection? (y/n): y
Do you want to use a custom icon for the output? (y/n): y

# For each executable:
Enter filename: myapp.exe
Enter stub link: https://example.com/myapp.exe
Enter icon URL: https://example.com/icon.ico
Enter icon filename: app.ico
```

## 🔍 Feature Deep Dive

### Code Obfuscation
The tool implements sophisticated obfuscation techniques:
- **Namespace Randomization**: Generates random 8-character namespace names
- **Class Name Obfuscation**: Creates random 10-character class identifiers
- **Method Name Scrambling**: Produces random 12-character method names
- **Variable Name Randomization**: Assigns random 8-character variable names

### Anti-Debugger Protection
Utilizes Windows API calls for advanced protection:
```csharp
[DllImport("kernel32.dll")]
static extern bool IsDebuggerPresent();

[DllImport("kernel32.dll")]
static extern void OutputDebugString(string lpOutputString);
```
## 📚 Educational Value

This project demonstrates several important software development concepts:

### 1. **Dynamic Code Generation**
- Runtime C# code creation
- String-based code assembly
- Template-based code generation

### 2. **Build Automation**
- Automated project compilation
- Dependency management
- Output file handling

### 3. **System Integration**
- Windows API calls (P/Invoke)
- Process management
- File system operations

### 4. **Software Protection**
- Anti-debugging techniques
- Code obfuscation methods
- Runtime protection mechanisms

### 5. **Error Handling**
- Graceful failure management
- Silent error handling
- Robust cleanup procedures

## 🔒 Security & Ethics

### Intended Use Cases
- **Software Development**: Testing deployment strategies
- **Educational Research**: Learning about software protection
- **Penetration Testing**: Understanding attack vectors
- **Security Research**: Developing defense mechanisms

### Responsible Disclosure
- Report security vulnerabilities via GitHub Issues
- Follow responsible disclosure guidelines
- Contribute to improving security practices
## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## ⚠️ Legal Notice

- **Educational Purpose Only**: This tool is designed for learning and research
- **User Responsibility**: Users must comply with all applicable laws
- **No Warranty**: Provided "as is" without any warranties
- **Liability**: Developers are not responsible for misuse of this tool

## 🆘 Support

- **Documentation**: Check this README and code comments
- **Issues**: Report bugs via [GitHub Issues](https://github.com/MidasRX/binder/issues)
- **Discussions**: Join community discussions
- **Wiki**: Additional documentation and tutorials

## 🙏 Acknowledgments

- **.NET Community**: For the excellent framework and tools
- **Open Source Contributors**: Everyone who has contributed to this project
- **Security Researchers**: For insights into software protection techniques
- **Educational Community**: For promoting responsible software development

## 📈 Roadmap

- [ ] **Cross-Platform Support**: Linux and macOS compatibility
- [ ] **Advanced Obfuscation**: More sophisticated code protection
- [ ] **Plugin System**: Extensible architecture for custom features
- [ ] **GUI Interface**: Modern graphical user interface
- [ ] **Cloud Integration**: Remote configuration and deployment
- [ ] **Analytics Dashboard**: Usage statistics and monitoring

---

**⭐ Star this repository if you find it helpful for your learning journey!**

**🔗 Connect with us:**
- [GitHub](https://github.com/MidasRX)
- [Documentation](https://github.com/MidasRX/binder/wiki)
- [Issues](https://github.com/MidasRX/binder/issues)

---

*Built with ❤️ for the developer community* 
