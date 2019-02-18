require 'rake'
require 'rake/clean'

# --------------------------------------------------------------------------- #
# Configuration
# --------------------------------------------------------------------------- #
SOLUTION    = 'Cube.Net.Rss'
PACKAGE     = 'Cube.Net'
BRANCHES    = [ 'stable', 'net35' ]
TESTCASES   = {
    'Cube.Net.Tests'     => 'Tests',
    'Cube.Net.Rss.Tests' => 'Applications/Rss/Tests'
}

# --------------------------------------------------------------------------- #
# Commands
# --------------------------------------------------------------------------- #
COPY        = 'cp -pf'
CHECKOUT    = 'git checkout'
BUILD       = 'msbuild /t:Clean,Build /m /verbosity:minimal /p:Configuration=Release;Platform="Any CPU";GeneratePackageOnBuild=false'
RESTORE     = 'nuget restore'
PACK        = 'nuget pack -Properties "Configuration=Release;Platform=AnyCPU"'
TEST        = '../packages/NUnit.ConsoleRunner.3.9.0/tools/nunit3-console.exe'

# --------------------------------------------------------------------------- #
# Tasks
# --------------------------------------------------------------------------- #
task :default do
    Rake::Task[:clean].execute
    Rake::Task[:build].execute
    Rake::Task[:pack].execute
end

# --------------------------------------------------------------------------- #
# Build
# --------------------------------------------------------------------------- #
task :build do
    BRANCHES.each do |branch|
        sh("#{CHECKOUT} #{branch}")
        sh("#{RESTORE} #{SOLUTION}.sln")
        sh("#{BUILD} #{SOLUTION}.sln")
    end
end

# --------------------------------------------------------------------------- #
# Pack
# --------------------------------------------------------------------------- #
task :pack do
    sh("#{CHECKOUT} net35")
    sh("#{PACK} Libraries/#{PACKAGE}.nuspec")
    sh("#{CHECKOUT} master")
end

# --------------------------------------------------------------------------- #
# Test
# --------------------------------------------------------------------------- #
task :test do
    sh("#{RESTORE} #{SOLUTION}.sln")
    sh("#{BUILD} #{SOLUTION}.sln")

    branch = `git symbolic-ref --short HEAD`.chomp
    TESTCASES.each { |proj, dir|
        src = branch == 'net35' ?
              "#{dir}/bin/net35/Release/#{proj}.dll" :
              "#{dir}/bin/Release/#{proj}.dll"
        sh("#{TEST} #{src}")
    }
end

# --------------------------------------------------------------------------- #
# Clean
# --------------------------------------------------------------------------- #
CLEAN.include("#{PACKAGE}.*.nupkg")
CLEAN.include(%w{dll log}.map{ |e| "**/*.#{e}" })