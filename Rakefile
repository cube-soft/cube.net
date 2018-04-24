require 'rake'
require 'rake/clean'

# Configuration
PROJECT  = 'Cube.Net'
BRANCHES = [ 'master', 'net35' ]
COPY     = 'cp -pf'
CHECKOUT = 'git checkout'
BUILD    = 'msbuild /m /verbosity:minimal /p:Configuration=Release;Platform="Any CPU";GeneratePackageOnBuild=false'
RESTORE  = 'nuget restore'
PACK     = 'nuget pack -Properties "Configuration=Release;Platform=AnyCPU"'

# Tasks
task :default {
    Rake::Task[:clean].execute
    Rake::Task[:build].execute
    Rake::Task[:pack].execute
    Rake::Task[:postproc].execute
}

task :build {
    BRANCHES.each { |branch|
        sh("#{CHECKOUT} #{branch}")
        sh("#{RESTORE} #{PROJECT}.sln")
        sh("#{BUILD} #{PROJECT}.sln")
    }
}

task :pack {
    sh("#{CHECKOUT} net35")
    sh("#{PACK} Libraries/#{PROJECT}.nuspec")
}

task :postproc {
    sh("#{CHECKOUT} master")
}

CLEAN.include("#{PROJECT}.*.nupkg")
