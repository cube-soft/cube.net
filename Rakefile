# --------------------------------------------------------------------------- #
#
# Copyright (c) 2010 CubeSoft, Inc.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#  http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
#
# --------------------------------------------------------------------------- #
require 'rake'
require 'rake/clean'

# --------------------------------------------------------------------------- #
# configuration
# --------------------------------------------------------------------------- #
PROJECT     = "Cube.Net"
MAIN        = "#{PROJECT}.Apps"
LIB         = "../packages"
CONFIG      = "Release"
BRANCHES    = ["master", "net35"]
PLATFORMS   = ["Any CPU"]
PACKAGES    = ["Libraries/Core/#{PROJECT}.nuspec"]
TESTCASES   = {"#{PROJECT}.Tests"     => "Libraries/Tests",
               "#{PROJECT}.Rss.Tests" => "Applications/Rss/Tests"}

# --------------------------------------------------------------------------- #
# commands
# --------------------------------------------------------------------------- #
BUILD = "msbuild -v:m -t:build -p:Configuration=#{CONFIG}"
PACK  = %(nuget pack -Properties "Configuration=#{CONFIG};Platform=AnyCPU")
TEST  = "../packages/NUnit.ConsoleRunner/3.10.0/tools/nunit3-console.exe"

# --------------------------------------------------------------------------- #
# clean
# --------------------------------------------------------------------------- #
CLEAN.include("#{PROJECT}.*.nupkg")
CLEAN.include("#{LIB}/cube.*")
CLEAN.include(['bin', 'obj'].map{ |e| "**/#{e}" })

# --------------------------------------------------------------------------- #
# default
# --------------------------------------------------------------------------- #
desc "Clean, build, test, and create NuGet packages."
task :default => [:clean, :build_all, :test_all, :pack]

# --------------------------------------------------------------------------- #
# pack
# --------------------------------------------------------------------------- #
desc "Create NuGet packages in the net35 branch."
task :pack do
    sh("git checkout net35")
    PACKAGES.each { |e| sh("#{PACK} #{e}") }
    sh("git checkout master")
end

# --------------------------------------------------------------------------- #
# build
# --------------------------------------------------------------------------- #
desc "Build projects in the current branch."
task :build, [:platform] do |_, e|
    e.with_defaults(:platform => PLATFORMS[0])
    sh("nuget restore #{MAIN}.sln")
    sh(%(#{BUILD} -p:Platform="#{e.platform}" #{MAIN}.sln))
end

# --------------------------------------------------------------------------- #
# build_all
# --------------------------------------------------------------------------- #
desc "Build projects in pre-defined branches and platforms."
task :build_all do
    BRANCHES.product(PLATFORMS).each { |e|
        sh("git checkout #{e[0]}")
        Rake::Task[:build].reenable
        Rake::Task[:build].invoke(e[1])
    }
    sh("git checkout master")
end

# --------------------------------------------------------------------------- #
# build_test
# --------------------------------------------------------------------------- #
desc "Build and test projects in the current branch."
task :build_test => [:build, :test]

# --------------------------------------------------------------------------- #
# test
# --------------------------------------------------------------------------- #
desc "Test projects in the current branch."
task :test do
    fw  = %x(git symbolic-ref --short HEAD).chomp
    fw  = 'net45' if (fw != 'net35')
    bin = ['bin', PLATFORMS[0], CONFIG, fw].join('/')
    TESTCASES.each { |p, d| sh(%(#{TEST} "#{d}/#{bin}/#{p}.dll" --work="#{d}/#{bin}")) }
end

# --------------------------------------------------------------------------- #
# test_all
# --------------------------------------------------------------------------- #
desc "Test projects in pre-defined branches."
task :test_all do
    BRANCHES.each { |e|
        sh("git checkout #{e}")
        Rake::Task[:test].execute
    }
    sh("git checkout master")
end