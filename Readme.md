Cube.Net
====

[![AppVeyor](https://img.shields.io/appveyor/ci/clown/cube-net/master.svg?label=master&logo=appveyor)](https://ci.appveyor.com/project/clown/cube-net)
[![AppVeyor](https://img.shields.io/appveyor/ci/clown/cube-net-ftg7w/chrome.svg?label=chrome&logo=appveyor)](https://ci.appveyor.com/project/clown/cube-net-ftg7w)
[![Codecov](https://codecov.io/gh/cube-soft/Cube.Net/branch/master/graph/badge.svg)](https://codecov.io/gh/cube-soft/Cube.Net)
[![Codacy](https://api.codacy.com/project/badge/Grade/216dbc280bcb4b019df30825ea2293ba)](https://www.codacy.com/app/clown/Cube.Net)

Cube.Net is a network library for CubeSoft applications.
The repository also has some implemented applications based on the Cube.Net project as follows.

### CubeRSS Reader

[CubeRSS Reader](https://github.com/cube-soft/Cube.Net/tree/master/Applications/Rss) is an RSS/Atom feed reader that runs without receiving any assistance from servers for monitoring and crawling feeds.

![Screenshot](https://www.cube-soft.jp/cuberssreader/image/screenshot_original.png)

You can get the executable installer from the [download page](https://www.cube-soft.jp/cuberssreader/index.php).
We currently show menus and other messages only in Japanese, and support for other languages are one of our future works...

## Dependencies

The Cube.Net project has the following dependencies.
Note that implemented applications based on the Cube.Net project have some more dependencies.

* [Cube.Core](https://github.com/cube-soft/Cube.Core)
* [Cube.FileSystem](https://github.com/cube-soft/Cube.FileSystem)
* [SgmlReader](https://github.com/MindTouch/SGMLReader)

## Contributing

1. Fork [Cube.Net](https://github.com/cube-soft/Cube.Net/fork) repository.
2. Create a feature branch from the [release](https://github.com/cube-soft/Cube.Net/tree/release) branch (git checkout -b my-new-feature origin/release). The [master](https://github.com/cube-soft/Cube.Net/tree/master) branch may refer some pre-released version of NuGet libraries. See [AppVeyor.yml](https://github.com/cube-soft/Cube.Net/blob/master/AppVeyor.yml) if you want to build and commit in the master branch.
3. Commit your changes.
4. Rebase your local changes against the release (or master) branch.
5. Run test suite with the [NUnit](http://nunit.org/) console or the Visual Studio (NUnit 3 test adapter) and confirm that it passes.
6. Create new Pull Request.

## License

Copyright (c) 2010 [CubeSoft, Inc.](https://www.cube-soft.jp/)

The project is licensed under the [Apache 2.0](https://github.com/cube-soft/Cube.Net/blob/master/License.txt).
Note that trade names, trademarks, service marks, or logo images distributed in CubeSoft applications are not allowed to reuse or modify all or parts of them.