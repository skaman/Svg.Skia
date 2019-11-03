# Svg.Skia

[![Build status](https://dev.azure.com/wieslawsoltes/GitHub/_apis/build/status/Sources/Svg.Skia)](https://dev.azure.com/wieslawsoltes/GitHub/_build/latest?definitionId=-1)

Skia SVG rendering library.

## NuGet

| Package              | Version                                                                                                                 |
|----------------------|-------------------------------------------------------------------------------------------------------------------------|
| Svg.Custom           | [![NuGet](https://img.shields.io/nuget/v/Svg.Custom.svg)](https://www.nuget.org/packages/Svg.Custom)                    |
| Svg.Skia             | [![NuGet](https://img.shields.io/nuget/v/Svg.Skia.svg)](https://www.nuget.org/packages/Svg.Skia)                        |
| Svg.Skia.Converter   | [![NuGet](https://img.shields.io/nuget/v/Svg.Skia.Converter.svg)](https://www.nuget.org/packages/Svg.Skia.Converter)    |

## Usage

### Library

```
dotnet add package Svg.Skia
```

```C#
using SkiaSharp;
using Svg.Skia;

using (var svg = new Svg())
{
    if (svg.Load("image.svg") != null)
    {
        svg.Save("image.png", SKEncodedImageFormat.Png, 100, 1f, 1f);
    }
}
```

```C#
using SkiaSharp;
using Svg.Skia;

using (var svg = new Svg())
{
    if (svg.Load("image.svgz") != null)
    {
        svg.Save("image.png", SKEncodedImageFormat.Png, 100, 1f, 1f);
    }
}
```

### Tool

```
dotnet tool install -g Svg.Skia.Converter
```

```
Svg.Skia.Converter:
  Converts a svg file to an encoded bitmap image.

Usage:
  Svg.Skia.Converter [options]

Options:
  -f, --files <files>                The relative or absolute path to the input files
  -d, --directories <directories>    The relative or absolute path to the input directories
  -o, --output <output>              The relative or absolute path to the output directory
  -p, --pattern <pattern>            The search string to match against the names of files in the input directory
  --format <format>                  The output image format
  -q, --quality <quality>            The output image quality
  -b, --background <background>      The output image background
  -s, --scale <scale>                The output image horizontal and vertical scaling factor
  -sx, --scaleX <scalex>             The output image horizontal scaling factor
  -sy, --scaleY <scaley>             The output image vertical scaling factor
  --debug                            Write debug output to a file
  --quiet                            Set verbosity level to quiet
  -c, --load-config <load-config>    The relative or absolute path to the config file
  --save-config <save-config>        The relative or absolute path to the config file
  --version                          Display version information
```

### Example

```
cd ./src/Svg.Skia.Converter/bin/Debug/netcoreapp3.0
Svg.Skia.Converter.exe -d ../../../../../externals\SVG\Tests\W3CTestSuite\svg\ -o ./png
```

## Build

To build the projects you need to install [.NET Core 3.0](https://dotnet.microsoft.com/download/dotnet-core/3.0) version `SDK 3.0.100`.

```
git clone git@github.com:wieslawsoltes/Svg.Skia.git
cd Svg.Skia
git submodule update --init --recursive
dotnet build -c Release ./src/Svg.Custom/Svg.Custom.csproj
dotnet build -c Release ./src/Svg.Skia/Svg.Skia.csproj
dotnet build -c Release ./src/Svg.Skia.Converter/Svg.Skia.Converter.csproj
```

## Publish

```
git clone git@github.com:wieslawsoltes/Svg.Skia.git
cd Svg.Skia
git submodule update --init --recursive
export LANG=en-US.UTF-8
dotnet pack -c Release -o ./artifacts --version-suffix "preview5" ./src/Svg.Custom/Svg.Custom.csproj
dotnet pack -c Release -o ./artifacts --version-suffix "preview5" ./src/Svg.Skia/Svg.Skia.csproj
dotnet pack -c Release -o ./artifacts --version-suffix "preview5" ./src/Svg.Skia.Converter/Svg.Skia.Converter.csproj
dotnet nuget push ./artifacts/Svg.Custom.0.0.1-preview5.nupkg -k <key> -s https://api.nuget.org/v3/index.json
dotnet nuget push ./artifacts/Svg.Skia.0.0.1-preview5.nupkg -k <key> -s https://api.nuget.org/v3/index.json
dotnet nuget push ./artifacts/Svg.Skia.Converter.0.0.1-preview5.nupkg -k <key> -s https://api.nuget.org/v3/index.json
```

## Testing

### Install

```
dotnet tool install --global --add-source ./artifacts Svg.Skia.Converter
export DOTNET_ROOT=$HOME/dotnet
```

### Uninstall

```
dotnet tool uninstall -g Svg.Skia.Converter
Svg.Skia.Converter --help
```

## Externals

The `Svg.Skia` library is using code from the https://github.com/vvvv/SVG

## License

Svg.Skia is licensed under the [MIT license](LICENSE.TXT).
