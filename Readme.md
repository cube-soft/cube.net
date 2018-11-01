Cube.Net
====

[![AppVeyor](https://img.shields.io/appveyor/ci/clown/cube-net/master.svg?label=master&logo=appveyor)](https://ci.appveyor.com/project/clown/cube-net)
[![AppVeyor](https://img.shields.io/appveyor/ci/clown/cube-net-ftg7w/chrome.svg?label=chrome&logo=appveyor)](https://ci.appveyor.com/project/clown/cube-net-ftg7w)
[![Codecov](https://codecov.io/gh/cube-soft/Cube.Net/branch/master/graph/badge.svg)](https://codecov.io/gh/cube-soft/Cube.Net)

Cube.Net is a network library.
The repository also has some implemented applications based on the Cube.Net project as follows.
Libraries and applications are available for .NET Framework 3.5, 4.5 or more.

### CubeRSS Reader

[CubeRSS Reader](https://github.com/cube-soft/Cube.Net/tree/master/Applications/Rss) is an RSS/Atom feed reader that runs without receiving any assistance from servers for monitoring and crawling feeds.

![Screenshot](https://github.com/cube-soft/Cube.Net/tree/master/Applications/Rss/Overview.png?raw=true)

You can get the executable installer from the [download page](https://www.cube-soft.jp/cuberssreader/index.php) (Japanese), or [directly download link](https://www.cube-soft.jp/cuberssreader/dl.php).
We currently show menus and other messages only in Japanese, and support for other languages are one of our future works...

## Dependencies

Dependencies of [Libraries](https://github.com/cube-soft/Cube.Net/tree/master/Libraries) are as follows. [Applications](https://github.com/cube-soft/Cube.Net/tree/master/Applications) may use some other third-party libraries.

* [Cube.Core](https://github.com/cube-soft/Cube.Core)
* [Cube.FileSystem](https://github.com/cube-soft/Cube.FileSystem)
* [SgmlReader](https://github.com/MindTouch/SGMLReader)

## Contributing

1. Fork [Cube.Net](https://github.com/cube-soft/Cube.Net/fork) repository.
2. Create a feature branch from the [stable](https://github.com/cube-soft/Cube.Net/tree/stable) branch (git checkout -b my-new-feature origin/stable). The [master](https://github.com/cube-soft/Cube.Net/tree/master) branch may refer some pre-released NuGet packages. See [AppVeyor.yml](https://github.com/cube-soft/Cube.Net/blob/master/AppVeyor.yml) if you want to build and commit in the master branch.
3. Commit your changes.
4. Rebase your local changes against the stable (or master) branch.
5. Run test suite with the [NUnit](https://nunit.org/) console or the Visual Studio (NUnit 3 test adapter) and confirm that it passes.
6. Create new Pull Request.

## License

Copyright © 2010 [CubeSoft, Inc.](https://www.cube-soft.jp/)

The project is licensed under the [Apache 2.0](https://github.com/cube-soft/Cube.Net/blob/master/License.txt).
Note that trade names, trademarks, service marks, or logo images distributed in CubeSoft applications are not allowed to reuse or modify all or parts of them.