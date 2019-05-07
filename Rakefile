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
PROJECT     = 'Cube.Net'
LIBRARY     = '../packages'
CONFIG      = 'Release'
BRANCHES    = ['master', 'net35']
PLATFORMS   = ['Any CPU']
PACKAGES    = ["Libraries/Core/#{PROJECT}.nuspec"]
TESTCASES   = {"#{PROJECT}.Tests"     => 'Libraries/Tests',
               "#{PROJECT}.Rss.Tests" => 'Applications/Rss/Tests'}

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
CLEAN.include("#{LIBRARY}/cube.*")
CLEAN.include(['bin', 'obj'].map{ |e| "**/#{e}" })

# --------------------------------------------------------------------------- #
# default
# --------------------------------------------------------------------------- #
desc "Build the solution and create NuGet packages."
task :default => [:clean_build, :pack]

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
# clean_build
# --------------------------------------------------------------------------- #
desc "Clean objects and build the solution in pre-defined branches and platforms."
task :clean_build => [:clean] do
    BRANCHES.product(PLATFORMS).each { |e|
        sh("git checkout #{e[0]}")
        RakeFileUtils::rm_rf(FileList.new("#{LIBRARY}/cube.*"))
        Rake::Task[:build].reenable
        Rake::Task[:build].invoke(e[1])
    }
end

# --------------------------------------------------------------------------- #
# build
# --------------------------------------------------------------------------- #
desc "Build the solution in the current branch."
task :build, [:platform] do |_, e|
    e.with_defaults(:platform => PLATFORMS[0])
    sh("nuget restore #{PROJECT}.Apps.sln")
    sh(%(#{BUILD} -p:Platform="#{e.platform}" #{PROJECT}.Apps.sln))
end

# --------------------------------------------------------------------------- #
# test
# --------------------------------------------------------------------------- #
desc "Build and test projects in the current branch."
task :test => [:build] do
    fw  = %x(git symbolic-ref --short HEAD).chomp
    fw  = 'net45' if (fw != 'net35')
    bin = ['bin', PLATFORMS[0], CONFIG, fw].join('/')

    TESTCASES.each { |proj, root|
        dir = "#{root}/#{bin}"
        sh(%(#{TEST} "#{dir}/#{proj}.dll" --work="#{dir}"))
    }
end
