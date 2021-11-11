Cube.Net
====

[![Net](https://badgen.net/nuget/v/cube.net?label=net)](https://www.nuget.org/packages/Cube.Net/)
[![Rss](https://badgen.net/nuget/v/cube.net.rss?label=rss)](https://www.nuget.org/packages/Cube.Net.Rss/)
[![AppVeyor](https://badgen.net/appveyor/ci/clown/cube-net)](https://ci.appveyor.com/project/clown/cube-net)
[![Codecov](https://badgen.net/codecov/c/github/cube-soft/cube.net)](https://codecov.io/gh/cube-soft/Cube.Net)

Cube.Net is a network library.
The repository also has some implemented applications based on the Cube.Net project.
Libraries and applications are available for .NET Framework 3.5, 4.5, .NET Standard 2.0 (.NET Core 3.1 for applications), or later.

### CubeRSS Reader

[CubeRSS Reader](https://github.com/cube-soft/Cube.Net/blob/master/Applications/Rss) is an RSS/Atom feed reader that runs without receiving any assistance from servers for monitoring and crawling feeds.

![Screenshot](https://github.com/cube-soft/Cube.Net/blob/master/Applications/Rss/Overview.png?raw=true)

You can get the executable installer from the [download page](https://www.cube-soft.jp/cuberssreader/index.php) (Japanese), or [directly download link](https://www.cube-soft.jp/cuberssreader/dl.php).
We currently show menus and other messages only in Japanese, and support for other languages are one of our future works...

## Contributing

1. Fork [Cube.Net](https://github.com/cube-soft/Cube.Net/fork) repository.
2. Create a feature branch from the master, net45, or net50 branch (e.g. git checkout -b my-new-feature origin/master). Note that the master branch may refer some pre-released NuGet packages. Try the [rake clobber](https://github.com/cube-soft/Cube.Net/blob/master/Rakefile) command when build errors occur.
3. Commit your changes.
4. Rebase your local changes to the corresponding branch.
5. Run the dotnet test command or the Visual Studio and confirm that it passes.
6. Create new Pull Request.

## License

Copyright © 2010 [CubeSoft, Inc.](https://www.cube-soft.jp/)

The project is licensed under the [Apache 2.0](https://github.com/cube-soft/Cube.Net/blob/master/License.txt).
Note that trade names, trademarks, service marks, or logo images distributed in CubeSoft applications are not allowed to reuse or modify all or parts of them.