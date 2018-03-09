Cube.Net
====

[![AppVeyor](https://ci.appveyor.com/api/projects/status/oj2tyitj114fpt5h?svg=true&passingText=master)](https://ci.appveyor.com/project/clown/cube-net)
[![AppVeyor](https://ci.appveyor.com/api/projects/status/qa68l54c48c6jrly?svg=true&passingText=chrome)](https://ci.appveyor.com/project/clown/cube-net-ftg7w)
[![Codecov](https://codecov.io/gh/cube-soft/Cube.Net/branch/master/graph/badge.svg)](https://codecov.io/gh/cube-soft/Cube.Net)

Cube.Net is a network library for CubeSoft applications.
The repository also has some implemented applications based on the Cube.Net project as follows.

### CubeRSS Reader

[CubeRSS Reader](https://github.com/cube-soft/Cube.Net/tree/master/Applications/Rss) is a RSS/Atom feed reader that runs without receiving any assistance from servers for monitoring and crawling feeds.

![Screenshot](https://www.cube-soft.jp/cuberssreader/image/screenshot_original.png)

You can get the executable installer from the [download page](https://www.cube-soft.jp/cuberssreader/index.php).
We currently show menus and other messages only in Japanese, and support for other languages are one of our future works...

## Dependencies

Cube.Net and Cube.Net.Tests projects have the following dependencies.
Note that implemented applications based on the Cube.Net project have some more dependencies.

* [Cube.Core](https://github.com/cube-soft/Cube.Core)
* [Cube.FileSystem](https://github.com/cube-soft/Cube.FileSystem)
* [log4net](https://logging.apache.org/log4net/)
* [SgmlReader](https://github.com/MindTouch/SGMLReader)
* [NUnit](http://nunit.org/)
* [CefSharp](https://github.com/cefsharp/CefSharp) ... Used in the [chrome](https://github.com/cube-soft/Cube.Net/tree/chrome) branch.

## License

Copyright (c) 2010 [CubeSoft, Inc.](https://www.cube-soft.jp/)

The project is licensed under the [Apache 2.0](https://github.com/cube-soft/Cube.Net/blob/master/License.txt).
Note that trade names, trademarks, service marks, or logo images distributed in CubeSoft applications are not allowed to reuse or modify all or parts of them.