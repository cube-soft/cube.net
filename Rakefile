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
APPLICATION = 'Rss'
LIBRARY     = '../packages'
BRANCHES    = ['stable', 'net35']
PACKAGES    = ["Libraries/#{PROJECT}.nuspec"]
TESTCASES   = {
    'Cube.Net.Tests'     => 'Tests',
    'Cube.Net.Rss.Tests' => 'Applications/Rss/Tests'
}

# --------------------------------------------------------------------------- #
# commands
# --------------------------------------------------------------------------- #
BUILD = 'msbuild /t:Clean,Build /m /verbosity:minimal /p:Configuration=Release;Platform="Any CPU";GeneratePackageOnBuild=false'
PACK  = 'nuget pack -Properties "Configuration=Release;Platform=AnyCPU"'
TEST  = '../packages/NUnit.ConsoleRunner/3.10.0/tools/nunit3-console.exe'

# --------------------------------------------------------------------------- #
# clean
# --------------------------------------------------------------------------- #
CLEAN.include("#{PROJECT}.*.nupkg")
CLEAN.include("#{LIBRARY}/cube.*")
CLEAN.include(%w{bin obj}.map{ |e| "**/#{e}" })

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
desc "Clean objects and build the solution in pre-defined branches."
task :clean_build => [:clean] do
    BRANCHES.each { |e|
        sh("git checkout #{e}")
        rm_rf("#{LIBRARY}/cube.*")
        Rake::Task[:build].execute
    }
end

# --------------------------------------------------------------------------- #
# build
# --------------------------------------------------------------------------- #
desc "Build the solution in the current branch."
task :build do
    sh("nuget restore #{PROJECT}.#{APPLICATION}.sln")
    sh("#{BUILD} #{PROJECT}.#{APPLICATION}.sln")
end

# --------------------------------------------------------------------------- #
# test
# --------------------------------------------------------------------------- #
desc "Build and test projects in the current branch."
task :test => [:build] do
    fw  = `git symbolic-ref --short HEAD`.chomp
    fw  = 'net45' if (fw != 'net35')
    bin = ['bin', 'Any CPU', 'Release', fw].join('/')

    TESTCASES.each { |proj, root|
        dir = "#{root}/#{bin}"
        sh("#{TEST} \"#{dir}/#{proj}.dll\" --work=\"#{dir}\"")
    }
end
