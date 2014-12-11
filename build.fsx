// --------------------------------------------------------------------------------------
// FAKE build script 
// --------------------------------------------------------------------------------------

#r @"packages/FAKE/tools/FakeLib.dll"
open Fake 
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open System
open System.IO
open System.Text.RegularExpressions

// Information about the project are used
//  - for version and project name in generated AssemblyInfo file
//  - by the generated NuGet package 
//  - to run tests and to publish documentation on GitHub gh-pages
//  - for documentation, you also need to edit info in "docs/tools/generate.fsx"

// The name of the project 
// (used by attributes in AssemblyInfo, name of a NuGet package and directory in 'src')
let project = "Paket.VisualStudio"

// Short summary of the project
// (used as description in AssemblyInfo and as a short summary for NuGet package)
let summary = "Manage your Paket dependencies from Visual Studio!"

// Longer description of the project
// (used as a description for NuGet package; line breaks are automatically cleaned up)
let description = "Manage your Paket dependencies from Visual Studio!"
// List of author names (for NuGet package)
let authors = [ "Igal Tabachnik" ]
// Tags for your project (for NuGet package)
let tags = "package management paket nuget"

// File system information 
// (<solutionFile>.sln is built during the building process)
let solutionFile  = "Paket.VisualStudio"
// Pattern specifying assemblies to be tested using NUnit
let testAssemblies = "tests/**/bin/Release/*Tests*.dll"

// Git configuration (used for publishing documentation in gh-pages branch)
// The profile where the project is posted 
let gitOwner = "fsprojects"
let gitHome = "https://github.com/" + gitOwner

// The name of the project on GitHub
let gitName = "Paket.VisualStudio"
let cloneUrl = "git@github.com:hmemcpy/Paket.VisualStudio.git"

// Read additional information from the release notes document
Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
let release = parseReleaseNotes (IO.File.ReadAllLines "RELEASE_NOTES.md")

let isAppVeyorBuild = environVar "APPVEYOR" <> null
let buildVersion = sprintf "%s-a%s" release.NugetVersion (DateTime.UtcNow.ToString "yyMMddHHmm")

Target "BuildVersion" (fun _ ->
    Shell.Exec("appveyor", sprintf "UpdateBuild -Version \"%s\"" buildVersion) |> ignore
)

// Generate assembly info files with the right version & up-to-date information
Target "AssemblyInfo" (fun _ ->
  let shared =
      [ Attribute.Product project
        Attribute.Description summary
        Attribute.Version release.AssemblyVersion
        Attribute.FileVersion release.AssemblyVersion ] 

  CreateCSharpAssemblyInfo "src/Paket.VisualStudio/Properties/AssemblyInfo.cs"
      (Attribute.InternalsVisibleTo "Paket.VisualStudio.Tests" :: Attribute.Title "Paket.VisualStudio" :: shared)
)

// --------------------------------------------------------------------------------------
// Clean build results

Target "Clean" (fun _ ->
    CleanDirs ["bin"; "bin/vsix"; "temp"; "nuget"]
)

Target "CleanDocs" (fun _ ->
    CleanDirs ["docs/output"]
)

// --------------------------------------------------------------------------------------
// Build library & test project

Target "Build" (fun _ ->
    // We would like to build only one solution
    !! (solutionFile + ".sln")
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

Target "CleanVSIX" (fun _ ->
    ZipHelper.Unzip "bin/vsix" "bin/Paket.VisualStudio.vsix"
    let regex = Regex("bin")
    let filesToKeep =
      Directory.GetFiles("bin", "*.dll")
      |> Seq.map (fun fileName -> regex.Replace(fileName, "bin/vsix", 1))
    let filesToDelete = 
      Seq.fold (--) (!! "bin/vsix/*.dll") filesToKeep
        ++ "bin/vsix/Microsoft.VisualStudio*"
        ++ "bin/vsix/Microsoft.Build*"
    DeleteFiles filesToDelete
    ZipHelper.Zip "bin/vsix" "bin/Paket.VisualStudio.vsix" (!! "bin/vsix/**")
)

// Build test projects in Debug mode in order to provide correct paths for multi-project scenarios
Target "BuildTests" (fun _ ->    
    !! "tests/data/**/*.sln"
    |> MSBuildDebug "" "Rebuild"
    |> ignore
)

// --------------------------------------------------------------------------------------
// Run the unit tests using test runner

Target "UnitTests" (fun _ ->
    if !! testAssemblies |> Seq.isEmpty |> not then
        !! testAssemblies 
        |> NUnit (fun p ->
            let param =
                { p with
                    DisableShadowCopy = true
                    TimeOut = TimeSpan.FromMinutes 20.
                    Framework = "4.5"
                    Domain = NUnitDomainModel.MultipleDomainModel
                    OutputFile = "TestResults.xml" }
            if isAppVeyorBuild then { param with ExcludeCategory = "AppVeyorLongRunning" } else param)
)

// --------------------------------------------------------------------------------------
// Run the integration tests using test runner

Target "IntegrationTests" (fun _ ->
    if !! "tests/**/bin/Release/Paket.VisualStudio.dll" |> Seq.isEmpty |> not then
        !! "tests/**/bin/Release/Paket.VisualStudio.dll" 
        |> MSTest.MSTest (fun p ->
            { p with
                TimeOut = TimeSpan.FromMinutes 20.
            })
)

// --------------------------------------------------------------------------------------
// Generate the documentation

Target "GenerateDocs" (fun _ ->
    executeFSIWithArgs "docs/tools" "generate.fsx" ["--define:RELEASE"] [] |> ignore
)

// --------------------------------------------------------------------------------------
// Release Scripts

Target "ReleaseDocs" (fun _ ->
    let tempDocsDir = "temp/gh-pages"
    CleanDir tempDocsDir
    Repository.cloneSingleBranch "" cloneUrl "gh-pages" tempDocsDir

    fullclean tempDocsDir
    CopyRecursive "docs/output" tempDocsDir true |> tracefn "%A"
    StageAll tempDocsDir
    Git.Commit.Commit tempDocsDir (sprintf "[skip ci] Update generated documentation for version %s" release.NugetVersion)
    Branches.push tempDocsDir
)

#load "paket-files/fsharp/FAKE/modules/Octokit/Octokit.fsx"
open Octokit

Target "Release" (fun _ ->
    StageAll ""
    Git.Commit.Commit "" (sprintf "Bump version to %s" release.NugetVersion)
    Branches.push ""

    Branches.tag "" release.NugetVersion
    Branches.pushTag "" "origin" release.NugetVersion

    // release on github
    createClient (getBuildParamOrDefault "github-user" "") (getBuildParamOrDefault "github-pw" "")
    |> createDraft gitOwner gitName release.NugetVersion (release.SemVer.PreRelease <> None) release.Notes 
    |> uploadFile "./bin/FSharpVSPowerTools.vsix"
    |> releaseDraft
    |> Async.RunSynchronously
)

Target "ReleaseAll"  DoNothing

// --------------------------------------------------------------------------------------
// Run main targets by default. Invoke 'build <Target>' to override

Target "Main" DoNothing

Target "All" DoNothing

"Clean"
  =?> ("BuildVersion", isAppVeyorBuild)
  ==> "AssemblyInfo"
  ==> "Build"
  ==> "BuildTests"
  ==> "UnitTests"
  ==> "Main"

"Build"
  ==> "CleanVSIX"

"Release"
  ==> "ReleaseAll"

"Main"
  =?> ("IntegrationTests", isLocalBuild)
  ==> "All"

"Main" 
  ==> "CleanDocs"
  ==> "GenerateDocs"
  ==> "ReleaseDocs"
  ==> "Release"

RunTargetOrDefault "Main"
